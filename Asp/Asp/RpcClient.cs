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
        private readonly BlockingCollection<byte[]> responseQueue 
            = new BlockingCollection<byte[]>();
        private readonly IBasicProperties props;
        private readonly string requestQueueName;
        private readonly string correlationId;
        private event EventHandler<BasicDeliverEventArgs> receivingHandler;

        public RpcClient(
            EventHandler<BasicDeliverEventArgs> receivingHandler,
            string requestQueueName = "rpc_queue"
            )
        {
            this.receivingHandler += receivingHandler;
            this.requestQueueName = requestQueueName;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            this.replyQueueName = channel.QueueDeclare().QueueName;
            this.consumer = new EventingBasicConsumer(channel);

            this.props= channel.CreateBasicProperties();
            this.correlationId = Guid.NewGuid().ToString();
            this.props.CorrelationId = this.correlationId;
            this.props.ReplyTo = replyQueueName;

            this.consumer.Received += HandleReceiving;
        }

        ~RpcClient()
        {
            this.connection.Dispose();
            this.channel.Dispose();
        }
        public byte[] Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            this.channel.BasicPublish(
                exchange: "",
                routingKey: this.requestQueueName,
                basicProperties: this.props,
                body: messageBytes);

            this.channel.BasicConsume(
                consumer: this.consumer,
                queue: this.replyQueueName,
                autoAck: true);

            return this.responseQueue.Take();
        }
        public void Close()
        {
            this.channel.Close();
            this.connection.Close();
        }
        private void HandleReceiving(
            object sender,
            BasicDeliverEventArgs e
            )
        {
            var body = e.Body.ToArray();
            if (e.BasicProperties.CorrelationId == this.correlationId)
            {
                this.receivingHandler?.Invoke(sender, e);
                this.responseQueue.Add(body);
            }
        }
        
    }
}