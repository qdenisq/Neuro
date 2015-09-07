using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neuro
{
    interface IVersionable
    {
        uint Version { get; set; }
    }
}
