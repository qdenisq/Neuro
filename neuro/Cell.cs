using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace neuro
{
    public class CCell: ICell, IVersionable
    {
        public uint CellId { get; set; }
        public List<uint> PreSynapticIds { get; set; }
        public List<uint> PostSynapticIds { get; set; }
        public Voltage VoltageRp { get; set; }
        public Voltage VoltageThresh { get; set; }
        public List<PSP> Psps { get; set; }
        public uint Version { get; set; }
    }
}
