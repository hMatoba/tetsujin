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
    public class TagGroupedSummary
    {

        public static void MapReduce()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>(Entry.CollectionName);
            var map = (BsonJavaScript)@"
                function() {
                    emit('tag', this.tag);
                }";

            var reduce = (BsonJavaScript)@"
                function(key, values) {
                    var result = {};
                    values.forEach(function(value){
                        value.forEach(function(v){
                            if (!(v in result)) {
                                result[v] = 1;
                            } else {
                                result[v] += 1;
                            }
                        });
                    });
                    return result;
                }";

            var options = new MapReduceOptions<BsonDocument, object>();
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", true);
            options.Filter = filter;
            options.OutputOptions = MapReduceOutputOptions.Replace("TagGroupedSummary");

            collection.MapReduce(map, reduce, options);

        }

        public static Dictionary<string, int> Get()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("TagGroupedSummary");
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
