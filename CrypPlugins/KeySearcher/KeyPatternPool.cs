using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Miscellaneous;

namespace KeySearcher
{
    class KeyPatternPool
    {
        private BigInteger partsize;
        private Stack<KeyPattern> stack;

        private void makePool(KeyPattern pattern)
        {
            if (pattern.size() > partsize)
            {
                KeyPattern[] patterns = pattern.split();
                stack.Push(patterns[1]);
                makePool(patterns[0]);
            }
            else
            {
                stack.Push(pattern);                
            }
        }

        public KeyPattern getNext()
        {
            if (stack.Count == 0)
                return null;

            KeyPattern top = stack.Pop();
            if (top.size() > partsize)
            {
                KeyPattern[] patterns = top.split();
                stack.Push(patterns[1]);
                stack.Push(patterns[0]);
                return getNext();
            }
            else
                return top;
        }

        public KeyPatternPool(KeyPattern pattern, BigInteger partsize)
        {
            this.partsize = partsize;
            stack = new Stack<KeyPattern>();
            makePool(pattern);
        }
    }
}
