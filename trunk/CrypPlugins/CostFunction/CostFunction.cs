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
using System.Collections;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;

namespace Cryptool.Plugins.CostFunction
{
    [Author("Nils Kopal, Simon Malischewski", "Nils.Kopal@cryptool.org , malischewski@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CostFunction.Properties.Resources", "PluginCaption", "PluginTooltip", "CostFunction/DetailedDescription/doc.xml", "CostFunction/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CostFunction : ICrypComponent
    {
        #region private variables

        private CostFunctionSettings settings = new CostFunctionSettings();
        private byte[] inputText = null;
        private double value = 0.0;
        private Boolean stopped = true;
        private IControlCost controlSlave;
        private String bigramInput;

        private IDictionary<string, double[]> corpusBigrams; // Used for Weighted Bigrams/Trigrams Cost function
        private IDictionary<string, double[]> corpusTrigrams;

        //Fitness Weight Table for Weighted Bigrams/Trigrams
        private IDictionary<string, double> fwt = new Dictionary<string, double>();

        //Weights
        private double beta = 1.0;
        private double gamma = 1.0;

        private DataManager dataMgr = new DataManager(); 
        private const string DATATYPE = "transposition";

        private IDictionary<String, DataFileMetaInfo> txtList;
        private IDictionary<int, IDictionary<string, double[]>> statistics = new Dictionary<int, IDictionary<string, double[]>>();

        private RegEx regularexpression = null;

        #endregion

        #region internal constants

        internal const int ABSOLUTE = 0;
        internal const int PERCENTAGED = 1;
        internal const int LOG2 = 2;
        internal const int SINKOV = 3;

        #endregion

        #region public interface

        public CostFunction()
        {
            this.settings = new CostFunctionSettings();
            this.settings.PropertyChanged += settings_OnPropertyChange;
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StatisticsCorpus")
            {
                // delete statistics corpuses after a language change,
                // so that the correct language can be loaded
                corpusBigrams = null;
                corpusTrigrams = null;
                statistics.Clear();
            }
        }

        #endregion

        #region CostFunctionInOut

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip")]
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

        #region testing

        public void changeFunctionType(int type)
        {
            this.settings.changeFunctionType(type);
        }

        public void setDataPath(string path)
        {
            this.testing = true;
            this.settings.customFilePath = path;
        }

        public void setRegEx(string regex)
        {
            this.settings.RegEx = regex;
        }

        #endregion

        [PropertyInfo(Direction.OutputData, "ValueCaption", "ValueTooltip")]
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

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
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

        public void PreExecution()
        {
            this.stopped = false;
        }

        public void Execute()
        {
            if (this.InputText != null && this.stopped == false)
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

                int bytesOffset = 0;
                try
                {
                    bytesOffset = int.Parse(settings.BytesOffset);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Entered bytesOffset is not an integer: " + ex.Message, NotificationLevel.Error);
                    return;
                }

                if (bytesToUse == 0 || bytesToUse > (this.InputText.Length - bytesOffset))
                {
                    bytesToUse = this.InputText.Length - bytesOffset;
                }

                //Create a new Array of size of bytesToUse
                byte[] array = new byte[bytesToUse];
                Array.Copy(InputText, bytesOffset, array, 0, bytesToUse);

                ProgressChanged(0.5, 1);

                this.Value = CalculateCost(array);

                ProgressChanged(1, 1);

            }//end if

        }

        public double CalculateCost(byte[] text)
        {
            switch (settings.FunctionType)
            {
                case 0: //Index of coincidence 
                    if (settings.BlockSize == 1)
                        return calculateFastIndexOfCoincidence(text, settings.BytesToUseInteger);
                    return calculateIndexOfCoincidence(text, settings.BytesToUseInteger, settings.BlockSize);

                case 1: //Entropy
                    return calculateEntropy(text, settings.BytesToUseInteger);

                case 2: // N-grams: log 2
                    return calculateNGrams(ByteArrayToString(text), settings.NgramSize, LOG2, false);

                case 3: // N-grams: Sinkov
                    return calculateNGrams(ByteArrayToString(text), settings.NgramSize, SINKOV, true);

                case 4: // N-grams: Percentaged
                    return calculateNGrams(ByteArrayToString(text), settings.NgramSize, PERCENTAGED, false);

                case 5: // regular expression
                    return regex(text);

                case 6:
                    return calculateWeighted(ByteArrayToString(text));

                default:
                    throw new NotImplementedException("The value " + settings.FunctionType + " is not implemented.");
            }
        }

        public void PostExecution()
        {
            this.stopped = true;
        }

        public void Stop()
        {
            this.stopped = false;
        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
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

        // Reads data directory, passes filepaths to parser
        private void fillfwt() {
             txtList = dataMgr.LoadDirectory(DATATYPE);

             switch (this.settings.weighttable)
             {
                 case 0:
                     parseCSV(txtList["fwtmatthews"].DataFile.FullName);
                     break;
                 case 1:
                     parseCSV(txtList["fwttoemeharumugam"].DataFile.FullName);
                     break;
                 case 2:
                     parseCSV(this.settings.customfwtpath);
                     break;
             }


              
        }
        // simple "parser" for CSV files
        private void parseCSV(string path)
        {
            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;

                while ((line = readFile.ReadLine()) != null)
                {

                    row = line.Split(';');
                    if (row.Length == 2)
                    {
                        fwt.Add(row[0], Double.Parse(row[1]));
                    }
                }
            }
            
        }

        private void weights(string ngram, int ngramlength)
        {
            if (fwt.TryGetValue(ngram, out value) && ngramlength == 2)
            {
                beta += value;
            }

            if (fwt.TryGetValue(ngram, out value) && ngramlength == 3)
            {
                gamma += value;
            }


        }

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

        public double calculateWeighted(string input)
        {
            this.statistics = new Dictionary<int, IDictionary<string, double[]>>();

            if (fwt == null) fillfwt();

            if (corpusBigrams == null) corpusBigrams = GetStatistics(2); // Get Known Language statistics for Bigrams
            if (corpusTrigrams == null) corpusTrigrams = GetStatistics(3); // and Trigrams

            input = input.ToUpper();

            double bigramscore = calculateNGrams(input, 2, SINKOV, true); // Sinkov
            double trigramscore = calculateNGrams(input, 3, SINKOV, true);
         
            return (beta * bigramscore) + (gamma * trigramscore);
        }

        private string lastRegex = null;
        private bool lastCaseInsensitive;

        public double regex(byte[] input)
        {
            if (settings.RegEx == null)
            {
                GuiLogMessage("There is no Regular Expression to be searched for. Please insert regex in the 'Regular Expression' - Textarea", NotificationLevel.Error);
                return -1.0;
            }

            if (lastRegex != settings.RegEx || lastCaseInsensitive != settings.CaseInsensitive)
            {
                regularexpression = new RegEx(settings.RegEx, settings.CaseInsensitive);
                lastRegex = settings.RegEx;
                lastCaseInsensitive = settings.CaseInsensitive;
            }

            try
            {
                return regularexpression.MatchesValue(input);
            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message, NotificationLevel.Error);
                return Double.NegativeInfinity;
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
        public double calculateIndexOfCoincidence(byte[] text, int blocksize = 1)
        {
            return calculateIndexOfCoincidence(text, text.Length, blocksize);
        }

        /// <summary>
        /// Calculates the Index of Coincidence of
        /// a given byte array using different "block sizes"
        /// block size means that a block of symbols is used as if it would be a single symbol
        /// This can be used for example for breaking the ADFGVX where bigrams count as a single symbol
        /// 
        /// for example a German text has about 0.0762
        ///           an English text has about 0.0661
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateIndexOfCoincidence(byte[] text, int bytesToUse, int blocksize = 1)
        {
            if (bytesToUse > text.Length)
                bytesToUse = text.Length;

            var n = new Dictionary<String, int>();
            for (var i = 0; i < bytesToUse / blocksize; i++)
            {
                var b = new byte[blocksize];
                Array.Copy(text, i * blocksize, b, 0, blocksize);
                var key = Encoding.ASCII.GetString(b);

                if (!n.Keys.Contains(key))
                {
                    n.Add(key, 0);
                }
                n[key] = n[key] + 1;
            }

            double coindex = 0;
            //sum them
            for (var i = 0; i < n.Count; i++)
            {
                coindex = coindex + n.Values.ElementAt(i) * (n.Values.ElementAt(i) - 1);
            }

            coindex = coindex / (bytesToUse / (double)blocksize);
            coindex = coindex / ((bytesToUse / (double)blocksize) - 1);

            return coindex;

        }//end calculateIndexOfCoincidence

        /// <summary>
        /// Calculates the Index of Coincidence of
        /// a given byte array only for single letters
        /// (Fast implementation)
        /// 
        /// for example a German text has about 0.0762
        ///           an English text has about 0.0661
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Index of Coincidence</returns>
        public double calculateFastIndexOfCoincidence(byte[] text, int bytesToUse)
        {
            if (bytesToUse > text.Length)
                bytesToUse = text.Length;

            var n = new double[256];
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

            return coindex;

        }//end calculateFastIndexOfCoincidence

        private int lastUsedSize = -1;
        private float[] xlogx;
        private Mutex prepareMutex = new Mutex();
        private bool testing = false;

        private void prepareEntropy(int size)
        {
            xlogx = new float[size + 1];
            //precomputations for fast entropy calculation	
            xlogx[0] = 0.0f;
            for (int i = 1; i <= size; i++)
                xlogx[i] = (float) (-1.0f * i * Math.Log(i / (double)size) / Math.Log(2.0));
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
            switch (this.settings.entropyselect)
            {
                case 0:
                    return NativeCryptography.Crypto.calculateEntropy(text, bytesToUse);
                case 1:
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

                    float entropy = 0;
                    //calculate probabilities and sum entropy
                    for (int i = 0; i < 256; i++)
                        entropy += xlogx[n[i]];

                    return entropy / (double)bytesToUse;
                default:
                    return NativeCryptography.Crypto.calculateEntropy(text, bytesToUse);
            }
        }//end calculateEntropy

        /// <summary>
        /// This method calculates a trigram log2 score of a given text on the basis of a given grams dictionary.
        /// Case is insensitive.
        /// </summary>
        /// <param name="input">The text to be scored</param>
        /// <param name="length">n-gram length</param>
        /// <returns>The trigram score result</returns>
        public double calculateNGrams(string input, int length, int valueSelection, bool weighted)
        {
            //this.statistics = new Dictionary<int, IDictionary<string, double[]>>();
            double score = 0;

            if (corpusBigrams == null && length == 2) corpusBigrams = GetStatistics(length);
            if (corpusTrigrams == null && length == 3) corpusTrigrams = GetStatistics(length);
            var corpus = GetStatistics(length);

            input = input.ToUpper();
            // FIXME: case handling?

            HashSet<string> inputGrams = new HashSet<string>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                // ensure each n-gram is counted only once
                if (!weighted || inputGrams.Add(g))
                {
                    if (corpus.ContainsKey(g))
                    {
                        score += corpus[g][valueSelection];
                        if (weighted) weights(g, length);
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

            if (testing) return calculateAbsolutes(this.settings.customFilePath, length);

            switch (this.settings.StatisticsCorpus)
            {
                case 2:
                    return calculateAbsolutes(this.settings.customFilePath, length);
                
                case 1:
                    return calculateAbsolutes(txtList["statisticscorpusen"].DataFile.FullName, length);

                case 0:
                default:
                    return calculateAbsolutes(txtList["statisticscorpusde"].DataFile.FullName, length);
            }
        }

        private IDictionary<string, double[]> calculateAbsolutes(String path, int length)
        {
            Dictionary<string, double[]> grams = new Dictionary<string, double[]>();
            StreamReader reader = new StreamReader(path);

            String text = reader.ReadToEnd();
            text = text.ToUpper();
            text = text.Replace("Ä", "AE").Replace("Ö", "OE").Replace("Ü", "UE").Replace("ß", "SS");
            text = Regex.Replace(text, "[^A-Z]*", "");

            for (int i = 0; i+length <= text.Length; i++)
            {
                String key = text.Substring(i, length);

                if (!grams.ContainsKey(key))
                    grams.Add(key, new double[] { 0, 0, 0, 0 });

                grams[key][0]++;
            }

            //double sum = grams.Values.Sum(item => item[ABSOLUTE]);
            int sum = text.Length - length + 1;
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

        private readonly CostFunction plugin;
        private readonly CostFunctionSettings settings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
        public CostFunctionControl(CostFunction plugin)
        {
            this.plugin = plugin;
            this.settings = (CostFunctionSettings) this.plugin.Settings;
        }

        public string ModifyOpenCLCode(string code)
        {
            switch (settings.FunctionType)
            {
                case 0: //Index of coincidence 
                    return ModifyOpenCLCodeIndexOfCoincidence(code, bytesToUse());
                case 1: //Entropy
                    return ModifyOpenCLCodeEntropy(code, bytesToUse());
                case 5: // Regular Expression
                    var regex = new RegEx(settings.RegEx, settings.CaseInsensitive);
                    return regex.ModifyOpenCLCode(code, bytesToUse());
                default:
                    throw new NotImplementedException("The value " + settings.FunctionType + " is not implemented for OpenCL.");
            }//end switch
        }

        private string ModifyOpenCLCodeEntropy(string code, int bytesToUse)
        {
            //declaration code:
            float[] xlogx = new float[bytesToUse + 1];
            xlogx[0] = 0.0f;
            for (int i = 1; i <= bytesToUse; i++)
                xlogx[i] = (float) (-1.0f * i * Math.Log(i / (float)bytesToUse) / Math.Log(2.0));

            string declaration = string.Format("__constant float xlogx[{0}] = {{ \n", bytesToUse + 1);
            foreach (float xlx in xlogx)
            {
                declaration += xlx.ToString("F9", System.Globalization.CultureInfo.InvariantCulture) + "f, ";
            }
            declaration = declaration.Substring(0, declaration.Length - 2);
            declaration += " }; \n";
            
            code = code.Replace("$$COSTFUNCTIONDECLARATIONS$$", declaration);

            //initialization code:
            code = code.Replace("$$COSTFUNCTIONINITIALIZE$$", "unsigned char distr[256]; \n for (int c = 0; c < 256; c++) \n distr[c]=0; \n");

            //calculation code:
            code = code.Replace("$$COSTFUNCTIONCALCULATE$$", "distr[c]++;");

            //result calculation code:
            code = code.Replace("$$COSTFUNCTIONRESULTCALCULATION$$", "int i = 0; \n " 
                + "while (i<256) { \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n "
                + "result += xlogx[distr[i++]]; \n " 
                + "} \n "
                + string.Format("result /= {0}.0f;", bytesToUse));

            return code;
        }

        private string ModifyOpenCLCodeIndexOfCoincidence(string code, int bytesToUse)
        {
            code = code.Replace("$$COSTFUNCTIONDECLARATIONS$$", "");

            //initialization code:
            code = code.Replace("$$COSTFUNCTIONINITIALIZE$$", "unsigned char distr[256]; \n for (int c = 0; c < 256; c++) \n distr[c]=0; \n");

            //calculation code:
            code = code.Replace("$$COSTFUNCTIONCALCULATE$$", "distr[c]++;");

            //result calculation code:
            code = code.Replace("$$COSTFUNCTIONRESULTCALCULATION$$", "for (int i = 0; i < 256; i++) \n { \n "
                + "result += distr[i] * (distr[i] - 1) ; \n "
                + "} \n "
                + string.Format("result /= {0}.0f; \n", bytesToUse)
                + string.Format("result /= {0}.0f; \n", (bytesToUse-1)));                
                //+ "result *= 100.0f; \n" );

            return code;
        }

        /// <summary>
        /// Return bytes to use setting.
        /// Throws exception if setting is invalid.
        /// </summary>
        /// <returns></returns>
        public int GetBytesToUse()
        {
            try
            {
                return int.Parse(settings.BytesToUse);
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesToUse is not an integer: " + ex.Message);
            }
        }

        // Just return the number for internal use. Don't care about input errors, use whatever we have.
        private int bytesToUse()
        {
            return settings.BytesToUseInteger;
        }

        /// <summary>
        /// Return bytes offset setting.
        /// Throws exception if setting is invalid.
        /// </summary>
        /// <returns></returns>
        public int GetBytesOffset()
        {
            try
            {
                return int.Parse(settings.BytesOffset);
            }
            catch (Exception ex)
            {
                throw new Exception("Entered bytesOffset is not an integer: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns the relation operator of the cost function which is set by by CostFunctionSettings
        /// </summary>
        /// <returns>RelationOperator</returns>
        public RelationOperator GetRelationOperator()
        {
            switch (settings.FunctionType)
            {
                case 0: // Index of coincidence 
                    return RelationOperator.LargerThen;
                case 1: // Entropy
                    return RelationOperator.LessThen;
                case 2: // Log 2
                    return RelationOperator.LargerThen;
                case 3: // Sinkov
                    return RelationOperator.LargerThen;
                case 4: // percentage
                    return RelationOperator.LargerThen;
                case 5: // Regular Expression
                    return RelationOperator.LargerThen;
                case 6: // Weighted Bigrams/Trigrams
                    return RelationOperator.LargerThen;

                default:
                    throw new NotImplementedException("The value " + settings.FunctionType + " is not implemented.");
            }//end switch
        }//end GetRelationOperator

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
        public double CalculateCost(byte[] text)
        {
            /*
             * Note: If being used together with KeySearcher, the text given here is already shortened and thus
             * bytesToUse and bytesOffset will have no further effect (neither positive nor negative).
             */
            return plugin.CalculateCost(text);
        }


    #endregion
    }

}