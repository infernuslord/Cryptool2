using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;

namespace KeySearcher
{
    public class KeyPattern
    {
        private class Wildcard
        {
            private char[] values = new char[256];
            private int length;
            private int counter;
            public bool isSplit
            {
                get;
                private set;
            }

            public Wildcard(string valuePattern)
            {
                isSplit = false;
                counter = 0;
                length = 0;
                int i = 1;
                while (valuePattern[i] != ']')
                {
                    if (valuePattern[i + 1] == '-')
                    {
                        for (char c = valuePattern[i]; c <= valuePattern[i + 2]; c++)
                            values[length++] = c;
                        i += 2;
                    }
                    else
                        values[length++] = valuePattern[i];
                    i++;
                }
            }

            public Wildcard(Wildcard wc)
            {
                isSplit = wc.isSplit;
                length = wc.length;
                counter = wc.counter;
                for (int i = 0; i < 256; i++)
                    values[i] = wc.values[i];
            }

            private Wildcard()
            {
            }

            public Wildcard[] split()
            {
                int length = this.length - this.counter;
                Wildcard[] wcs = new Wildcard[2];
                wcs[0] = new Wildcard();
                wcs[0].counter = 0;
                wcs[0].length = length / 2;
                wcs[1] = new Wildcard();
                wcs[1].counter = 0;
                wcs[1].length = length - wcs[0].length;
                for (int i = 0; i < wcs[0].length; i++)
                    wcs[0].values[i] = values[this.counter + i];
                for (int i = 0; i < wcs[1].length; i++)
                    wcs[1].values[i] = values[i + this.counter + wcs[0].length];
                wcs[0].isSplit = true;
                wcs[1].isSplit = true;
                return wcs;
            }

            public char getChar()
            {
                return values[counter];
            }

            public char getChar(int add)
            {
                return values[(counter + add) % length];
            }

            public bool succ()
            {
                counter++;
                if (counter >= length)
                {
                    counter = 0;
                    return true;
                }
                return false;
            }

            public int size()
            {
                return length;
            }


            public int count()
            {
                return counter;
            }

            public void resetCounter()
            {
                counter = 0;
            }
        }

        private string pattern;
        private string key;
        private ArrayList wildcardList;

        public KeyPattern(string pattern)
        {
            this.pattern = pattern;
        }

        public KeyPattern[] split()
        {
            KeyPattern[] patterns = new KeyPattern[2];
            for (int i = 0; i < 2; i++)
            {
                patterns[i] = new KeyPattern(pattern);
                patterns[i].key = key;
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
                throw new Exception("Can't be split!");
            return patterns;
        }

        public string giveWildcardKey()
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

        public bool testKey(string key)
        {
            int kcount = 0;
            int pcount = 0;
            while (kcount < key.Length && pcount < pattern.Length)
            {
                if (pattern[pcount] != '[')
                {
                    if (key[kcount] != '*' && pattern[pcount] != key[kcount])
                        return false;
                    kcount++;
                    pcount++;
                }
                else
                {
                    bool contains = false;
                    pcount++;
                    while (pattern[pcount] != ']')
                    {
                        if (key[kcount] != '*')
                        {
                            if (pattern[pcount + 1] == '-')
                            {
                                if (key[kcount] >= pattern[pcount] && key[kcount] <= pattern[pcount + 2])
                                    contains = true;
                                pcount += 2;
                            }
                            else
                                if (pattern[pcount] == key[kcount])
                                    contains = true;
                        }
                        pcount++;
                    }
                    if (!contains && !(key[kcount] == '*'))
                        return false;
                    kcount++;
                    pcount++;
                }
            }
            if (pcount != pattern.Length || kcount != key.Length)
                return false;
            return true;
        }

        public BigInteger initKeyIteration(string key)
        {
            BigInteger counter = 1;
            this.key = key;
            int pcount = 0;
            wildcardList = new ArrayList();
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '*')
                {
                    Wildcard wc = new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount) + 1 - pcount));
                    wildcardList.Add(wc);
                    counter *= wc.size();
                }

                if (pattern[pcount] == '[')
                    while (pattern[pcount] != ']')
                        pcount++;
                pcount++;
            }
            return counter;
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
            bool overflow = ((Wildcard)wildcardList[wildcardCount]).succ();
            wildcardCount--;
            while (overflow && (wildcardCount >= 0))
                overflow = ((Wildcard)wildcardList[wildcardCount--]).succ();
            return !overflow;
        }

        public string getKey()
        {
            string res = "";
            int wildcardCount = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != '*')
                    res += key[i];
                else
                {
                    Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                    res += wc.getChar();
                }
            }
            return res;
        }

        public string getKey(int add)
        {
            string res = "";
            int div = 1;
            int wildcardCount = wildcardList.Count - 1;
            for (int i = key.Length - 1; i >= 0; i--)
            {
                if (key[i] != '*')
                    res += key[i];
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
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != '*')
                    res += key[i];
                else
                {
                    if (pointer < wildcardCount)
                        res += "*";
                    else
                    {
                        Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                        res += wc.getChar();
                    }
                }
            }
            return res;
        }

        /**
         * Returns an ArrayList with the splitted parts of this pattern.
         * The parts shouldn't be larger than 'partsize'.
         * Do not call this before initializing the key.
         **/
        public ArrayList makeKeySearcherPool(BigInteger partsize)
        {
            if (size() > partsize)
            {
                ArrayList p1, p2;
                KeyPattern[] patterns = split();
                p1 = patterns[0].makeKeySearcherPool(partsize);
                p2 = patterns[1].makeKeySearcherPool(partsize);
                p1.AddRange(p2);
                return p1;
            }
            else
            {
                ArrayList p = new ArrayList();
                p.Add(this);
                return p;
            }
        }

    }
}
