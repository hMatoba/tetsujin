using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

public static class DbConnection
{
    public static IMongoDatabase db;
    public static string ConnectionString;

    public static void Connect(string connectionString)
    {
        ConnectionString = connectionString;
        var client = new MongoClient(ConnectionString);
        db = client.GetDatabase("helicon");
    }
}
