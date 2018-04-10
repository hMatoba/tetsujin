using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;



namespace tetsujin.Models
{
    public class GithubOAuth
    {
        private static string _clientId = "";

        public static string ClientId
        {
            set => _clientId = value;
            get
            {
                if (String.IsNullOrEmpty(_clientId))
                {
                    throw new ArgumentNullException(
                        "Github OAuth client ID isn't given. Set as 'GITHUB_CLIENT_ID'."
                    );
                }
                return _clientId;
            }
        }

        private static string _clientSecret = "";

        public static string ClientSecret
        {
            set => _clientSecret = value;
            get
            {
                if (String.IsNullOrEmpty(_clientSecret))
                {
                    throw new ArgumentNullException(
                        "Github OAuth client secret isn't given. Set as 'GITHUB_CLIENT_SECRET'."
                    );
                }
                return _clientSecret;
            }
        }

        /// <summary>
        /// ログインを実行する
        /// </summary>
        /// <param name="id">ユーザID</param>
        /// <param name="cookies">クッキー</param>
        /// <returns>ログインの成否</returns>
        public static bool Login(string id, IResponseCookies cookies)
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

    [MongoDoc]
    public class OAuthUser
    {
        public const string SessionCookie = "markofcain";
        public const string CollectionName = "OAuthUser";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        public static List<CreateIndexModel<BsonDocument>> IndexModels = new List<CreateIndexModel<BsonDocument>>()
        {
            new CreateIndexModel<BsonDocument>(
                new IndexKeysDefinitionBuilder<BsonDocument>().Ascending(new StringFieldDefinition<BsonDocument>("createdAt")),
                new CreateIndexOptions(){ ExpireAfter = TimeSpan.FromDays(10) }
            )
        };

    }
}
