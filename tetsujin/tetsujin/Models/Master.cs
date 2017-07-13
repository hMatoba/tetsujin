using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangoFramework;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace tetsujin.Models
{

    public class Master
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("pw")]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        public string Password { get; set; }
    }
}
