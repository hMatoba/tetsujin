using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Security.Cryptography;
using MangoFramework;
using OAuthProvider;
using tetsujin.Models;

namespace tetsujin
{
    public class GithubOAuth : GithubOAuthBase
    {
        public GithubOAuth(string clientId, string clientSecret) : base(clientId, clientSecret) { }

        /// <summary>
        /// ログインを実行する
        /// </summary>
        /// <param name="id">ユーザID</param>
        /// <param name="cookies">クッキー</param>
        /// <returns>ログインの成否</returns>
        override public bool Login(string id, IResponseCookies cookies)
        {
            var userCollection = DbConnection.Db.GetCollection<OAuthUser>(OAuthUser.CollectionName);
            var filter = Builders<OAuthUser>.Filter.Eq("_id", id);
            var master = userCollection.Find(filter).FirstOrDefault<OAuthUser>();
            if (master == null) // ユーザが登録されていない場合
            {
                return false;
            }
            else // ユーザが登録されていた場合
            {
                // トークンを使ってセッションを開始
                var token = GetToken();
                var collection = DbConnection.Db.GetCollection<Session>(Session.CollectionName);
                collection.InsertOne(new Session
                {
                    Id = token,
                    CreatedAt = DateTime.Now
                });

                // cookieにsecure属性を付与
                var cookieOption = new CookieOptions()
                {
                    Secure = true
                };
                cookies.Append(OAuthUser.SessionCookie, token, cookieOption);

                return true;
            }
        }

        /// <summary>
        /// ランダムトークンを取得
        /// </summary>
        /// <returns>ランダムトークン</returns>
        private static string GetToken()
        {
            var rng = RandomNumberGenerator.Create();
            var buff = new byte[25];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }


    }

}
