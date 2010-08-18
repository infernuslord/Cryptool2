using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscreteLogarithm
{
    class LinearDependentException : Exception
    {
        public int RowToDelete
        {
            get;
            private set;
        }

        public LinearDependentException(int rowToDelete)
        {
            this.RowToDelete = rowToDelete;
        }
    }
}
