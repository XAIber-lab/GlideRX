import time
import threading
import pika
import pymysql
import torch
import os
from dotenv import load_dotenv
from data_utils import invert_transform
from datetime import datetime
from wait_for_services import wait_for_mysql, wait_for_rabbitmq
from trajectory import process_trajectory, TrajectorySection, Trajectory
from flarm_utils import *
from rt_reader import Reader


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
RABBITMQ_FRONTEND_QUEUE = os.getenv("RABBITMQ_FRONTEND_QUEUE")

# MySQL Configuration
MYSQL_HOST = os.getenv("MYSQL_HOST")
MYSQL_USER = os.getenv("MYSQL_USER")
MYSQL_PASSWORD = os.getenv("MYSQL_PASSWORD")
MYSQL_DB = os.getenv("MYSQL_DB")


def is_trajectory_present(cursor, trajectory_name):
    query = """
    SELECT EXISTS (
    SELECT 1
    FROM trajectories
    WHERE name="{trajectory_name}"
    ) AS trajectory_present
    """.format(trajectory_name=trajectory_name)
    cursor.execute(query)
    trajectory_present = cursor.fetchone()[0]
    return trajectory_present == 1


def get_trajectory_id_by_name(cursor, trajectory_name):
    query = """
    SELECT id
    FROM trajectories
    WHERE name="{trajectory_name}"
    """.format(trajectory_name=trajectory_name)
    cursor.execute(query)
    trajectory_id = cursor.fetchone()[0]
    return trajectory_id


# Function to Insert State Vectors into the Database
def write_state_vector_to_db(trajectory_id, latitude, longitude, altitude):
    connection = pymysql.connect(
        host=MYSQL_HOST,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        database=MYSQL_DB
    )
    try:
        with connection.cursor() as cursor:
            timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            query = """
                INSERT INTO state_vectors (trajectory_id, latitude, longitude, altitude, timestamp)
                VALUES (%s, %s, %s, %s, %s)
            """
            cursor.execute(query, (trajectory_id, latitude, longitude, altitude, timestamp))
            connection.commit()
            print(f"Inserted state vector: ({trajectory_id}, {latitude}, {longitude}, {altitude}, {timestamp})")
    finally:
        connection.close()


def notify_prediction_service(trajectories_ids):
    connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBITMQ_HOST))
    channel = connection.channel()

    channel.queue_declare(queue=RABBITMQ_PREDICTION_QUEUE)

    message = "(" + ",".join(map(str, trajectories_ids)) + ")"
    channel.basic_publish(exchange="", routing_key=RABBITMQ_PREDICTION_QUEUE, body=message)
    print("Notification sent to RabbitMQ.")

    connection.close()


def notify_frontend(self_gps_message, pflaa_messages):
    connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBITMQ_HOST))
    channel = connection.channel()

    channel.queue_declare(queue=RABBITMQ_FRONTEND_QUEUE)

    channel.basic_publish(exchange="", routing_key=RABBITMQ_FRONTEND_QUEUE, body=self_gps_message)
    
    for pflaa_message in pflaa_messages:
        channel.basic_publish(exchange="", routing_key=RABBITMQ_FRONTEND_QUEUE, body=pflaa_message)
    print("Notification sent to frontend.")

    connection.close()
    

def send_trajectory(name, trajectory):
    for latitude, longitude, altitude in trajectory:
        connection = pymysql.connect(
            host=MYSQL_HOST,
            user=MYSQL_USER,
            password=MYSQL_PASSWORD,
            database=MYSQL_DB
        )
        with connection.cursor() as cursor:
            trajectory_present = is_trajectory_present(cursor, name)
            print(f'trajectory_present: {trajectory_present}')
            if not trajectory_present:
                query = """
                INSERT INTO trajectories (name)
                VALUES (%s)
                """
                cursor.execute(query, (name))
                connection.commit()
            trajectory_id = get_trajectory_id_by_name(cursor, name)
        connection.close()
        write_state_vector_to_db(trajectory_id, latitude, longitude, altitude)
        notify_prediction_service([trajectory_id])
        time.sleep(1)


def send_state_vector(trajectory_name, state_vector):
    # Declare a new trajectory if trajectory_name does not exist
    trajectory_id = declare_trajectory(trajectory_name)
    # Add state vector to the trajectory
    write_state_vector_to_db(trajectory_id, state_vector[0], state_vector[1], state_vector[2])
    return trajectory_id

