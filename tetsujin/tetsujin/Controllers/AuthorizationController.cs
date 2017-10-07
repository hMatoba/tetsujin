using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Composite;
using tetsujin.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

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

        [Route("OAuth")]
        public async Task<IActionResult> OAuthAsync()
        {
            var clientId = GithubOAuth.ClientId;
            var clientSecret = GithubOAuth.ClientSecret;


            if (!Request.Query.ContainsKey("code"))
            {
                ViewData["OAuthLink"] = $"https://github.com/login/oauth/authorize?client_id={clientId}";
                return View("OAuthAsync");
            }

            var redirectUri = "https://" + Request.Host.Value + "/Auth/OAuth";
            var code = Request.Query["code"];

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // まず持って帰ってきた認証コードを使ってトークンを取得する
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
            });
            var codeResponse = await httpClient.PostAsync("https://github.com/login/oauth/access_token", content);
            var codeResponseBody = await codeResponse.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(codeResponseBody);
            var token = jsonObj["access_token"];

            // 取得したトークンを使ってOneDriveにユーザ情報を要求する
            var uri = $"https://api.github.com/user?access_token={token}";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("blog");
            var tokenResponse = await httpClient.GetAsync(uri);
            var tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine(tokenResponseBody);
            var tokenJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenResponseBody);

            // ユーザを確認できる情報が得られたのでごにょごにょする
            // Foo(tokenResponseBody);
            var id = tokenJson["id"];
            var loginSuccess = GithubOAuth.Login(id, Response.Cookies);

            if (loginSuccess)
            {
                return Redirect("/Master/");
            }
            else
            {
                return Redirect("/Auth/OAuth");
            }
        }
    }
}