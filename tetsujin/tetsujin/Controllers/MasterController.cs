using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tetsujin.Filters;

namespace tetsujin.Controllers
{
    [AuthorizationFilter]
    public class MasterController : Controller
    {
        [Route("Master")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
