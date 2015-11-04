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
    [Serializable]
    [BsonIgnoreExtraElements]
    public class Cell: CellBase, IVersionable
    {
        //public uint CellId { get; set; }
        //[BsonElement("preIds")]
        //public List<int> PreSynapticIds { get; set; }
        //[BsonElement("postIds")]
        //public List<int> PostSynapticIds { get; set; }
        //[BsonElement("vrp")]
        //public double VoltageRp { get; set; }
        //[BsonElement("vthr")]
        //public double VoltageThresh { get; set; }
        //[BsonElement("psps")]
        //public List<PSP> Psps { get; set; }
        //public Posistion Pos { get; set; }
        
    }
}
