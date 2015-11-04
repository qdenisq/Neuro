using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using neuro;

namespace neuro
{
    using volt = System.Single;
    [Serializable]
    public class PSP
    {
        public volt Voltage { get; set; }
        public EnSynapseType Type { get; set; }
        public uint SynId { get; set; }
    }
}
