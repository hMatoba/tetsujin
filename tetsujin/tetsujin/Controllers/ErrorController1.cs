using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace tetsujin.Controllers
{
    [Route("Error")]
    public class ErrorController1 : Controller
    {
        [Route("{errorCode:int}")]
        public IActionResult Index(int errorCode)
        {
            ViewBag.errorCode = errorCode;
            return View("Error");
        }
    }
}