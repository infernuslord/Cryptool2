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
using System.Numerics;

namespace Cryptool.Plugins.Numbers
{
    [Author("Sven Rech, Nils Kopal", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.Numbers.Properties.Resources", "PluginOperationCaption", "PluginOperationTooltip", "PluginOperationDescriptionURL", "Numbers/icons/plusIcon.png", "Numbers/icons/minusIcon.png", "Numbers/icons/timesIcon.png", "Numbers/icons/divIcon.png", "Numbers/icons/powIcon.png", "Numbers/icons/gcdicon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    class NumberOperations : ICrypComponent
    {

        #region private variable
        //The variables are defined
        private BigInteger input1 = 0; 
        private BigInteger input2 = 0;
        private BigInteger mod = 0;
        private BigInteger output = 0;
        private NumberSettings settings = new NumberSettings();

        #endregion

        #region event

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;      

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region public

        public NumberOperations()
        {
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        /// <summary>
        /// The inputs are defined.
        /// Only BigInteger are accepted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", true)]
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

        
        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip", true)]
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

        
        [PropertyInfo(Direction.InputData, "ModCaption", "ModTooltip")]
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
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
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
            set { settings = (NumberSettings)value; }
        }


        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            input1 = 0;
            input2 = 0;
            mod = 0;
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
                            if (Mod != 0)
                                Output = (Input1 + Input2) % Mod;
                            else
                                Output = Input1 + Input2;
                            break;
                        // x - y
                        case 1:
                            if (Mod != 0)
                                Output = (Input1 - Input2) % Mod;
                            else
                                Output = Input1 - Input2;
                            break;
                        //x * y
                        case 2:
                            if (Mod != 0)
                                Output = (Input1 * Input2) % Mod;
                            else
                                Output = Input1 * Input2;
                            break;
                        // x / y
                        case 3:
                            if (Mod != 0)
                                Output = (Input1 / Input2) % Mod;
                            else
                                Output = Input1 / Input2;
                            break;
                        // x ^ y
                        case 4:
                            if (Mod != 0)
                            {
                                if (Input2 >= 0)
                                    Output = BigInteger.ModPow(Input1, Input2, Mod);
                                else
                                {
                                    Output = BigInteger.ModPow(BigIntegerHelper.ModInverse(Input1, Mod), -Input2, Mod);
                                }
                            }
                            else
                            {
                                Output = BigInteger.Pow(Input1, (int)Input2);
                            }
                            break;
                        // gcd(x,y)
                        case 5:
                            Output = BigInteger.GreatestCommonDivisor(Input1, Input2);
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

        public void Stop()
        {
        }

        public void Initialize()
        {            
            //change to the correct icon which belongs to actual selected arithmetic function 
            ((NumberSettings)this.settings).changeToCorrectIcon(((NumberSettings)this.settings).Operat);
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
