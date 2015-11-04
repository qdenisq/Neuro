using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace neuro
{
    public class NeuroRenderer
    {
        public NeuroRenderer(NeuroService s)
        {
            _service = s;
            CellsActors = new Dictionary<int, vtkActor>();
            SynActors = new Dictionary<int, vtkActor>();
            Ren = new vtkOpenGLRenderer();
            IRen = new vtkRenderWindowInteractor();
            Camera = new vtkOpenGLCamera();
            RenWin = vtkRenderWindow.New();
            RenWin.AddRenderer(Ren);
            IRen.SetRenderWindow(RenWin);
            _rnd = new Random();
        }

        ~NeuroRenderer()
        {
            foreach (var act in CellsActors)
            {
                act.Value.Dispose();
            }
            foreach (var act in SynActors)
            {
                act.Value.Dispose();
            }
            Ren.Dispose();
            IRen.Dispose();
            Camera.Dispose();
            RenWin.Dispose();
        }

        public void AddSynapseActor(SynapseBase synapse)
        {
            var line = new vtkLineSource();
            var inPos = _service._cells[0][synapse.PreSynCellObjectId].Pos;
            var outPos = _service._cells[0][synapse.PostSynCellObjectId].Pos;
            line.SetPoint1(inPos.X, inPos.Y, inPos.Z);
            line.SetPoint2(outPos.X, outPos.Y, outPos.Y);
            
            var mapper1 = vtkPolyDataMapper.New();
            mapper1.SetInputConnection(line.GetOutputPort());
            // The actor links the data pipeline to the rendering subsystem
            var actor1 = vtkActor.New();
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetOpacity(0.05);
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetColor(0, 0.3, 0.3);

            Ren.AddActor(actor1);
            
            SynActors[synapse.SynapseId] = actor1;
        }

        public void AddCellsActor(CellBase cell)
        {
            var cube = new vtkCubeSource();
            var pos = cell.Pos;
            cube.SetCenter(pos.X, pos.Y, pos.Z);
            cube.SetXLength(0.5);
            cube.SetYLength(0.5);
            cube.SetZLength(0.5);

            var mapper1 = vtkPolyDataMapper.New();
            mapper1.SetInputConnection(cube.GetOutputPort());
            // The actor links the data pipeline to the rendering subsystem
            var actor1 = vtkActor.New();
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetOpacity(0.1);
            actor1.SetMapper(mapper1);
            actor1.GetProperty().SetColor(0, 0.3, 0.3);

            Ren.AddActor(actor1);

            CellsActors[cell.CellId] = actor1;
        }

        public void Init()
        {
            foreach (var cell in _service._cells[0])
            {
                AddCellsActor(cell.Value);
            }
            foreach (var syn in _service._synapses[0])
            {
                AddSynapseActor(syn.Value);
            }
        }

        public static List<double> GetColor(double voltage)
        {
            double r = (voltage)/10;
            var clr = new List<double>()
            {
                r,
                0.3,
                0.3
            };
            return clr;
        } 

        public async Task<int> Update(int i)
        {
            foreach (var pair in _service._cells[i])
            {
                var cell = pair.Value;
                var clr = GetColor(cell.CurrentVoltage);
                CellsActors[cell.CellId].GetProperty().SetColor(clr[0], clr[1], clr[2]);
                CellsActors[cell.CellId].GetProperty().SetOpacity(0.1 + clr[0]);
            }

            foreach (var pair in _service._synapses[i])
            {
                var syn = pair.Value;
                var clr = GetColor(syn.CurrentVoltage);
                SynActors[syn.SynapseId].GetProperty().SetColor(clr[0], clr[1], clr[2]);
                SynActors[syn.SynapseId].GetProperty().SetOpacity(0.1);
                //SynActors[syn.SynapseId].Render(Ren, SynActors[syn.SynapseId].GetMapper());
            }
            return 0;
        }


        public void Start()
        {
            RenWin.SetSize(500, 500);
            RenWin.Render();
            Camera = Ren.GetActiveCamera();
            Camera.Zoom(1.5);
            // // render the image and start the event loop
            // //
            RenWin.SetDesiredUpdateRate(40.0);
            IRen.Initialize();
            //// iren.FlyToImage(ren1, 5.0, 5.0);
            // _service.Run();
            IRen.Start();
        }

        public new Dictionary<int, vtkActor> CellsActors { get; set;}
        public new Dictionary<int, vtkActor> SynActors { get; set; }
        public vtkRenderer Ren { get; set; }
        public vtkRenderWindow RenWin { get; set; }
        public vtkRenderWindowInteractor IRen { get; set; }
        public vtkCamera Camera { get; set; }

        private NeuroService _service;

        private Random _rnd;

    }
}
