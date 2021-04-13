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
        private readonly string requestQueueName;
        private readonly string responseQueueName;
        private readonly RequestHandler RequestExecuter;

        public RpcServer(
            RequestHandler handler,
            string requestQueueName = "rpc_queue",
            string responseQueueName = "rpc_queue"
            )
        {
            this.requestQueueName = requestQueueName;
            this.responseQueueName = responseQueueName;
            this.RequestExecuter = handler;

            this.StartListening();
        }

        private void StartListening(
        )
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            using (var connection = factory.CreateConnection())
            //open channel connection
            using (this.channel = connection.CreateModel())
            {
                //declare the queue
                //name - rpc_queue
                //client have to be connected to the queue with the name
                channel.QueueDeclare(
                    queue: this.requestQueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );

                //add queue options
                //channel.BasicQos(0, 1, false);

                //define consumer fore the channel
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(
                    queue: this.requestQueueName,
                    autoAck: false,
                    consumer: consumer
                    );

                consumer.Received += this.HandleReceiving;
            }
        }
        private void HandleReceiving(object sender, BasicDeliverEventArgs e)
        {
            var response = this.RequestExecuter?.Invoke(sender, e);

            this.Reply(response, e.BasicProperties);
        }
        private void Reply(
                byte[] response,
                IBasicProperties eventProps
            )
        {
            //channel.QueueDeclare(
            //    queue: this.responseQueueName, 
            //    durable: false,
            //    exclusive: false, 
            //    autoDelete: false, 
            //    arguments: null);

            channel.BasicPublish(
                exchange: "",
                routingKey: eventProps.ReplyTo,
                basicProperties: eventProps,
                body: response);

            //channel.BasicAck(deliveryTag: ea.DeliveryTag,
            //  multiple: false);
        }
    }
}
