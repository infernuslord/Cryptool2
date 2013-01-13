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
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Numerics;
using Primes.Bignum;
using Primes.Library;
using Primes.WpfControls.Components;
using Cryptool.PluginBase.Miscellaneous;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class ExtEuclid : BaseNTFunction
    {
        object lockobj = new object();
        public ExtEuclid() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            try
            {
                BigInteger second = BigInteger.Parse(m_SecondParameter.ToString());
                BigInteger from = BigInteger.Parse(m_From.ToString());
                BigInteger to = BigInteger.Parse(m_To.ToString());
                BigInteger a, b;

                for (BigInteger x = from; x <= to; x++)
                {
                    string msg;

                    try
                    {
                        BigInteger gcd = BigIntegerHelper.ExtEuclid(x, second, out a, out b);
                        string sa = a.ToString(); if (a < 0) sa = "(" + sa + ")";
                        string sb = b.ToString(); if (b < 0) sb = "(" + sb + ")";
                        if (b < 0) b = -b;
                        msg = String.Format("{0}*{1} + {2}*{3} = {4}", sa, x, sb, second, gcd );
                    }
                    catch (Exception ex)
                    {
                        msg = "-";
                    }

                    FireOnMessage(this, new PrimesBigInteger(x.ToString()), msg);
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
                return m_ResourceManager.GetString(BaseNTFunction.exteuclid);
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
