using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using tetsujin.Models;

namespace tetsujin.Models
{
    public class Entry
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.Int32)]
        [HiddenInput]
        public int? EntryID { get; set; } = null;

        [DisplayName("タイトル")]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("title")]
        [BsonRequired]
        public string Title { get; set; }

        [DisplayName("発行日時")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("publishDate")]
        [BsonRequired]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("tag")]
        public List<string> Tag { get; set; }

        [DisplayName("タグ")]
        [BsonIgnore]
        public string _Tag { get; set; }

        [DisplayName("本文")]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        [BsonElement("body")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }

        [DisplayName("掲載フラグ")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonRequired]
        [BsonElement("isShown")]
        public bool IsShown { get; set; }

        public static int LIMIT { get; } = 5;


        public static int Count(bool isShown = false)
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", !isShown);
            return Convert.ToInt32(collection.Count(filter));
        }

        public static int CountFiltered(List<string> tag, bool isShown = true)
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", !isShown) &
                         Builders<BsonDocument>.Filter.In("tag", tag);
            return Convert.ToInt32(collection.Count(filter));
        }


        public static Entry GetEntry(int id, bool admin = false)
        {
            var collection = DbConnection.db.GetCollection<Entry>("entries");

            FilterDefinition<Entry> filter;
            var f = Builders<Entry>.Filter;
            if (admin)
            {
                filter = f.Eq(e => e.EntryID, id);
            }
            else
            {

                filter = f.Eq(e => e.EntryID, id) &
                         f.Eq(e => e.IsShown, true);
            }

            var entry = collection.Find<Entry>(filter).FirstOrDefault();
            if (entry != null && entry.Tag.Count() > 0)
            {
                entry._Tag = String.Join(",", entry.Tag);
            }

            return entry;
        }

        public static List<Entry> GetRecentEntry(int skip_n, bool admin = false, bool isSitemap = false)
        {
            var collection = DbConnection.db.GetCollection<Entry>("entries");

            var skip = skip_n * LIMIT;

            FilterDefinition<Entry> filter;
            if (admin)
            {
                filter = new BsonDocument { };
            }
            else
            {
                var f = Builders<Entry>.Filter;
                filter = f.Eq(e => e.IsShown, true);
            }

            var sortDoc = new BsonDocument
            {
                { "publishDate", -1 },
            };

            int limit;
            if (!isSitemap)
            {
                limit = LIMIT;
            }
            else
            {
                limit = 10000;
            }
            var entries = collection.Find<Entry>(filter).Sort(sortDoc).Limit(limit).Skip(skip).ToList();

            return entries;
        }

        public static List<Entry> GetSameTagEntry(List<string> filterTag, int skip_n, int? excludeId = null)
        {
            var collection = DbConnection.db.GetCollection<Entry>("entries");

            var skip = skip_n * LIMIT;

            FilterDefinition<Entry> filter;
            var f = Builders<Entry>.Filter;
            filter = f.Eq(e => e.IsShown, true) &
                     f.Ne(e => e.EntryID, excludeId) &
                     f.AnyIn(e => e.Tag, filterTag);

            var sortDoc = new BsonDocument
            {
                { "publishDate", -1 },
            };
            var entries = collection.Find<Entry>(filter).Sort(sortDoc).Limit(LIMIT).Skip(skip).ToList();

            return entries;
        }

        public bool InsertOrUpdate()
        {
            if (EntryID == null)
            {
                Insert();
            }
            else
            {
                Update();
            }

            UpdateSubInfo();
            return true;
        }

        public bool Insert()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");

            var sortDoc = Builders<BsonDocument>.Sort.Descending("_id");
            var d = collection.Find<BsonDocument>(new BsonDocument { })
                              .Sort(sortDoc)
                              .FirstOrDefault<BsonDocument>();

            var id = (d == null) ? 0 : d.GetValue("_id").AsInt32 + 1;


            this.EntryID = (int)id;
            collection.InsertOne(this.ToBsonDocument());

            return true;
        }

        public bool Update()
        {
            Tag.RemoveAll((item) => item == null);

            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");

            var filter = Builders<BsonDocument>.Filter.Eq("_id", EntryID);
            collection.ReplaceOne(filter, this.ToBsonDocument());

            return true;
        }

        public static bool DeleteMany(List<int> ids)
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");

            var filter = Builders<BsonDocument>.Filter.In("_id", ids);
            collection.DeleteMany(filter);

            UpdateSubInfo();

            return true;
        }

        
        public static bool UpdateSubInfo()
        {
            EntryStatistics.MapReduceTag();
            EntryStatistics.MapReduceDate();

            return true;
        }

        public static List<Entry> FilterByMonth(int year, int month, int skip = 0)
        {
            var collection = DbConnection.db.GetCollection<Entry>("entries");

            var dateMin = new DateTime(year, month, 1);
            var dateMax = dateMin.AddMonths(1);

            FilterDefinition<Entry> filter;
            var f = Builders<Entry>.Filter;
            filter = f.Eq(e => e.IsShown, true) &
                     f.Gte(e => e.PublishDate, dateMin) &
                     f.Lt(e => e.PublishDate, dateMax);

            var sortDoc = new BsonDocument
            {
                { "publishDate", -1 },
            };
            var entries = collection.Find<Entry>(filter).Sort(sortDoc).Limit(LIMIT).Skip(LIMIT * skip).ToList();

            return entries;
        }

        public static int CountDateFiltered(int year, int month, bool isShown = true)
        {
            var dateMin = new DateTime(year, month, 1);
            var dayInMonth = DateTime.DaysInMonth(year, month);
            var dateMax = dateMin.AddMonths(1);

            var collection = DbConnection.db.GetCollection<BsonDocument>("entries");
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", isShown) &
                         Builders<BsonDocument>.Filter.Gte("publishDate", dateMin) &
                         Builders<BsonDocument>.Filter.Lt("publishDate", dateMax);
            return Convert.ToInt32(collection.Count(filter));
        }

    }

}