/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
namespace Cryptool.Plugins.CostFunction
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "CostFunction", "CostFunction", null, "CostFunction/icon.png")]
    public class CostFunction : IAnalysisMisc
    {
        #region private variables
        private CostFunctionSettings settings = new CostFunctionSettings();
        private byte[] inputText = null;
        private byte[] outputText = null;
        private double value = 0;
        private Boolean stopped = true;
        private IControlCost controlSlave;
        private String bigramInput;
        private double[,] bigramMatrix;
        private IDictionary<string, double[]> corpusGrams;

        private DataManager dataMgr = new DataManager();
        private const string DATATYPE = "transposition";

        private IDictionary<String, DataFileMetaInfo> txtList;
        private IDictionary<int, IDictionary<string, double[]>> statistics;

        #endregion
        #region internal constants
        internal const int ABSOLUTE = 0;
        internal const int PERCENTAGED = 1;
        internal const int LOG2 = 2;
        internal const int SINKOV = 3;
        #endregion
        #region CostFunctionInOut

        [PropertyInfo(Direction.InputData, "Text Input", "Input your Text here", "", DisplayLevel.Beginner)]
        public byte[] InputText
        {
            get
            {
                return inputText;
            }
            set
            {
                this.inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "Text Output", "Your Text will be send here", "", DisplayLevel.Beginner)]
        public byte[] OutputText
        {
            get
            {
                return outputText;
            }
            set
            {
                this.outputText = value;
                OnPropertyChanged("OutputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "Value", "The value of the function will be send here", "", DisplayLevel.Beginner)]
        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        [PropertyInfo(Direction.ControlSlave, "SDES Slave", "Direct access to SDES.", "", DisplayLevel.Beginner)]
        public IControlCost ControlSlave
        {
            get
            {
                if (controlSlave == null)
                    controlSlave = new CostFunctionControl(this);
                return controlSlave;
            }
        }

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;


        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (CostFunctionSettings)value; }
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
            this.stopped = false;
        }

        public void Execute()
        {
            if (this.InputText is Object && this.stopped == false)
            {
                int bytesToUse = 0;
                try
                {
                    bytesToUse = int.Parse(settings.BytesToUse);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Entered bytesToUse is not an integer: " + ex.Message, NotificationLevel.Error);
                    return;
                }

                if (bytesToUse > this.InputText.Length)
                {
                    bytesToUse = 0;
                }

                byte[] array;

                if (bytesToUse > 0)
                {
                    //Create a new Array of size of bytesToUse if needed
                    array = new byte[bytesToUse];
                    for (int i = 0; i < bytesToUse && i < this.InputText.Length; i++)
                    {
                        array[i] = InputText[i];
                    }
                }
                else
                {
                    array = this.InputText;
                }

                ProgressChanged(0.5, 1);
                bigramInput = ByteArrayToString(array);
                switch (settings.FunctionType)
                {

                    case 0: // Index of Coincedence
                        this.Value = calculateIndexOfCoincidence(array);
                        break;

                    case 1: // Entropy
                        this.Value = calculateEntropy(array);
                        break;

                    case 2: // Log 2 Bigrams
                        this.Value = calculateNGrams(bigramInput, 2, 2);
                        break;

                    case 3: // sinkov Bigrams
                        this.Value = calculateNGrams(bigramInput, 2, 3);
                        break;
                    case 4: //percentaged Bigrams
                        this.Value = calculateNGrams(bigramInput, 2, 1);
                        break;
                    case 5: //regular expressions
                        this.Value = regex(bigramInput);
                        break;
                    default:
                        this.Value = -1;
                        break;
                }//end switch               

                this.OutputText = this.InputText;
                ProgressChanged(1, 1);

            }//end if

        }//end Execute


        public void PostExecution()
        {
            this.stopped = true;
        }

        public void Pause()
        {

        }

        public void Stop()
        {
            this.stopped = false;
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

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region private methods


        //public double contains(string input)
        //{
        //    if (settings.Contains == null)
        //    {
        //        GuiLogMessage("There is no text to be searched for. Please insert text in the 'Contains text / Regular Expression' - Textarea", NotificationLevel.Error);
        //        return new Double();
        //    }

        //    if (input.Contains(settings.Contains))
        //    {
        //        return 1.0;
        //    }
        //    return -1.0;
        //}

        public double regex(string input)
        {
            if (settings.RegEx == null)
            {
                GuiLogMessage("There is no Regular Expression to be searched for. Please insert regex in the 'Regular Expression' - Textarea", NotificationLevel.Error);
                return new Double();
            }
            try
            {
                Match match = Regex.Match(input, settings.RegEx);
                if (match.Success)
                {
                    return 1.0;
                }
                else
                {
                    return -1.0;
                }
            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message, NotificationLevel.Error);
                return -1.0;
            }

        }


        /// <summary>
        /// Calculates the Index of Coincidence multiplied with 100 of
        /// a given byte array
        /// 
        /// for example a German text has about 7.62
        ///           an English text has about 6.61
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateIndexOfCoincidence(byte[] text)
        {
            return calculateIndexOfCoincidence(text, text.Length);
        }

        /// <summary>
        /// Calculates the Index of Coincidence multiplied with 100 of
        /// a given byte array
        /// 
        /// for example a German text has about 7.62
        ///           an English text has about 6.61
        /// </summary>
        /// <param name="text">text to use</param>
        /// <param name="text">bytesToUse</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateIndexOfCoincidence(byte[] text, int bytesToUse)
        {            
            if (bytesToUse > text.Length)
                bytesToUse = text.Length;

            double[] n = new double[256];
            //count all ASCII symbols 
            int counter = 0;
            foreach (byte b in text)
            {
                n[b]++;
                counter++;
                if (counter == bytesToUse)
                    break;
            }

            double coindex = 0;
            //sum them
            for (int i = 0; i < n.Length; i++)
            {
                coindex = coindex + n[i] * (n[i] - 1);
            }

            coindex = coindex / (bytesToUse);
            coindex = coindex / (bytesToUse - 1);

            return coindex * 100;

        }//end calculateIndexOfCoincidence


        private int lastUsedSize = -1;
        private double[] xlogx;
        private Mutex prepareMutex = new Mutex();

        private void prepareEntropy(int size)
        {
            xlogx = new double[size + 1];
            //precomputations for fast entropy calculation	
            xlogx[0] = 0.0;
            for (int i = 1; i <= size; i++)
                xlogx[i] = -1.0 * i * Math.Log(i / (double)size) / Math.Log(2.0);
        }

        /// <summary>
        /// Calculates the Entropy of a given byte array 
        /// for example a German text has about 4.0629
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Entropy</returns>
        public double calculateEntropy(byte[] text)
        {
            return calculateEntropy(text, text.Length);
        }

        /// <summary>
        /// Calculates the Entropy of a given byte array 
        /// for example a German text has about 4.0629
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Entropy</returns>
        public double calculateEntropy(byte[] text, int bytesToUse)
        {
            return NativeCryptography.Crypto.calculateEntropy(text, bytesToUse);
            if (bytesToUse > text.Length)
                bytesToUse = text.Length;

            if (lastUsedSize != bytesToUse)
            {
                try
                {
                    prepareMutex.WaitOne();
                    if (lastUsedSize != bytesToUse)
                    {
                        prepareEntropy(bytesToUse);
                        lastUsedSize = bytesToUse;
                    }
                }
                finally
                {
                    prepareMutex.ReleaseMutex();
                }
            }

            int[] n = new int[256];
            //count all ASCII symbols
            for (int counter = 0; counter < bytesToUse; counter++)
            {
                n[text[counter]]++;
            }

            double entropy = 0;
            //calculate probabilities and sum entropy
            for (int i = 0; i < 256; i++)
                entropy += xlogx[n[i]];

            return entropy / (double)bytesToUse;

        }//end calculateEntropy

        /// <summary>
        /// This method calculates a trigram log2 score of a given text on the basis of a given grams dictionary.
        /// Case is insensitive.
        /// </summary>
        /// <param name="input">The text to be scored</param>
        /// <param name="length">n-gram length</param>
        /// <returns>The trigram score result</returns>
        public double calculateNGrams(string input, int length, int valueSelection)
        {
            this.statistics = new Dictionary<int, IDictionary<string, double[]>>();
            double score = 0;
            if (corpusGrams == null)
            { corpusGrams = GetStatistics(length); }
            input = input.ToUpper();
            // FIXME: case handling?

            HashSet<string> inputGrams = new HashSet<string>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                // ensure each n-gram is counted only once
                if (inputGrams.Add(g))
                {
                    if (corpusGrams.ContainsKey(g))
                    {
                        score += corpusGrams[g][valueSelection];

                    }
                }
            }

            return score;
        }
        public IDictionary<string, double[]> GetStatistics(int gramLength)
        {
            // FIXME: inputTriGrams is not being used!

            // FIXME: implement exception handling
            if (!statistics.ContainsKey(gramLength))
            {
                //GuiLogMessage("Trying to load default statistics for " + gramLength + "-grams", NotificationLevel.Info);
                statistics[gramLength] = LoadDefaultStatistics(gramLength);
            }

            return statistics[gramLength];
        }

        private IDictionary<string, double[]> LoadDefaultStatistics(int length)
        {
            txtList = dataMgr.LoadDirectory(DATATYPE);

            return calculateAbsolutes(txtList["2gram.txt"].DataFile.FullName);
        }

        private IDictionary<string, double[]> calculateAbsolutes(String path)
        {


            Dictionary<string, double[]> grams = new Dictionary<string, double[]>();

            StreamReader reader = new StreamReader(path);
            String text = reader.ReadToEnd();

            text.ToUpper();
            text = Regex.Replace(text, "[^A-Z]*", "");


            for (int i = 0; i < text.Length - 1; i++)
            {
                char a = text[i];
                char b = text[i + 1];
                String key = a.ToString();
                key = key + b.ToString();

                if (!grams.ContainsKey(key))
                {
                    grams.Add(key, new double[] { 1, 0, 0, 0 });
                }
                else
                {
                    grams[key][0] = grams[key][0] + 1.0;
                }
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



            return grams;
        }

        public string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }


        #endregion


    }

    #region slave

    public class CostFunctionControl : IControlCost
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        #region IControlCost Members

        private CostFunction plugin;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
        public CostFunctionControl(CostFunction plugin)
        {
            this.plugin = plugin;
        }

        public int getBytesToUse()
        {
            try
            {
                return int.Parse(((CostFunctionSettings)this.plugin.Settings).BytesToUse);
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesToUse is not an integer: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns the relation operator of the cost function which is set by by CostFunctionSettings
        /// </summary>
        /// <returns>RelationOperator</returns>
        public RelationOperator getRelationOperator()
        {
            switch (((CostFunctionSettings)this.plugin.Settings).FunctionType)
            {
                case 0: //Index of coincidence 
                    return RelationOperator.LargerThen;
                case 1: //Entropy
                    return RelationOperator.LessThen;
                case 2: // Bigrams: log 2
                    return RelationOperator.LessThen;
                case 3: // Sinkov
                    return RelationOperator.LargerThen;
                case 4: // percentage
                    return RelationOperator.LargerThen;
                case 5: // Regular Expression
                    return RelationOperator.LargerThen;
                default:
                    throw new NotImplementedException("The value " + ((CostFunctionSettings)this.plugin.Settings).FunctionType + " is not implemented.");
            }//end switch
        }//end getRelationOperator

        /// <summary>
        /// Calculates the cost function of the given text
        /// 
        /// Cost function can be set by CostFunctionSettings
        /// This algorithm uses a bytesToUse which can be set by CostFunctionSettings
        /// If bytesToUse is set to 0 it uses the whole text
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>cost</returns>
        public double calculateCost(byte[] text)
        {
            int bytesToUse = 0;
            try
            {
                bytesToUse = ((CostFunctionSettings)this.plugin.Settings).BytesToUseInteger;
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesToUse is not an integer: " + ex.Message);
            }

            switch (((CostFunctionSettings)this.plugin.Settings).FunctionType)
            {
                case 0: //Index of coincidence 
                    return plugin.calculateIndexOfCoincidence(text, bytesToUse);
                case 1: //Entropy
                    return plugin.calculateEntropy(text, bytesToUse);
                case 2: // Bigrams: log 2
                    return plugin.calculateNGrams(plugin.ByteArrayToString(text), 2, 2);
                case 3: // Bigrams: Sinkov
                    return plugin.calculateNGrams(plugin.ByteArrayToString(text), 2, 3);
                case 4: // Bigrams: Percentaged
                    return plugin.calculateNGrams(plugin.ByteArrayToString(text), 2, 1);
                case 5: // regular expression
                    return plugin.regex(plugin.ByteArrayToString(text));
                default:
                    throw new NotImplementedException("The value " + ((CostFunctionSettings)this.plugin.Settings).FunctionType + " is not implemented.");
            }//end switch
        }


    #endregion
    }

}

