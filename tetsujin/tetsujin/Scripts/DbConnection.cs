using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Net.Sockets;

public class DbConnection
{
    public static IMongoDatabase Db { get; set; }

    public static void Connect(string connectionString, string dbName)
    {
        var url = new MongoUrl(connectionString);
        var clientSettings = MongoClientSettings.FromUrl(url);
        Action<Socket> socketConfigurator = s => s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        clientSettings.ClusterConfigurator = cb => cb.ConfigureTcp(tcp => tcp.With(socketConfigurator: socketConfigurator));
        var client = new MongoClient(clientSettings);
        Db = client.GetDatabase(dbName);
        bool isMongoLive = Db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);

        if (!isMongoLive)
        {
            throw new Exception("Failed to connect database.");
        }
    }
}
