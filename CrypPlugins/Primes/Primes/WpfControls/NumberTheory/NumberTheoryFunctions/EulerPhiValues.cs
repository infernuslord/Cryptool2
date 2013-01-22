/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Text;
using Primes.WpfControls.Components;
using System.Windows.Controls;
using System.Threading;
using Primes.Bignum;
using Primes.Library;
using System.Windows;
using System.Diagnostics;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class EulerPhiValues : BaseNTFunction
    {
        object lockobj = new object();

        public EulerPhiValues() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            PrimesBigInteger from = m_From;

            while (from.CompareTo(m_To) <= 0)
            {
                StringBuilder sbMessage = new StringBuilder("[");
                PrimesBigInteger d = PrimesBigInteger.One;
                while (d.CompareTo(from) < 0)
                {
                    if (PrimesBigInteger.GCD(d, from).Equals(PrimesBigInteger.One))
                    {
                        if (sbMessage.Length > 1)
                            sbMessage.Append(", ");
                        sbMessage.Append(d.ToString());
                        FireOnMessage(this, from, sbMessage.ToString());
                    }
                    d = d.Add(PrimesBigInteger.One);
                }
                sbMessage.Append("]");
                FireOnMessage(this, from, sbMessage.ToString());

                from = from.Add(PrimesBigInteger.One);
            }

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.eulerphivalues);
            }
        }

        #endregion
    }
}
