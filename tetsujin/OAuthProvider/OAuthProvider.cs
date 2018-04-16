using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace OAuthProvider
{
    abstract public class GithubOAuthBase
    {
        public GithubOAuthBase(string clientId, string clientSecret)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
        }

        private string _clientId = "";

        public string ClientId
        {
            get
            {
                if (String.IsNullOrEmpty(_clientId))
                {
                    throw new ArgumentNullException(
                        "Github OAuth client ID isn't given."
                    );
                }
                return _clientId;
            }
        }

        private string _clientSecret = "";

        public string ClientSecret
        {
            set => _clientSecret = value;
            get
            {
                if (String.IsNullOrEmpty(_clientSecret))
                {
                    throw new ArgumentNullException(
                        "Github OAuth client secret isn't given."
                    );
                }
                return _clientSecret;
            }
        }

        public string GetRedirectUri()
        {
            var uri = $"https://github.com/login/oauth/authorize?client_id={this.ClientId}";
            return uri;

        }


        public async Task<string> GetIdAsync(string code)
        {
            // 取得したトークンを使ってGithubにユーザ情報を要求する
            var token = await GetAccessTokenAsync(code);

            var id = await GetUserId(token);

            return id;
        }
        
        virtual public bool Login(string id, IResponseCookies cookies)
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> LoginAsync(string id, IResponseCookies cookies)
        {
            throw new NotImplementedException();
        }

        private async Task<string> GetAccessTokenAsync(string code)
        {
            // 認証コードを使ってトークンを取得する
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", this.ClientId },
                { "client_secret", this.ClientSecret },
                { "code", code },
            });
            var response = await httpClient.PostAsync("https://github.com/login/oauth/access_token", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var token = jsonObj["access_token"];

            return token;
        }

        private async Task<string> GetUserId(string token)
        {
            // 取得したトークンを使ってGithubにユーザ情報を要求、取得した情報からIDを返す
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var uri = $"https://api.github.com/user?access_token={token}";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Zenigata");
            var response = await httpClient.GetAsync(uri);
            var responseBody = await response.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var id = userInfo["id"];

            return id;
        }

        private async Task<Dictionary<string, string>> GetUserInfo(string token)
        {
            // 取得したトークンを使ってGithubにユーザ情報を要求する
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var uri = $"https://api.github.com/user?access_token={token}";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Zenigata");
            var response = await httpClient.GetAsync(uri);
            var responseBody = await response.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            return userInfo;
        }

    }
}