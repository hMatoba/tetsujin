using MongoDB.Bson;
using MongoDB.Driver;

namespace tetsujin.Models
{
    public class Profile
    {
        public const string CollectionName = "Profile";

        public static string Get()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(CollectionName);
            var result = collection.Find<BsonDocument>(new BsonDocument { });
            var doc = result.FirstOrDefault();
            if (doc == null)
            {
                return "";
            }
            var body = (string)doc.GetValue("body");
            return body;
        }

        public static bool Save(string body)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "prof");
            var builder = Builders<BsonDocument>.Update;
            var doc = builder.Set("body", body);
            var options = new UpdateOptions { IsUpsert = true };
            collection.UpdateOne(filter, doc, options);
            return true;
        }

    }
}