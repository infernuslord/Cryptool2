/* HOWTO: Set year, author name and organization.
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

            if (strX == strY) {
                return 0;
            } else if (strX == null) {
                return -1;
            } else if (strY == null) {
                return 1;
            } else {

                if (strX.Length == 0) {
                    return -1;
                } else if (strY.Length == 0) {
                    return 1;
                } else {

                    //both values contain strings

                    int result = 0;
                    int length = (strX.Length < strY.Length) ? strX.Length : strY.Length;

                    for (int charIndex = 0; (charIndex < length) && (result == 0); charIndex++) {
                        //compare character until a difference is found
                        int indexX = alphabet.IndexOf(strX[charIndex]);
                        if (indexX < 0) {
                            //character not found on alphabet - try to find lowerase
                            indexX = alphabet.ToLower().IndexOf(strX.ToLower()[charIndex]);
                        }

                        int indexY = alphabet.IndexOf(strY[charIndex]);
                        if (indexY < 0) {
                            //character not found on alphabet - try to find lowerase
                            indexY = alphabet.ToLower().IndexOf(strY.ToLower()[charIndex]);
                        }

                        result = indexX.CompareTo(indexY);
                    }

                    if (result == 0) {
                        //no difference found - compare length
                        if (strX.Length < strY.Length) {
                            result = -1;
                        } else if (strY.Length < strX.Length) {
                            result = 1;
                        }
                    }

                    return result;
                }
            }
        }
    }
}
