using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MangoFramework;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tetsujin.Models
{
    [MongoDoc]
    public class Profile : MangoBase<Profile>
    {
        [BsonId]
        [BsonElement("_id")]
        [HiddenInput]
        public string Id { get; set; }

        [DisplayName("プロフィール")]
        [BsonRequired]
        [BsonElement("body")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; } = "";

        public static string Get()
        {
            var collection = DbConnection.db.GetCollection<Profile>(Profile.CollectionName);
            var result = collection.Find<Profile>(new BsonDocument { });
            var profile = result.FirstOrDefault();
            if (profile == null)
            {
                return "";
            }
            return profile.Body;
        }

        public static void Save(string body)
        {
            var collection = DbConnection.db.GetCollection<Profile>(Profile.CollectionName);
            collection.DeleteMany(new BsonDocument());
            var profile = new Profile()
            {
                Body = body
            };
            collection.InsertOne(profile);
        }

    }
}