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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.BigNumber
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "BigNumberOperation", "Big Number Operation", null, "BigNumber/icons/plusIcon.png", "BigNumber/icons/minusIcon.png", "BigNumber/icons/timesIcon.png", "BigNumber/icons/divIcon.png", "BigNumber/icons/powIcon.png")]
    class BigNumberOperation : IThroughput
    {

        public BigNumberOperation()
        {
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        #region Properties

        private BigInteger input1 = null;
        [PropertyInfo(Direction.InputData, "x Input", "Number Input 1", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
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

        private BigInteger input2 = null;
        [PropertyInfo(Direction.InputData, "y Input", "Number Input 2", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
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

        private BigInteger mod = null;
        [PropertyInfo(Direction.InputData, "Modulo", "Modulo Input", "", DisplayLevel.Beginner)]
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

        private BigInteger output = null;
        [PropertyInfo(Direction.OutputData, "Output", "Number Output", "", DisplayLevel.Beginner)]
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

        #endregion

        #region IPlugin Members

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private BigNumberOperationSettings settings = new BigNumberOperationSettings();
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
        }

        public void Execute()
        {
            if(input1 is object && input2 is object)
            {
                
                ProgressChanged(0.5, 1.0);
                try
                {
                    switch (settings.Operat)
                    {
                        case 0:
                            if (Mod is object)
                                Output = (Input1 + Input2) % Mod;
                            else
                                Output = Input1 + Input2;
                            break;
                        case 1:
                            if (Mod is object)
                                Output = (Input1 - Input2) % Mod;
                            else
                                Output = Input1 - Input2;
                            break;
                        case 2:
                            if (Mod is object)
                                Output = (Input1 * Input2) % Mod;
                            else
                                Output = Input1 * Input2;
                            break;
                        case 3:
                            if (Mod is object)
                                Output = (Input1 / Input2) % Mod;
                            else
                                Output = Input1 / Input2;
                            break;
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
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }
    }
}
