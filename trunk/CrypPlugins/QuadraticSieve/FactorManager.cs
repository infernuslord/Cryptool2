/*                              
   Copyright 2010 Sven Rech, Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Numerics;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Cryptool.Plugins.QuadraticSieve
{
    class FactorManager
    {
        private List<BigInteger> primeFactors = new List<BigInteger>();
        private List<BigInteger> compositeFactors = new List<BigInteger>();
        private MethodInfo getPrimeFactorsMethod;
        private MethodInfo getCompositeFactorsMethod;

        public delegate void FactorsChangedHandler(List<BigInteger> primeFactors, List<BigInteger> compositeFactors);
        public event FactorsChangedHandler FactorsChanged;

        public FactorManager(MethodInfo getPrimeFactors, MethodInfo getCompositeFactors)
        {
            this.getPrimeFactorsMethod = getPrimeFactors;
            this.getCompositeFactorsMethod = getCompositeFactors;
        }

        #region public

        /// <summary>
        /// For debug purposes: Calculates the number from the factors.
        /// </summary>
        public BigInteger CalculateNumber()
        {
            BigInteger n = 1;
            foreach (BigInteger p in primeFactors)
                n *= p;
            foreach (BigInteger p in compositeFactors)
                n *= p;
            return n;
        }

        /// <summary>
        /// adds the factors from the msieve internal factorList to the factor lists of the FactorManager.
        /// </summary>
        public void AddFactors(IntPtr factorList)
        {
            AddFactorsWithoutFiringEvent(factorList);
            FactorsChanged(primeFactors, compositeFactors);
        }

        /// <summary>
        /// replaces the composite factor "composite" by the factors of factorList.
        /// of course, factorList has to multiply up to composite.
        /// </summary>
        public void ReplaceCompositeByFactors(BigInteger composite, IntPtr factorList)
        {
            int amount = compositeFactors.Count(c => (c == composite));
            for (int i = 0; i < amount; i++)
                AddFactorsWithoutFiringEvent(factorList);
            int amount2 = compositeFactors.RemoveAll(c => (c == composite));

            //Some debug stuff:
            Debug.Assert(amount == amount2);
            FactorManager debugFactorManager = new FactorManager(getPrimeFactorsMethod, getCompositeFactorsMethod);
            debugFactorManager.AddFactorsWithoutFiringEvent(factorList);
            Debug.Assert(debugFactorManager.CalculateNumber() == composite);

            FactorsChanged(primeFactors, compositeFactors);
        }

        /// <summary>
        /// Returns a single composite factor (or 0, if no composite factors are left).
        /// </summary>
        public BigInteger GetCompositeFactor()
        {
            if (compositeFactors.Count == 0)
                return 0;
            return compositeFactors[0];
        }

        /// <summary>
        /// Returns if we only have prime factors. True means, that factorizing is finished.
        /// </summary>
        public bool OnlyPrimes()
        {
            return (compositeFactors.Count == 0);
        }

        public BigInteger[] getPrimeFactors()
        {
            return primeFactors.ToArray();
        }

        #endregion

        #region private

        /// <summary>
        /// See AddFactors.
        /// </summary>
        private void AddFactorsWithoutFiringEvent(IntPtr factorList)
        {
            ArrayList pf = (ArrayList)(getPrimeFactorsMethod.Invoke(null, new object[] { factorList }));
            foreach (Object o in pf)
                primeFactors.Add(BigInteger.Parse((string)o));

            ArrayList cf = (ArrayList)(getCompositeFactorsMethod.Invoke(null, new object[] { factorList }));
            foreach (Object o in cf)
                compositeFactors.Add(BigInteger.Parse((string)o));
        }

        #endregion
    }
}