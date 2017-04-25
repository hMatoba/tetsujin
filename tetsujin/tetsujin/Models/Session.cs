using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Linq;

using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace tetsujin.Models
{
    public class Session
    {
        public const string SESSION_COOKIE = "markofcain";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonRequired]
        public DateTime CreatedAt { get; set; }

        
        /// <summary>
        /// ログインしているかの状態を返す
        /// </summary>
        /// <param name="token">セッショントークン</param>
        /// <returns>bool</returns>
        public static bool isAuthorized(string token)
        {
            if (token == null)
            {
                return false;
            }
            var collection = DbConnection.db.GetCollection<Session>("sessions");
            var sessionManager = collection.Find<Session>(d => d.Id == token)
                                           .FirstOrDefault<Session>();
            var isLogin = (sessionManager == null) ? false : true;
            return isLogin;
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

        /// <summary>
        /// ユーザIDとパスワードからハッシュを作成する
        /// </summary>
        /// <param name="userName">ユーザID</param>
        /// <param name="pw">パスワード</param>
        /// <returns>ハッシュ</returns>
        private static string GetSHA256(string userName, string pw)
        {
            //HMAC-SHA1を計算する文字列
            var s = $"{userName}-{pw}";
            //キーとする文字列
            var key = Startup.Configuration.GetSection("Secrets")["Hashkey"];
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Couldn't get Hashkey.");
            }

            //文字列をバイト型配列に変換する
            byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
            byte[] keyData = System.Text.Encoding.UTF8.GetBytes(key);


            byte[] bs;
            //HMACSHA1オブジェクトの作成
            using (var hmac = new HMACSHA256(keyData))
            {
                //ハッシュ値を計算
                bs = hmac.ComputeHash(data);
            }

            //byte型配列を16進数に変換
            var result = BitConverter.ToString(bs).ToLower().Replace("-", "");

            return result;
        }


        /// <summary>
        /// ログインを実行する
        /// </summary>
        /// <param name="id">ユーザID</param>
        /// <param name="pw">パスワード</param>
        /// <param name="cookies">クッキー</param>
        /// <returns>ログインが成功したかの真偽値</returns>
        public static bool Login(string id, string pw, IResponseCookies cookies)
        {
            var userCollection = DbConnection.db.GetCollection<BsonDocument>("masters");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var doc = userCollection.Find<BsonDocument>(filter).FirstOrDefault<BsonDocument>();
            if (doc == null) // ユーザが登録されていない場合
            {
                return false;
            }
            else // ユーザが登録されていた場合
            {
                // パスワードをハッシュ化
                var sha256 = GetSHA256(doc.GetValue("_id").AsString, pw);

                // パスワードの一致確認
                if (sha256 != doc.GetValue("pw").AsString)
                {
                    return false;
                }
                else
                {
                    // トークンを使ってセッションを開始
                    var token = GetToken();
                    var collection = DbConnection.db.GetCollection<Session>("sessions");
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
                    cookies.Append(SESSION_COOKIE, token, cookieOption);

                    return true;
                }
            }
        }


    }
}