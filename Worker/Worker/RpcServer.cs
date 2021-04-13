using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Worker
{
    public class RpcServer
    {
        public delegate byte[] RequestHandler(
            object sender,
            BasicDeliverEventArgs eventArgs
            );

        private IModel channel;
        private IConnection connection;
        private readonly string requestQueueName;
        private readonly RequestHandler RequestExecuter;

        public RpcServer(
            RequestHandler handler,
            string requestQueueName = "rpc_queue"
            )
        {
            this.requestQueueName = requestQueueName;
            this.RequestExecuter = handler;

            this.StartListening();
        }

        ~RpcServer()
        {
            this.channel.Dispose();
            this.connection.Dispose();
        }

        public void Stop()
        {
            this.channel.Close();
            this.connection.Close();
        }
        private void StartListening(
        )
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            this.connection = factory.CreateConnection();
            //open channel connection
            this.channel = connection.CreateModel();
            //declare the queue
            //default name - rpc_queue
            //client has to be connected to the queue with the same name
            channel.QueueDeclare(
                queue: this.requestQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            //define consumer fore the channel
            var consumer = new EventingBasicConsumer(channel);
            //bind consumer to the queue
            channel.BasicConsume(
                queue: this.requestQueueName,
                autoAck: false,
                consumer: consumer
                );

            consumer.Received += this.HandleReceiving;
        }
        private void HandleReceiving(
            object sender, 
            BasicDeliverEventArgs e
            )
        {
            var response = this.RequestExecuter?.Invoke(sender, e);

            this.Reply(response, e.BasicProperties);
        }
        private void Reply(
                byte[] response,
                IBasicProperties eventProps
            )
        {
            channel.BasicPublish(
                exchange: "",
                routingKey: eventProps.ReplyTo,
                basicProperties: eventProps,
                body: response);;
        }
    }
}
