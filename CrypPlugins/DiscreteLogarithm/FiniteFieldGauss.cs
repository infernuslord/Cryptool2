using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Cryptool.PluginBase.Miscellaneous;
using System.Diagnostics;

namespace DiscreteLogarithm
{
    /**
     * This class implements the gauss algorithm over a finite field (i.e. modulo a prime number).
     * It uses BigInteger, so it works on really big residue classes.
     **/
    class FiniteFieldGauss
    {
        private int size;
        private List<BigInteger[]> matrix;
        private List<int> rowSwaps;
        private BigInteger mod;

        public BigInteger[] Solve(List<BigInteger[]> matrix, BigInteger mod)
        {
            this.matrix = matrix;
            this.mod = mod;
            size = matrix.Count;
            this.rowSwaps = new List<int>();
            for (int i = 0; i < matrix.Count; i++)
                rowSwaps[i] = i;

            //make lower triangular matrix:
            for (int x = 0; x < size; x++)
            {
                if ((matrix[x][x] % mod) == 0)
                {
                    int y = x + 1;
                    while (y < size && (matrix[x][y] % mod) == 0)
                        y++;
                    if (y == size)
                    {
                        throw new LinearDependentException(rowSwaps[size-1]);
                    }
                    SwapRows(x, y);
                }

                BigInteger matrixXXinverse;

                try
                {
                    matrixXXinverse = BigIntegerHelper.ModInverse(matrix[x][x], mod);
                }
                catch (ArithmeticException)
                {                    
                    throw new NotInvertibleException(matrix[x][x] % mod);
                }

                for (int y = x + 1; y < size; y++)
                {
                    if ((matrix[y][x] % mod) != 0)
                        SubAndMultiplyWithConstantRows(x, y, (matrixXXinverse * matrix[y][x]) % mod);
                    Debug.Assert(matrix[y][x] == 0);
                }
            }

            //make upper triangular matrix:
            for (int x = (size - 1); x >= 0; x--)
            {
                BigInteger matrixXXinverse;

                try
                {
                    matrixXXinverse = BigIntegerHelper.ModInverse(matrix[x][x], mod);
                }
                catch (ArithmeticException)
                {
                    throw new NotInvertibleException(matrix[x][x] % mod);
                }

                for (int y = x - 1; y >= 0; y--)
                {
                    if ((matrix[y][x] % mod) != 0)
                        SubAndMultiplyWithConstantRows(x, y, (matrixXXinverse * matrix[y][x]) % mod);
                    Debug.Assert(matrix[y][x] == 0);
                }
            }
            
            //get solution:
            BigInteger[] sol = new BigInteger[size];
            for (int x = 0; x < size; x++)
            {
                BigInteger matrixXXinverse = BigIntegerHelper.ModInverse(matrix[x][x], mod);
                sol[x] = (matrixXXinverse * matrix[x][size]) % mod;
            }
            return sol;
        }

        private void SwapRows(int index1, int index2)
        {
            BigInteger[] tmp = matrix[index1];
            matrix[index1] = matrix[index2];
            matrix[index2] = tmp;

            int tmp2 = rowSwaps[index1];
            rowSwaps[index1] = rowSwaps[index2];
            rowSwaps[index2] = tmp2;
        }

        private void SubAndMultiplyWithConstantRows(int srcIndex, int destIndex, BigInteger constant)
        {
            //matrix[destIndex] -= constant * matrix[srcIndex]

            for (int i = 0; i < size + 1; i++)
            {
                matrix[destIndex][i] -= (constant * matrix[srcIndex][i]) % mod;
                while (matrix[destIndex][i] < 0)
                    matrix[destIndex][i] += mod;
                matrix[destIndex][i] %= mod;
            }
        }
    }
}
