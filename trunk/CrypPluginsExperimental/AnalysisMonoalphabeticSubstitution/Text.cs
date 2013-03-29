using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Text
    {
        #region Private Variables

        private List<int> text = new List<int>();
        private List<string> notInAlphabet = new List<string>();

        #endregion

        #region Constructor

        public Text(string text, Alphabet alpha)
        {
            string curString = "";
            string c;

            for (int i = 0; i < text.Length; i++)
            {
                int j = 0;
                do
                {
                    j++;
                    curString = text.Substring(i, j);
                    c = curString;

                }
                while (alpha.GetNumberOfLettersStartingWith(c) > 1);

                if (alpha.GetNumberOfLettersStartingWith(c) == 1)
                {
                    this.text.Add(alpha.GetPositionOfLetter(c));
                }
                else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                {
                    this.text.Add(-1);
                    this.notInAlphabet.Add(curString);
                }

            }
        }

        public Text(string text, char separator)
        {
            string[] letters = text.Split(separator);
            /*
            for (int i = 0; i < letters.Count(); i++)
            {
                this.txt.Add(letters[i]);
            }*/
        }

        public Text()
        {

        }

        #endregion

        #region Properties

        public int Length
        {
            get { return this.text.Count; }
            private set { ; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert text to string
        /// </summary>
        public string ToString(Alphabet alpha)
        {
            string res = "";

            foreach (int letter in this.text)
            {
                if (letter != -1)
                {
                    res += alpha.GetLetterFromPosition(letter);
                }
                else
                {
                    res += this.notInAlphabet[0];
                    this.notInAlphabet.RemoveAt(0);
                }
            }

            return res;
        }

        /// <summary>
        /// Get letter at position
        /// </summary>
        public int GetLetterAt(int position)
        {
            if ((position >= 0) && (position < this.text.Count))
            {
                return this.text[position];
            }
            else 
            {
                return -1;
            }
        }
        
        /// <summary>
        /// Add letter to the end of the text
        /// </summary>
        public void AddLetter(string letter, Alphabet alpha)
        {
            int position = alpha.GetPositionOfLetter(letter);
            if (position >= 0)
            {
                this.text.Add(position);
            }
            else
            {
                this.text.Add(-1);
                this.notInAlphabet.Add(letter);
            }
        }

        private void AddLetter(int letter)
        {
            this.text.Add(letter);
        }

        /// <summary>
        /// Change letter at position
        /// </summary>
        public bool ChangeLetterAt(int position, int letter)
        {
            if ((position >= 0) && (position < this.text.Count) && (this.text[position]!=-1))
            {
                this.text[position] = letter;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Copy text
        /// </summary>
        public Text CopyTo()
        {
            Text res = new Text();
            for (int i = 0; i < this.text.Count; i++)
            {
                res.AddLetter(this.text[i]);
            }
            for (int i = 0; i < this.notInAlphabet.Count; i++)
            {
                res.AddLetterNotInAlphabet(this.notInAlphabet[i]);
            }
            return res;
        }

        private void AddLetterNotInAlphabet(string letter)
        {
            this.notInAlphabet.Add(letter);
        }

        #endregion
    }
}
