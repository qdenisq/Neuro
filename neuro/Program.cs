using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

/*
uint Id:    cells mod 2 = 0
            synapses mod 2 = 1

*/

namespace neuro
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("neuro");
            var collection = db.GetCollection<BsonDocument>("cells");
            // InsertAsync(collection);
            // Console.ReadLine();
            //PrintIds(collection);
            collection.DeleteManyAsync(cell => true);
            for (int i = 0; i != 20; ++i)
            {
                Warp.InsertAsync(collection);
            }

            var res = Warp.Do(collection);
            res.Wait();

            Console.ReadLine();

        }




    }

    class Warp
    {

        public static async Task<int> Do(IMongoCollection<BsonDocument> collection)
        {
            for (int i = 0; i != 5; ++i)
            {
               await ChangeVoltageRp(collection, i);
               await Print(collection);
            }
            return 0;
        }

        public static async Task<int> InsertAsync(IMongoCollection<BsonDocument> coll)
        {
            var cell1 = new CCell() { PostSynapticIds = new List<uint>(), PreSynapticIds = new List<uint>(), VoltageRp = new Voltage(-70.0) };
            await coll.InsertOneAsync(cell1.ToBsonDocument());
            return 0;
        }

        static async Task<int> PrintIds(IMongoCollection<BsonDocument> coll)
        {
            using (var cursor = await coll.FindAsync<BsonDocument>(cell => true))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        var id = BsonSerializer.Deserialize<CCell>(document).CellId;
                        Console.WriteLine("ID : {0}", id);
                    }
                }
            }
            return 0;
        }

        public static async Task<int> Print(IMongoCollection<BsonDocument> coll)
        {
            using (var cursor = await coll.FindAsync<BsonDocument>(cell => true))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        // var id = BsonSerializer.Deserialize<CCell>(document).CellObjectId;
                        Console.WriteLine("Object : {0}", document);
                    }
                }
            }
            return 0;
        }

        static async Task<string> ChangeVoltageRp(IMongoCollection<BsonDocument> coll, double dV)
        {
            //using (var cursor = await coll.FindAsync<BsonDocument>(cell => true))
            //{
            //    while (await cursor.MoveNextAsync())
            //    {
            //        var batch = cursor.Current;
            //        foreach (var document in batch)
            //        {
            //            // process document

            //            var obj = BsonSerializer.Deserialize<CCell>(document);
            //            obj.VoltageRp.Value = obj.VoltageRp.Value + dV;


            //        }
            //    }
            //}
            //var builder = Builders<BsonDocument>.Filter;
            //Func<BsonDocument, bool> filter = (BsonDocument cell) => true;

            var update = Builders<BsonDocument>.Update
                .Inc("VoltageRp.Value", dV);

            var result = await coll.UpdateManyAsync(cell => true, update);
            string res = string.Format("iteration : {0}", dV.ToString());
            Console.WriteLine(res);
            return res;
        }
    }
}
