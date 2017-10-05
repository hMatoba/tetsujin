using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tetsujin.Models
{
    public class BlobFile
    {
        private static string AccountName { get; set; } = "";
        private static string Key { get; set; } = "";
        public static string Url { get; set; } = "";
        private const string BlobName = "blog";
        private const string InfoCollectionName = "BlobInfo";

        public static void SetAccountInfo(string accountName, string key, string blobUrl)
        {
            if (String.IsNullOrEmpty(accountName))
            {
                throw new ArgumentException("Give azure filestorage account name as environment value 'STRORAGE_ACCOUNT'.");
            }
            else
            {
                AccountName = accountName;
            }

            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Give azure filestorage account key as environment value 'STORAGE_KEY'.");
            }
            else
            {
                Key = key;
            }

            if (String.IsNullOrEmpty(blobUrl))
            {
                throw new ArgumentException("Give azure filestorage URL as environment value 'STORAGE_URL'.");
            }
            else
            {
                Url = blobUrl;
            }
        }

        public static async Task SaveImagesAsync(List<IFormFile> files)
        {
            var account = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                    AccountName,
                    Key),
                true);
            var blobClient = account.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(BlobName);
            await blobContainer.CreateIfNotExistsAsync();

            foreach (var file in files)
            {
                var name = GetFilename(file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    var blockBlob = blobContainer.GetBlockBlobReference(name);
                    blockBlob.Properties.CacheControl = "public, max-age=2678400";
                    blockBlob.Properties.ContentType = file.ContentType;

                    await blockBlob.UploadFromStreamAsync(stream);
                    var imageInfo = GetImageInfo(stream);
                    imageInfo.Name = name;
                    imageInfo.Modified = DateTime.Now.AddHours(9);
                    await SaveImageInfo(imageInfo);
                }
            }
        }

        private static string GetFilename(string fullpath)
        {
            List<string> split;
            if (fullpath.Contains(@"\"))
            {
                split = fullpath.Split('\\').ToList();
            }
            else
            {
                split = fullpath.Split('/').ToList();
            }
            return split.Last();
        }

        private static ImageInfo GetImageInfo(System.IO.Stream stream)
        {
            var image = System.Drawing.Image.FromStream(stream);
            var imageInfo = new ImageInfo()
            {
                Width = image.Width,
                Height = image.Height,
            };
            return imageInfo;
        }

        private async static Task SaveImageInfo(ImageInfo imageInfo)
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(InfoCollectionName);
            var doc = new BsonDocument()
            {
                { "w", imageInfo.Width },
                { "h", imageInfo.Height },
                { "nm", imageInfo.Name },
                { "d", imageInfo.Modified }
            };
            await collection.InsertOneAsync(doc);
        }

        public static async Task<string> GetImageInfoAsync()
        {
            var collection = DbConnection.Db.GetCollection<BsonDocument>(InfoCollectionName);

            FilterDefinition<BsonDocument> filter = new BsonDocument();
            var sortDoc = new BsonDocument
            {
                { "d", -1 },
            };
            var limit = 20;
            var infoDocs = await collection.Find(filter).Sort(sortDoc).Limit(limit).ToListAsync();
            var jsonDict = new Dictionary<string, BsonDocument>();
            var i = 0;
            foreach (var doc in infoDocs)
            {
                doc.Remove("_id");
                doc.Remove("d");
                jsonDict.Add(i.ToString(), doc);
                i++;
            }

            return jsonDict.ToJson();
        }
    }

    class ImageInfo : IEnumerable
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("w")]
        [BsonRepresentation(BsonType.Int32)]
        public int Width { get; set; }

        [BsonElement("h")]
        [BsonRepresentation(BsonType.Int32)]
        public int Height { get; set; }

        [BsonElement("nm")]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("d")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Modified { get; set; }

        public void Add(object obj)
        {
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}