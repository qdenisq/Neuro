using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Nuclex.Support.Cloning;

namespace neuro
{
    using volt = System.Single;
    public class CellService : ServiceBase<CellBase>
    {
        public List<Dictionary<int, CellBase>> Cells { get; set; }

        public CellService(IMongoCollection<BsonDocument> context) : base(context)
        {
            Cells = new List<Dictionary<int, CellBase>>();
        }

        public async Task Create(int X, int Y, int Z)
        {
            Console.WriteLine("-Cells creating");
            var cells = new Dictionary<int, CellBase>();
            int cId = 0;
            Random rnd = new Random();
            for (int x = 0; x != X; ++x) // x to service member
            {
                for (int y = 0; y != Y; ++y) // y to service member
                {
                    for (int z = 0; z != Z; ++z) // z to service member
                    {
                        Cell c = new Cell()
                        {
                            CellId = cId,
                            Version = 0,
                            PostSynapticIds = new List<int>(),
                            PreSynapticIds = new List<int>(),
                            VoltageRp = -70.0F,
                            VoltageThresh = -30.0F,
                            Pos = new Position(x * 10 + rnd.Next(100), y * 10 + rnd.Next(100), z * 10 + rnd.Next(100)),
                            CurrentVoltage = 0.0F,
                            IsSpiking = false
                        };
                        cells[cId] = c;
                        cId += 2;
                    }
                }
            }
            Cells.Add(cells);
            return;
        }

        public async Task<Dictionary<int, CellBase>> Clone(int i, uint version)
        {
            var tempCell = new Dictionary<int, CellBase>();
            var it = Cells[i].GetEnumerator();
            while (it.MoveNext())
            {
                int key = it.Current.Key;
                tempCell[key] = ExpressionTreeCloner.DeepFieldClone(it.Current.Value as Cell);
            }
            Parallel.ForEach(tempCell, c => {
                c.Value.Version = version;
                c.Value.CurrentVoltage = -70.0F;
            });
            return tempCell;
        }

        public async Task<Dictionary<int, CellBase>> Clone(int i)
        {
            return Clone(i, Cells[i][0].Version + 1).Result;
        }


        public void Process(List<CellBase> cells)
        {
            Parallel.ForEach(cells, @base =>
            {
                @base.CurrentVoltage = 0;

                if (@base.CurrentVoltage >= @base.VoltageThresh)
                {
                    @base.CurrentVoltage = 70.0F;
                }
            });

        }


    }
}
