using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class ModMatrix
    {
        private int dimension;
        private BigInteger modulus;
        private BigInteger[,] m;

        public ModMatrix(int d, BigInteger mod)
        {
            dimension = d;
            modulus = mod;
            m = new BigInteger[dimension, dimension];
            UnitMatrix();
        }

        public ModMatrix(ModMatrix mat)
        {
            dimension = mat.Dimension;
            modulus = mat.Modulus;
            m = new BigInteger[dimension, dimension];

            for (int y = 0; y < dimension; y++)
                for (int x = 0; x < dimension; x++)
                    m[x, y] = mat[x, y];
        }
        
        public void setElements(string values, string alphabet)
        {
            for (int y = 0; y < dimension; y++)
                for (int x = 0; x < dimension; x++)
                    m[x, y] = alphabet.IndexOf(values[x * dimension + y]);
        }

        public int Dimension
        {
            get { return dimension; }
        }

        public BigInteger Modulus
        {
            get { return modulus; }
        }

        public BigInteger this[int x, int y]
        {
            get
            {
                return m[x, y];
            }
            set
            {
                m[x, y] = value % modulus;
            }
        }

        public void UnitMatrix()
        {
            for (int i = 0; i < dimension; i++)
                for (int j = 0; j < dimension; j++)
                    m[i, j] = (i == j) ? 1 : 0;
        }

        public ModMatrix invert()
        {
            ModMatrix mm = new ModMatrix(this);
            ModMatrix mi = new ModMatrix(dimension, modulus);

            BigInteger g = 0, tmp, inv;

            for (int y = 0; y < dimension; y++)
            {
                // find row with invertible leading value
                int yy;
                for (yy = y; yy < dimension; yy++)
                {
                    g = mm[y, yy].GCD(modulus);
                    if (g == 1) break;
                }
                if (g != 1) return null;

                // swap rows y and yy
                for (int x = 0; x < dimension; x++)
                {
                    tmp = mm[x, y]; mm[x, y] = mm[x, yy]; mm[x, yy] = tmp;
                    tmp = mi[x, y]; mi[x, y] = mi[x, yy]; mi[x, yy] = tmp;
                }

                // normalize rows
                inv = BigIntegerHelper.ModInverse(mm[y, y], modulus);
                for (int x = 0; x < dimension; x++)
                {
                    mm[x, y] = (mm[x, y] * inv) % modulus;
                    mi[x, y] = (mi[x, y] * inv) % modulus;
                }

                for (yy = 0; yy < dimension; yy++)
                {
                    if (yy == y) continue;
                    tmp = (modulus - mm[y, yy]) % modulus;

                    for (int x = 0; x < dimension; x++)
                    {
                        mm[x, yy] = (mm[x, yy] + tmp * mm[x, y]) % modulus;
                        mi[x, yy] = (mi[x, yy] + tmp * mi[x, y]) % modulus;
                    }
                }
            }

            return mi;
        }

        public static implicit operator string(ModMatrix mat)
        {
            List<BigInteger> lst = new List<BigInteger>();
            string s = "";

            for (int y = 0; y < mat.Dimension; y++)
            {
                lst.Clear();
                for (int x = 0; x < mat.Dimension; x++)
                    lst.Add(mat[x, y]);
                s += "[" + String.Join(",",lst) + "]";
            }

            return "[" + s + "] (modulus=" + mat.Modulus + ")";
        }

        public static ModMatrix operator *(ModMatrix matA, ModMatrix matB)
        {
            ModMatrix result = new ModMatrix(matA.Dimension, matA.Modulus);

            for (int y = 0; y < result.Dimension; y++)
                for (int x = 0; x < result.Dimension; x++)
                {
                    result[x, y] = 0;
                    for (int i = 0; i < result.Dimension; i++)
                    {
                        result[x, y] += matA[i, y] * matB[x, i];
                        result[x, y] %= result.Modulus;
                    }
                }

            return result;
        }

        public static BigInteger[] operator *(ModMatrix mat, BigInteger[] vector)
        {
            if (mat.Dimension != vector.Length)
                return null;

            BigInteger[] result = new BigInteger[mat.Dimension];

            for (int y = 0; y < mat.Dimension; y++)
            {
                result[y] = 0;
                for (int i = 0; i < mat.Dimension; i++)
                {
                    result[y] += mat[i, y] * vector[i];
                    result[y] %= mat.Modulus;
                }
            }

            return result;
        }

    }
}