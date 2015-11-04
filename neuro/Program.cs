using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kitware.VTK;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using neuro;
using Nuclex.Support.Cloning;

/*
uint Id:    cells mod 2 = 0
            synapses mod 2 = 1

*/

namespace neuro
{

    public static class ObjectExtensions
    {
        #region Methods

        public static T DeepCopy<T>(this T source)
        {
            var isNotSerializable = !typeof(T).IsSerializable;
            if (isNotSerializable)
                throw new ArgumentException("The type must be serializable.", "source");

            var sourceIsNull = ReferenceEquals(source, null);
            if (sourceIsNull)
                return default(T);

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        #endregion
    }

    class Program
    {

        static void Main(string[] args)
        {
           
            var client = new MongoClient();
            
            var db = client.GetDatabase("neuro");
            var cellCollection = db.GetCollection<BsonDocument>("cells");
            var synCollection = db.GetCollection<BsonDocument>("synapses");
            // InsertAsync(collection);
            // Console.ReadLine();
            //PrintIds(collection);
            cellCollection.DeleteManyAsync(cell => true);
            synCollection.DeleteManyAsync(cell => true);

            CellService cellService = new CellService(cellCollection);
            SynapseService synService = new SynapseService(synCollection);
            NeuroService neuroService = new NeuroService(cellService, synService);
            neuroService.Create();

            NeuroRenderer renderer = new NeuroRenderer(neuroService);
            neuroService.Renderer = renderer;
            renderer.Init();

            Wrap wrapper = new Wrap();
            wrapper.Service = neuroService;
            wrapper.Renderer = renderer;
            wrapper.Run();
            
            //neuroService.Run();
            //
            //renderer.Start();

            Console.ReadLine();
        }
    }

    class Wrap
    {
        public NeuroService Service { get; set; }
        public NeuroRenderer Renderer { get; set; }

        public async Task Run()
        {
            //Task.Factory.StartNew(Service.Run);
            Task.Factory.StartNew(Renderer.Start);
            Renderer.IRen.Render();
            Service.Run();
            //();
        }

    }

    class Warp
    {

        public static async Task<int> RndColor(List<vtkActor> data)
        {
                Random rnd = new Random();
           
                Parallel.ForEach(data,
                    actor =>
                    {
                        actor.GetProperty().SetColor(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble());
                    });
            
            
            return 0;
        }

        public static async Task<int> Do(IMongoCollection<BsonDocument> CellCollection, IMongoCollection<BsonDocument> SynapseCollection, MongoClient client)
        {
            CellService cellService = new CellService(CellCollection);
            SynapseService synService = new SynapseService(SynapseCollection);
            NeuroService neuroService = new NeuroService(cellService, synService);
            Stopwatch watch = new Stopwatch();
            watch.Start();
          //  await neuroService.Run(false);
            watch.Stop();
            long time1 = watch.ElapsedMilliseconds;

            //neuroService.Create();
            //watch.Restart();
            //await neuroService.Run();
            //watch.Stop();
            long time2 = watch.ElapsedMilliseconds;
            Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("Time elapsed on {0} iterations without saving : {1}", 200, time1);
            //Console.WriteLine("Time elapsed on {0} iterations with saving : {1}", 200, time2);
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");

            return 0;
            List<List<CellBase>> data = new List<List<CellBase>>();
            List<List<SynapseBase>> dataSynapses = new List<List<SynapseBase>>();

            //Console.WriteLine("Insert async");

            //var list = new List<CellBase>();
            //for (int i = 0; i != 1000; ++i)
            //{
            //    await Warp.InsertAsync(collection);

            //    var cell1 = new Cell() { CellId = (uint)i * 2, PostSynapticIds = Enumerable.Range(1, 1000).ToList(), PreSynapticIds = Enumerable.Range(10000, 1000).ToList(), VoltageRp = -70.0, Version = 0 };
            //    list.Add(cell1);
            //}
            //data.Add(list);

            //var verManager = new VersionManager(client);
            //verManager.Init();
            var lastVersion = await cellService.GetLastGenerationNumber();
            var list = await cellService.GetLastGeneration();

            foreach (var c in list)
            {
                Console.WriteLine("id: {0} , synapses in : {1} , synapses out : {2} , version : {3}",
                    c.CellId, c.PreSynapticIds.Count, c.PostSynapticIds.Count, c.Version);
            }
            return 0;

            data.Add(new List<CellBase>(list));

            int lastSaved = 0;

            for (int i = 0; i != 100; ++i)
            {
               // Console.WriteLine("iteration : {0}", lastVersion++);
                Console.WriteLine("data count: {0}", data.Count * data[0].Count);

                var gen = data.Last();
                List<CellBase> temp1 = new List<CellBase>();
                foreach (var c in gen)
                {
                    var t0 = (Cell) c;
                    temp1.Add(ExpressionTreeCloner.DeepFieldClone(t0));
                };
                
                foreach (var c in temp1)
                {
                    c.Version++;
                }
                data.Add(temp1);

                if ( i > 20)
                {
                    Console.WriteLine("serialize objects to store");

                    var listToStore = data[0];
                    
                    var options = new InsertManyOptions { IsOrdered = false };
                    List<BsonDocument> temp = new List<BsonDocument>();

                    foreach (var c in listToStore)
                    {
                        temp.Add(c.ToBsonDocument());
                    }
                    Console.WriteLine("remove last from list");
                    data.RemoveAt(0);
                    Console.WriteLine("save to mongo");
                    await CellCollection.InsertManyAsync(temp, options);
                    
                    lastSaved = i;
                }
            }

            return 0;
        }

        public static async Task<int> InsertAsync(IMongoCollection<BsonDocument> coll)
        {
            var cell1 = new Cell() { PostSynapticIds = new List<int>(), PreSynapticIds = new List<int>(), VoltageRp = -70.0F, Version =  0};
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
                        var id = BsonSerializer.Deserialize<Cell>(document).CellId;
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

        static async Task<string> ChangeVoltageRp(IMongoCollection<BsonDocument> coll, double dV, uint ver)
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
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("Version", ver);

            var result = await coll.UpdateManyAsync(filter, update);
            string res = string.Format("iteration : {0}", dV.ToString());
            Console.WriteLine(res);
            return res;
        }
    }
}
