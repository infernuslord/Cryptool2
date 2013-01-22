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
using Primes.Library.Function;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public class PiX : BaseNTFunction
    {
        object lockobj = new object();
        public PiX() : base() { }

        #region Calculating

        protected override void DoExecute()
        {
            FireOnStart();

            FunctionPiX pix = new FunctionPiX();
            PrimesBigInteger from = m_From;
            PrimesBigInteger _counterTmp = PrimesBigInteger.Two;

            while (_counterTmp.CompareTo(from) <= 0)
            {
                double result = pix.Execute(_counterTmp.DoubleValue);
                FireOnMessage(this, from, StringFormat.FormatDoubleToIntString(result));
                _counterTmp = _counterTmp.Add(PrimesBigInteger.One);
            }

            while (from.CompareTo(m_To) <= 0)
            {
                double result = pix.Execute(from.DoubleValue);
                FireOnMessage(this, from, StringFormat.FormatDoubleToIntString(result));
                from = from.Add(PrimesBigInteger.One);
            }

            FireOnStop();
        }

        public override string Description
        {
            get
            {
                return m_ResourceManager.GetString(BaseNTFunction.pix);
            }
        }

        #endregion
    }
}