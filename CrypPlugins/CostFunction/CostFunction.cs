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

namespace Cryptool.Plugins.CostFunction
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "CostFunction", "CostFunction", null, "CostFunction/icon.png")]
    class CostFunction : IAnalysisMisc
    {
        #region private variables
        private CostFunctionSettings settings = new CostFunctionSettings();
        private byte[] inputText = null;
        private byte[] outputText = null;
        private double value = 0;
        private Boolean stopped = true;
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

        [PropertyInfo(Direction.OutputData, "Value", "The value of the function wull be send here", "", DisplayLevel.Beginner)]
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
                int blocksize = 0;
                try
                {
                    blocksize = int.Parse(settings.Blocksize);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Entered blocksize is not an integer: " + ex.Message, NotificationLevel.Error);
                    return;
                }
                byte[] array;

                if (blocksize > 0)
                {
                    //Create a new Array of size of Blocksize if needed
                    array = new byte[blocksize];
                    for (int i = 0; i < blocksize && i < this.InputText.Length; i++)
                    {
                        array[i] = InputText[i];
                    }
                }
                else
                {
                    array = this.InputText;
                }

                ProgressChanged(0.5, 1); 

                switch (settings.FunctionType)
                {

                    case 0: // Index of Coincedence
                        this.Value = calculateIndexOfCoincidence(array);
                        break;

                    case 1: // Entropy
                        this.Value = calculateEntropy(array);
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

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region private methods

        /*
        * Calculates the Index of Coincidence multiplied with 100 of
        * a given byte array
        * 
        * for example a German text has about 7.62
        *             an English text has about 6.61
        */
        private static double calculateIndexOfCoincidence(byte[] text)
        {

            double[] n = new double[256];
            //count all ASCII symbols 
            foreach (byte b in text)
            {
                if (b >= 0 && b <= 255)
                    n[b]++;
            }

            double coindex = 0;
            //sum them
            for (int i = 0; i < n.Length; i++)
            {
                coindex = coindex + n[i] * (n[i] - 1);
            }

            coindex = coindex / (text.Length * (text.Length - 1));
            return coindex * 100;

        }//end calculateIndexOfCoincidence

        /*
         * Calculates the Entropy of
         * a given byte array
         * 
         * for example a German text has about 4.0629
         */
        private static double calculateEntropy(byte[] text)
        {

            double[] n = new double[256];
            //count all ASCII symbols 
            foreach (byte b in text)
            {
                if (b >= 0 && b <= 255)
                    n[b]++;
            }

            double entropy = 0;
            //calculate probabilities and sum entropy
            for (int i = 0; i < n.Length; i++)
            {
                double pz = n[i] / text.Length; //probability of character n[i]
                if (pz > 0)
                    entropy = entropy + pz * Math.Log(pz, 2);
            }

            return -1 * entropy; // because of log we have negative values, but we want positive

        }//end calculateEntropy

        #endregion
    }
}
