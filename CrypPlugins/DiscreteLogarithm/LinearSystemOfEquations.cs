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
            if (!NeedMoreEquations())
                return;

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

        public bool NeedMoreEquations()
        {
            return (matrix.Count < size);
        }

        public BigInteger[] Solve()
        {
            /* Solving a linear equation over a residue class is not so trivial if the modulus is not a prime.
             * This is because residue classes with a composite modulus is not a field, which means that not all elements
             * of this ring do have an inverse.
             * We cope with this problem by factorizing the modulus in its prime factors and solving gauss over them
             * separately (in the case of p^q (q>1) by using "hensel lifting").
             * We can use the chinese remainder theorem to get the solution we need then.
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
            List<KeyValuePair<BigInteger[], Msieve.Factor>> results;   //Stores the partial solutions together with their factors

            bool tryAgain;

            try
            {
            
                do
                {
                    results = new List<KeyValuePair<BigInteger[], Msieve.Factor>>();
                    tryAgain = false;

                    for (int i = 0; i < modfactors.Count; i++)
                    {
                        if (modfactors[i].prime)    //mod prime
                        {
                            if (modfactors[i].count == 1)
                                results.Add(new KeyValuePair<BigInteger[], Msieve.Factor>(gauss.Solve(MatrixCopy(), modfactors[i].factor), modfactors[i]));
                            else
                                results.Add(new KeyValuePair<BigInteger[], Msieve.Factor>(hensel.Solve(MatrixCopy(), modfactors[i].factor, modfactors[i].count), modfactors[i]));
                        }
                        else    //mod composite
                        {
                            //Try using gauss:
                            try
                            {
                                BigInteger[] res = gauss.Solve(MatrixCopy(), modfactors[i].factor);
                                results.Add(new KeyValuePair<BigInteger[], Msieve.Factor>(res, modfactors[i]));   //Yeah, we had luck :)
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
            
            }
            catch (LinearDependentException ex)
            {
                //We have to throw away one row and try again later:
                matrix.RemoveAt(ex.RowToDelete);
                return null;
            }

            BigInteger[] result = new BigInteger[size];
            //"glue" the results together:
            for (int i = 0; i < size; i++)
            {
                List<KeyValuePair<BigInteger, BigInteger>> partSolItem = new List<KeyValuePair<BigInteger,BigInteger>>();
                for (int c = 0; c < results.Count; c++)
                {
                    partSolItem.Add(new KeyValuePair<BigInteger, BigInteger>(results[c].Key[i], BigInteger.Pow(results[c].Value.factor, results[c].Value.count)));
                }
                result[i] = CRT(partSolItem);
            }

            return result;
        }

        /// <summary>
        /// Implementation of solutionfinding for chinese remainder theorem.
        /// i.e. finding an x that fullfills
        /// x = a1 (mod m1)
        /// x = a2 (mod m2)
        /// ...
        /// </summary>
        /// <param name="congruences">The congruences (a_i, m_i)</param>
        /// <returns>the value that fits into all congruences</returns>
        private BigInteger CRT(List<KeyValuePair<BigInteger, BigInteger>> congruences)
        {
            BigInteger x = 0;
            for (int i = 0; i < congruences.Count; i++)
            {
                BigInteger k = 1;
                for (int c = 0; c < congruences.Count; c++)
                    if (c != i)
                        k *= congruences[c].Value;
                BigInteger r = BigIntegerHelper.ModInverse(k, congruences[i].Value);

                x += congruences[i].Key * r * k;
            }

            return x;
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


        /*public static void test()
        {
            LinearSystemOfEquations l = new LinearSystemOfEquations(2*2*2*2*3*3, 3);
            l.AddEquation(new BigInteger[] { 2, 4, 1 }, 2);
            l.AddEquation(new BigInteger[] { 4, 3, 1 }, 0);
            l.AddEquation(new BigInteger[] { 5, 1, 0 }, 2);

            BigInteger[] sol = l.Solve();

            int a = 4;      //Set breackpoint here
        }*/

    }
}
