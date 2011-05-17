/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;

namespace NumberOperators
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("NumberOperators.Properties.Resources", false, "PluginCaption", "PluginTooltip", null, "NumberOperators/icon.png")]
    public class NumberOperators : IThroughput
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private readonly NumberOperatorsSettings _numberOperatorsSettings;
        private int _value1;
        private int _value2;
        private int _outputValue;

        public NumberOperators()
        {
            _numberOperatorsSettings = new NumberOperatorsSettings();
        }

        public ISettings Settings
        {
            get { return _numberOperatorsSettings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            try
            {
                switch((NumberOperatorType)_numberOperatorsSettings.Operation)
                {
                    case NumberOperatorType.Addition:
                        _outputValue = _value1 + _value2;
                        break;
                    case NumberOperatorType.Subtraction:
                        _outputValue = _value1 - _value2;
                        break;
                    case NumberOperatorType.Multiplication:
                        _outputValue = _value1 * _value2;
                        break;
                    case NumberOperatorType.Division:
                        _outputValue = _value1 / _value2;
                        break;
                    case NumberOperatorType.Exponentiation:
                        _outputValue = (int)Math.Pow(_value1, _value2);
                        break;
                    case NumberOperatorType.Modulo:
                        _outputValue = _value1 % _value2;
                        break;
                    case NumberOperatorType.Equals:
                        _outputValue = _value1.Equals(_value2) ? 1 : 0;
                        break;
                    case NumberOperatorType.Increment:
                        _outputValue = _value1 + 1;
                        break;
                    case NumberOperatorType.Decrement:
                        _outputValue = _value1 - 1;
                        break;
                }
                OnPropertyChanged("OutputValue");
            }
            catch(Exception ex)
            {
                GuiLogMessage("Error occured during execution of NumberOperators:" + ex.Message,NotificationLevel.Error);
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

        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", null, false, false, QuickWatchFormat.None, null)]
        public int Value1
        {
            set { _value1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", null, false, false, QuickWatchFormat.None, null)]
        public int Value2
        {
            set { _value2 = value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public int OutputValue
        {
            get { return _outputValue; }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }
    }
}