def declare_trajectory(name):
    trajectory_id = None
    connection = pymysql.connect(
        host=MYSQL_HOST,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        database=MYSQL_DB
    )
    with connection.cursor() as cursor:
        trajectory_present = is_trajectory_present(cursor, name)
        print(f'trajectory_present: {trajectory_present}')
        if not trajectory_present:
            query = """
            INSERT INTO trajectories (name)
            VALUES (%s)
            """
            cursor.execute(query, (name))
            connection.commit()
        trajectory_id = get_trajectory_id_by_name(cursor, name)
    connection.close()
    return trajectory_id
    

def write_trajectories_to_files():
    sects_1 = [TrajectorySection(vertical_speed=1.0, duration=40),
           TrajectorySection(vertical_speed=3.0, radius=-200, duration=20),
           TrajectorySection(vertical_speed=-2.0, duration=27),
           TrajectorySection(vertical_speed=2.0, radius=300, duration=30),
           TrajectorySection(vertical_speed=-1.0, radius=500, duration=20),
           TrajectorySection(vertical_speed=2.5, radius=150, duration=40),
           TrajectorySection(vertical_speed=1.5, radius=-350, duration=27)]
    origin_1 = [0, 0, 0]

    sects_2 = [TrajectorySection(vertical_speed=3.0, radius=300, duration=27),
               TrajectorySection(radius=0, duration=13),
               TrajectorySection(vertical_speed=3.0, radius=-200, duration=40),
               TrajectorySection(vertical_speed=5.0, radius=500, duration=27),
               TrajectorySection(vertical_speed=-5.0, radius=-200, duration=16),
               TrajectorySection(vertical_speed=-3.0, radius=-150, duration=27),
               TrajectorySection(vertical_speed=-10.0, radius=0, duration=30),
               TrajectorySection(vertical_speed=-3.0, radius=-100, duration=27)]
    origin_2 = [-500, -200, 0]
    
    traj_descriptions = [sects_1, sects_2]
    traj_origins = [origin_1, origin_2]

    self_gps_file_name = None
    trajectories = []
    n_aircraft = 0
    for idx, traj_descr in enumerate(traj_descriptions):
        complete_trajectory = Trajectory()
        complete_trajectory.traj_origin_x = traj_origins[idx][0]
        complete_trajectory.traj_origin_y = traj_origins[idx][1]
        complete_trajectory.traj_origin_z = traj_origins[idx][2]
        traj_file_name = f"trajectory_{idx}.csv"
        if idx == 0:
            self_gps_file_name = traj_file_name
        process_trajectory(complete_trajectory, traj_descr, traj_file_name)
        trajectories.append(complete_trajectory)
        n_aircraft += 1

    pflaa_file_name = "PFLAA_1.csv"
    write_PFLAU_data_to_file(trajectories, "PFLAU_1.csv")
    write_PFLAA_data_to_file(trajectories, pflaa_file_name)

    return n_aircraft, self_gps_file_name, pflaa_file_name
        

def main():
    wait_for_mysql(mysql_host=MYSQL_HOST, mysql_user=MYSQL_USER, mysql_password=MYSQL_PASSWORD)
    wait_for_rabbitmq(rabbitmq_host=RABBITMQ_HOST)
    
    n_aircraft, self_gps_file_name, pflaa_file_name = write_trajectories_to_files()

    reader = Reader(n_aircraft-1,self_gps_file_name, pflaa_file_name)
    while reader.has_next():
        pflaa_messages, positions = reader.get_next_message()
        global_svs = invert_transform(positions,
                               base_latitude=42.427634,
                               base_longitude=12.820264,
                               base_altitude=1200)
    
        global_svs = global_svs.numpy()
        self_gps_message = f'GPS: {global_svs[0][0]:.6f} {global_svs[0][1]:.6f} {global_svs[0][2]:.6f}'
        trajectories_ids = []
        for trajectory_name_id, state_vector in enumerate(global_svs):
            trajectory_id = send_state_vector(trajectory_name_id, state_vector)
            trajectories_ids.append(trajectory_id)
        notify_prediction_service(trajectories_ids)
        notify_frontend(self_gps_message, pflaa_messages)
        time.sleep(1)
    return


if __name__ == "__main__":
    main()

