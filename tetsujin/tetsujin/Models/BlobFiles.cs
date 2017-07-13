using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Collections;

using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace tetsujin.Models
{
    public class BlobFiles
    {
        public async static Task<string> SaveImagesAsync(List<IFormFile> files)
        {
            var accontName = Startup.Configuration.GetSection("Secrets")["AzureStorageAccount"];
            var key = Startup.Configuration.GetSection("Secrets")["AzureStorageKey"];

            var account = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                    accontName,
                    key),
                true);
            Console.WriteLine("connect: " + account.BlobStorageUri);
            var blobClient = account.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("blog");
            await blobContainer.CreateIfNotExistsAsync();

            foreach (var file in files)
            {
                var name = GetFilename(file.FileName);
                Console.WriteLine(name);
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

            return "";

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
            var collection = DbConnection.db.GetCollection<BsonDocument>("blobInfo");
            var doc = new BsonDocument()
            {
                { "w", imageInfo.Width },
                { "h", imageInfo.Height },
                { "nm", imageInfo.Name },
                { "d", imageInfo.Modified }
            };
            await collection.InsertOneAsync(doc);
        }

        public async static Task<string> GetImageInfoAsync()
        {
            var collection = DbConnection.db.GetCollection<BsonDocument>("blobInfo");

            FilterDefinition<BsonDocument> filter = new BsonDocument();
            var sortDoc = new BsonDocument
            {
                { "d", -1 },
            };
            var limit = 20;
            var infoDocs = await collection.Find<BsonDocument>(filter).Sort(sortDoc).Limit(limit).ToListAsync<BsonDocument>();
            var jsonDict = new Dictionary<string, BsonDocument>() { };
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
            ;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

