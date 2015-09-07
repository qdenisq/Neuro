using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace neuro
{
    public class Synapse : ISynapse, IVersionable
    {
        public uint SynapseId { get; set; }
        public uint PreSynCellObjectId { get; set; }
        public uint PostSynCellObjectId { get; set; }
        public EnSynapseType Type { get; set; }
        public List<double> Vt { get; set; } 
        public Time TimeToPostSynCellSoma { get; set; }
        public Time TimeFromPreSynCellSoma { get; set; }
        public uint Version { get; set; }
    }
}
