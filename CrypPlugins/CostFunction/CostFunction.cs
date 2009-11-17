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
                        this.Value = calculateNGrams(bigramInput,2,2);
                        break;

                    case 3: // sinkov Bigrams
                        this.Value = calculateNGrams(bigramInput,2,3);
                        break;
                    case 4: //percentaged Bigrams
                        this.Value = calculateNGrams(bigramInput,2,1);
                        break;
                    case 5: // alternative Bigram
                        this.Value = relativeBigramFrequency(bigramInput);
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

            double entropy = 0;
            //calculate probabilities and sum entropy
            for (int i = 0; i < n.Length; i++)
            {
                double pz = n[i] / bytesToUse; //probability of character n[i]
                if (pz > 0)
                    entropy = entropy + pz * Math.Log(pz, 2);
            }

            return -1 * entropy; // because of log we have negative values, but we want positive

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


        public double relativeBigramFrequency(string input)
        {
            string text = input.ToUpper();
            if (bigramMatrix == null)
            {
                bigramMatrix = getBiGramMatrix();
            }
            double sum = 0.0;
            double count = 0.0;

            for (int i = 0; i < input.Length - 1; i++)
            {
                char a = text[i];
                char b = text[i + 1];

                if (isInAlphabet(a) && isInAlphabet(b))
                {
                    int x = (int)a - 65;
                    int y = (int)b - 65;
                    sum += bigramMatrix[x, y];
                    count++;
                }
            }
            return (sum/count);
        }

        private double[,] getBiGramMatrix()
        {
            double[,] matrix = new double[26, 26];
            StreamReader reader = new StreamReader(Path.Combine(PluginResource.directoryPath, "CostFunctionDeutsch.txt"));
            
                    
            String text;

            while ((text = reader.ReadLine()) != null)
            {
                text = text.ToUpper();
                for (int i = 0; i < text.Length - 1; i++)
                {
                    char a = text[i];
                    char b = text[i + 1];

                    if (isInAlphabet(a) && isInAlphabet(b))
                    {
                        int x = (int)a - 65;
                        int y = (int)b - 65;
                        matrix[x, y] = matrix[x, y] + 1;
                    }
                }

            }

            for (int i = 0; i < 26; i++)
            {
                double count = 0;
                for (int j = 0; j < 26; j++)
                {
                    count = count + matrix[i, j];

                }

                for (int j = 0; j < 26; j++)
                {
                    matrix[i, j] = matrix[i, j] / (count / 100);
                }
            }
            return matrix;
        }

        private bool isInAlphabet(char c)
        {
            int val = (int)(c);
            int test = val - 65;
            if (test >= 0 && test <= 25)
            {
                return true;
            }
            return false;
        }

        private IDictionary<string, double[]> LoadDefaultStatistics(int length)
        {
            Dictionary<string, double[]> grams = new Dictionary<string, double[]>();

            StreamReader reader = new StreamReader(Path.Combine(PluginResource.directoryPath, GetStatisticsFilename(length)));

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;

                string[] tokens = WordTokenizer.tokenize(line).ToArray();
                if (tokens.Length == 0)
                    continue;
                //Debug.Assert(tokens.Length == 2, "Expected 2 tokens, found " + tokens.Length + " on one line");

                grams.Add(tokens[0], new double[] { Double.Parse(tokens[1]), 0, 0, 0 });
            }

            double sum = grams.Values.Sum(item => item[ABSOLUTE]);
            //GuiLogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

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
                case 5: // alternative bigrams
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
                bytesToUse = int.Parse(((CostFunctionSettings)this.plugin.Settings).BytesToUse);
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
                case 5: // alternative Bigram 
                    return plugin.relativeBigramFrequency(plugin.ByteArrayToString(text));

                default:
                    throw new NotImplementedException("The value " + ((CostFunctionSettings)this.plugin.Settings).FunctionType + " is not implemented.");
            }//end switch
        }

        #endregion
    }
    
}

