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
using Primes.Bignum;
using Primes.Library;
using System.Windows;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class TauValues : BaseNTFunction
    {
        public TauValues()
            : base()
        {
        }

        protected override void DoExecute()
        {
            FireOnStart();

            PrimesBigInteger from = m_From;
            while (from.CompareTo(m_To) <= 0)
            {
                StringBuilder sbMessage = new StringBuilder("[");

                PrimesBigInteger d = PrimesBigInteger.One;

                while (d.Multiply(PrimesBigInteger.Two).CompareTo(from) <= 0)
                {
                    if (from.Mod(d).Equals(PrimesBigInteger.Zero))
                    {
                        if (sbMessage.Length > 1)
                            sbMessage.Append(", ");
                        sbMessage.Append(d.ToString());

                        FireOnMessage(this, from, sbMessage.ToString());
                    }
                    d = d.Add(PrimesBigInteger.One);
                }
                sbMessage.Append(", " + from.ToString() + "]");
                FireOnMessage(this, from, sbMessage.ToString());
                from = from.Add(PrimesBigInteger.One);
            }

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.tauvalues);
            }
        }
    }
}
