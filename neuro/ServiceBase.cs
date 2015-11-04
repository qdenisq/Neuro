using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace neuro
{
    public abstract class ServiceBase<TEntity>
    {
        private readonly IMongoCollection<BsonDocument> _context;

        public ServiceBase(IMongoCollection<BsonDocument> context)
        {
            this._context = context;
        }

        public async Task<int> Save(IEnumerable<TEntity> input, InsertManyOptions opt = null)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (opt == null)
            {
                opt = new InsertManyOptions {IsOrdered = false};
            }

            var temp = input.AsParallel().Select(c => c.ToBsonDocument()).ToArray();
            watch.Stop();
            Console.WriteLine("         - Data serialization : {0} ms", watch.ElapsedMilliseconds);
            watch.Restart();
            await _context.InsertManyAsync(temp, opt);
            watch.Stop();
            Console.WriteLine("         - Data writing : {0} ms", watch.ElapsedMilliseconds);
            return 0;
        }

        public async Task<int> GetLastGenerationNumber()
        {
            var doc = await _context.Find(item => true).SortByDescending(item => item["v"]).FirstAsync();
            return doc["v"].AsInt32;
        }

        public async Task<List<TEntity>> GetLastGeneration()
        {
            var res = new List<TEntity>();
            var genNum = await GetLastGenerationNumber();
            using (var cursor = await _context.FindAsync<BsonDocument>(item => item["v"] == genNum))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        var item = BsonSerializer.Deserialize<TEntity>(document);
                        res.Add(item);
                    }
                }
            }
            return res;
        }

        public async Task<int> IndexBy(string value = "v")
        {
            var key = Builders<BsonDocument>.IndexKeys.Ascending(value);
            await _context.Indexes.CreateOneAsync(key);
            return 0;
        }

       //public abstract Task<List<TEntity>> CreateGeneration();
    }
}
