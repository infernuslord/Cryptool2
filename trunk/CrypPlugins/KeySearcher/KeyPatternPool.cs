﻿using System;
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
    public class KeyPatternPool
    {
        private BigInteger partsize;
        private BigInteger counter = 0;
        private KeyPattern pattern;
        private Stack<KeyPattern> stack = new Stack<KeyPattern>();
        private int[] splittingQuotient;
        private int[] splittingCounter;
        private bool end = false;

        private void CalculateSplitting()
        {
            for (int c = pattern.wildcardList.Count - 1; c >= 0; c--)
                splittingQuotient[c] = 1;

            BigInteger bestSize = GetPartSize();

            for (int c = pattern.wildcardList.Count - 1; c >= 0; c--)
            {
                for (int k = 1; k <= c; k++)
                {
                    int d = ((Wildcard)pattern.wildcardList[c]).size();
                    if (d % k == 0)
                    {
                        int tmp = splittingQuotient[c];
                        splittingQuotient[c] = d;
                        BigInteger size = GetPartSize();
                        if ((size - partsize).abs() < (bestSize - partsize).abs())                        
                            bestSize = size;                        
                        else
                            splittingQuotient[c] = tmp;
                    }
                }
            }
        }
                
        private bool SuccCounter()
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

        public bool Contains(KeyPattern pattern)
        {
            if (pattern.wildcardList.Count != this.pattern.wildcardList.Count)
                return false;
            if (pattern.GetPattern() != this.pattern.GetPattern())
                return false;

            bool equal = true;
            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                Wildcard thiswc = ((Wildcard)this.pattern.wildcardList[k]);
                if (wc.size() != (thiswc.size() / splittingQuotient[k]))
                    return false;
                
                bool bolContains2 = true;
                int begin = equal ? splittingCounter[k] : 0;
                for (int j = begin; j < splittingQuotient[k]; j++)
                {
                    bool bolContains = true;
                    for (int i = 0; i < wc.size(); i++)
                    {
                        if (wc.getChar(i - wc.count()) != thiswc.getChar(i + j * wc.size()))
                        {
                            bolContains = false;
                            break;
                        }
                    }
                    if (bolContains)
                    {                        
                        equal = (j == splittingCounter[k]);
                        bolContains2 = true;
                        break;
                    }
                }
                if (!bolContains2)
                    return false;

            }
            return !equal;
        }

        public void Push(KeyPattern pattern)
        {
            counter--;
            if (!Contains(pattern))
                stack.Push(pattern);
            else
                throw new Exception("Pattern already given.");
        }

        public KeyPattern Pop()
        {
            counter++;
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

            if (!SuccCounter())
                end = true;

            return part;
        }

        public BigInteger GetPartSize()
        {
            BigInteger res = 1;
            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                res *= wc.size() / splittingQuotient[k];
            }
            return res;
        }

        public BigInteger Count()
        {
            return TotalAmount() - counter;
        }

        public BigInteger TotalAmount()
        {
            BigInteger res = 1;
            for (int k = 0; k < pattern.wildcardList.Count; k++)
            {
                Wildcard wc = ((Wildcard)pattern.wildcardList[k]);
                res *= splittingQuotient[k];
            }
            return res;
        }

        public KeyPatternPool(KeyPattern pattern, BigInteger partsize)
        {
            this.partsize = partsize;
            this.pattern = pattern;
            splittingQuotient = new int[pattern.wildcardList.Count];
            CalculateSplitting();
            splittingCounter = new int[pattern.wildcardList.Count];
        }
    }
}
