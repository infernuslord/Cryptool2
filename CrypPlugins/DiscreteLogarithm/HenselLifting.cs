using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace DiscreteLogarithm
{
    class HenselLifting
    {
        private FiniteFieldGauss gauss = new FiniteFieldGauss();

        /// <summary>
        /// Hensel-lifting is used when it is necessary to solve a matrix modulo p^r
        /// where p is a prime and r an integer > 1.
        /// </summary>
        /// <param name="matrix">The matrix to solve</param>
        /// <param name="mod">the prime which the modulo consists of</param>
        /// <param name="exp">the exponent for mod</param>
        /// <returns>solution of "matrix" modulo mod^exp </returns>
        public BigInteger[] Solve(List<BigInteger[]> matrix, BigInteger mod, int exp)
        {
            List<List<BigInteger[]>> As = new List<List<BigInteger[]>>(exp+1);
            List<BigInteger[]> bs = new List<BigInteger[]>(exp + 1);
            List<BigInteger[]> xs = new List<BigInteger[]>(exp + 1);            
            
            //create As:
            As.Add(matrix);
            for (int i = 0; i < (exp-1); i++)
            {
                As.Add(SplitMatrix(As[i], BigInteger.Pow(mod, exp-i-1)));
            }
            As.Reverse();

            //create bs:            
            for (int i = 0; i < exp; i++)
            {
                bs.Add(GetLastColumnFromMatrix(As[i]));
            }

            //find xs:
            List<BigInteger[]> A0 = As[0];
            BigInteger[] b = bs[0];
            for (int i = 0; i < exp-1; i++)
            {
                xs.Add(gauss.Solve(CreateAb(A0, b), mod));
                
                BigInteger[] q = DivVector(SubVectors(MultMatrixWithVector(A0, xs[i]), b), mod);
                b = SubVectors(bs[i + 1], q);
                for (int c = 0; c <= i; c++)
                {
                    b = SubVectors(b, MultMatrixWithVector(As[c + 1], xs[i-c]));
                }
            }            
            xs.Add(gauss.Solve(CreateAb(A0, b), mod));

            //glue xs together:
            BigInteger[] x = new BigInteger[xs[0].Length];
            for (int i = 0; i < exp; i++)
            {
                BigInteger p = BigInteger.Pow(mod, i);
                for (int y = 0; y < x.Length; y++)
                    x[y] += xs[i][y] * p;
            }

            return x;
        }

        private BigInteger[] MultMatrixWithVector(List<BigInteger[]> matrix, BigInteger[] vector)
        {
            BigInteger[] res = new BigInteger[matrix.Count];
            for (int y = 0; y < matrix.Count; y++)
            {
                res[y] = 0;
                for (int x = 0; x < vector.Length; x++)
                {
                    res[y] += matrix[y][x] * vector[x];
                }
            }
            return res;
        }

        private BigInteger[] SubVectors(BigInteger[] vector1, BigInteger[] vector2)
        {
            BigInteger[] res = new BigInteger[vector1.Length];
            for (int y = 0; y < vector1.Length; y++)
                res[y] = vector1[y] - vector2[y];
            return res;
        }

        private BigInteger[] ModVector(BigInteger[] vector, BigInteger mod)
        {
            BigInteger[] res = new BigInteger[vector.Length];
            for (int y = 0; y < vector.Length; y++)
            {
                res[y] = vector[y] % mod;
            }
            return res;
        }

        private BigInteger[] DivVector(BigInteger[] vector, BigInteger mod)
        {
            BigInteger[] res = new BigInteger[vector.Length];
            for (int y = 0; y < vector.Length; y++)
            {
                res[y] = vector[y] / mod;
            }
            return res;
        }


        /// <summary>
        /// Creates a matrix with values of parameter "A" and the last column with values of parameter "b"
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <returns>the created matrix</returns>
        private List<BigInteger[]> CreateAb(List<BigInteger[]> A, BigInteger[] b)
        {
            List<BigInteger[]> result = new List<BigInteger[]>();
            for (int y = 0; y < A.Count; y++)
            {
                BigInteger[] row = new BigInteger[A[y].Length];
                for (int x = 0; x < A[y].Length - 1; x++)
                {
                    row[x] = A[y][x];
                }
                row[A[y].Length - 1] = b[y];
                result.Add(row);
            }
            return result;
        }

        private BigInteger[] GetLastColumnFromMatrix(List<BigInteger[]> matrix)
        {
            BigInteger[] res = new BigInteger[matrix.Count];
            for (int y = 0; y < matrix.Count; y++)
            {
                res[y] = matrix[y][matrix[y].Length - 1];
            }
            return res;
        }

        private List<BigInteger[]> SplitMatrix(List<BigInteger[]> matrix, BigInteger modulo)
        {
            List<BigInteger[]> res = new List<BigInteger[]>();
            for (int y = 0; y < matrix.Count; y++)
            {
                BigInteger[] row = new BigInteger[matrix[y].Length];
                for (int x = 0; x < matrix[y].Length; x++)
                {
                    row[x] = matrix[y][x] % modulo;
                    matrix[y][x] /= modulo;
                }
                res.Add(row);
            }
            return res;
        }
    }
}
