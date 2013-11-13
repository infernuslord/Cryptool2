using System;
using System.Collections.Generic;
using System.Numerics;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using NTL;

namespace LatticeCrypto.Models
{
    public class LatticeND
    {
        public VectorND[] Vectors { get; set; }
        public VectorND[] ReducedVectors { get; set; }
        public bool UseRowVectors { get; set; }
        public VectorND[] TransVectors { get; set; }
        public int Dim { get; private set; }
        public BigInteger Determinant { get; set; }
        public double AngleBasisVectors { get; set; }
        public double AngleReducedVectors { get; set; }
        public double Density { get; set; }
        public double DensityRelToOptimum { get; set; }
        private static readonly double optimalDensity = Math.PI / Math.Sqrt(12);

        public LatticeND()
        {}

        public LatticeND(int dim, bool transpose)
        {
            Vectors = new VectorND[dim];
            ReducedVectors = new VectorND[dim];
            Dim = dim;
            UseRowVectors = transpose;
        }

        public LatticeND(VectorND[] vectors, bool useRowVectors)
        {
            Vectors = vectors;
            UseRowVectors = useRowVectors;
            Dim = vectors.Length;
            ReducedVectors = new VectorND[Dim];
            Determinant = CalculateDeterminant(Vectors);
            if (Vectors.Length == 2)
                AngleBasisVectors = Vectors[0].AngleBetween(Vectors[1]);
        }

        public void GenerateRandomVectors(bool checkForGoodBasis, BigInteger codomainStart, BigInteger codomainEnd)
        {
            VectorND[] newVectorNds = new VectorND[Dim];
            BigInteger det;
            int counter = 0;
            while (true)
            {
                for (int i = 0; i < Dim; i++)
                {
                    BigInteger[] vector = new BigInteger[Dim];
                    newVectorNds[i] = new VectorND(Dim);

                    for (int j = 0; j < Dim; j++)
                        vector[j] = Util.ComputeRandomBigInt(codomainStart, codomainEnd);

                    for (int k = 0; k < vector.Length; k++ )
                        newVectorNds[i].values[k] = vector[k];
                }

                det = CalculateDeterminant(newVectorNds);
                if (det != 0 && (!checkForGoodBasis || IsGoodBasis(newVectorNds)))
                    break;
                counter++;
                if (counter > 1024)
                    throw new Exception(Languages.errorFailedToGenerateLattice);
            }

            Vectors = (VectorND[])newVectorNds.Clone();
            Determinant = det;
            if (Vectors.Length == 2)
                AngleBasisVectors = Vectors[0].AngleBetween(Vectors[1]);
        }

