using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace neuro 
{
    public enum EnSynapseType
    {
        Inhibitory ,
        Excatotary
    }
    public interface ISynapse
    {
        [BsonElement("elementId")]
        uint SynapseId { get; set; }
        [BsonElement("preSynId")]
        uint PreSynCellObjectId { get; set; }
        [BsonElement("postSynId")]
        uint PostSynCellObjectId { get; set; }
        [BsonElement("synType")]
        EnSynapseType Type { get; set; }
        List<double> Vt { get; set; }
        [BsonElement("timeToSoma")]
        Time TimeToPostSynCellSoma { get; set; }
        [BsonElement("timeFromSoma")]
        Time TimeFromPreSynCellSoma { get; set; }
    }
}
