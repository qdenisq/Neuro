using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace neuro
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("neuro");
            var collection = db.GetCollection<BsonDocument>("cells");
            InsertAndCountAsync(collection);
            Console.ReadLine();

        }

        static async void InsertAndCountAsync(IMongoCollection<BsonDocument> coll)
        {
            var cell = new BsonDocument
            {
                { "thresh" , 10},
                { "rp" , 5}
            };
            await coll.InsertOneAsync(cell);
            var count = await coll.CountAsync(new BsonDocument());

            Console.WriteLine(count);
        }
    }
}
