using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;

namespace tetsujin.Controllers
{
    [Route("Master")]
    public class MasterController : Controller
    {

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

    }
}
