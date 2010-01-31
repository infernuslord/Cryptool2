using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;

namespace KeySearcher
{
    /**
     * This class is able to split a KeyPattern into several disjunct parts, which are guaranteed to have equal size.
     * It tries to split the pattern in such a way, that the parts have nearly the given partsize.
     **/
    class KeyPatternPool
    {
        private BigInteger partsize;
        private KeyPattern pattern;
        private Stack<KeyPattern> stack = new Stack<KeyPattern>();
        private int[] splittingQuotient;
        private int[] splittingCounter;
        private bool end = false;               

        /*private BigInteger calculateSplitting(int i)
        {
            //This method is better, but too slow :(
            if (i >= pattern.wildcardList.Count)            
                return getPartSize();

            int best = 0;
            BigInteger bestval = -1;
            int c = ((Wildcard)pattern.wildcardList[i]).size();
            if (c == 1)
            {
                splittingQuotient[i] = 1;
                return calculateSplitting(i + 1);
            }
            for (int k = 1; k <= c; k++)
                if (c % k == 0)
                {
                    splittingQuotient[i] = k;
                    BigInteger res = calculateSplitting(i + 1);
                    if ((bestval == -1) || ((res-partsize).abs() < (bestval-partsize).abs()))
                    {
                        bestval = res;
                        best = k;
                    }
                }
            splittingQuotient[i] = best;
            calculateSplitting(i + 1);
            return bestval;
        }*/

        private void calculateSplitting()
        {
            for (int c = pattern.wildcardList.Count - 1; c >= 0; c--)
                splittingQuotient[c] = 1;

            BigInteger bestSize = getPartSize();

            for (int c = pattern.wildcardList.Count - 1; c >= 0; c--)
            {
                for (int k = 1; k <= c; k++)
                {
                    int d = ((Wildcard)pattern.wildcardList[c]).size();
                    if (d % k == 0)
                    {
                        int tmp = splittingQuotient[c];
                        splittingQuotient[c] = d;
                        BigInteger size = getPartSize();
                        if ((size - partsize).abs() < (bestSize - partsize).abs())                        
                            bestSize = size;                        
                        else
                            splittingQuotient[c] = tmp;
                    }
                }
            }
        }
                
        private bool succCounter()
        {
            for (int k = pattern.wildcardList.Count-1; k >= 0; k--)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                splittingCounter[k]++;
                if (splittingCounter[k] >= splittingQuotient[k])
                    splittingCounter[k] = 0;
                else
                    return true;
            }
            return false;
        }

        public bool contains(KeyPattern pattern)
        {
            if (pattern.wildcardList.Count != this.pattern.wildcardList.Count)
                return false;
            if (pattern.GetPattern() != this.pattern.GetPattern())
                return false;

            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                Wildcard thiswc = ((Wildcard)this.pattern.wildcardList[k]);
                if (wc.size() != (thiswc.size() / splittingQuotient[k]))
                    return false;

                bool contains2 = true;
                for (int j = 0; j < splittingQuotient[k]; j++)
                {
                    bool contains = true;
                    for (int i = 0; i < wc.size(); i++)
                    {
                        if (wc.getChar(i - wc.count()) != thiswc.getChar(i + j * wc.size()))
                        {
                            contains = false;
                            break;
                        }
                    }
                    if (contains)
                    {
                        contains2 = true;
                        break;
                    }
                }
                if (!contains2)
                    return false;

            }
            return true;
        }

        public void push(KeyPattern pattern)
        {
            if (!contains(pattern))
                stack.Push(pattern);
            else
                throw new Exception("Pattern already given.");
        }

        public KeyPattern pop()
        {
            if (stack.Count != 0)
                return (KeyPattern)stack.Pop();

            if (end)
                return null;

            KeyPattern part = new KeyPattern(pattern.GetPattern());
            part.wildcardList = new ArrayList();
            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {                
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                char[] values = new char[256];
                int length = wc.size() / splittingQuotient[k];
                for (int i = 0; i < length; i++)
                    values[i] = wc.getChar(i + splittingCounter[k] * length);
                Wildcard newwc = new Wildcard(values, length);
                part.wildcardList.Add(newwc);
            }

            if (!succCounter())
                end = true;

            return part;
        }

        public BigInteger getPartSize()
        {
            BigInteger res = 1;
            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                res *= wc.size() / splittingQuotient[k];
            }
            return res;
        }

        public KeyPatternPool(KeyPattern pattern, BigInteger partsize)
        {
            this.partsize = partsize;
            this.pattern = pattern;
            splittingQuotient = new int[pattern.wildcardList.Count];
            calculateSplitting();
            splittingCounter = new int[pattern.wildcardList.Count];
        }
    }
}
