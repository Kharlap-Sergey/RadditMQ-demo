using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Worker
{
    public class RpcServer
    {
        private readonly ConnectionFactory factory;
        private readonly string queueName;

        public RpcServer(
            string queueName
            )
        {
           this.factory = new ConnectionFactory(){ HostName = "localhost"};
            this.queueName = queueName;
        }


        public void Reply(
            byte[] response
            )
        {
            channel.QueueDeclare(queue: "rpc_queue", durable: false,
                exclusive: false, autoDelete: false, arguments: null);

            channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                         basicProperties: replyProps, body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag,
              multiple: false);
        }
    }
}
