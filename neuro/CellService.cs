using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace neuro
{
    using volt = System.Single;
    public class CellService : ServiceBase<CellBase>
    {
        public CellService(IMongoCollection<BsonDocument> context) : base(context)
        {}

        public void Process(List<CellBase> cells)
        {
            Parallel.ForEach(cells, @base =>
            {
                @base.CurrentVoltage = 0;
                foreach (var v in @base.Psps)
                {
                    @base.CurrentVoltage += v.Voltage;
                }

                if (@base.CurrentVoltage >= @base.VoltageThresh)
                {
                    @base.CurrentVoltage = 70.0F;
                }
            });

        }


    }
}
