using System;

namespace Compilers
{
    /**
     * Class Value : it stores both the value and the type 
     * for the variables
     */
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

