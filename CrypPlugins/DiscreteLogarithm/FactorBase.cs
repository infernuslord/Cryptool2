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
         * (Trial Division)
         * Tries to factor parameter "number" with the factor base and returns an array indicating how many times each factor had to be divided.
         * If factorization is not possible, returns null.
         **/
        public BigInteger[] Factorize(BigInteger number)
        {
            BigInteger[] result = new BigInteger[primes.Count];
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

        public int FactorCount()
        {
            return primes.Count;
        }

    }
}
