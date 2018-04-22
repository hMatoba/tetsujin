using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace tetsujin.Models
{
    public class Profile
    {
        public const string CollectionName = "Profile";

        public static async Task<string> GetAsync()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(CollectionName);
            var result = collection.Find<BsonDocument>(new BsonDocument { });
            var doc = await result.FirstOrDefaultAsync();
            if (doc == null)
            {
                return "";
            }
            var body = (string)doc.GetValue("body");
            return body;
        }

        public static async Task<bool> SaveAsync(string body)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "prof");
            var builder = Builders<BsonDocument>.Update;
            var doc = builder.Set("body", body);
            var options = new UpdateOptions { IsUpsert = true };
            await collection.UpdateOneAsync(filter, doc, options);
            return true;
        }

    }
}