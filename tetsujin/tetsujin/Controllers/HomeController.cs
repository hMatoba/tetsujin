using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;

namespace tetsujin.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public RedirectResult LoginAuth(Session session)
        {
            var id = Request.Form["_id"];
            var password = Request.Form["password"];
            var isAuthorized = Session.Login(id, password, Response.Cookies);

            if (isAuthorized)
            {
                return Redirect("/Master");
            }
            else
            {
                return Redirect("/Login");
            }
        }


        [Route("{*path}")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
