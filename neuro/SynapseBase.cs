using System;
using Microsoft.Office.Interop.Outlook;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace neuro
{
    using volt = System.Single;
    [Serializable]
    public enum EnSynapseType
    {
        Inhibitory ,
        Excatotary
    }

    [Serializable]
    [BsonKnownTypes(typeof(Synapse))]
    public abstract class SynapseBase : IVersionable, IPositional, IConductive
    {
        [BsonElement("id")]
        public int SynapseId { get; set; }
        [BsonElement("preId")]
        public int PreSynCellObjectId { get; set; }
        [BsonElement("postId")]
        public int PostSynCellObjectId { get; set; }
        [BsonElement("sType")]
        public EnSynapseType Type { get; set; }
        //[BsonElement("vt")]
        //public List<int> Vt { get; set; }
        //[BsonElement("timeToSoma")]
        //Time TimeToPostSynCellSoma { get; set; }
        //[BsonElement("timeFromSoma")]
        //Time TimeFromPreSynCellSoma { get; set; }
        [BsonElement("v")]
        public uint Version { get; set; }
        [BsonElement("p")]
        public Position Pos { get; set; }
        [BsonElement("cv")]
        public volt CurrentVoltage { get; set; }

       
    }
}
