import pika
import json
import torch
import numpy as np
import pymysql
import time
import os
from dotenv import load_dotenv
from inference import infer
from datetime import datetime, timedelta
from data_utils import transform, invert_transform
from wait_for_services import wait_for_mysql, wait_for_rabbitmq


def is_docker():
    try:
        with open('/proc/self/cgroup', 'r') as f:
            cgroup_info = f.read()
            return 'docker' in cgroup_info or 'kubepods' in cgroup_info
    except FileNotFoundError:
        return False

if not is_docker():
    load_dotenv()

# RabbitMQ Configuration
RABBITMQ_HOST = os.getenv("RABBITMQ_HOST")
RABBITMQ_PREDICTION_QUEUE = os.getenv("RABBITMQ_PREDICTION_QUEUE")

# MySQL Configuration
MYSQL_HOST = os.getenv("MYSQL_HOST")
MYSQL_USER = os.getenv("MYSQL_USER")
MYSQL_PASSWORD = os.getenv("MYSQL_PASSWORD")
MYSQL_DB = os.getenv("MYSQL_DB")


def get_trajectory_id_by_name(cursor, trajectory_name):
    query = """
    SELECT id
    FROM trajectories
    WHERE name="{trajectory_name}"
    """.format(trajectory_name=trajectory_name)
    cursor.execute(query)
    trajectory_id = cursor.fetchone()[0]
    return trajectory_id

def write_prediction_to_db(trajectory_metadata, state_vectors):
    """
    Inserts a new state vector into the MySQL database.
    """
    connection = pymysql.connect(
        host=MYSQL_HOST,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        database=MYSQL_DB
    )
    with connection.cursor() as cursor:
        for i, (trajectory_id, base_latitude, base_longitude, base_altitude, latest_timestamp) in enumerate(trajectory_metadata):
            print(f'{i}\ttrajectory_id: {trajectory_id}\tlatest_timestamp: {latest_timestamp}')
            predicted_state_vectors = state_vectors[i][-11:].clone().detach().cpu()
            predicted_state_vectors = predicted_state_vectors.to(torch.float64)
            predicted_state_vectors = invert_transform(predicted_state_vectors, base_latitude, base_longitude, base_altitude)
            query = """
                DELETE FROM predicted_state_vectors
                WHERE trajectory_id={trajectory_id}
            """.format(trajectory_id=trajectory_id)
            cursor.execute(query)
            for t, state_vector in enumerate(predicted_state_vectors):
                print(state_vector)
                query = """
                    INSERT INTO predicted_state_vectors (trajectory_id, latitude, longitude, altitude, timestamp)
                    VALUES (%s, %s, %s, %s, %s)
                """
                predicted_timestamp = (latest_timestamp + timedelta(seconds=t+1)).strftime('%Y-%m-%d %H:%M:%S')
                print(f'predicted_timestamp: {predicted_timestamp}')
                cursor.execute(query, (trajectory_id, float(state_vector[0]), float(state_vector[1]), float(state_vector[2]), predicted_timestamp))
        connection.commit()
        connection.close()

# Function to Fetch the Latest 20 State Vectors
def fetch_latest_state_vectors(trajectories_ids):
    """
    Fetch the latest 20 state vectors from the database, ordered by timestamp.
    """
    connection = pymysql.connect(
        host=MYSQL_HOST,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        database=MYSQL_DB
    )
    with connection.cursor(pymysql.cursors.DictCursor) as cursor:
        oldest_timestamp_admitted = (datetime.now() - timedelta(seconds=512)).strftime('%Y-%m-%d %H:%M:%S')
        query = """
            SELECT *
            FROM state_vectors
            WHERE timestamp >= "{timestamp}" AND trajectory_id IN {trajectories_ids}
            ORDER BY trajectory_id, timestamp DESC;
        """.format(timestamp=oldest_timestamp_admitted, trajectories_ids=trajectories_ids)
        tic = time.time()
        cursor.execute(query)
        state_vectors = cursor.fetchall()
        connection.close()
        toc = time.time()
        print(f'query time: {(toc-tic):.6f}')
        if len(state_vectors) == 0:
            return None
        # state_vectors = torch.transpose(torch.from_numpy(restore_state_vectors(state_vectors)), 0, 1)
        restored_state_vectors = restore_state_vectors(state_vectors)
        if restored_state_vectors is None:
            return None
        trajectory_metadata, state_vectors = restored_state_vectors
        return trajectory_metadata, state_vectors


