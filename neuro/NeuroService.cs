using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nuclex.Support.Cloning;
using Microsoft.VisualBasic.Devices;
using MongoDB.Driver.Linq;
using Kitware.VTK;

namespace neuro
{
    using volt = System.Single;
    public class NeuroService : IEnumerable
    {
        private CellService _cService { get; set; }
        private SynapseService _sService { get; set; }
        public NeuroRenderer Renderer { get; set; }
        public List<Dictionary<int, CellBase>> _cells { get; private set; } 
        public List<Dictionary<int, SynapseBase>> _synapses { get; private set; }
        public NeuroService(CellService cellService, SynapseService synService)
        {
            _cService = cellService;
            _sService = synService;
            _cells = new List<Dictionary<int, CellBase>>();
            _synapses = new List<Dictionary<int, SynapseBase>>();
        }

        //public async Task<int> Save(IEnumerable<CellBase> input, InsertManyOptions opt = null)
        //{
        //    if (opt == null)
        //    {
        //        opt = new InsertManyOptions { IsOrdered = false };
        //    }
        //    var temp = input.Select(c => c.ToBsonDocument()).ToArray();
        //    await _context.InsertManyAsync(temp, opt);
        //    return 0;
        //}

        public async Task<int> GetLastGenerationNumber()
        {
            var genNum = await _cService.GetLastGenerationNumber();
            return genNum;
        }

        public void LoadLastGeneration(out List<SynapseBase> synapses, out List<CellBase> cells)
        {
            var stask = _sService.GetLastGeneration();
            stask.RunSynchronously();
            synapses = stask.Result;

            var ctask = _cService.GetLastGeneration();
            ctask.RunSynchronously();
            cells = ctask.Result;
        }

        public void Clear()
        {
            _synapses.Clear();
            _cells.Clear();
        }

        public void Create()
        {
            Clear();
            Console.WriteLine("Creating new instances of cells");
            var cells = new  Dictionary<int, CellBase>();
            int cId = 0;
            int sId = 1;
            Random rnd = new Random();
            for (int x = 0; x != 5; ++x) // x to service member
            {
                for (int y = 0; y != 5; ++y) // y to service member
                {
                    for (int z = 0; z != 10; ++z) // z to service member
                    {
                        Cell c = new Cell()
                        {
                            CellId = cId,
                            Version = 0,
                            Psps = null,
                            PostSynapticIds = new List<int>(),
                            PreSynapticIds = new List<int>(),
                            VoltageRp = -70.0F,
                            VoltageThresh = -30.0F,
                            Pos = new Position(x * 10 + rnd.Next(100), y * 10 + rnd.Next(100), z * 10 + rnd.Next(100)),
                            CurrentVoltage = -70.0F
                        };
                        cells[cId]  = c;
                        cId += 2;
                    }
                }
            }
            _cells.Add(cells);
            Console.WriteLine("cells count : {0}", cells.Count);
            Console.WriteLine("Creating new instance of synapses");

            Random rand = new Random();
            var synapses = new Dictionary<int, SynapseBase>();
            for (int input = 0; input < cId; input = input + 2)
            {
                for (int output = 0; output < cId; output = output + 2)
                {

                    
                    if (input == output || synapses.Any(syn =>
                    {
                        return (syn.Value.PreSynCellObjectId == output && syn.Value.PostSynCellObjectId == input);
                    }))
                    {
                        continue;
                    }
                    int r = rand.Next(100);
                    if (r <= 10) // to service member
                    {
                        Position postSynCellPosition = cells[output].Pos;
                        Position synPos = new Position( postSynCellPosition.X + rand.Next(-10, 10),
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
                            //Vt = new List<int>(),
                            CurrentVoltage = 0.0F
                        };
                        synapses[sId] = synapse;
                        cells[input].PreSynapticIds.Add(sId);
                        cells[output].PostSynapticIds.Add(sId);
                        sId += 2;
                    }
                }
            }
            _synapses.Add(synapses);
            Console.WriteLine("synapse count : {0}", synapses.Count);
        }

        void Next(int gen)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var tempCell = new Dictionary<int, CellBase>();
            var it = _cells[gen].GetEnumerator();
            while (it.MoveNext())
            {
                int key = it.Current.Key;
                tempCell[key] = ExpressionTreeCloner.DeepFieldClone(it.Current.Value as Cell);
            }
            Parallel.ForEach(tempCell, c => { c.Value.Version++;
                                                c.Value.CurrentVoltage = -70.0F;
            });
            _cells.Add(tempCell);

            stopWatch.Stop();
            Console.WriteLine("         - copying cells :{0} ms", stopWatch.ElapsedMilliseconds);
            stopWatch.Reset();
            stopWatch.Start();

