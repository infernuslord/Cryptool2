using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace DiscreteLogarithm
{
    class FactorBase
    {
        private List<int> primes;

        /**
         * Generates the factor base.
         **/
        public void Generate(int max)
        {
            primes = new List<int>();

            /** Implementation of sieve of Eratosthenes **/
            bool[] sieve = new bool[max];
            for (int c = 2; c < max; c++)
                sieve[c] = true;

            for (int c = 2; c < max; c++)
            {
                if (sieve[c])
                    primes.Add(c);
                for (int i = c + 1; i < max; i++)
                {
                    if (i % c == 0)
                        sieve[i] = false;
                }
            }
        }

        /**
         * Tries to factor parameter "number" with the factor base and returns an array giving the relations.
         **/
        public int[] Factorize(BigInteger number)
        {
            int[] result = new int[primes.Count];
            for (int c = 0; c < primes.Count; c++)
            {
                while (number % primes[c] == 0)
                {
                    result[c]++;
                    number /= primes[c];
                }
            }

            if (number != 1)
                return null;

            return result;
        }

    }
}
