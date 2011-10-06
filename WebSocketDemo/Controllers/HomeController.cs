using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebsocketTerminal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.WebSocketPort = HttpContext.Application["WebSocketPort"].ToString();

            return View();
        }
    }
}
