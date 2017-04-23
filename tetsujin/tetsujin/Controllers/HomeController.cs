using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
        public RedirectResult LoginAuth()
        {
            var loginPassed = false;

            if (loginPassed)
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
