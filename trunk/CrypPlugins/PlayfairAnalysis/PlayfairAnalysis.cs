/* HOWTO: Change year, author name and organization.
   Copyright 2010 Christoph Hartmann, Johannes Gutenberg-Universität Mainz

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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Mischel.Collections;
using System.Threading;
using System.Windows;


namespace Cryptool.Plugins.PlayfairAnalysis
{   
    [Author("Christoph Hartmann", "chris-ha@freenet.de", "Johannes Gutenberg-Universität Mainz", "http://www.uni-mainz.de")]
    [PluginInfo(false, "Playfair (Bruteforce) Analysis", "Ciphertext only attack on Playfair", "PlayfairAnalysis/DetailedDescription/Description.xaml", "PlayfairAnalysis/Images/icon.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class PlayfairAnalysis : ISpecific
    {
        #region Private Variables
                
        private PlayfairAnalysisSettings settings = new PlayfairAnalysisSettings();
        private string inputString;
        private string outputString;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private Thread playFairAttackThread;
        private bool executionStopped;
        private double[,] customLogStat;
        private double[] customLogStat2;
        private string alphabet = "";

        #endregion

        #region Data Properties

        /// <summary>
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be analyzed", null)]
        public virtual string InputString
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

        /*
        [PropertyInfo(Direction.InputData, "Bigraph Statistic", "Bigraph Statistic from Playfair Analysis Statistic Plugin", null, false, false, QuickWatchFormat.Hex, null)]
        public virtual double[] CustomLogStat
        {
            get { return this.customLogStat2; }
            set
            {
                if (value != this.customLogStat2)
                {
                    this.customLogStat2 = value;
                    OnPropertyChanged("CustomLogStat");
                }
            }
        }
        */

        CryptoolStream csBigraphStatistic;
        [PropertyInfo(Direction.InputData, "Bigraph Statistic", "Bigraph Statistic from Playfair Analysis Statistic Plugin", null)]
        public CryptoolStream CustomLogStat
        {
            get
            {
                if (this.csBigraphStatistic != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead(this.csBigraphStatistic.FileName);
                    return cs;
                }
                return null;
            }

            set
            {
                if (value != this.csBigraphStatistic)
                {
                    this.csBigraphStatistic = value;
                    OnPropertyChanged("CustomLogStat");
                }
            }
        }




        /// <summary>
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Text output", "The output string after playfair analysis", null)]
        public virtual string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }


        [PropertyInfo(Direction.OutputData, "CryptoolStream output", "The raw CryptoolStream data after playfair analysis", null)]
        public virtual CryptoolStream OutputData
        {
            get
            {
                if (outputString != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputString.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            executionStopped = false;            
        }

       

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            Double[,] BigraphStat;
            string BsPath;
            int matrixSize;                     

            ProgressChanged(0, 1);

            // Check settings
            if (settings.HeapSize < 1)
            {
                System.Windows.MessageBox.Show("Heap size has to be a positiv integer!\nHeap size is set to 5000");
                settings.HeapSize = 5000;
            }


            // BigraphStatistic.CreateBS(@"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\", 6);

            if (settings.UseCustomStatistic == 1)       // Use Bigraph Statistic that is generated by PlayfairAnalysisStatistic plugin 
            {
                int alphabetLength;
                int offset = 0;
                csBigraphStatistic = CustomLogStat;

                alphabetLength = (int)csBigraphStatistic.Read(offset, 1)[0];
                matrixSize = (int)(Math.Sqrt(alphabetLength));

                BigraphStat = new Double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
                byte[] statisticBuffer = new byte[8 * BigraphStat.Length];

                alphabet = "";
                for (int i = 0; i < alphabetLength; i++)
                {
                    alphabet += (char)csBigraphStatistic.Read(offset, 1)[0];
                }

                
                statisticBuffer = csBigraphStatistic.Read(offset, 8 * BigraphStat.Length);
                

                for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
                {                                
                    for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                    {                        
                        BigraphStat[i, j] = BitConverter.ToDouble(statisticBuffer, offset);
                        offset += 8;
                    }                                
                }

                GuiLogMessage("MatrixSize: " + Convert.ToString(matrixSize), NotificationLevel.Info);
                GuiLogMessage("Custom Bigraph Stat successfully set", NotificationLevel.Info);
            }

            else          // Read Bigraph Statistic from xml file
            {                
                switch (settings.MatrixSize)
                {
                    case 0:
                        if (settings.Language == 0)
                        {
                            BsPath = @"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\BSLog10sde.xml";
                        }
                        else
                        {
                            BsPath = @"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\BSLog10seng.xml";
                        }
                        matrixSize = 5;
                        alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

                        break;

                    case 1:
                        if (settings.Language == 0)
                        {
                            BsPath = @"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\BSLog10lde.xml";
                        }
                        else
                        {
                            BsPath = @"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\BSLog10leng.xml";
                        }
                        matrixSize = 6;
                        alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        break;

                    default:
                        BsPath = @"c:\Documents and Settings\PT7130\My Documents\Visual Studio 2010\Projects\Cryptool2\trunk\CrypPlugins\PlayfairAnalysis\Data\BSLog10sde.xml";
                        matrixSize = 5;
                        alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
                        break;
                }

                BigraphStat = new Double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];

                System.Xml.Serialization.XmlSerializer ReadBS = new System.Xml.Serialization.XmlSerializer(typeof(Double[][]));
                System.Xml.XmlReader XmlReader = System.Xml.XmlReader.Create(BsPath);
                Double[][] BigraphStatDummy = (Double[][])ReadBS.Deserialize(XmlReader);
                XmlReader.Close();

                for (int i = 0; i < Math.Pow(matrixSize, 2); i++)
                {
                    for (int j = 0; j < Math.Pow(matrixSize, 2); j++)
                    {
                        BigraphStat[i, j] = BigraphStatDummy[i][j];
                    }
                }

                GuiLogMessage("MatrixSize: " + Convert.ToString(matrixSize), NotificationLevel.Info);
                GuiLogMessage("Bigraph statistics loaded: " + BsPath, NotificationLevel.Info);
            }

            GuiLogMessage("Starting Analysis", NotificationLevel.Info);

            KeySearcher keySearcher = new KeySearcher(matrixSize, settings.HeapSize, BigraphStat, alphabet, InputString);

            keySearcher.LogMessageByKeySearcher += new KeySearcher.LogMessageByKeySearcherEventHandler(OnLogMessageByKeySearcher);
            keySearcher.ProgressChangedByKeySearcher += new KeySearcher.ProgressChangedByKeySearcherEventHandler(OnProgressChangedByKeySearcher);

            playFairAttackThread = new Thread(keySearcher.Attack);
            playFairAttackThread.IsBackground = true;
            
            playFairAttackThread.Start();
            playFairAttackThread.Join();
            
            if (!executionStopped)
            {
                OutputString = keySearcher.PlainText;
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");
                GuiLogMessage("Analysis completed", NotificationLevel.Info);
                ProgressChanged(1, 1);
            }
            else
            {
                GuiLogMessage("Analysis aborted", NotificationLevel.Info);
                ProgressChanged(0, 1);
            }

        }

        

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (playFairAttackThread != null)
            {
                playFairAttackThread.Abort();
            }
            executionStopped = true;
        }

        public void Initialize()
        {
            settings.UseCustomStatistic = 0;
        }

        public void Dispose()
        {
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
                stream.Close();
            }
            listCryptoolStreamsOut.Clear();
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;
                

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        // Events fired by other classes:
                
        void OnLogMessageByKeySearcher(string msg, NotificationLevel loglevel)
        {
            GuiLogMessage(msg, loglevel);
        }

        void OnProgressChangedByKeySearcher(double value, double max)
        {
            ProgressChanged(value, max);
        }


        #endregion

        #region public properties

        public Thread PlayFairAttackThread
        {
            get
            {
                return playFairAttackThread;
            }
            set
            {
                playFairAttackThread = value;
            }
        }

        #endregion
    }

}
