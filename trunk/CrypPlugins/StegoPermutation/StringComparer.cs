/* 
   Copyright 2011 CrypTool 2 Team

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

#region Using directives

using System;
using System.Collections;
using System.Text;

#endregion

namespace Cryptool.Plugins.StegoPermutation
{
    /// <summary>Compares Strings according to a custom alphabet.</summary>
    public class StringComparer : IComparer {

        private String alphabet;

        internal StringComparer(string alphabet)
        {
            this.alphabet = alphabet;
        }
		
		int IComparer.Compare(object x, object y)
		{
			return DoCompare(x, y);
		}

        public int DoCompare(object x, object y)
		{
            String strX = x as String;
            String strY = y as String;

            if (strX == strY) return 0;
            if (strX == null) return -1;
            if (strY == null) return 1;

            if (strX.Length == 0) return -1;
            if (strY.Length == 0) return 1;

            //both values contain strings

            int length = (strX.Length < strY.Length) ? strX.Length : strY.Length;

            for (int i = 0; i < length; i++)
            {
                //compare character until a difference is found
                int indexX = alphabet.IndexOf(strX[i]);
                int indexY = alphabet.IndexOf(strY[i]);

                int result = (indexX < 0)
                    ? ((indexY < 0) ? String.CompareOrdinal(strX, i, strY, i, 1) : 1)
                    : ((indexY < 0) ? -1 : indexX.CompareTo(indexY));

                if (result != 0) return result;
            }

            //no difference found - compare lengths
            return strX.Length.CompareTo(strY.Length);
        }
    }
}
