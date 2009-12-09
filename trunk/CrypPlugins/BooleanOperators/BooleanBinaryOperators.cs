﻿/*                              
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false, "Boolean Binary Operator", "Boolean Binary Operator", "BooleanOperators/DetailedDescription/Description.xaml",
        "BooleanOperators/icons/and.png",
        "BooleanOperators/icons/or.png",
        "BooleanOperators/icons/nand.png",
        "BooleanOperators/icons/nor.png",
        "BooleanOperators/icons/xor.png"
        )]
    public class BooleanBinaryOperators : IThroughput
    {
        private Boolean FlagA = false;
        private Boolean FlagB = false;

        private Boolean inputA = false;
        private Boolean inputB = false;
        private Boolean output = false;

        private BooleanBinaryOperatorsSettings settings;

        public BooleanBinaryOperators()
        {
            this.settings = new BooleanBinaryOperatorsSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;

        }

        [PropertyInfo(Direction.InputData, "InputA", "inputA", "", DisplayLevel.Beginner)]
        public Boolean InputA
        {
            get
            {
                return this.inputA;
            }
            set
            {
                this.inputA = value;
                this.FlagA = true;
                OnPropertyChange("InputA");
            }
        }

        [PropertyInfo(Direction.InputData, "InputB", "inputB", "", DisplayLevel.Beginner)]
        public Boolean InputB
        {
            get
            {
                return this.inputB;
            }
            set
            {
                this.inputB = value;
                this.FlagB = true;
                OnPropertyChange("InputB");
            }
        }


        [PropertyInfo(Direction.OutputData, "Output", "output", "", DisplayLevel.Beginner)]
        public Boolean Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChange("Output");
            }
        }


        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanBinaryOperatorsSettings)value; }
        }


        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Execute()
        {

            if (((BooleanBinaryOperatorsSettings)settings).UpdateOnlyAtBothInputsChanged &&
                !(this.FlagA && this.FlagB))
            {
                //We only update our output if both inputs have changed
                return;
            }

            switch (this.settings.OperatorType)
            {
                case 0: //AND
                    this.Output = this.inputA && this.InputB;
                    break;

                case 1: //OR
                    this.Output = this.inputA || this.InputB;
                    break;

                case 2: //NAND
                    this.Output = !(this.inputA && this.InputB);
                    break;

                case 3: //NOR
                    this.Output = !(this.inputA || this.InputB);
                    break;

                case 4: //XOR
                    this.Output = this.inputA ^ this.InputB;
                    break;

                default:
                    this.Output = false;
                    break;

            }//end switch

            this.FlagA = false;
            this.FlagB = false;

            ProgressChanged(1, 1);

        }

        public void Initialize()
        {
        }

        public void Pause()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get
            {
                return null;
            }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get
            {
                return null;
            }
        }

        public void Stop()
        {
            this.FlagA = false;
            this.FlagB = false;
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }
        
        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
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
    }
}
