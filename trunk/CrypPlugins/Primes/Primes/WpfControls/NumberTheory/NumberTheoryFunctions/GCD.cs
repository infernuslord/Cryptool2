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
    public class GCD : BaseNTFunction
    {
        object lockobj = new object();
        public GCD() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            try
            {
                PrimesBigInteger modulus = m_SecondParameter;

                PrimesBigInteger from = m_From;

                while (from.CompareTo(m_To) <= 0)
                {
                    PrimesBigInteger result = PrimesBigInteger.GCD(from, modulus);
                    FireOnMessage(this, from, result.ToString("D"));
                    from = from.Add(PrimesBigInteger.One);
                }
            }
            catch (Exception ex)
            {
            }

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.gcd);
            }
        }

        public override bool NeedsSecondParameter
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}