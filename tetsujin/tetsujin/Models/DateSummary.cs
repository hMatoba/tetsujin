using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tetsujin.Models
{
    public class DateSummary
    {
        public const string CollectionName = "DateSummary";

        public static void MapReduce()
        {
            var databaseName = new DatabaseNamespace(DbConnection.Db.DatabaseNamespace.DatabaseName);

            var code = (BsonJavaScript)@"
                function () {
                    var docs = db.Entry.find({isShown: true}, {'publishDate':1});
                    var length = docs.count();
                    var merged = { };
                    for (var i = 0; i < length; i++)
                    {
                        var value = docs[i].publishDate;
                        var date = (new Date(value)).getUTCFullYear().toString() + '/' + ('0' + ((new Date(value)).getUTCMonth() + 1)).slice(-2);
                        if (date in merged) {
                            merged[date] += 1;
                        } else {
                            merged[date] = 1;
                        }
                    }
                    db.DateSummary.remove({});
                    db.DateSummary.insert({_id:'dateGrouping', value:merged});
                }";

            var messageEncodingSettings = new MessageEncoderSettings();
            var operation = new EvalOperation(databaseName, code, messageEncodingSettings);
            var session = new CoreSessionHandle(NoCoreSession.Instance);
            var writeBinding = new WritableServerBinding(DbConnection.Db.Client.Cluster, session);
            operation.Execute(writeBinding, CancellationToken.None);
        }

        public static async Task<Dictionary<string, int>> GetAsync()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "dateGrouping");
            var result = collection.Find<BsonDocument>(filter);
            var doc = await result.FirstOrDefaultAsync();
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
