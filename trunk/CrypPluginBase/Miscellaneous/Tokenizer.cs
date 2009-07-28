using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class StringUtil
    {
        public static string StripUnknownSymbols(string alphabet, string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (alphabet.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class WordTokenizer : IEnumerable<string>
    {
        private string input;

        public WordTokenizer(string input)
        {
            this.input = input;
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new WordTokenEnum(input);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new WordTokenEnum(input);
        }

        #endregion
    }

    public class WordTokenEnum : IEnumerator<string>
    {
        private readonly Regex boundary = new Regex("\\b");

        private string[] tokens;
        private int position;

        public WordTokenEnum(string input)
        {
            tokens = boundary.Split(input);
            position = -1;
        }

        #region IEnumerator<string> Members

        /// <summary>
        /// According to IEnumerator contract this property throws an exception if Current is not pointing on a valid element.
        /// </summary>
        public string Current
        {
            get
            {
                try
                {
                    return tokens[position];
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            tokens = null;
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            do
            {
                if (++position >= tokens.Length)
                {
                    return false;
                }
            }
            while (! boundary.IsMatch(tokens[position]));

            return true;
        }

        public void Reset()
        {
            position = -1;
        }

        #endregion
    }

    public class GramTokenizer : IEnumerable<string>
    {
        private string word;
        private int gramLength;
        private bool includeFragments;

        public GramTokenizer(string word) : this(word, 1)
        {
        }

        public GramTokenizer(string word, int gramLength): this(word, gramLength, false)
        {
        }

        public GramTokenizer(string word, int gramLength, bool includeFragments) : this(word, gramLength, includeFragments, false)
        {
        }

        public GramTokenizer(string word, int gramLength, bool includeFragments, bool caseSensitive)
        {
            if (word == null || word.Length < 1)
            {
                throw new ArgumentException("word length must be > 0");
            }
            if (gramLength < 1)
            {
                throw new ArgumentOutOfRangeException("gram length must be > 0");
            }

            if (includeFragments)
            {
                string underline = new string('_', gramLength - 1);
                this.word = underline + word + underline;
            }
            else
            {
                this.word = word;
            }

            if (!caseSensitive)
            {
                this.word = this.word.ToUpper();
            }

            this.gramLength = gramLength;
            this.includeFragments = includeFragments;


        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new GramTokenEnum(word, gramLength);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new GramTokenEnum(word, gramLength);
        }

        #endregion
    }

    public class GramTokenEnum : IEnumerator<string>
    {
        private readonly string word;
        private readonly int gramLength;

        private int position = -1;

        public GramTokenEnum(string word, int gramLength)
        {
            this.word = word;
            this.gramLength = gramLength;
        }

        #region IEnumerator<string> Members

        public string Current
        {
            get
            {
                return word.Substring(position, gramLength);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //word = null; // readonly
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            return ++position < (word.Length - gramLength + 1);
        }

        public void Reset()
        {
            position = 0;
        }

        #endregion
    }

}