        public string LatticeToString()
        {
            string lattice = "";
            for (int i = 0; i < Dim; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < Dim; j++)
                {
                    lattice += Vectors[i].values[j];
                    if (j < Dim - 1)
                        lattice += FormatSettings.CoordinateSeparator;
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < Dim - 1)
                    lattice += FormatSettings.VectorSeparator;
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public string LatticeReducedToString()
        {
            string lattice = "";
            for (int i = 0; i < Dim; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < Dim; j++)
                {
                    lattice += ReducedVectors[i].values[j];
                    if (j < Dim - 1)
                        lattice += FormatSettings.CoordinateSeparator;
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < Dim - 1)
                    lattice += FormatSettings.VectorSeparator;
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public string LatticeTransformationToString()
        {
            string lattice = "";
            for (int i = 0; i < Dim; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < Dim; j++)
                {
                    lattice += TransVectors[i].values[j];
                    if (j < Dim - 1)
                        lattice += FormatSettings.CoordinateSeparator;
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < Dim - 1)
                    lattice += FormatSettings.VectorSeparator;
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public string VectorLengthToString()
        {
            string lengths = "";
            for (int i = 0; i < Dim; i++)
            {
                lengths += FormatSettings.VectorTagOpen;
                BigInteger length = 0;
                for (int j = 0; j < Dim; j++)
                    length += BigInteger.Pow(Vectors[i].values[j], 2);
                lengths += string.Format("{0:f}", Math.Sqrt((double)length));
                lengths += FormatSettings.VectorTagClosed;
                if (i < Dim - 1)
                    lengths += FormatSettings.VectorSeparator;
            }
            return FormatSettings.LatticeTagOpen + lengths + FormatSettings.LatticeTagClosed;
        }

        public string VectorReducedLengthToString()
        {
            string lengths = "";
            for (int i = 0; i < Dim; i++)
            {
                lengths += FormatSettings.VectorTagOpen;
                BigInteger length = 0;
                for (int j = 0; j < Dim; j++)
                    length += BigInteger.Pow(ReducedVectors[i].values[j], 2);
                lengths += string.Format("{0:f}", Math.Sqrt((double)length));
                lengths += FormatSettings.VectorTagClosed;
                if (i < Dim - 1)
                    lengths += FormatSettings.VectorSeparator;
            }
            return FormatSettings.LatticeTagOpen + lengths + FormatSettings.LatticeTagClosed;
        }

        public BigInteger CalculateDeterminant(VectorND[] newVectorNds)
        {
            BigInteger[,] basisArray = new BigInteger[Dim, Dim];
            
            for (int i = 0; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                    basisArray[i, j] = newVectorNds[i].values[j];

            BigInteger det;
            using (var nativeObject = new NTL_Wrapper())
            {
                det = nativeObject.Determinant(basisArray, Dim);
            }
            //Absolut, da Gitterdeterminanten immer positiv sind
            return BigInteger.Abs(det);
        }

        public void LLLReduce()
        {
            BigInteger[,] basisArray = new BigInteger[Dim, Dim];
            BigInteger[,] transArray;
            
            for (int i = 0; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                    basisArray[i, j] = Vectors[i].values[j];

            using (var nativeObject = new NTL_Wrapper())
            {
                transArray = nativeObject.LLLReduce(basisArray, Dim, 0.75);
            }

            ReducedVectors = new VectorND[Dim];
            for (int i = 0; i < Dim; i++)
            {
                ReducedVectors[i] = new VectorND(Dim);
                for (int j = 0; j < Dim; j++)
                    ReducedVectors[i].values[j] = basisArray[i, j];
            }
            TransVectors = new VectorND[Dim];
            for (int i = 0; i < Dim; i++)
            {
                TransVectors[i] = new VectorND(Dim);
                for (int j = 0; j < Dim; j++)
                    TransVectors[i].values[j] = transArray[i, j];
            }
            AngleReducedVectors = ReducedVectors[0].AngleBetween(ReducedVectors[1]);
        }

        public void GaussianReduce()
        {
            VectorND v1 = Vectors[0];
            VectorND v2 = Vectors[1];

            BigInteger rem;
            do
            {
                if (v1.Length > v2.Length)
                    Util.Swap(ref v1, ref v2);
                BigInteger t = BigInteger.DivRem(v1 * v2, v1.LengthSquared, out rem);
                //Bei der Division von BigIntegers muss noch auf korrektes Runden geprüft werden
                if (BigInteger.Abs(rem) > v1.LengthSquared/2)
                    t += rem.Sign;
                v2 = v2 - v1*t;
            } while (v1.Length > v2.Length);

            //Damit ein spitzer Winkel entsteht
            if (Settings.Default.forceAcuteAngle)
            {
                BigInteger.DivRem(v1 * v2, v1.LengthSquared, out rem);
                if (rem.Sign == -1)
                    v2 = v2 * -1;
            }

            ReducedVectors[0] = v1;
            ReducedVectors[1] = v2;
            AngleReducedVectors = ReducedVectors[0].AngleBetween(ReducedVectors[1]);
            Density = Math.PI * Math.Pow(ReducedVectors[0].Length / 2, 2) / (double)Determinant;
            DensityRelToOptimum = Density / optimalDensity;
        }

        private static bool IsGoodBasis(IList<VectorND> vectors)
        {
            if (vectors.Count != 2) return true;
            //Entscheidung, ob eine Basis gut aussieht, über Winkel (im Bogenmaß) und über die Längen
            if ((vectors[0].Length > vectors[1].Length && vectors[0].Length > 1000 * vectors[1].Length)
                || vectors[1].Length > vectors[0].Length && vectors[1].Length > 1000 * vectors[0].Length)
                return false;
            return vectors[0].AngleBetween(vectors[1]) > 5;
        }

        public void Transpose()
        {
            for (int i = 1; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                    if (i > j)
                        Util.Swap(ref Vectors[i].values[j], ref Vectors[j].values[i]);
        }

        public override bool Equals(object obj)
        {
            for (int i = 0; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                    if (Vectors[i].values[j] != ((LatticeND)obj).Vectors[i].values[j])
                        return false;
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                    hash ^= Vectors[i].values[j].GetHashCode();
            return hash;
        }
    }
}
