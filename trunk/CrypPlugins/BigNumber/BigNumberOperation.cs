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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.BigNumber
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "BigNumberOperation", "Big Number Operation", "BigNumber/DetailedDescription/DescriptionOperation.xaml", "BigNumber/icons/plusIcon.png", "BigNumber/icons/minusIcon.png", "BigNumber/icons/timesIcon.png", "BigNumber/icons/divIcon.png", "BigNumber/icons/powIcon.png", "BigNumber/icons/gcdicon.png")]
    class BigNumberOperation : IThroughput
    {

        #region private variable
        //The variables are defined
        private BigInteger input1 = null; 
        private BigInteger input2 = null;
        private BigInteger mod = null;
        private BigInteger output = null;
        private BigNumberOperationSettings settings = new BigNumberOperationSettings();

        #endregion

        #region event

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;      

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region public

        public BigNumberOperation()
        {
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        /// <summary>
        /// The inputs are defined.
        /// Only BigInteger are accepted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "x Input", "Number input 1", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        public BigInteger Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;
                OnPropertyChanged("Input1");
            }
        }

        
        [PropertyInfo(Direction.InputData, "y Input", "Number input 2", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        public BigInteger Input2
        {
            get
            {
                return input2;
            }
            set
            {
                input2 = value;
                OnPropertyChanged("Input2");
            }
        }

        
        [PropertyInfo(Direction.InputData, "Modulo", "Modulo input", "", DisplayLevel.Beginner)]
        public BigInteger Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
                OnPropertyChanged("Mod");
            }
        }

        /// <summary>
        /// The output is defined.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Output", "Number output", "", DisplayLevel.Beginner)]
        public BigInteger Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                OnPropertyChanged("Output");
             }
        }

        
        /// <summary>
        /// Showing the progress change while plug-in is working
        /// </summary>
        /// <param name="value">Value of current process progress</param>
        /// <param name="max">Max value for the current progress</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (BigNumberOperationSettings)value; }
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
            input1 = null;
            input2 = null;
            mod = null;
        }

        /// <summary>
        /// Main method
        /// </summary>
        public void Execute()
        {
            //First checks if both inputs are set
            if (input1 != null && input2 != null)
            {
                ProgressChanged(0.5, 1.0);
                try
                {
                    //As the user changes the operation different outputs are calculated.
                    switch (settings.Operat)
                    {
                        // x + y
                        case 0:
                            if (Mod is object)
                                Output = (Input1 + Input2) % Mod;
                            else
                                Output = Input1 + Input2;
                            break;
                        // x - y
                        case 1:
                            if (Mod is object)
                                Output = (Input1 - Input2) % Mod;
                            else
                                Output = Input1 - Input2;
                            break;
                        //x * y
                        case 2:
                            if (Mod is object)
                                Output = (Input1 * Input2) % Mod;
                            else
                                Output = Input1 * Input2;
                            break;
                        // x / y
                        case 3:
                            if (Mod is object)
                                Output = (Input1 / Input2) % Mod;
                            else
                                Output = Input1 / Input2;
                            break;
                        // x ^ y
                        case 4:
                            if (Mod is object)
                            {
                                if (Input2 >= 0)
                                    Output = Input1.modPow(Input2, Mod);
                                else
                                {
                                    Output = Input1.modInverse(Mod).modPow(-Input2, Mod);
                                }
                            }
                            else
                            {
                                Output = Input1.pow(Input2);
                            }
                            break;
                        // gcd(x,y)
                        case 5:
                                Output = Input1.gcd(Input2);
                            break;
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage("Big Number fail: " + e.Message, NotificationLevel.Error);
                    return;
                }
                ProgressChanged(1.0, 1.0);
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
        }

        public void Initialize()
        {            
            //change to the correct icon which belongs to actual selected arithmetic function 
            ((BigNumberOperationSettings)this.settings).changeToCorrectIcon(((BigNumberOperationSettings)this.settings).Operat);
        }

        public void Dispose()
        {
        }

        #endregion

        #region private        

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        #endregion
    }
}
