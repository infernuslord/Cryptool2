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

using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Numerics;

namespace Cryptool.Plugins.BigNumber
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("BigNumber.Properties.Resources", true, "PluginInputCaption", "PluginInputTooltip", "PluginInputDescriptionURL", "BigNumber/icons/inputIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    class BigNumberInput : ICrypComponent
    {

        public BigNumberInput()
        {
            settings = new BigNumberInputSettings();
        }

        #region Properties

        private BigInteger numberOutput = 0;
        /// <summary>
        /// The output is defined
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NumberOutputCaption", "NumberOutputTooltip", "")]
        public BigInteger NumberOutput
        {
            get
            {
                return numberOutput;
            }
            set
            {
                numberOutput = value;
                OnPropertyChanged("NumberOutput");
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

        private BigNumberInputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (BigNumberInputSettings)value; }
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
            Dispose();
        }

        public void Execute()
        {
            BigInteger bi;
            //The input from the taskpane is convertet to a BigNumber and is send to the output.
            try
            {
                bi = BigIntegerHelper.ParseExpression(settings.Number);                
            }
            catch (Exception ex)
            {
                GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                return;
            }
            NumberOutput = bi;
            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {
            Dispose();
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
    }
}
