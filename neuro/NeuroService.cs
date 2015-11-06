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
        /// <summary>
        /// This field provides access to Cells in network
        /// </summary>
        /// <details>
        /// Used to get access to cells, loading from db and saving to it
        /// </details>
        private readonly CellService _cService;

        public List<Dictionary<int, CellBase>> Cells { get; private set; }
  
        /// <summary>
        /// This field provides access to Synapses in network
        /// </summary>
        /// <details>
        /// Used to get access to synapses, loading from db and saving to it
        /// </details>
        private readonly SynapseService _sService;

        public List<Dictionary<int, SynapseBase>> Synapses { get; private set; }
    
        /// <summary>
        /// This field is used for computing the membrane potential in the cell while spiking
        /// </summary>
        private readonly List<volt> _spikeVoltage;

        /// <summary>
        /// This field is used for computing the membrane potential 
        /// in synapse terminal on the postsynaptic side
        /// </summary>
        private readonly List<volt> _pspVoltage;

        /// <summary>
        /// This field is used for implementation of LTP in synapses
        /// </summary>
        private readonly List<float> _LTPFunc;

        /// <summary>
        /// This field is used for implementation of LTD in synapses
        /// </summary>
        private readonly List<float> _LTDFunc;

        public NeuroRenderer Renderer { get; set; }
        public NeuroService(CellService     cellService,
                            SynapseService  synService,
                            List<volt>      spikeVoltage,
                            List<volt>      pspVoltage,
                            List<float>    LTPFunc,
                            List<float>    LTDFunc
                            )
        {
            _cService = cellService;
            _sService = synService;

            Cells = _cService.Cells;
            Synapses = _sService.Synapses;

            _spikeVoltage = spikeVoltage;
            _pspVoltage = pspVoltage;

            _LTPFunc = LTPFunc;
            _LTDFunc = LTDFunc;
        }

        public NeuroService(CellService cellService,
                            SynapseService synService
                            )
        {
            _cService = cellService;
            _sService = synService;

            Cells = _cService.Cells;
            Synapses = _sService.Synapses;

            _spikeVoltage = new List<volt>() 
                    {
                        100.0F,
                        //-500.0F,
                        //-500.0F,
                        50.0F,
                        0.0F
                    }; 

            _pspVoltage = new List<volt>() 
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

            _LTPFunc = new List<float>()
            {
                0.01F,
                0.01F,
                0.02F,
                0.02F,
                0.03F
            };

            _LTDFunc = new List<float>()
            {
                -0.01F,
                -0.01F,
                -0.02F,
                -0.02F,
                -0.03F
            };
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
            Cells.Clear();
            Synapses.Clear();
        }

        public async Task Create()
        {
           // Clear();

            await _cService.Create(5, 5, 10);
            Cells = _cService.Cells;

            await _sService.Create(Cells.Last(), 85);
            Synapses = _sService.Synapses;
            return;



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
                            PostSynapticIds = new List<int>(),
                            PreSynapticIds = new List<int>(),
                            VoltageRp = -70.0F,
                            VoltageThresh = -30.0F,
                            Pos = new Position(x * 10 + rnd.Next(100), y * 10 + rnd.Next(100), z * 10 + rnd.Next(100)),
                            CurrentVoltage = -70.0F,
                            IsSpiking = false
                        };
                        cells[cId]  = c;
                        cId += 2;
                    }
                }
            }
            Cells.Add(cells);
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
            Synapses.Add(synapses);
            Console.WriteLine("synapse count : {0}", synapses.Count);
        }

        void Next(int gen)
        {
            var taskCell =_cService.Clone(gen);
            var taskSyn = _sService.Clone(gen);
            Cells.Add(taskCell.Result);
            Synapses.Add(taskSyn.Result);
            return;


            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var tempCell = new Dictionary<int, CellBase>();
            var it = Cells[gen].GetEnumerator();
            while (it.MoveNext())
            {
                int key = it.Current.Key;
                tempCell[key] = ExpressionTreeCloner.DeepFieldClone(it.Current.Value as Cell);
            }
            Parallel.ForEach(tempCell, c => { c.Value.Version++;
                                              c.Value.CurrentVoltage = -70.0F;
            });
            Cells.Add(tempCell);

            stopWatch.Stop();
            Console.WriteLine("         - copying cells :{0} ms", stopWatch.ElapsedMilliseconds);
            stopWatch.Reset();
            stopWatch.Start();

            var tempSyn = new Dictionary<int, SynapseBase>();
            var its = Synapses[gen].GetEnumerator();
            while (its.MoveNext())
            {
                int key = its.Current.Key;
                tempSyn[key] = ExpressionTreeCloner.DeepFieldClone(its.Current.Value as Synapse);
            }
            Parallel.ForEach(tempSyn, s => s.Value.Version++);
            Synapses.Add(tempSyn);

            stopWatch.Stop();
            Console.WriteLine("         - copying synapses Parallel :{0} ms", stopWatch.ElapsedMilliseconds);
        }

        void Process(int gen)
        {
            // check if there any spikes in cells
            Parallel.ForEach(Cells[gen], pair =>
            {
                var cell = pair.Value;
                volt curVoltage = cell.VoltageRp + cell.CurrentVoltage;

                if (!cell.IsSpiking)
                {
                    foreach (var synapseId in cell.PostSynapticIds)
                    {
                        var synapse = Synapses[gen][synapseId];
                        double dist = neuro.Position.Dist(synapse.Pos, cell.Pos);
                        int type = synapse.Type == EnSynapseType.Excatotary ? 1 : -1;
                        curVoltage += (synapse.CurrentVoltage * type * synapse.Strongness * System.Convert.ToSingle(Math.Exp(-dist / 100))); //10 to service member
                    }
                }

                //if (curVoltage > -70.0F)
                //{
                //    Console.WriteLine(" -- Voltage: Id {0}, Voltage {1}, Thresh {2}", cell.CellId, curVoltage, cell.VoltageThresh);
                //}
                if (!cell.IsSpiking && curVoltage >= cell.VoltageThresh )
                {
                    Console.WriteLine(" -- Spike: Id {0}, Voltage {1}, Thresh {2}", cell.CellId, curVoltage, cell.VoltageThresh);
                    //spike generation

                    // LTP computing
                    // LTP: For each synapse where was spike activity PreSyn cell spikes then PostSyn cell spikes,
                    // increase synapse strongness according to time distance between spikes dS ~ dT

                    Parallel.ForEach(cell.PostSynapticIds, synId =>
                    {
                        for (int v = 0; v != _LTPFunc.Count; ++v)
                        {
                            int cellId = Synapses[gen][synId].PreSynCellObjectId;
                            if (Cells[gen - _LTPFunc.Count + v][cellId].IsSpiking)
                            {
                                float dS = _LTPFunc[v] / (_LTPFunc.Count - v);
                                for (int f = gen + 1; f != Synapses.Count; ++ f)
                                {
                                    Synapses[f][synId].Strongness += dS;
                                }
                            }
                        }
                    });

                    // LTD computing
                    // LTD: For each synapse where was spike activity PostSyn cell spikes then PreSyn cell spikes,
                    // decrease synapse strongness according to time distance between spikes dS ~ dT
                    Parallel.ForEach(cell.PreSynapticIds, synId =>
                    {
                        for (int v = 0; v != _LTDFunc.Count; ++v)
                        {
                            int cellId = Synapses[gen][synId].PostSynCellObjectId;
                            if (Cells[gen - _LTDFunc.Count + v][cellId].IsSpiking)
                            {
                                float dS = _LTDFunc[v] / (_LTPFunc.Count - v);
                                for (int f = gen + 1; f != Synapses.Count; ++f)
                                {
                                    Synapses[f][synId].Strongness += dS;
                                }
                            }
                        }
                    });


                    List<volt> spikeVoltage = _spikeVoltage;

                    for (int i = 0; i != spikeVoltage.Count; ++i)
                    {
                        Cells[gen + i][cell.CellId].IsSpiking = true;
                        Cells[gen + i][cell.CellId].CurrentVoltage += spikeVoltage[i];
                    }

                    //psp generation
                    List<volt> pspVoltage = _pspVoltage;

                    Parallel.ForEach(cell.PreSynapticIds, synId =>
                    {
                        for (int i = 0; i != pspVoltage.Count; ++i)
                        {
                           Synapses[gen + i][synId].CurrentVoltage += pspVoltage[i];
                        }
                    });
                }
            });
        }


        public async Task<int> Save()
        {
            //var tS = _sService.Save(Synapses[0]);
            //var tC = _cService.Save(Cells[0]);
            //await Task.WhenAll(tS, tC);
            return 0;
        }

        public async Task Run()
        {

            //Create();
            
            Random rnd = new Random();
            List<int> cellIdsToFire = new List<int>();
            foreach (var cell in Cells[0])
            {
                if (rnd.Next(100) > 80)
                {
                    cellIdsToFire.Add(cell.Value.CellId);
                }
            }

            for (int i = 0; i != 400; ++i)
            {
                Stopwatch watch = new Stopwatch();
                Console.WriteLine("iteration : {0}", i);
                watch.Start();

                Next(Cells.Count - 1);
                watch.Stop();
                Console.WriteLine("     - Reserving next : {0} ms", watch.ElapsedMilliseconds);
                // spotanious soikes
               
                if (i < 50)
                {
                    continue;
                }

                if (i % 10 == 0 && i < 400)
                {

                    foreach (var cellId in cellIdsToFire)
                    {
                        Cells[Cells.Count - 19][cellId].CurrentVoltage = 70.0F;
                    }
                    //var en = Synapses[Synapses.Count - 2].GetEnumerator();
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

                Process(Cells.Count - 20);
                await Renderer.Update(Cells.Count - 19);
                //Renderer.RenWin.Render();
                //Thread.Sleep(100);
                watch.Stop();
                Console.WriteLine("     - Processing all network : {0}", watch.ElapsedMilliseconds);

                Cells.RemoveAt(0);
                Synapses.RemoveAt(0);
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
                    Cells.RemoveAt(0);
                    Synapses.RemoveAt(0);
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
