using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Media;

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
        private IDictionary<string, double[]> grams = new SortedDictionary<string, double[]>();
        private DataSource data = new DataSource();
        private double presentationScaler = 1.0; // the initial zoom value
        private double presentationBarWidth = 24.7; // the width in pixel of a single chart bar
        private double presentationBarHeightAdd = 8.0 + 2.0 * 26.0; // the additional heigth to a chart bar, comprised of two rectangles (3px, 5px) and two textblocks

        // TODO: this shall be an algorithm setting or an optional word
        private const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        

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
            lock (grams)
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
            presentation = new FrequencyTestPresentation();
            Presentation = presentation;
            QuickWatchPresentation = presentation;

            presentation.SizeChanged += new System.Windows.SizeChangedEventHandler(presentation_SizeChanged);

            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
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

            presentation.SetBackground(Brushes.LightYellow);

            string workstring = stringInput;

            // Any change in the word discards and recalculates the output. This is not that effective.
            lock (grams)
            {
                grams.Clear();

                if (settings.BoundaryFragments == 1)
                {
                    foreach (string word in WordTokenizer.tokenize(workstring))
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

                // OUTPUT
                StringBuilder sb = new StringBuilder();
                arrayOutput = new int[grams.Count];
                for (int i = 0; i < grams.Count; i++)
                {
                    KeyValuePair<string, double[]> item = grams.ElementAt(i);

                    sb.Append(item.Key + ":");
                    sb.Append(item.Value[ABSOLUTE] + ":");
                    sb.Append((item.Value[PERCENTAGED]) + Environment.NewLine);

                    arrayOutput[i] = (int)item.Value[ABSOLUTE];
                }
                stringOutput = sb.ToString();

                // update the presentation data
                updatePresentation();
            }

            // Show progress finished.
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(1.0, 1.0));

            OnPropertyChanged("StringOutput");
            OnPropertyChanged("ArrayOutput");
            OnPropertyChanged("DictionaryOutput");
            
            
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

            if (settings.CaseSensitivity == 0)
            {
                workstring = workstring.ToUpper();
            }

            foreach (string g in GramTokenizer.tokenize(workstring, settings.GrammLength, settings.BoundaryFragments == 1))
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


        private void updatePresentation()
        {
            if (grams.Count > 0 && presentation.ActualWidth > 0)
            {
                // retrieve the maximum value from all grams
                double max = grams.Values.Max(item => item[PERCENTAGED]);
                GuiLogMessage("Max n-gram percentage is: " + max, NotificationLevel.Debug);

                // calculate the needed width for the chart (unscaled) in pixel
                double unscaledChartWidth = (double)grams.Count * presentationBarWidth;

                // retrieve the maximum bar height from settings in pixel
                double maxBarHeight = (double) settings.ChartHeight;


                if (settings.Autozoom)
                {
                    // calculate the scaling-value depeding on the needed width ad the current presetnation width
                    presentationScaler = presentation.ActualWidth / unscaledChartWidth;
                    settings.Scale = (int)(presentationScaler * 10000.0);

                    //set the maximum bar height to the current heigth of chart-area in presentation (best fill)
                    //maxBarHeight = presentation.chartBars.ActualHeight - presentationBarHeightAdd;
                    maxBarHeight = (presentation.ActualHeight / presentationScaler) - (presentation.chartHeadline.ActualHeight + presentationBarHeightAdd);
                }

                // remove all entries
                data.ValueCollection.Clear();

                // calculate presentation bars height and add the to our local DataSource
                foreach (KeyValuePair<string, double[]> item in grams)
                {
                    double height = item.Value[PERCENTAGED] * (maxBarHeight / max);
                    CollectionElement row = new CollectionElement(height, Math.Round(item.Value[PERCENTAGED] * 100, 2), item.Key);
                    data.ValueCollection.Add(row);
                }

                switch (settings.GrammLength)
                {
                    case 1:
                        presentation.SetHeadline("Character (unigram) frequency (in %)");
                        break;
                    case 2:
                        presentation.SetHeadline("Bigram frequency (in %)");
                        break;
                    case 3:
                        presentation.SetHeadline("Trigram frequency (in %)");
                        break;
                    default:
                        presentation.SetHeadline(settings.GrammLength + "-gram frequency (in %)");
                        break;
                }
                

                presentation.ShowData(data);
            }
        }

        private void presentation_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // Just for debugging
            //if (e.HeightChanged)
            //{
            //    GuiLogMessage("Height changed from " + e.PreviousSize.Height + " to " + e.NewSize.Height, NotificationLevel.Info);
            //}

            //if (e.WidthChanged)
            //{
            //    GuiLogMessage("Width changed from " + e.PreviousSize.Width + " to " + e.NewSize.Width, NotificationLevel.Info);
            //}

            updatePresentation();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Autozoom":
                case "ChartHeight":
                    updatePresentation();
                    break;

                case "Scale":
                    presentation.SetScaler( (double)settings.Scale / 10000.0);
                    break;
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
            presentation.SetBackground(Brushes.LightGray);
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





