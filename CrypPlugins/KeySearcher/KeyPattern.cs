/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;
using Cryptool.Plugins.PeerToPeer.Jobs;

namespace KeySearcher
{
    public class KeyPattern
    {       
        private string pattern;
        internal ArrayList wildcardList;

        /// <summary>
        /// Property for the WildCardKey. Could return null, if the KeyPattern isn't initialized correctly.
        /// </summary>
        public string WildcardKey
        {
            get
            {
                return getWildcardKey();
            }
            set
            {
                if (!testWildcardKey(value))
                    throw new Exception("Invalid wildcard key!");
                setWildcardKey(value);
            }
        }

        public KeyPattern(string pattern)
        {
            if (!testPattern(pattern))
                throw new Exception("Invalid pattern!");
            this.pattern = pattern;
        }        

        public KeyPattern[] split()
        {
            KeyPattern[] patterns = new KeyPattern[2];
            for (int i = 0; i < 2; i++)
            {
                patterns[i] = new KeyPattern(pattern);                
                patterns[i].wildcardList = new ArrayList();
            }
            bool s = false;
            for (int i = 0; i < wildcardList.Count; i++)
            {
                Wildcard wc = ((Wildcard)wildcardList[i]);
                if (!s && (wc.size() - wc.count()) > 1)
                {
                    Wildcard[] wcs = wc.split();
                    patterns[0].wildcardList.Add(wcs[0]);
                    patterns[1].wildcardList.Add(wcs[1]);                    
                    s = true;
                }
                else
                {
                    patterns[0].wildcardList.Add(new Wildcard(wc));
                    Wildcard copy = new Wildcard(wc);
                    if (s)
                        copy.resetCounter();
                    patterns[1].wildcardList.Add(copy);
                }
            }
            if (!s)
                return null;
            return patterns;
        }

