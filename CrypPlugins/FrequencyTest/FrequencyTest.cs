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

        private string stringOutput = "";
        private string stringInput;
        private int [] arrayOutput;

        // TODO: this shall be an algorithm setting or an optional input
        private const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal static DataSource data = new DataSource();

        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "The string to be analyzed", "Text Input", "",true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.Output, "Text output", " letter:absolute frequency of the letter:relative frequency of the letter (in %)  ", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get { return stringOutput; }
        }

        [PropertyInfo(Direction.Output, "List output", "absolute frequency of a letter", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public int[] ArrayOutput
        {
            get { return arrayOutput; }
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
            if (stringInput != null)
            {
                string workstring = stringInput;
                if (settings.CaseSensitivity == 0)
                {
                    workstring = workstring.ToLower();
                }
                if (settings.RemoveUnknownSymbols == 0)
                {
                    StringBuilder workstring2 = new StringBuilder();
                    foreach (char c in workstring)
                    {
                        if (validChars.Contains(c))
                        {
                            workstring2.Append(c);
                        }
                    }
                    workstring = workstring2.ToString();
                }

                // Any change in the input discards and recalculates the output. This is not that effective.
                data.ValueCollection.Clear();

                SortedDictionary<string, GramCount> grams = new SortedDictionary<string, GramCount>();

                for (int i = 0; i < workstring.Length - settings.GrammLength + 1; i++)
                {
                    string g = workstring.Substring(i, settings.GrammLength);
                    if (!grams.ContainsKey(g))
                    {
                        grams[g] = new GramCount(1);
                    }
                    else
                    {
                        grams[g].Absolute++;
                    }
                }

                int sum = grams.Values.Sum(item => item.Absolute);
                GuiLogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

                // calculate scaled values
                foreach (GramCount g in grams.Values)
                {
                    g.Percentaged = Math.Round(g.Absolute / (double)sum * 100, 3);
                    g.Log2 = Math.Log(g.Absolute, 2);
                }

                double max = grams.Values.Max(item => item.Percentaged);
                GuiLogMessage("Max n-gram percentage is: " + max, NotificationLevel.Debug);

                // calculate presentation bars height
                foreach (KeyValuePair<string, GramCount> item in grams)
                {
                    int height = (int) (item.Value.Percentaged * (160 / max));
                    CollectionElement row = new CollectionElement(height, item.Value.Percentaged, item.Key);
                    data.ValueCollection.Add(row);
                }

                // OUTPUT
                StringBuilder sb = new StringBuilder();
                arrayOutput = new int[grams.Count];
                for (int i = 0; i < grams.Count; i++)
                {
                    KeyValuePair<string, GramCount> item = grams.ElementAt(i);

                    sb.Append(item.Key + ":");
                    sb.Append(item.Value.Absolute + ":");
                    sb.Append(item.Value.Percentaged + Environment.NewLine);

                    arrayOutput[i] = item.Value.Absolute;
                }
                stringOutput = sb.ToString();

                OnPropertyChanged("StringOutput");
                OnPropertyChanged("ArrayOutput");
                //  if (OnPluginProgressChanged != null)
                //     {
                //      OnPluginProgressChanged(this, new PluginProgressEventArgs(l, l));
                // }
                presentation.OpenPresentationFile();
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

    public class GramCount
    {
        public int Absolute
        {
            get;
            internal set;
        }

        public double Percentaged
        {
            get;
            internal set;
        }

        public double Log2
        {
            get;
            internal set;
        }

        internal GramCount(int absolute)
        {
            this.Absolute = absolute;
            this.Percentaged = -1;
            this.Log2 = -1;
        }

        public override string ToString()
        {
            return "GramCount[Absolute=" + Absolute + ", Percentaged=" + Percentaged + ", Log2=" + Log2 + "]";
        }
    }
   
  
}





