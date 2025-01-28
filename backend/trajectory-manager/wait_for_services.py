import time
import pymysql
import pika


# Retry logic for MySQL
def wait_for_mysql(mysql_host, mysql_user, mysql_password):
    while True:
        try:
            conn = pymysql.connect(host=mysql_host, user=mysql_user, password=mysql_password)
            print("MySQL is ready!")
            return
        except pymysql.MySQLError:
            print("Waiting for MySQL...")
            time.sleep(5)

# Retry logic for RabbitMQ
def wait_for_rabbitmq(rabbitmq_host):
    while True:
        try:
            connection = pika.BlockingConnection(pika.ConnectionParameters(rabbitmq_host))
            print("RabbitMQ is ready!")
            return
        except pika.exceptions.AMQPConnectionError:
            print("Waiting for RabbitMQ...")
            time.sleep(5)
