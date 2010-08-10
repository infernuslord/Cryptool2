using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using Cryptool.PluginBase.Miscellaneous;

namespace DiscreteLogarithm
{
    class LinearSystemOfEquations
    {
        private BigInteger mod;
        private BigInteger size;
        private List<int[]> matrix;

        public LinearSystemOfEquations(BigInteger mod, BigInteger size)
        {
            this.mod = mod;
            this.size = size;
            matrix = new List<int[]>((int)size);
        }

        public void AddEquation(int[] coefficients, int b)
        {
            Debug.Assert(coefficients.Length == size);            

            int[] row = new int[coefficients.Length + 1];
            for (int c = 0; c < coefficients.Length; c++)
                row[c] = coefficients[c];
            row[row.Length - 1] = b;

            if (!LinearDependent(row))
                matrix.Add(row);
        }

        private bool LinearDependent(int[] row)
        {
            foreach (int[] mr in matrix)
            {
                bool linear = true;
                BigInteger row0invers = BigIntegerHelper.ModInverse(row[0], mod);
                BigInteger mr0 = new BigInteger(mr[0]);
                BigInteger div = (mr0 * row0invers) % mod;
                for (int i = 1; i < size + 1; i++)
                {
                    if (row[i] * div != mr[0])
                    {
                        linear = false;
                        break;
                    }
                }
                if (linear)
                    return true;
            }
            return false;
        }

        public bool MoreEquations()
        {
            return (matrix.Count < size);
        }

        public int[] Solve()
        {
            //TODO: Gauss here
            return null;
        }

    }
}
