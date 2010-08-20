using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace DiscreteLogarithm
{
    class IndexCalculusMethod
    {
        private FactorBase factorBase;

        public BigInteger DiscreteLog(BigInteger n, BigInteger logbase, BigInteger mod)
        {
            int B = GetSmoothnessBound(mod);
            factorBase = new FactorBase();
            factorBase.Generate(B);

            //First part: Find logarithms of the factorbase elements:

            LinearSystemOfEquations linearSystem = new LinearSystemOfEquations(mod - 1, factorBase.FactorCount());
            BigInteger r = 1;
            BigInteger[] logs = null;

            do
            {
                while (linearSystem.NeedMoreEquations())
                {
                    BigInteger gr = BigInteger.ModPow(logbase, r, mod);
                    if (gr == 1)
                        throw new Exception("Input base not a generator of given residue class");

                    BigInteger[] coefficients = factorBase.Factorize(gr);
                    if (coefficients != null)
                        linearSystem.AddEquation(coefficients, r);

                    r++;
                    if (r > mod - 2)
                        throw new Exception("Index-Calculus step 1 Exception: Not enough relations!");
                }

                logs = linearSystem.Solve();
            } while (logs == null);

            //Second part: Find the discrete logarithm of n:

            r = 0;
            BigInteger[] factorization;
            do
            {
                r++;
                if (r > mod - 2)
                    throw new Exception("Index-Calculus step 2 Exception: Not factorizable!");

                factorization = factorBase.Factorize(BigInteger.ModPow(logbase, r, mod) * n);                
            } while (factorization == null);

            BigInteger result = -r;
            for (int i = 0; i < logs.Length; i++)
                result += (factorization[i] * logs[i]) % (mod-1);

            result %= (mod - 1);
            while (result < 0)
                result += (mod - 1);

            return result;
        }

        private int GetSmoothnessBound(BigInteger mod)
        {
            double logm = BigInteger.Log(mod);
            int B = (int)(Math.Exp(Math.Sqrt(logm * Math.Log(logm)))/2);
            return B;
        }
    }
}