def restore_state_vectors(state_vectors):
    state_vectors_by_trajectory = {}
    for state_vector in state_vectors:
        trajectory_id = state_vector['trajectory_id']
        if trajectory_id not in state_vectors_by_trajectory:
            state_vectors_by_trajectory[trajectory_id] = []
        state_vectors_by_trajectory[trajectory_id].append(state_vector)
    trajectory_metadata = []
    filtered_state_vectors = []
    for trajectory_id, state_vectors in state_vectors_by_trajectory.items():
        if len(state_vectors) < 20:
            print(f"Cannot compute prediction on trajectory {trajectory_id}: too few state vectors")
            continue
        trajectory_svs = np.empty((0,3))
        for i, state_vector in enumerate(state_vectors[:20]):
            if i == 0:
                trajectory_metadata.append((trajectory_id, state_vector['timestamp']))
            trajectory_svs = np.vstack((trajectory_svs, [float(state_vector['latitude']), float(state_vector['longitude']), float(state_vector['altitude'])]))
        filtered_state_vectors.append(trajectory_svs[::-1])
    if not filtered_state_vectors:
        return None
    filtered_state_vectors = torch.from_numpy(np.stack(filtered_state_vectors))
    return trajectory_metadata, filtered_state_vectors
        
# RabbitMQ Callback
def on_message(channel, method_frame, header_frame, body):
    """
    Callback function triggered when a message is received from RabbitMQ.
    """
    steps_ahead = 10
    trajectories_ids = body.decode()
    print("Received notification from RabbitMQ:", trajectories_ids)
    if trajectories_ids == "()":
        return
    # Fetch the latest state vectors from the database
    data = fetch_latest_state_vectors(trajectories_ids)
    if data is not None:
        trajectory_metadata, state_vectors = data
    else:
        return
    print(f'input: {state_vectors.shape}')
    print(f'state_vectors: {state_vectors}')
    trajectories_to_predict = None
    for i, trajectory in enumerate(state_vectors):
        base_latitude = trajectory[0][0]
        base_longitude = trajectory[0][1]
        base_altitude = trajectory[0][2]
        trajectory_id = trajectory_metadata[i][0]
        latest_timestamp = trajectory_metadata[i][1]
        trajectory_metadata[i] = (trajectory_id, base_latitude, base_longitude, base_altitude, latest_timestamp)
        trajectory = transform(trajectory)
        if trajectories_to_predict is None:
            trajectories_to_predict = trajectory
        else:
            trajectories_to_predict = torch.stack((trajectories_to_predict, trajectory))
    if trajectories_to_predict.shape[1:] != (20,3):
        trajectories_to_predict = torch.unsqueeze(trajectories_to_predict, 0)
    print(f'trajectories_to_predict: {trajectories_to_predict}')
    # Perform inference using the fetched data
    tic = time.time()
    predicted_trajectories = infer(sample=trajectories_to_predict, steps_ahead=steps_ahead)
    toc = time.time()
    print(f'inference time: {(toc-tic):.6f}')
    write_prediction_to_db(trajectory_metadata, predicted_trajectories)

# Main Function to Set Up RabbitMQ Consumer
def main():
    wait_for_mysql(mysql_host=MYSQL_HOST, mysql_user=MYSQL_USER, mysql_password=MYSQL_PASSWORD)
    wait_for_rabbitmq(rabbitmq_host=RABBITMQ_HOST)
    
    # Connect to RabbitMQ
    connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBITMQ_HOST))
    channel = connection.channel()
    
    # Declare the queue (ensure it exists)
    channel.queue_declare(queue=RABBITMQ_PREDICTION_QUEUE)

    # Set up the consumer
    channel.basic_consume(queue=RABBITMQ_PREDICTION_QUEUE, on_message_callback=on_message, auto_ack=True)

    print(f"Waiting for messages in RabbitMQ queue: {RABBITMQ_PREDICTION_QUEUE}")
    channel.start_consuming()

if __name__ == "__main__":
    main()
