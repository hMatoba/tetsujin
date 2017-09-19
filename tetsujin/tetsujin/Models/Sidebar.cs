using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace tetsujin.Models
{
    public class Sidebar
    {
        public static string GetProfile()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>("profile");
            var result = collection.Find<BsonDocument>(new BsonDocument { });
            var doc = result.FirstOrDefault();
            if (doc == null)
            {
                return "";
            }
            var body = (string)doc.GetValue("body");
            return body;
        }

        public static bool SaveProfile(string body)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>("profile");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "prof");
            var builder = Builders<BsonDocument>.Update;
            var doc = builder.Set("body", body);
            collection.UpdateOne(filter, doc);
            return true;
        }

    }
}