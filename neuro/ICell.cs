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
    public interface ICell 
    {
        [BsonElement("elementId")]
        uint CellId { get; set; }
        List<uint> PreSynapticIds { get; set; }
        List<uint> PostSynapticIds { get; set; }
        Voltage VoltageRp { get; set; }
        Voltage VoltageThresh { get; set; }
        List<PSP> Psps { get; set; }

    }
}
