using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using tetsujin.Models;
using OAuthProvider;

namespace tetsujin.Controllers
{
    [Route("Auth")]
    public class AuthorizationController : Controller
    {
        private readonly GithubOAuth _githubOAuth;

        public AuthorizationController(GithubOAuth githubOAuth)
        {
            this._githubOAuth = githubOAuth;
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

        [Route("OAuth")]
        public async Task<IActionResult> OAuthAsync()
        {
            if (!Request.Query.ContainsKey("code"))
            {
                ViewData["OAuthLink"] = this._githubOAuth.GetRedirectUri();
                return View("OAuthAsync");
            }

            var id = await this._githubOAuth.GetIdAsync(Request.Query["code"]);
            var loginSuccess = this._githubOAuth.Login(id, Response.Cookies);

            return Redirect(loginSuccess ? "/Master/" : "/Auth/OAuth");
        }
    }
}