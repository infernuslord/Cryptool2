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
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.BooleanOperators;
using Cryptool.PluginBase.IO;

using BooleanOperators;

namespace Cryptool.Plugins.BooleanOperators
{
    [Author("Julian Weyers", "julian.weyers@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", "PluginBI_Caption", "PluginBI_Tooltip", "BooleanOperators/DetailedDescription/doc.xml", "BooleanOperators/icons/false.png", "BooleanOperators/icons/true.png")]
    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class BooleanInput : ICrypComponent
    {

        private Boolean output = true;
        private BooleanInputSettings settings;
        private ButtonInputPresentation myButton;
        private Boolean setorbut = false;

        public BooleanInput()
        {
            this.settings = new BooleanInputSettings();
            myButton = new ButtonInputPresentation();
            Presentation = myButton;
            myButton.StatusChanged += new EventHandler(myButton_StatusChanged);
            this.settings.PropertyChanged += settings_OnPropertyChange;
        }
        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            setorbut = true;
            Execute();
        }
        private void myButton_StatusChanged(object sender, EventArgs e)
        {
            setorbut = false;
            Execute();
        }


        [PropertyInfo(Direction.OutputData, "BI_OutputCaption", "BI_OutputTooltip", false)]
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

        #region IPlugin Member



        public void Dispose()
        {
        }

        public void Execute()
        {
            if (!setorbut)
            {
                Output = myButton.Value;

                if (myButton.Value)
                {
                    settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
                    settings.Value = 1;
                }else{
                    settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
                    settings.Value = 0;
                }

            }else{

                Output = (settings.Value == 1);

                if (settings.Value == 1)
                {
                    settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
                    myButton.Value = true;
                    myButton.update();
                }else{
                    settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
                    myButton.Value = false;
                    myButton.update();
                }
            }

            ProgressChanged(1, 1);
        }

        public void Initialize()
        {
            if (settings.Value == 1){
                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
            }else{
                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            }
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanInputSettings)value; }
        }

        public void Stop()
        {
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
