using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.IO;

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

    public class KeyValueReader : Dictionary<string, string>
    {
        public KeyValueReader(FileInfo file)
        {
            ReadLines(file.OpenText());
        }

        private void ReadLines(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // ignore comment lines
                if (line.StartsWith("#"))
                    continue;

                string[] tokens = line.Split(new char[] { '=' }, 2);
                if (tokens.Length < 1 || tokens[0] == null)
                    continue;

                string key = tokens[0].Trim();
                string value = tokens.Length < 2 ? null : tokens[1];

                if (value != null)
                    value = value.Trim();

                if (!ContainsKey(key))
                    Add(key, value);
            }
        }
    }

    public class WordTokenizer : IEnumerable<string>
    {
        /// <summary>
        /// Tokenize an input string to words.
        /// </summary>
        /// <param name="input">some input string containing human-readable text</param>
        /// <returns></returns>
        public static WordTokenizer tokenize(string input)
        {
            return new WordTokenizer(input);
        }

        /// <summary>
        /// Tokenize an an input file to words.
        /// </summary>
        /// <param name="file">some input file containing human-readable text</param>
        /// <returns></returns>
        public static IEnumerable<string> tokenize(FileInfo file)
        {
            StreamReader sr = file.OpenText();
            string line;
            while((line = sr.ReadLine()) != null)
            {
                foreach(string token in new WordTokenizer(line))
                {
                    yield return token;
                }
            }
        }

        private string input;

        private WordTokenizer(string input)
        {
            this.input = input;
        }

        /// <summary>
        /// Returns enumerator with access to some special methods.
        /// </summary>
        /// <returns></returns>
        public WordEnumerator GetCustomEnumerator()
        {
            return new WordEnumerator(input);
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return GetCustomEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetCustomEnumerator();
        }

        #endregion
    }

    public class WordEnumerator : IEnumerator<string>
    {
        private string input;
        private int offset = -1;
        private string token = null;

        public WordEnumerator(string input)
        {
            this.input = input;
        }

        #region IEnumerator<string> Members

        /// <summary>
        /// According to IEnumerator contract this property throws an exception if Current is not pointing on a valid element.
        /// </summary>
        public string Current
        {
            get
            {
                if (token == null)
                    throw new InvalidOperationException("Enumerator does not point on a valid token");;

                return token;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        // explicit implementation of non-generic interface
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            StringBuilder sb = new StringBuilder();

            bool foundWord = false;
            bool feeding = true;

            do
            {
                if (++offset >= input.Length) // end of string
                {
                    if (foundWord)
                    {
                        break; // stop loop gracefully
                    }
                    else
                    {
                        token = null;
                        return false; // abort
                    }
                }

                switch (input[offset])
                {
                    case '\r':
                    case '\n':
                    case ' ':
                        if (foundWord) // found delimiter at the end of a word
                            feeding = false;
                        break;
                    default: // got letter
                        foundWord = true;
                        sb.Append(input[offset]);
                        break;
                }
            } while (feeding);

            token = sb.ToString();
            return true;
        }

        public void Reset()
        {
            offset = -1;
            token = null;
        }

        #endregion

        #region Additional properties

        /// <summary>
        /// Returns current position in processing input string.
        /// </summary>
        public int Position
        {
            get
            {
                return Math.Max(offset, 0);
            }
        }

        /// <summary>
        /// Returns length of input string.
        /// </summary>
        public int Length
        {
            get
            {
                return input.Length;
            }
        }

        #endregion
    }

    public class GramTokenizer : IEnumerable<string>
    {
        public static IEnumerable<string> tokenize(string word)
        {
            return new GramTokenizer(word, 1, false);
        }

        public static IEnumerable<string> tokenize(string word, int gramLength)
        {
            return new GramTokenizer(word, gramLength, false);
        }

        public static IEnumerable<string> tokenize(string word, int gramLength, bool includeFragments)
        {
            return new GramTokenizer(word, gramLength, includeFragments);
        }

        private string word;
        private int gramLength;
        private bool includeFragments;

        private GramTokenizer(string word, int gramLength, bool includeFragments)
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

            this.gramLength = gramLength;
            this.includeFragments = includeFragments;
        }

        /// <summary>
        /// Tokenizes the whole input and returns a dictionary with all found grams and their quantity
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, int> ToDictionary()
        {
            IDictionary<string, int> dict = new Dictionary<string, int>();

            foreach (string gram in this)
            {
                if (dict.ContainsKey(gram))
                {
                    dict[gram]++;
                }
                else
                {
                    dict[gram] = 1;
                }
            }

            return dict;
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new GramEnumerator(word, gramLength);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new GramEnumerator(word, gramLength);
        }

        #endregion
    }

    public class GramEnumerator : IEnumerator<string>
    {
        private readonly string word;
        private readonly int gramLength;

        private int position = -1;

        public GramEnumerator(string word, int gramLength)
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
