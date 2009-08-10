using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Cryptool.PluginBase.IO;
using System.Collections.ObjectModel;
using Cryptool.FrequencyTest;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;

using System.Runtime.Remoting.Contexts;


namespace Cryptool.FrequencyTest
{
    [Author("Georgi Angelov & Danail Vazov & Matthäus Wander", "angelov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false,
    "Frequency Test",
    "Calculates the frequency of letters or groups of letters in a string.",
    "FrequencyTest/DetailedDescription/Description.xaml",
    "FrequencyTest/icon.png")]
    public class FrequencyTest : IStatistic
    {
        #region Const and variable definition

        public const int ABSOLUTE = 0;
        public const int PERCENTAGED = 1;
        public const int LOG2 = 2;
        public const int SINKOV = 3;

        private string stringInput;

        private string stringOutput = "";
        private int[] arrayOutput = new int[0];
        private SortedDictionary<string, double[]> grams = new SortedDictionary<string, double[]>();

        // TODO: this shall be an algorithm setting or an optional word
        private const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal static DataSource data = new DataSource();

        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.InputData, "The string to be analyzed", "Text Input", "",true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringInput 
        {
            get
            {
                return stringInput;
            }
            set
            {
                stringInput = value;
                OnPropertyChanged("StringInput");
            }
        }

        [PropertyInfo(Direction.OutputData, "Text output", "letter:absolute frequency of the letter:relative frequency of the letter (in %)", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get { return stringOutput; }
        }

        [PropertyInfo(Direction.OutputData , "List output", "absolute frequency of a letter", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.None, "QuickWatchArray")]
        public int[] ArrayOutput
        {
            get { return arrayOutput; }
        }

        public object QuickWatchArray(string propertyNameToConvert)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in arrayOutput)
            {
                sb.Append(i);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        [PropertyInfo(Direction.OutputData, "Dictionary output", "Found grams and their quantities in different scalings", "", false, false, DisplayLevel.Experienced, QuickWatchFormat.None, "QuickWatchDictionary")]
        public IDictionary<string, double[]> DictionaryOutput
        {
            get { return grams; }
        
        }

        public object QuickWatchDictionary(string propertyNameToConvert)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, double[]> item in grams)
            {
                sb.Append(item.Key);
                for (int i = 0; i < item.Value.Length; i++)
                {
                    sb.Append(";" + item.Value[i]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        #endregion

        #region IPlugin Members

        private FrequencyTestSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (FrequencyTestSettings)value; }
        }
        private FrequencyTestPresentation presentation;
        public FrequencyTest()
        {
            settings = new FrequencyTestSettings();
            presentation = new FrequencyTestPresentation(this);
            Presentation = presentation;
            QuickWatchPresentation = presentation;
        }
        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation { get; private set; }

        public void PreExecution()
        {
            //throw new NotImplementedException();
        }

        public void Execute()
        {
            if (stringInput == null)
            {
                return;
            }

            // Any change in the word discards and recalculates the output. This is not that effective.
            data.ValueCollection.Clear();
            grams.Clear();

            string workstring = stringInput;

            if (settings.BoundaryFragments == 1)
            {
                foreach (string word in new WordTokenizer(workstring))
                {
                    ProcessWord(word);
                }
            }
            else
            {
                ProcessWord(workstring);
            }

            double sum = grams.Values.Sum(item => item[ABSOLUTE]);
            GuiLogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

            // calculate scaled values
            foreach (double[] g in grams.Values)
            {
                g[PERCENTAGED] = g[ABSOLUTE] / sum;
                g[LOG2] = Math.Log(g[ABSOLUTE], 2);
                g[SINKOV] = Math.Log(g[PERCENTAGED], Math.E);
            }

            double max = grams.Values.Max(item => item[PERCENTAGED]);
            GuiLogMessage("Max n-gram percentage is: " + max, NotificationLevel.Debug);

            // calculate presentation bars height
            foreach (KeyValuePair<string, double[]> item in grams)
            {
                int height = (int) (item.Value[PERCENTAGED] * (160 / max));
                CollectionElement row = new CollectionElement(height, Math.Round(item.Value[PERCENTAGED] * 100, 3), item.Key);
                data.ValueCollection.Add(row);
            }

            // OUTPUT
            StringBuilder sb = new StringBuilder();
            arrayOutput = new int[grams.Count];
            for (int i = 0; i < grams.Count; i++)
            {
                KeyValuePair<string, double[]> item = grams.ElementAt(i);

                sb.Append(item.Key + ":");
                sb.Append(item.Value[ABSOLUTE] + ":");
                sb.Append(Math.Round(item.Value[PERCENTAGED] * 100, 3) + Environment.NewLine);

                arrayOutput[i] = (int) item.Value[ABSOLUTE];
            }
            stringOutput = sb.ToString();

            OnPropertyChanged("StringOutput");
            OnPropertyChanged("ArrayOutput");
            OnPropertyChanged("DictionaryOutput");
            //  if (OnPluginProgressChanged != null)
            //     {
            //      OnPluginProgressChanged(this, new PluginProgressEventArgs(l, l));
            // }
            presentation.OpenPresentationFile();
        }

        private void ProcessWord(string workstring)
        {
            if (settings.ProcessUnknownSymbols == 0)
            {
                workstring = StringUtil.StripUnknownSymbols(validChars, workstring);
            }

            if (workstring.Length == 0)
            {
                return;
            }

            foreach (string g in new GramTokenizer(workstring, settings.GrammLength, settings.BoundaryFragments==1))
            {
                if (!grams.ContainsKey(g))
                {
                    grams[g] = new double[] { 1, 0, 0, 0 };
                }
                else
                {
                    grams[g][ABSOLUTE]++;
                }
            }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

       
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

    }
   
  
}





