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
    using volt = System.Single;
    [BsonKnownTypes(typeof(Cell))]
    public abstract class CellBase : IVersionable, IPositional, IConductive
    {
        [BsonElement("id")]
        public int CellId { get; set; }
        [BsonElement("preIds")]
        public List<int> PreSynapticIds { get; set; }
        [BsonElement("postIds")]
        public List<int> PostSynapticIds { get; set; }
        [BsonElement("vrp")]
        public volt VoltageRp { get; set; }
        [BsonElement("vthr")]
        public volt VoltageThresh { get; set; }
        [BsonElement("p")]
        public Position Pos { get; set; }
        [BsonElement("v")]
        public uint Version { get; set; }
        [BsonElement("cv")]
        public volt CurrentVoltage { get; set; }
        [BsonElement("is")]
        public bool IsSpiking { get; set; }
    }
}
