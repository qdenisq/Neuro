using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neuro
{
    public class Version : IEnumerator
    {
        public Version(uint value)
        {
            Value = value;
        }
        public bool MoveNext()
        {
            Value ++;
            return Value != 0;
        }

        public void Reset()
        {
            Value = 0;
        }

        public object Current => Value;

        public uint Value { get; set; }

    }
}
