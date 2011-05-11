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
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;

namespace StringOperations
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("StringOperations.Properties.Resources", false, "PluginCaption", "PluginTooltip", null, "StringOperations/icon.png")]
    public class StringOperations : IThroughput
    {
        private StringOperationsSettings _settings = null;

        private string _string1;
        private string _string2;
        private int _value1;
        private int _value2;
        private string _outputString;
        private int _outputValue;

        public StringOperations()
        {
            _settings = new StringOperationsSettings();
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return _settings; }
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
            try
            {
                switch ((StringOperationType) (_settings).Operation)
                {
                    case StringOperationType.Concatenate:
                        _outputString = String.Concat(_string1,_string2);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Substring:
                        _outputString = _string1.Substring(_value1, _value2);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.ToUppercase:
                        _outputString = _string1.ToUpper();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.ToLowercase:
                        _outputString = _string1.ToLower();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Length:
                        _outputValue = _string1.Length;                        
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.CompareTo:
                        _outputValue = _string1.CompareTo(_string2);
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Trim:
                        _outputString = _string1.Trim();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.IndexOf:
                        _outputValue = _string1.IndexOf(_string2);
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Equals:
                        _outputValue = (_string1.Equals(_string2) ? 1 : 0);                        
                        OnPropertyChanged("OutputValue");
                        break;
                }
                
            }
            catch(Exception ex)
            {
                GuiLogMessage("Could not execute operation '" + ((StringOperationType) (_settings).Operation) +  "' :" + ex.Message, NotificationLevel.Error);
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

        #endregion

        [PropertyInfo(Direction.InputData, "String1Caption", "String1Tooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string String1
        {           
            set { _string1 = value; }
        }

        [PropertyInfo(Direction.InputData, "String2Caption", "String2Tooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string String2
        {
            set { _string2 = value; }
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

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return _outputString; }
        }

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", null, false, false, QuickWatchFormat.None, null)]
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
