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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Plugins.Numbers
{
    [Author("Sven Rech, Nils Kopal", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.Numbers.Properties.Resources", true, "PluginInputCaption", "PluginInputTooltip", "PluginInputDescriptionURL", "Numbers/icons/inputIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    class NumberInput : ICrypComponent
    {
        private NumberInputPresentation _presentation = new NumberInputPresentation();
        private Boolean _running = false;

        public NumberInput()
        {
            settings = new NumberInputSettings();
            _presentation.TextBox.TextChanged +=new TextChangedEventHandler(TextBox_TextChanged);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            ((NumberInputSettings)this.settings).Number = _presentation.TextBox.Text;
            if (_running)
            {
                Execute();
            }
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

        private NumberInputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (NumberInputSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return _presentation; }
        }

        public void PreExecution()
        {
            _running = true;
        }

        public void Execute()
        {
            BigInteger bi;
            //The input from the taskpane is convertet to a BigNumber and is send to the output.
            if (settings.Number == null || settings.Number.Equals(""))
            {
                NumberOutput = BigInteger.Zero;
            }
            else
            {
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
            }
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
            _running = false;
        }

        public void Initialize()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this._presentation.TextBox.Text = ((NumberInputSettings)settings).Number;
            }
            , null);            
        }

        public void Dispose()
        {
            _running = false;
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
