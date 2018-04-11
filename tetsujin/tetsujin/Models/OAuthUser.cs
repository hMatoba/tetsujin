using MangoFramework;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace tetsujin.Models
{
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
