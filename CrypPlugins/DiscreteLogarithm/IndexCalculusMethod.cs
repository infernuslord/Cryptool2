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
                    BigInteger[] coefficients = factorBase.Factorize(BigInteger.ModPow(logbase, r, mod));
                    if (coefficients != null)
                        linearSystem.AddEquation(coefficients, r);

                    r++;
                    if (r > mod - 2)
                        throw new Exception("Not enough relations!");
                }

                logs = linearSystem.Solve();
            } while (logs == null);

            //TODO: Implement second part of algorithm

            return 0;
        }

        private int GetSmoothnessBound(BigInteger mod)
        {
            double logm = BigInteger.Log(mod);
            int B = (int)(Math.Exp(Math.Sqrt(logm * Math.Log(logm)))/2);
            return B;
        }
    }
}
