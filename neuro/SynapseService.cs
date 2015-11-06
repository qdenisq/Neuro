using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nuclex.Support.Cloning;

namespace neuro
{
    public class SynapseService : ServiceBase<SynapseBase>
    {
        public List<Dictionary<int, SynapseBase>> Synapses { get; set; }

        public SynapseService(IMongoCollection<BsonDocument> context) : base(context)
        {
            Synapses = new List<Dictionary<int, SynapseBase>>();
        }

        public async Task Create(Dictionary<int, CellBase> cells, int probability)
        {
            int sId = 1;
            Random rand = new Random();
            var synapses = new Dictionary<int, SynapseBase>();
            var temp = new Dictionary<int, List<int>>(); // map to avoid self-reflections and A2B B2A reflections (circles)
            for (int input = 0; input < cells.Count * 2; input = input + 2)
            {
                temp[input] = new List<int> {input};
                for (int output = 0; output < cells.Count * 2; output = output + 2)
                {
                    if (temp.ContainsKey(output) && temp[output].Contains(input))
                    {
                        continue;
                    }

                    int r = rand.Next(100);
                    if (r >= probability) // to service member
                    {
                        Position postSynCellPosition = cells[output].Pos;
                        Position synPos = new Position(postSynCellPosition.X + rand.Next(-10, 10),
                                                        postSynCellPosition.Y + rand.Next(-10, 10),
                                                        postSynCellPosition.Z + rand.Next(-10, 10));
                        var synapse = new Synapse()
                        {
                            Pos = synPos,
                            PreSynCellObjectId = cells[input].CellId,
                            PostSynCellObjectId = cells[output].CellId,
                            SynapseId = sId,
                            Type = EnSynapseType.Excatotary,
                            Version = 0,
                            CurrentVoltage = 0.0F,
                            Strongness = 1.0F
                        };
                        synapses[sId] = synapse;
                        cells[input].PreSynapticIds.Add(sId);
                        cells[output].PostSynapticIds.Add(sId);
                        sId += 2;
                    }
                }
            }
            Synapses.Add(synapses);
            Console.WriteLine("-Synapses created");
            return;
        }

        public async Task<Dictionary<int, SynapseBase>> Clone(int i, uint version)
        {
            var tempSyn = new Dictionary<int, SynapseBase>();
            var its = Synapses[i].GetEnumerator();
            while (its.MoveNext())
            {
                int key = its.Current.Key;
                tempSyn[key] = ExpressionTreeCloner.DeepFieldClone(its.Current.Value as Synapse);
            }
            Parallel.ForEach(tempSyn, s => s.Value.Version = version);
            return tempSyn;
        }

        public async Task<Dictionary<int, SynapseBase>> Clone(int i)
        {
            return Clone(i, Synapses[i][1].Version + 1).Result;
        }

    }
}
