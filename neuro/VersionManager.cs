using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace neuro
{
    public class VersionManager
    {
        public VersionManager(MongoClient client)
        {
            Client = client;
            var db = Db;
        }

        public async void Init()
        {
            var coll = Db.GetCollection<BsonDocument>("cells");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Exists("version");
            var sort = Builders<BsonDocument>.Sort.Descending("Version");
            var res = await coll.Find(filter).Sort(sort).FirstOrDefaultAsync();
            //res.RunSynchronously();
            if (res != null)
            {
                Version = new Version((uint) res["Version"]);
            }
        }

        public async Task<uint> CreateRevision()
        {
            var collectionNames = new List<string> {"cells"};
            foreach (var collName in collectionNames)
            {
                var coll = Db.GetCollection<BsonDocument>(collName);
                var collCopy = Db.GetCollection<BsonDocument>("temp");
                await    coll.Find(Builders<BsonDocument>.Filter.Eq("Version", (uint)Version.Current))
                        .ForEachAsync(d => {
                            var clone = d;
                            clone["Version"] = (uint)Version.Current + 1;
                            clone["_id"] = ObjectId.GenerateNewId();
                            coll.InsertOneAsync(clone);
                        } );
                var keys = Builders<BsonDocument>.IndexKeys.Ascending("Version");
                await coll.Indexes.CreateOneAsync(keys);
            }
            var res = Version.Current;
            Version.MoveNext();
            return (uint)res;
        }
        public Version Version { get; set; }
        IMongoDatabase Db => Client.GetDatabase("neuro");
        MongoClient Client { get; }

    }
}