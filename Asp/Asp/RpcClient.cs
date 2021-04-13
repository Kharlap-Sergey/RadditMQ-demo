using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Asp
{
    internal class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<byte[]> respQueue 
            = new BlockingCollection<byte[]>();
        private readonly IBasicProperties props;

        private event EventHandler<BasicDeliverEventArgs> receivingHandler;

        public RpcClient(
            EventHandler<BasicDeliverEventArgs> receivingHandler
            )
        {
            this.receivingHandler += receivingHandler;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.replyQueueName = channel.QueueDeclare().QueueName;
            this.consumer = new EventingBasicConsumer(channel);

            this.props= channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            this.props.CorrelationId = correlationId;
            this.props.ReplyTo = replyQueueName;

            this.consumer.Received += (model, eventargs) =>
            {
                var body = eventargs.Body.ToArray();
                if (eventargs.BasicProperties.CorrelationId == correlationId)
                {
                    this.receivingHandler?.Invoke(model, eventargs);
                    this.respQueue.Add(body);
                }
            };
        }

        public byte[] Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            this.channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: this.props,
                body: messageBytes);

            this.channel.BasicConsume(
                consumer: this.consumer,
                queue: this.replyQueueName,
                autoAck: true);

            return this.respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }
}