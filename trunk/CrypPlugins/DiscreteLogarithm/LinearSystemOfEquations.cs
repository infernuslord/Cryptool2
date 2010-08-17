﻿using System;
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
        private int size;
        private List<BigInteger[]> matrix;

        public LinearSystemOfEquations(BigInteger mod, int size)
        {
            this.mod = mod;
            this.size = size;
            matrix = new List<BigInteger[]>(size);
        }

        public void AddEquation(BigInteger[] coefficients, BigInteger b)
        {
            Debug.Assert(coefficients.Length == size);

            BigInteger[] row = new BigInteger[coefficients.Length + 1];
            for (int c = 0; c < coefficients.Length; c++)
                row[c] = coefficients[c];
            row[row.Length - 1] = b;

            /** It would be better to check first if "row" is linear dependent to the other rows and neglect the adding in this case.
             *  However, checking this is not very efficient. 
             *  So instead we hope that independence is given most of the time, and if not, solving the equation system will reveal this.
             *  TODO: Check if this is a good approach!
             **/
            matrix.Add(row);
        }

        public bool MoreEquations()
        {
            return (matrix.Count < size);
        }

        public BigInteger[] Solve()
        {
            /* Solving a linear equation over a residue class is not so trivial if the modulus is not a prime.
             * This is because residue classes with a composite modulus is not a field, which means that not all elements
             * of this ring do have an inverse.
             * We cope with this problem by factorizing the modulus in its prime factors and solving gauss over them
             * separately. We can use the chinese remainder theorem to get the solution we need then.
             * But what happens if we aren't able to factorize the modulus completely, because this is to inefficient?
             * There is a simple trick to cope with that:
             * Try the gauss algorithm with the composite modulus. Either you have luck and it works out without a problem
             * (in this case we can just go on), or the gauss algorithm will have a problem inverting some number.
             * In the last case, we can search for the gcd of this number and the composite modulus. This gcd is a factor of the modulus,
             * so that solving the equation helped us finding the factorization.
             */

            FiniteFieldGauss gauss = new FiniteFieldGauss();
            HenselLifting hensel = new HenselLifting();

            List<Msieve.Factor> modfactors = Msieve.TrivialFactorization(mod);
            List<BigInteger[]> results;

            bool tryAgain;

            do
            {
                results = new List<BigInteger[]>();
                tryAgain = false;

                for (int i = 0; i < modfactors.Count; i++)
                {
                    if (modfactors[i].prime)    //mod prime
                    {
                        if (modfactors[i].count == 1)
                            results.Add(gauss.Solve(MatrixCopy(), modfactors[i].factor));
                        else
                            results.Add(hensel.Solve(MatrixCopy(), modfactors[i].factor, modfactors[i].count));
                    }
                    else    //mod composite
                    {
                        //Try using gauss:
                        try
                        {
                            BigInteger[] res = gauss.Solve(MatrixCopy(), modfactors[i].factor);
                            results.Add(res);   //Yeah, we had luck :)
                        }
                        catch (NotInvertibleException ex)
                        {
                            //We found a factor of modfactors[i]:
                            BigInteger notInvertible = ex.NotInvertibleNumber;
                            List<Msieve.Factor> morefactors = Msieve.TrivialFactorization(modfactors[i].factor / notInvertible);
                            List<Msieve.Factor> morefactors2 = Msieve.TrivialFactorization(notInvertible);
                            modfactors.RemoveAt(i);
                            ConcatFactorLists(modfactors, morefactors);
                            ConcatFactorLists(modfactors, morefactors2);
                            tryAgain = true;
                            break;
                        }
                    }
                }
            } while (tryAgain);

            //TODO: "glue" the results together

            return null;
        }

        /// <summary>
        /// Creates a deep copy of member variable "matrix"
        /// </summary>
        /// <returns>a matrix copy</returns>
        private List<BigInteger[]> MatrixCopy()
        {
            List<BigInteger[]> res = new List<BigInteger[]>(matrix.Count);
            foreach (BigInteger[] row in matrix)
            {
                BigInteger[] resRow = new BigInteger[row.Length];
                for (int i = 0; i < row.Length; i++)
                    resRow[i] = row[i];
                res.Add(resRow);
            }
            return res;
        }

        private void ConcatFactorLists(List<Msieve.Factor> list1, List<Msieve.Factor> list2)
        {
            foreach (Msieve.Factor f in list2)
                list1.Add(f);
        }


        /**
         * For debugging only
         **/
        internal void PrintMatrix()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size + 1; y++)
                {
                    Console.Out.Write(matrix[x][y] + "\t");
                }
                Console.Out.WriteLine("");
            }
            Console.Out.WriteLine("");
        }

        /*
        public static void Main()
        {
            LinearSystemOfEquations l = new LinearSystemOfEquations(7, 3);
            l.AddEquation(new int[] { 1, 2, 3 }, 4);
            l.AddEquation(new int[] { 2, 3, 4 }, 5);
            l.AddEquation(new int[] { 5, 4, 5 }, 6);

            BigInteger[] sol = l.Solve();

            foreach (int i in sol)
                Console.Out.WriteLine(i);

            Console.In.ReadLine();
        }
         */

    }
}
