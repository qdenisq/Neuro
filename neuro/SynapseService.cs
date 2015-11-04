using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace neuro
{
    public class SynapseService : ServiceBase<SynapseBase>
    {
        public SynapseService(IMongoCollection<BsonDocument> context) : base(context)
        {}

       
    }
}
