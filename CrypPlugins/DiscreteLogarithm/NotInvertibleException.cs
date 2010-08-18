using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscreteLogarithm
{    
    class NotInvertibleException : Exception
    {
        public int NotInvertibleNumber
        {
            get;
            private set;
        }

        public NotInvertibleException(int notInvertible)
        {
            NotInvertibleNumber = notInvertible;
        }
    }
}
