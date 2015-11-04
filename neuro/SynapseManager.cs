using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace neuro
{
    public class SynapseManager
    {
        public Synapse CreateSynapse(  )
        {
            Synapse synapse = new Synapse();

            return synapse;
        } 

        public IMongoCollection<BsonDocument> CellsCollection { get; set; }
        public VersionManager VersionManager { get; set; }
    }
}