            var tempSyn = new Dictionary<int, SynapseBase>();
            var its = _synapses[gen].GetEnumerator();
            while (its.MoveNext())
            {
                int key = its.Current.Key;
                tempSyn[key] = ExpressionTreeCloner.DeepFieldClone(its.Current.Value as Synapse);
            }
            Parallel.ForEach(tempSyn, s => s.Value.Version++);
            _synapses.Add(tempSyn);

            stopWatch.Stop();
            Console.WriteLine("         - copying synapses Parallel :{0} ms", stopWatch.ElapsedMilliseconds);
        }

        void Process(int gen)
        {
            // check if there any spikes in cells
            Parallel.ForEach(_cells[gen], pair =>
            {
                var cell = pair.Value;
                volt curVoltage = cell.CurrentVoltage;

                foreach(var synapseId in cell.PostSynapticIds)
                {
                    var synapse = _synapses[gen][synapseId];
                    double dist = neuro.Position.Dist(synapse.Pos, cell.Pos);
                    int type = synapse.Type == EnSynapseType.Excatotary ? 1 : -1;
                    curVoltage += (synapse.CurrentVoltage * type * System.Convert.ToSingle(Math.Exp(-dist / 10)) ); //10 to service member
                }

                //if (curVoltage > -70.0F)
                //{
                //    Console.WriteLine(" -- Voltage: Id {0}, Voltage {1}, Thresh {2}", cell.CellId, curVoltage, cell.VoltageThresh);
                //}
                if (curVoltage >= cell.VoltageThresh )
                {
                    Console.WriteLine(" -- Spike: Id {0}, Voltage {1}, Thresh {2}", cell.CellId, curVoltage, cell.VoltageThresh);
                    //spike generation
                    List<volt> spikeVoltage = new List<volt>() // to service member
                    {
                        -500.0F,
                        //-500.0F,
                        //-500.0F,
                        -100.0F,
                        0.0F
                    };

                    for (int i = 0; i != spikeVoltage.Count; ++i)
                    {
                        _cells[gen + i][cell.CellId].CurrentVoltage += spikeVoltage[i];
                    }

                    //psp generation
                    List<volt> pspVoltage = new List<volt>() // to service member
                    {
                        1.0F,
                        3.0F,
                        10.0F,
                        9.0F,
                        7.0F,
                        3.0F,
                        2.0F,
                        2.0F,
                        1.0F,
                        1.0F,
                        1.0F
                    };

                    Parallel.ForEach(cell.PreSynapticIds, synId =>
                    {
                        for (int i = 0; i != pspVoltage.Count; ++i)
                        {
                           
                            _synapses[gen + i][synId].CurrentVoltage += pspVoltage[i];
                        }
                    });
                }
            });
        }


        public async Task<int> Save()
        {
            //var tS = _sService.Save(_synapses[0]);
            //var tC = _cService.Save(_cells[0]);
            //await Task.WhenAll(tS, tC);
            return 0;
        }

        public async Task Run()
        {

            //Create();
            
            Random rnd = new Random();

            for (int i = 0; i != 400; ++i)
            {
                Stopwatch watch = new Stopwatch();
                Console.WriteLine("iteration : {0}", i);
                watch.Start();

                Next(_cells.Count - 1);
                watch.Stop();
                Console.WriteLine("     - Reserving next : {0} ms", watch.ElapsedMilliseconds);
                // spotanious soikes
               
                if (i < 50)
                {
                    continue;
                }

                if (i % 6 == 0 && i < 400)
                {
                    foreach (var cell in _cells[_cells.Count - 20])
                    {
                        if (rnd.Next(100) > 90)
                        {
                            cell.Value.CurrentVoltage = 70.0F;
                        }
                    }

                    //var en = _synapses[_synapses.Count - 2].GetEnumerator();
                    //while (en.MoveNext())
                    //{
                    //    var c = en.Current.Value;
                    //    if (rnd.Next(0, 1000) > 900)
                    //    {
                    //        c.CurrentVoltage = 20.0F;
                    //    }
                    //}
                }

                watch.Restart();

                Process(_cells.Count - 20);
                await Renderer.Update(_cells.Count - 19);
                //Renderer.RenWin.Render();
                Thread.Sleep(100);
                watch.Stop();
                Console.WriteLine("     - Processing all network : {0}", watch.ElapsedMilliseconds);

                _cells.RemoveAt(0);
                _synapses.RemoveAt(0);
                continue;
                watch.Restart();
                //if (bSave)
                //{
                //    await Save();
                //}
                watch.Stop();
                Console.WriteLine("     - Saving to DB : {0}", watch.ElapsedMilliseconds);
                watch.Restart();
                if (new ComputerInfo().AvailablePhysicalMemory < 3E9)
                {
                    _cells.RemoveAt(0);
                    _synapses.RemoveAt(0);
                }
                watch.Stop();
                Console.WriteLine("     - Removing from Ram : {0}", watch.ElapsedMilliseconds);
                
            }
            return ;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
