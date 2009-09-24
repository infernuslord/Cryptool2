using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Diagnostics;

namespace WordPatterns
{
    /*
     * Proposed changes and enhancements:
     * - multiple word search with one TextInput (split words at whitespace)
     * - enter max match number
     * - enter pattern in number format (like 1-2-2-1)
     * - add filter function (see Borland C++ tool)
     * - save last input words and propose them to user
     * - improve performance
     * - support wildcard (*)
     */
    [Author("Matthäus Wander", "wander@cryptool.org", "Fachgebiet Verteilte Systeme, Universität Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "WordPatterns", "Searches for words with the same pattern", null, "CrypWin/images/default.png")]
    public class WordPatterns : IAnalysisMisc
    {
        #region Private stuff

            private WordPatternsSettings settings = new WordPatternsSettings();

            private string inputText;
            private string[] inputDict;
            private string outputText;

            private IDictionary<Pattern, IList<string>> dictPatterns;

            private bool stop = false;

        #endregion

        #region Properties

        [PropertyInfo(Direction.InputData, "Input word", "Word to search for patterns", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string InputText
        {
            get
            {
                return inputText;
            }
            set
            {
                inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        [PropertyInfo(Direction.InputData, "Input dictionary", "Word dictionary", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string[] InputDict
        {
            get
            {
                return inputDict;
            }
            set
            {
                inputDict = value;
                dictPatterns = null; // force rebuild of dictionary patterns
                OnPropertyChanged("InputDict");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output words", "Words matching the pattern", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string OutputText
        {
            get { return outputText; }
            private set
            {
                outputText = value;
                OnPropertyChanged("OutputText");
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public ISettings Settings
        {
            get { return settings; }
            set { settings = (WordPatternsSettings) value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            stop = false;
        }

        public void Execute()
        {
            if (inputText == null)
            {
                OutputText = "";
                return;
            }

            // calculate input word pattern
            Pattern inputPattern = new Pattern(inputText);

            if (inputDict == null)
                return;

            // If not already done, calculate pattern for each dictionary word
            if (dictPatterns == null)
            {
                dictPatterns = new Dictionary<Pattern, IList<string>>();
                int wordCount = 0;

                while (wordCount < inputDict.Length && !stop)
                {
                    string word = inputDict[wordCount];
                    Pattern p = new Pattern(word);

                    // two calls to Pattern.GetHashCode()
                    if (!dictPatterns.ContainsKey(p))
                        dictPatterns[p] = new List<string>();

                    // one call to Pattern.GetHashCode() and one to Pattern.Equals()
                    dictPatterns[p].Add(word);

                    if (++wordCount % 10000 == 0)
                    {
                        ProgressChanged(wordCount, inputDict.Length);
                    }
                }

                ProgressChanged(wordCount, inputDict.Length);
                GuiLogMessage(string.Format("Processed {0} words from dictionary.", wordCount), NotificationLevel.Info);
            }

            // retrieve words matching input pattern
            if (dictPatterns.ContainsKey(inputPattern))
            {
                StringBuilder sb = new StringBuilder();
                IList<string> matches = dictPatterns[inputPattern];
                foreach (string word in matches)
                {
                    sb.Append(word);
                    sb.AppendLine();
                }
                OutputText = sb.ToString();
            }
            else
            {
                OutputText = "";
            }
        }

        internal class Pattern
        {
            private const int prime = 31;

            private int[] patternArray;
            private int hashCode = 1;

            internal Pattern(string word)
            {
                patternArray = new int[word.Length];
                
                Dictionary<char, int> seenLetters = new Dictionary<char, int>();
                int letterNumber = 0;
                
                for (int i = 0; i < word.Length; i++)
                {
                    if (seenLetters.ContainsKey(word[i])) // letter already seen?
                    {
                        patternArray[i] = seenLetters[word[i]]; // get letter number
                    }
                    else
                    {
                        seenLetters[word[i]] = patternArray[i] = ++letterNumber; // create new letter number
                    }

                    // Fast hash algorithm similar to FNV.
                    hashCode = prime * hashCode + patternArray[i];
                }

                seenLetters = null;
            }

            /// <summary>
            /// Returns pre-calculated hash code.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return hashCode;
            }

            /// <summary>
            /// In-depth comparison of pattern array contents.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                // identical object
                if (this == obj)
                    return true;

                // uneven types
                if (!(obj is Pattern))
                    return false;

                Pattern another = obj as Pattern;

                // uneven pattern lengths
                if (patternArray.Length != another.patternArray.Length)
                    return false;

                for (int i = 0; i < patternArray.Length; i++)
                {
                    // uneven pattern content
                    if (patternArray[i] != another.patternArray[i])
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// equals to (int) Math.pow(10, x), but does not require type casting between double and int
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int power10(int x)
        {
            int result = 1;
            for (int i = 0; i < x; i++)
            {
                result *= 10;
            }
            return result;
        }

        public void PostExecution()
        {
            GuiLogMessage("PostExecution has been called. Cleaning pattern dictionary...", NotificationLevel.Info);
            dictPatterns = null;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            stop = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion

    }
}
