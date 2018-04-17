﻿using MangoFramework;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tetsujin.Models
{
    [MongoDoc]
    public class Entry
    {
        public const string CollectionName = "Entry";

        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.Int32)]
        [HiddenInput]
        public int? EntryID { get; set; } = null;

        [DisplayName("タイトル")]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("title")]
        public string Title { get; set; }

        [DisplayName("発行日時")]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("publishDate")]
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
        [BsonElement("body")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }

        [DisplayName("掲載フラグ")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonElement("isShown")]
        public bool IsShown { get; set; }

        public static int LIMIT { get; } = 5;

        public static int Count(bool isShown = false)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(Entry.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", !isShown);
            return Convert.ToInt32(collection.Count(filter));
        }

        public static int CountFiltered(List<string> tag, bool isShown = true)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(Entry.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", !isShown) &
                         Builders<BsonDocument>.Filter.In("tag", tag);
            return Convert.ToInt32(collection.Count(filter));
        }

        public static async Task<Entry> GetEntryAsync(int id, bool admin = false)
        {
            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

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

            var entry = await collection.Find<Entry>(filter).FirstOrDefaultAsync();
            if (entry != null && entry.Tag.Count() > 0)
            {
                entry._Tag = String.Join(",", entry.Tag);
            }

            return entry;
        }

        public void InsertOrUpdate()
        {
            this.Tag = !String.IsNullOrEmpty(this._Tag) ? this._Tag.Split(',').ToList<string>() : new List<string> { };

            if (EntryID == null)
            {
                Insert();
            }
            else
            {
                Update();
            }

            UpdateSubInfo();
        }

        public void Insert()
        {
            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

            var sortDoc = Builders<Entry>.Sort.Descending(e => e.EntryID);
            var d = collection.Find<Entry>(new BsonDocument { })
                              .Sort(sortDoc)
                              .FirstOrDefault<Entry>();

            var id = (d == null) ? 0 : d.EntryID + 1;

            this.EntryID = (int)id;
            collection.InsertOne(this);
        }

        public void Update()
        {
            Tag.RemoveAll(item => item == null);

            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

            var filter = Builders<Entry>.Filter.Eq("_id", this.EntryID);
            collection.ReplaceOne(filter, this);
        }


        public static List<Entry> GetRecentEntries(int skip_n, bool admin = false, bool isSitemap = false)
        {
            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

            var skip = skip_n * LIMIT;

            FilterDefinition<Entry> filter;
            if (admin)
            {
                var f = Builders<Entry>.Filter;
                filter = f.Empty;
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

        public static void DeleteMany(List<int> ids)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(Entry.CollectionName);

            var filter = Builders<BsonDocument>.Filter.In("_id", ids);
            collection.DeleteMany(filter);

            UpdateSubInfo();
        }

        private static void UpdateSubInfo()
        {
            TagSummary.MapReduce();
            DateSummary.MapReduce();
        }

        public static List<Entry> GetSameTagEntry(List<string> filterTag, int skip_n, int? excludeId = null)
        {
            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

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

        public static List<Entry> FilterByMonth(int year, int month)
        {
            var collection = DbConnection.Db.GetCollection<Entry>(Entry.CollectionName);

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
            var entries = collection.Find<Entry>(filter)
                                    .Sort(sortDoc)
                                    .ToList();

            return entries;
        }

        public static int CountDateFiltered(int year, int month, bool isShown = true)
        {
            var dateMin = new DateTime(year, month, 1);
            var dayInMonth = DateTime.DaysInMonth(year, month);
            var dateMax = dateMin.AddMonths(1);

            var collection = DbConnection.Db.GetCollection<BsonDocument>(Entry.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("isShown", isShown) &
                         Builders<BsonDocument>.Filter.Gte("publishDate", dateMin) &
                         Builders<BsonDocument>.Filter.Lt("publishDate", dateMax);
            return Convert.ToInt32(collection.Count(filter));
        }
    }

}