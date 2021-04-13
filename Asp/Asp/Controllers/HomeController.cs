using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asp.Controllers
{
    public class HomeController: Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(int value)
        {
            var rpcClient = new RpcClient(
                    (sender, eventArgs) =>
                    {
                        var body = eventArgs.Body.ToArray();
                        var response = Encoding.UTF8.GetString(body);
                    }
                );

            var response = rpcClient.Call(value.ToString());
            rpcClient.Close();

            ViewBag.answer = Encoding.UTF8.GetString(response);
            return View();
        }
    }
}
