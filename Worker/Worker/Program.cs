using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Worker
{
    //befor start run the docker image
    //docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
    class Program
    {
        static void Main(string[] args)
        { 
            var server = new RpcServer(ExecuteAnsver);
            Console.ReadLine();
            server.Stop();
        }
        private static byte[] ExecuteAnsver(object o, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var request = Encoding.UTF8.GetString(body);
            int n = int.Parse(request);

            return Encoding.UTF8.GetBytes(SomeCalculation(n).ToString());
        }
        private static int SomeCalculation(int n)
        {
            Thread.Sleep(2000);
            return n * n;
        }
    }
}
