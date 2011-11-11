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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", "PluginBO_Caption", "PluginBO_Tooltip", "PluginDescriptionURL", "BooleanOperators/icons/false.png", "BooleanOperators/icons/true.png")]
    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class BooleanOutput : ICrypComponent
    {
        private BooleanOutputSettings settings;
        private Boolean input = false;

        public BooleanOutput()
        {
            this.settings = new BooleanOutputSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        [PropertyInfo(Direction.InputData, "BO_InputCaption", "BO_InputTooltip")]
        public Boolean Input
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
                OnPropertyChange("Input");
            }
        }

        #region IPlugin Member

        public void Dispose()
        {
        }

        public void Execute()
        {
            if(this.Input)
            {
                settings_OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
            }
            else
            {
                settings_OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            }
            ProgressChanged(1, 1);
        }

        public void Initialize()
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
            get { return null; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanOutputSettings)value; }
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
