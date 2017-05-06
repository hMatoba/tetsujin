using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace tetsujin.Models
{
    public class EntryStatistics
    {
        public static void MapReduceTag()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");
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
            var filter = Builders<BsonDocument>.Filter.Eq("showFlg", true);
            options.Filter = filter;
            options.OutputOptions = MapReduceOutputOptions.Replace("tagInfo");

            collection.MapReduce(map, reduce, options);

        }

        public static void MapReduceDate()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");
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
            var filter = Builders<BsonDocument>.Filter.Eq("showFlg", true);
            options.Filter = filter;
            options.OutputOptions = MapReduceOutputOptions.Replace("dateInfo");

            collection.MapReduce(map, reduce, options);

        }

        public static Dictionary<string, int> GetTagInfo()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("tagInfo");
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

        public static Dictionary<string, int> GetDateInfo()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("dateInfo");
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
