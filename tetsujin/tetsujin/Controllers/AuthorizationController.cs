using Microsoft.AspNetCore.Mvc;
using tetsujin.Models;

namespace tetsujin.Controllers
{
    [Route("Auth")]
    public class AuthorizationController : Controller
    {
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
            var id = Request.Form["_id"];
            var password = Request.Form["password"];
            var isAuthorized = Session.Login(id, password, Response.Cookies);

            if (isAuthorized)
            {
                return Redirect("/Master");
            }
            else
            {
                return Redirect("/Auth/Login");
            }
        }
    }
}