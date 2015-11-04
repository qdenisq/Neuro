using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neuro
{
    using volt = System.Single;
    interface IConductive
    {
        volt CurrentVoltage { get; set; }
    }
}
