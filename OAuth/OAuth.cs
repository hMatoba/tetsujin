using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OAuth
{
    public static class GithubOAuth
    {
        public static async Task<IActionResult> OAuthAsync()
        {
            var clientId = GithubOAuth.ClientId;
            var clientSecret = GithubOAuth.ClientSecret;


            if (!Request.Query.ContainsKey("code"))
            {
                ViewData["OAuthLink"] = $"https://github.com/login/oauth/authorize?client_id={clientId}";
                return View("OAuthAsync");
            }

            var code = Request.Query["code"];

            var token = await GetAcessTokenAsync(clientId, clientSecret, code);

            // 取得したトークンを使ってGithubにユーザ情報を要求する
            var id = await GetUserId(token);
            var loginSuccess = GithubOAuth.Login(id, Response.Cookies);

            return Redirect(loginSuccess ? "/Master/" : "/Auth/OAuth");
        }

        private static async Task<string> GetAcessTokenAsync(string clientId, string clientSecret, string code)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 持って帰ってきた認証コードを使ってトークンを取得する
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
            });
            var response = await httpClient.PostAsync("https://github.com/login/oauth/access_token", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var token = jsonObj["access_token"];

            return token;
        }

        private static async Task<string> GetUserId(string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 取得したトークンを使ってGithubにユーザ情報を要求する
            var uri = $"https://api.github.com/user?access_token={token}";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Zenigata");
            var response = await httpClient.GetAsync(uri);
            var responseBody = await response.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var id = userInfo["id"];

            return id;
        }

    }
}
