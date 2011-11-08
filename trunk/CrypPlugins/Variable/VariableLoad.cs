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

using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.Collections;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.Variable
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Variable.Properties.Resources", true, "VariableLoadCaption", "VariableLoadTooltip", "VariableLoadDescriptionURL", "Variable/loadIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    class VariableLoad : ICrypComponent
    {
        #region Private variables

        private VariableSettings settings;
        private object loadObject;

        #endregion

        public VariableLoad()
        {
            this.settings = new VariableSettings();
            
        }

        public ISettings Settings
        {
            get { return settings; }
            set { settings = (VariableSettings)value; }
        }

        #region Properties

        [PropertyInfo(Direction.OutputData, "VariableLoadObjectCaption", "VariableLoadObjectTooltip", "")]
        public object VariableLoadObject
        {
            get
            {
                return loadObject;
            }
            set
            {
                loadObject = value;
                OnPropertyChanged("VariableLoadObject");
            }
        }
        #endregion

        private void onVariableStore(string variable, object input)
        {
            if (variable == settings.VariableName)
            {
                VariableLoadObject = input;
                ProgressChanged(1.0, 1.0);
            }
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Dispose();
            VariableStore.OnVariableStore += new StoreVariable(onVariableStore);
        }

        public void Execute()
        {
            if (settings.VariableName == "")
                GuiLogMessage("The variable name may not be empty.", NotificationLevel.Error);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
          
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            VariableStore.OnVariableStore -= onVariableStore;
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion


    }
}
