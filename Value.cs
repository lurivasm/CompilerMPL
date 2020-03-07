using System;

namespace Compilers
{
    public class Value
    {
        public VALTYPE Type { get; }

        public object Val { get; }

        public Value(VALTYPE type, object val)
        {
            Type = type;
            Val = val;
        }
    }
}

