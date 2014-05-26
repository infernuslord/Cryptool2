/* 
   Copyright 2008-2013, Arno Wacker, University of Kassel

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/


using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// additional needed libs
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;

//Cryptool 2.0 specific includes
using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;


namespace Cryptool.Enigma
{
    [Author("Arno Wacker, Matthäus Wander", "arno.wacker@cryptool.org", "Universität Kassel, Universität Duisburg-Essen", "http://www.ais.uni-kassel.de")]
    [PluginInfo("Cryptool.Enigma.Properties.Resources", "PluginCaption", "PluginTooltip", "Enigma/DetailedDescription/doc.xml",
      "Enigma/Images/Enigma.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class Enigma : ICrypComponent
    {
        #region Constants

        internal const int ABSOLUTE = 0;
        internal const int PERCENTAGED = 1;
        internal const int LOG2 = 2;
        internal const int SINKOV = 3;

        #endregion

        #region Private variables

        private EnigmaSettings settings;
        private EnigmaPresentationFrame enigmaPresentationFrame;
        private EnigmaCore core;
        private EnigmaAnalyzer analyzer;
        private string inputString;
        private IDictionary<int, IDictionary<string, double[]>> statistics;
        // FIXME: enable optional statistics input
        //private IDictionary<string, double[]> inputTriGrams;
        private string outputString;
        private string outputKey;
        private string savedKey;
        public Boolean isrunning;
        

        #endregion

        #region Private methods

        #region Formatting stuff

        /// <summary>
        /// Encrypts or decrypts a string with the given key (rotor positions) and formats
        /// the output according to the settings
        /// </summary>
        /// <param name="rotor1Pos">Position of rotor 1 (fastest)</param>
        /// <param name="rotor2Pos">Position of rotor 2 (middle)</param>
        /// <param name="rotor3Pos">Position of rotor 3 (slowest)</param>
        /// <param name="rotor4Pos">Position of rotor 4 (extra rotor for M4)</param>
        /// <param name="text">The text for en/decryption. This string may contain 
        /// arbitrary characters, which will be dealt with according to the settings given</param>
        /// <returns>The encrypted/decrypted string</returns>
        private string FormattedEncrypt(int rotor1Pos, int rotor2Pos, int rotor3Pos, int rotor4Pos, string text)
        {
            
            String input = preFormatInput(text);
            enigmaPresentationFrame.ChangeStatus(isrunning, enigmaPresentationFrame.EnigmaPresentation.IsVisible);

            if (Presentation.IsVisible && enigmaPresentationFrame.EnigmaPresentation.PresentationDisabled.DisabledBoolProperty)
            {

                String output = core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, rotor4Pos, input);

                enigmaPresentationFrame.EnigmaPresentation.output = output;
                if (enigmaPresentationFrame.EnigmaPresentation.checkReady())
                    enigmaPresentationFrame.EnigmaPresentation.setinput(input);
                else
                    LogMessage("Presentation Error!", NotificationLevel.Error);
                //myPresentation.playClick(null, EventArgs.Empty);
                //return postFormatOutput(output);
               
                return "";
            }           
            else
            {
                   
                return postFormatOutput(core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, rotor4Pos, input));
            }
            

        }

        internal class UnknownToken
        {
            internal string text;
            internal int position;

            internal UnknownToken(char c, int position)
            {
                this.text = char.ToString(c);
                this.position = position;
            }

            public override string ToString()
            {
                return "[" + text + "," + position + "]";
            }
        }

        IList<UnknownToken> unknownList = new List<UnknownToken>();
        IList<UnknownToken> lowerList = new List<UnknownToken>();
        /// <summary>
        /// Format the string to contain only alphabet characters in upper case
        /// </summary>
        /// <param name="text">The string to be prepared</param>
        /// <returns>The properly formated string to be processed direct by the encryption function</returns>
        private string preFormatInput(string text)
        {
            StringBuilder result = new StringBuilder();
            bool newToken = true;
            unknownList.Clear();
            lowerList.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                if (settings.Alphabet.Contains(char.ToUpper(text[i])))
                {
                    newToken = true;
                    if (text[i] == char.ToLower(text[i])) //Solution for preserve FIXME underconstruction
                    {
                        if (settings.UnknownSymbolHandling == 1)
                        {
                            lowerList.Add(new UnknownToken(text[i], result.Length));
                        }
                        else
                        {
                            lowerList.Add(new UnknownToken(text[i], i));
                        }
                        
                    }                                      //underconstruction end
                    result.Append(char.ToUpper(text[i])); // FIXME: shall save positions of lowercase letters
                }
                else if (settings.UnknownSymbolHandling != 1) // 1 := remove
                {
                    // 0 := preserve, 2 := replace by X
                    char symbol = settings.UnknownSymbolHandling == 0 ? text[i] : 'X';

                    if (newToken)
                    {
                        unknownList.Add(new UnknownToken(symbol, i));
                        newToken = false;
                    }
                    else
                    {
                        unknownList.Last().text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();

        }

        //// legacy code
        //switch (settings.UnknownSymbolHandling)
        //{
        //    case 0: // ignore
        //        result.Append(c);
        //        break;
        //    case 1: // remove
        //        continue;
        //    case 2: // replace by X
        //        result.Append('X');
        //        break;
        //}

        /// <summary>
        /// Formats the string processed by the encryption for presentation according
        /// to the settings given
        /// </summary>
        /// <param name="text">The encrypted text</param>
        /// <returns>The formatted text for output</returns>
        private string postFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in unknownList)
            {
                workstring.Insert(token.position, token.text);
            }

            foreach (UnknownToken token in lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token.position];
                workstring.Remove(token.position, 1);
                workstring.Insert(token.position, char.ToLower(help));
            }                                           //underconstruction end

            switch (settings.CaseHandling)
            {
                default:
                case 0: // preserve
                    // FIXME: shall restore lowercase letters
                    return workstring.ToString();
                case 1: // upper
                    return workstring.ToString().ToUpper();
                case 2: // lower
                    return workstring.ToString().ToLower();
            }
        }

        #endregion

        #region Analyzer event handler

        /// <summary>
        /// This eventhandler is called, when the analyzer has an intermediate result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analyzer_OnIntermediateResult(object sender, IntermediateResultEventArgs e)
        {
            // Got an intermidate results from the analyzer, hence display it
            outputString = postFormatOutput(e.Result);
            OnPropertyChanged("OutputString");
        }

        #endregion

        #region n-gram frequencies

        private IDictionary<string, double[]> LoadDefaultStatistics(int length)
        {
            Dictionary<string, double[]> grams = new Dictionary<string, double[]>();

            StreamReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, GetStatisticsFilename(length)));

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;

                string[] tokens = WordTokenizer.tokenize(line).ToArray();
                if (tokens.Length == 0)
                    continue;
                Debug.Assert(tokens.Length == 2, "Expected 2 tokens, found " + tokens.Length + " on one line");

                grams.Add(tokens[0], new double[] { Double.Parse(tokens[1]), 0, 0, 0 });
            }

            double sum = grams.Values.Sum(item => item[ABSOLUTE]);
            LogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

            // calculate scaled values
            foreach (double[] g in grams.Values)
            {
                g[PERCENTAGED] = g[ABSOLUTE] / sum;
                g[LOG2] = Math.Log(g[ABSOLUTE], 2);
                g[SINKOV] = Math.Log(g[PERCENTAGED], Math.E);
            }

            return grams;
        }

        /// <summary>
        /// Get file name for default n-gram frequencies.
        /// </summary>
        /// <param name="length"></param>
        /// <exception cref="NotSupportedException">No default n-gram frequencies available</exception>
        /// <returns></returns>
        private string GetStatisticsFilename(int length)
        {
            if (length < 1)
            {
                throw new ArgumentOutOfRangeException("There is no known default statistic for an n-gram length of " + length);
            }

            return "Enigma_" + length + "gram_Frequency.txt";
        }

        #endregion

        #endregion

        #region Constructor

        public Enigma()
        {
            this.settings = new EnigmaSettings();
            this.core = new EnigmaCore(this);
            this.analyzer = new EnigmaAnalyzer(this);
            this.analyzer.OnIntermediateResult += new EventHandler<IntermediateResultEventArgs>(analyzer_OnIntermediateResult);
            this.statistics = new Dictionary<int, IDictionary<string, double[]>>();
            
          
            
            enigmaPresentationFrame = new EnigmaPresentationFrame(this);
            EnigmaPresentation myPresentation = enigmaPresentationFrame.EnigmaPresentation;
            this.Presentation = enigmaPresentationFrame;
            //this.Presentation.IsVisibleChanged += presentation_isvisibleChanged;
            this.settings.PropertyChanged += enigmaPresentationFrame.EnigmaPresentation.settings_OnPropertyChange;
            this.settings.PropertyChanged += settings_OnPropertyChange;
            this.enigmaPresentationFrame.EnigmaPresentation.fireLetters += fireLetters;
            this.enigmaPresentationFrame.EnigmaPresentation.newInput += newInput;

        }

        #endregion

        #region Events

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void newInput(object sender, EventArgs args)
        {
                running = false;
            
        }

        private void fireLetters(object sender, EventArgs args)  
        {
            Object[] carrier = sender as Object[];

            OutputString = (String)carrier[0] ;
            int x = (int)carrier[1];
            int y = (int)carrier[2];
            
            ShowProgress(x,y);

            
        }



        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            EnigmaSettings dummyset = sender as EnigmaSettings;
            //myPresentation.settingsChanged(dummyset);                        
            //LogMessage("OnPropertyChange " + e.PropertyName, NotificationLevel.Debug);
        }

        #endregion

        #region IPlugin properties

        public ISettings Settings
        {
            get { return this.settings; }
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        #endregion

        #region Connector properties

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != inputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        //[PropertyInfo(Direction.InputData, "InputGramsCaption", "InputGramsTooltip", "", false, false, QuickWatchFormat.Text, "FrequencyTest.QuickWatchDictionary")]
        //public IDictionary<string, double[]> InputGrams
        //{
        //    get { return this.inputTriGrams; }
        //    set
        //    {
        //        if (value != inputTriGrams)
        //        {
        //            this.inputTriGrams = value;
        //            OnPropertyChanged("InputTriGrams");
        //        }
        //    }
        //}

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip", false)]
        public string OutputKey
        {
            get { return this.outputKey; }
            set
            {
                outputKey = value;
                OnPropertyChanged("OutputKey");
            }
        }


        #endregion

        #region Public methods

        public void PreExecution()
        {
            isrunning = true;

            running = false;
            stopped = false;

            if (enigmaPresentationFrame.EnigmaPresentation.checkReady())
                enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Preparing enigma for operation..", this,  NotificationLevel.Info));

            if (settings.Model != 3 && settings.Model != 2)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("This simulator is work in progress. As of right now only Enigma I and Enigma Reichsbahn (Rocket) is supported!!", this, NotificationLevel.Warning));
                return;
            }

            // remember the current key-setting, in order to restore on stop
            savedKey = settings.Key;

            //configure the enigma
            core.setInternalConfig(settings.Rotor1, settings.Rotor2, settings.Rotor3, settings.Rotor4,
                        settings.Reflector, settings.Ring1, settings.Ring2, settings.Ring3, settings.Ring4,
                        settings.PlugBoard);
        }

        private bool running = false;
        private bool stopped = false;

        public void Execute()
        {

            
            if (inputString == null)
                return;


            if (settings.Model != 3 && settings.Model != 2)
            {
                LogMessage("This simulator is work in progress. As of right now only Enigma I and Enigma Reichsbahn (Rocket) is supported!!", NotificationLevel.Error);
                return;
            }

            

            switch (settings.Action)
            {
                case 0:
                    while(running)
                    {
                        enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
                        if (stopped)
                        return;
                    }

                    running = true;
                    LogMessage("Enigma encryption/decryption started...", NotificationLevel.Info);

                    // re-set the key, in case we get executed again during single run
                    settings.Key = savedKey.ToUpper();

                    // do the encryption
                    outputString = FormattedEncrypt(settings.Alphabet.IndexOf(settings.Key[2]), 
                        settings.Alphabet.IndexOf(settings.Key[1]),
                        settings.Alphabet.IndexOf(settings.Key[0]), 
                        0, inputString);                    

                    // FIXME: output all scorings
                    LogMessage("Enigma encryption done. The resulting index of coincidences is " + analyzer.calculateScore(outputString, 0), NotificationLevel.Info);

                    // "fire" the output
                    OnPropertyChanged("OutputString");
                    break;
                case 1:
                    LogMessage("Enigma analysis starting ...", NotificationLevel.Info);

                    //prepare for analysis
                    LogMessage("ANALYSIS: Preformatting text...", NotificationLevel.Debug);
                    string preformatedText = preFormatInput(inputString);

                    // perform the analysis
                    foreach (string decrypt in analyzer.Analyze(preformatedText))
                    {
                        LogMessage(decrypt, NotificationLevel.Debug);

                        // fire all best candidates
                        outputString = postFormatOutput(decrypt);
                        OnPropertyChanged("OutputString");
                    }

                    ShowProgress(1000, 1000);
                    break;
                default:
                    break;
            }

        }

        public void PostExecution()
        {
            LogMessage("Enigma shutting down. Reverting key to inial value!", NotificationLevel.Info);
            if (savedKey != null && savedKey.Length > 0)
            {
                settings.Key = savedKey; // re-set the key
            }

            running = false;
            isrunning = false;
            enigmaPresentationFrame.ChangeStatus(isrunning, enigmaPresentationFrame.EnigmaPresentation.IsVisible);
            
        }

        public void Stop()
        {
            
            stopped = true;
            LogMessage("Enigma stopped", NotificationLevel.Info);
            enigmaPresentationFrame.EnigmaPresentation.stopclick(this, EventArgs.Empty);
            analyzer.StopAnalysis();
        }

        public void Initialize()
        {
            //LogMessage("Initializing..", NotificationLevel.Debug);
        }

        public void Dispose()
        {
            //LogMessage("Dispose", NotificationLevel.Debug);
        }



        /// <summary>
        /// Logs a message to the Cryptool console
        /// </summary>
        public void LogMessage(string msg, NotificationLevel level)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, level));
        }

        /// <summary>
        /// Sets the progress bar for this plugin
        /// </summary>
        /// <param name="val"></param>
        /// <param name="max"></param>
        public void ShowProgress(double val, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(val, max));
        }

        /// <summary>
        /// Returns a formated string with all plugs from a given substitution string
        /// This method should be move to some more adequate place
        /// </summary>
        /// <param name="pb">The substitution string for a plugboard</param>
        /// <returns>A list of plugs</returns>
        public string pB2String(string pb)
        {
            if (pb.Length != settings.Alphabet.Length)
                return "-- no plugs --";


            StringBuilder result = new StringBuilder();

            for (int i = 0; i < settings.Alphabet.Length; i++)
            {
                if (settings.Alphabet[i] != pb[i] && !result.ToString().Contains(settings.Alphabet[i]))
                {
                    if (result.Length > 0)
                        result.Append(' ');

                    result.Append(settings.Alphabet[i].ToString() + pb[i].ToString());
                }
            }

            if (result.Length == 0)
                result.Append("-- no plugs --");

            return result.ToString();
        }

        public IDictionary<string, double[]> GetStatistics(int gramLength)
        {
            // FIXME: inputTriGrams is not being used!

            // FIXME: implement exception handling

            if (!statistics.ContainsKey(gramLength))
            {
                LogMessage("Trying to load default statistics for " + gramLength + "-grams", NotificationLevel.Info);
                statistics[gramLength] = LoadDefaultStatistics(gramLength);
            }

            return statistics[gramLength];
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
