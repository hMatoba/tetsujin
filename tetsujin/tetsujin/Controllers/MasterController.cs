using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;
using tetsujin.Filters;

namespace tetsujin.Controllers
{
    [Route("Master")]
    public class MasterController : Controller
    {
        [AuthorizationFilter]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

    }
}
