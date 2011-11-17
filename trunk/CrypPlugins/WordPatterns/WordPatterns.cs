using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
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
    [PluginInfo("WordPatterns.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "WordPatterns/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class WordPatterns : ICrypComponent
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

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
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

        [PropertyInfo(Direction.InputData, "InputDictCaption", "InputDictTooltip", true)]
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

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", false)]
        public string OutputText
        {
            get { return outputText; }
            private set
            {
                outputText = value;
                OnPropertyChanged("OutputText");
            }
        }

        public bool CaseSensitive
        {
            get
            {
                return settings.CaseSelection == Case.Sensitive;
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
            Pattern inputPattern = new Pattern(inputText, CaseSensitive);

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
                    Pattern p = new Pattern(word, CaseSensitive);

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

        internal struct Pattern
        {
            private const int PRIME = 16777619;

            private readonly int[] patternArray;
            private readonly int hashCode;

            internal Pattern(string word, bool caseSensitive)
            {
                if (!caseSensitive)
                    word = word.ToLower();

                patternArray = new int[word.Length];
                hashCode = -2128831035; // int32 counterpart of uint32 2166136261
                
                Dictionary<char, int> seenLetters = new Dictionary<char, int>(15);
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

                    // FNV-1 hashing
                    hashCode = (hashCode * PRIME) ^ patternArray[i];
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
            public override bool Equals(object right)
            {
                if (right == null)
                    return false;

                // Never true for value types
                //if (object.ReferenceEquals(this, right))
                //    return true;

                // Using the as/is operators can break symmetry requirement for reference types.
                // However this does not apply for value types.
                //if (this.GetType() != right.GetType())
                //    return false;
                if (!(right is Pattern))
                    return false;

                return this == (Pattern)right;
            }

            public static bool operator==(Pattern left, Pattern right)
            {
                if (left.hashCode != right.hashCode)
                    return false;

                if (left.patternArray.Length != right.patternArray.Length)
                    return false;

                for (int i = 0; i < left.patternArray.Length; i++)
                {
                    // uneven pattern content
                    if (left.patternArray[i] != right.patternArray[i])
                        return false;
                }

                return true;
            }

            public static bool operator !=(Pattern left, Pattern right)
            {
                return !(left == right);
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
