/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Factorizer
{
    public class FactorizerSettings : ISettings
    {

        private const int BRUTEFORCEMIN = 100;
        private const int BRUTEFORCEMAX = 10000000;

        private long m_BruteForceLimit = 100000;

        [TaskPane("BruteForceLimitCaption", "BruteForceLimitTooltip", "BruteForceLimitGroup", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, BRUTEFORCEMIN, BRUTEFORCEMAX)]
        public long BruteForceLimit
        {
            get { return m_BruteForceLimit; }
            set
            {
                m_BruteForceLimit = Math.Max(BRUTEFORCEMIN, value);
                m_BruteForceLimit = Math.Min(BRUTEFORCEMAX, value);
                FirePropertyChangedEvent("BruteForceLimit");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChangedEvent(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    }
}
