using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangoFramework;

namespace tetsujin.Models
{
    [MongoDoc]
    public class DateGroupedSummary
    {
        public static void MapReduce()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>(Entry.CollectionName);
            var map = (BsonJavaScript)@"
                function() {
                    emit('dateGrouping', this.publishDate);
                }";

            var reduce = (BsonJavaScript)@"
                function(key, values) {
                    var result = {};
                    values.forEach(function(value){
                        var d = value.getUTCFullYear().toString() + '/' + ('0' + (value.getUTCMonth() + 1)).slice(-2);
                        if (!(d in result)) {
                            result[d] = 1;
                        } else {
                            result[d] += 1;
                        }
                    });
                    return result;
                }";

            var options = new MapReduceOptions<BsonDocument, object>();
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", true);
            options.Filter = filter;
            options.OutputOptions = MapReduceOutputOptions.Replace("DateGroupedSummary");

            collection.MapReduce(map, reduce, options);

        }

        public static Dictionary<string, int> Get()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("DateGroupedSummary");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "dateGrouping");
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