        public string giveInputPattern()
        {
            string res = "";
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] != '[')
                    res += pattern[i];
                else
                {
                    res += '*';
                    while (pattern[i] != ']')
                        i++;
                }
                i++;
            }
            return res;
        }

        /**
         * tests, if 'pattern' is a valid pattern.
         **/
        public static bool testPattern(string pattern)
        {
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] == '[')
                {
                    i++;
                    while (pattern[i] != ']')
                    {
                        if (specialChar(pattern[i]))
                            return false;
                        if (pattern[i + 1] == '-')
                        {
                            if (specialChar(pattern[i]) || specialChar(pattern[i + 2]))
                                return false;
                            i += 2;
                        }
                        i++;
                    }
                }
                i++;
            }
            return true;
        }

        private static bool specialChar(char p)
        {
            if (p == '-' || p == '[' || p == ']' || p == '*')
                return true;
            return false;
        }

        /**
         * tests, if 'wildcardKey' matches 'pattern'.
         **/
        public static bool testWildcardKey(string wildcardKey, string pattern)
        {
            try
            {
                int kcount = 0;
                int pcount = 0;
                while (kcount < wildcardKey.Length && pcount < pattern.Length)
                {
                    if (pattern[pcount] != '[')
                    {
                        if (pattern[pcount] != wildcardKey[kcount])
                            return false;
                        kcount++;
                        pcount++;
                    }
                    else
                    {
                        Wildcard wc1 = new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount) + 1 - pcount));
                        while (pattern[pcount++] != ']') ;
                        Wildcard wc2 = null;
                        if (wildcardKey[kcount] == '[')
                        {
                            wc2 = new Wildcard(wildcardKey.Substring(kcount, wildcardKey.IndexOf(']', kcount) + 1 - kcount));
                            while (wildcardKey[++kcount] != ']') ;
                        }
                        else if (wildcardKey[kcount] != '*')
                            wc2 = new Wildcard("" + wildcardKey[kcount]);

                        if (!wc1.contains(wc2) && !(wildcardKey[kcount] == '*'))
                            return false;
                        kcount++;
                    }
                }
                if (pcount != pattern.Length || kcount != wildcardKey.Length)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool testWildcardKey(string wildcardKey)
        {
            return testWildcardKey(wildcardKey, pattern);
        }

        private void setWildcardKey(string wildcardKey)
        {          
            int pcount = 0;
            wildcardList = new ArrayList();
            int i = 0;
            while (i < wildcardKey.Length)            
            {
                if (pattern[pcount] == '[')
                {
                    if (wildcardKey[i] == '*')
                    {
                        Wildcard wc = new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount) + 1 - pcount));
                        wildcardList.Add(wc);
                      }
                    else if (wildcardKey[i] == '[')
                    {
                        Wildcard wc = new Wildcard(wildcardKey.Substring(i, wildcardKey.IndexOf(']', i) + 1 - i));
                        wildcardList.Add(wc);
                        while (wildcardKey[++i] != ']') ;
                    }
                    else
                    {
                        Wildcard wc = new Wildcard("" + wildcardKey[i]);
                        wildcardList.Add(wc);
                    }
                    while (pattern[++pcount] != ']') ;
                }
                pcount++;
                i++;
            }
        }

        private string getWildcardKey()
        {
            string res = "";
            int pcount = 0;
            int wccount = 0;

            // error handling
            if (wildcardList != null)
            {
                while (pcount < pattern.Length)
                {
                    if (pattern[pcount] != '[')
                        res += pattern[pcount];
                    else
                    {
                        res += ((Wildcard)wildcardList[wccount++]).getRepresentationString();
                        while (pattern[++pcount] != ']') ;
                    }
                    pcount++;
                }
                return res;
            }
            else
                return null;
        }

        public BigInteger size()
        {
            if (wildcardList == null)
                return 0;
            BigInteger counter = 1;
            foreach (Wildcard wc in wildcardList)
                counter *= wc.size();
            return counter;
        }

        /** used to jump to the next Key.         
         * if nextWildcard == -1, we return false
         * if nextWildcard == -2, we return true
         * if nextWildcard == -3, we increase the rightmost wildcard
         * if nextWildcard >= 0, we increase the wildcard on the position 'nextWildcard'
         * returns false if there is no key left.
         */
        public bool nextKey(int nextWildcard)
        {
            if (nextWildcard == -2)
                return true;
            if (nextWildcard == -1)
                return false;

            int wildcardCount;
            if (nextWildcard == -3)
                wildcardCount = wildcardList.Count - 1;
            else
                wildcardCount = nextWildcard;
            bool overflow = ((Wildcard)wildcardList[wildcardCount--]).succ();            
            while (overflow && (wildcardCount >= 0))
                overflow = ((Wildcard)wildcardList[wildcardCount--]).succ();
            return !overflow;
        }

        public string getKey()
        {
            string res = "";
            int wildcardCount = 0;
            int i = 0;
            while (i < pattern.Length)            
            {
                if (pattern[i] != '[')
                    res += pattern[i++];
                else
                {
                    Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                    res += wc.getChar();
                    while (pattern[i++] != ']') ;
                }                
            }
            return res;
        }

        public string getKey(int add)
        {
            string res = "";
            int div = 1;
            int wildcardCount = wildcardList.Count - 1;
            int i = pattern.Length - 1;
            while (i >= 0)            
            {
                if (pattern[i] != ']')
                    res += pattern[i--];
                else
                {
                    Wildcard wc = (Wildcard)wildcardList[wildcardCount--];
                    if (add < div)
                        res += wc.getChar();
                    else
                    {
                        res += wc.getChar((add / div) % wc.size());
                        div *= wc.size();
                    }
                    while (pattern[i--] != '[') ;
                }
            }
            char[] r = res.ToCharArray();
            Array.Reverse(r);
            return new string(r);
        }

        public string getKeyBlock(ref int blocksize, ref int nextWildcard)
        {
            const int MAXSIZE = 65536;
            //find out how many wildcards we can group together:
            blocksize = 1;
            int pointer;
            for (pointer = wildcardList.Count - 1; pointer >= 0; pointer--)
            {
                Wildcard wc = (Wildcard)wildcardList[pointer];
                if (wc.isSplit || wc.count() != 0 || blocksize * wc.size() > MAXSIZE)
                    break;
                else
                    blocksize *= wc.size();
            }

            if (pointer >= wildcardList.Count)
                return null;

            nextWildcard = pointer;

            //generate key:
            string res = "";
            int wildcardCount = 0;
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] != '[')
                    res += pattern[i++];
                else
                {
                    if (pointer < wildcardCount)
                        res += "*";
                    else
                    {
                        Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                        res += wc.getChar();
                    }
                    while (pattern[i++] != ']') ;
                }
            }
            return res;
        }        

         /*
         * ARNIES SANDKASTEN - ALLE FOLGENDEN METHODEN SIND FÜR DIE VERTEILTE VERWENDUNG
         * DES KEYPATTERNS NOTWENDIG ODER ABER EINFACH UM DAS KEYPATTERN SCHÖN ALS
         * GUILOGMESSAGE AUSGEBEN ZU KÖNNEN ;-)
         */

        // Added by Arnold - 2010.02.04
        public KeyPattern(byte[] serializedPattern)
        {
            KeyPattern deserializedPattern = Deserialize(serializedPattern);
            // set deserialized Pattern to actual pattern
            this.pattern = deserializedPattern.pattern;
            this.WildcardKey = deserializedPattern.WildcardKey;
            //this.wildcardList = deserializedPattern.wildcardList;
            if (deserializedPattern == null)
                throw new Exception("Invalid byte[] representation of KeyPattern!");
        }

        //added by Christian Arnold - 2009.12.02
        /// <summary>
        /// returns type, key and pattern. If you want to get only the pattern for processing use GetPattern-method!
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(this.WildcardKey != null)
                return "Type: KeySearcher.KeyPattern. WildcardKey: '" + this.WildcardKey + "', Pattern: '" + this.pattern + "'";
            else
                return "Type: KeySearcher.KeyPattern. KeyPattern isn't initialized correctly, Pattern: '" + this.pattern + "'";
        }

        //added by Christian Arnold - 2009.12.03
        /// <summary>
        /// returns ONLY the pattern as a string!
        /// </summary>
        /// <returns></returns>
        public string GetPattern()
        {
            return this.pattern;
        }

        #region Serialization methods and auxiliary variables

        /* Serialization information:
         * 1st byte: Byte-Length of the WildCardKey
         * 2nd - wildcardLen: WildCardKey Byte representation
         * wildcardLen + 1: Byte-Length of the Pattern
         * wildcardLen + 2 - wildcardLen+2+patternLen: Pattern Byte representation
         *  -------------------------------------------------------------
         * | wildcardkey length | wildcardkey | pattern length | pattern |
         *  -------------------------------------------------------------  */
        private Encoding encoder = UTF8Encoding.UTF8;

        /// <summary>
        /// Serialize all needful information to rebuild the existing pattern elsewhere
        /// </summary>
        /// <returns>byte representation of all the needful information of the actual KeyPattern</returns>
        public byte[] Serialize()
        {
            byte[] retByte;
            string wildcardKey = this.WildcardKey;
            if (wildcardKey != null && this.pattern != null)
            {
                if (testWildcardKey(wildcardKey))
                {
                    byte[] byteWildCard = encoder.GetBytes(wildcardKey);
                    byte[] bytePattern = encoder.GetBytes(pattern);
                    retByte = new byte[byteWildCard.Length + bytePattern.Length + 2];
                    retByte[0] = (byte)byteWildCard.Length;
                    Buffer.BlockCopy(byteWildCard, 0, retByte, 1, byteWildCard.Length);
                    retByte[byteWildCard.Length + 1] = (byte)bytePattern.Length;
                    Buffer.BlockCopy(bytePattern, 0, retByte, byteWildCard.Length + 2, bytePattern.Length);
                }
                else
                {
                    throw (new Exception("Serializing KeyPattern canceled, because WildcardKey and/or Pattern aren't valid. "
                        + "WildcardKey: '" + wildcardKey + "', Pattern: '" + pattern + "'.\n"));
                }
            }
            else
            {
                throw (new Exception("Serializing KeyPattern canceled, because Key and/or Pattern are NULL. WildcardKey: '" + wildcardKey + "'. Pattern: '" + pattern + "'."));
            }
            return retByte;
        }

        /// <summary>
        /// Deserialize a byte-representation of an KeyPattern object. Returns a full-initialized KeyPattern object.
        /// </summary>
        /// <param name="serializedPattern">byte-representation of an keypattern object</param>
        /// <returns>a full-initialized KeyPattern object</returns>
        public KeyPattern Deserialize(byte[] serializedPattern)
        {
            KeyPattern keyPatternToReturn;
            string wildcardKey_temp;
            string pattern_temp;

            int iWildCardLen = serializedPattern[0];
            wildcardKey_temp = encoder.GetString(serializedPattern, 1, iWildCardLen);
            int iPatternLen = serializedPattern[iWildCardLen + 1];
            pattern_temp = encoder.GetString(serializedPattern, iWildCardLen + 2, iPatternLen);

            keyPatternToReturn = new KeyPattern(pattern_temp);
            // test extracted pattern and wildcardKey!
            if (keyPatternToReturn.testWildcardKey(wildcardKey_temp))
            {
                keyPatternToReturn.WildcardKey = wildcardKey_temp;
                return keyPatternToReturn;
            }
            else
            {
                throw (new Exception("Deserializing KeyPattern canceled, because WildcardKey or Pattern aren't valid. "
                    + "WildcardKey: '" + wildcardKey_temp + "', Pattern: '" + pattern_temp + "'.\n"));
            }
        }

        #endregion

    }
}