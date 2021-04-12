using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var rpcClient = new RpcClient();

            //Console.WriteLine(" [x] Requesting fib(30)");
            //var response = rpcClient.Call("30");
             
            //Console.WriteLine(" [.] Got '{0}'", response);
            //rpcClient.Close();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
