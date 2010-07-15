﻿namespace KeySearcher.KeyPattern
{
    internal class Wildcard
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
            if (valuePattern.Length == 1)
            {
                length = 1;
                values[0] = valuePattern[0];
            }
            else
            {
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
        }

        public Wildcard(Wildcard wc)
        {
            isSplit = wc.isSplit;
            length = wc.length;
            counter = wc.counter;
            for (int i = 0; i < 256; i++)
                values[i] = wc.values[i];
        }

        public Wildcard(char[] values, int length)
        {
            isSplit = false;
            this.length = length;
            this.values = values;
            this.counter = 0;
        }

        private Wildcard()
        {
        }

        public Wildcard[] split()
        {
            if (length <= 1)
                return null;
            int length1 = this.length - this.counter;
            Wildcard[] wcs = new Wildcard[2];
            wcs[0] = new Wildcard();
            wcs[0].counter = 0;
            wcs[0].length = length1 / 2;
            wcs[1] = new Wildcard();
            wcs[1].counter = 0;
            wcs[1].length = length1 - wcs[0].length;
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

        public string getRepresentationString()
        {
            if (length == 1)
                return "" + values[0];
            string res = "[";
            int begin = 0;
            for (int i = 1; i < length; i++)
            {
                if (values[i - 1] != values[i] - 1)
                {
                    if (begin == i - 1)
                        res += values[begin];
                    else
                    {
                        if (i - 1 - begin == 1)
                            res += values[begin] + "" + values[i - 1];
                        else
                            res += values[begin] + "-" + values[i - 1];
                    }
                    begin = i;
                }
            }
            if (begin == length - 1)
                res += values[begin];
            else
            {
                if (length - 1 - begin == 1)
                    res += values[begin] + "" + values[length - 1];
                else
                    res += values[begin] + "-" + values[length - 1];
            }

            res += "]";
            return res;
        }

        public bool contains(Wildcard wc)
        {
            if (wc == null)
                return false;
            for (int i = 0; i < wc.length; i++)
            {
                bool contains = false;
                for (int j = 0; j < this.length; j++)
                {
                    if (this.values[j] == wc.values[i])
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    return false;
            }
            return true;
        }
    }
}