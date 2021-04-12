using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var rpcClient = new RpcClient();
            var response = rpcClient.Call(value.ToString());
            rpcClient.Close();

            ViewBag.answer = response;
            return View();
        }
    }
}
