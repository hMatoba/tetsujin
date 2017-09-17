using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tetsujin.Models
{

    public class Master
    {
        public const string CollectionName = "Master";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("pw")]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        public string Password { get; set; }
    }
}