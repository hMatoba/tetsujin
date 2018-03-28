using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tetsujin.Models
{
    public class TagSummary
    {
        public static void MapReduce()
        {
            var databaseName = new DatabaseNamespace(DbConnection.Db.DatabaseNamespace.DatabaseName);

            var code = (BsonJavaScript)@"
                function () {
                    var docs = db.Entry.find({isShown: true}, {'tag':1});
                    var length = docs.count();
                    var tags = [];
                    for (var i = 0; i < length; i++)
                    {
                        var doc = docs[i];
                        tags = tags.concat(doc.tag);
                    }
                    var merged = { };
                    for (var i = 0; i < tags.length; i++)
                    {
                        var tag = tags[i].replace('.', '\uff0e');
                        if (tag in merged) {
                            merged[tag] += 1;
                        } else {
                            merged[tag] = 1;
                        }
                    }
                    db.TagSummary.remove({});
                    db.TagSummary.insert({_id:'tag', value:merged});
                }";

            var messageEncodingSettings = new MessageEncoderSettings();
            var operation = new EvalOperation(databaseName, code, messageEncodingSettings);
            var session = new CoreSessionHandle(NoCoreSession.Instance);
            var writeBinding = new WritableServerBinding(DbConnection.Db.Client.Cluster, session);
            operation.Execute(writeBinding, CancellationToken.None);
        }

        public static Dictionary<string, int> Get()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>("TagSummary");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "tag");
            var result = collection.Find<BsonDocument>(filter);
            var doc = result.FirstOrDefault();
            if (doc == null)
            {
                return new Dictionary<string, int> { };
            }
            var json = doc.Values.Last().ToJson();
            var dict = (Dictionary<string, int>)BsonSerializer.Deserialize(json, typeof(Dictionary<string, int>));
            return dict;
        }
    }
}
