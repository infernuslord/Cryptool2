/*                              
   Copyright 2009 Team CrypTool (Simon Malischewski), Uni Duisburg-Essen

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

using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.Plugins.SubbyteArrayCryptoolStream;


namespace Cryptool.Plugins.SubByteArrayCryptoolStream
{
    [Author("Simon Malischewski", "malischewski@cryptool.org", "Uni Duisburg-Essen", "http://wwww.uni-due.de")]
    [PluginInfo(false, "SubByteArray", "SubByteArray", "SubByteArray/DetailedDescription/Description.xaml", "SubByteArray/icon.png")]
    public class SubByteArrayCryptoolStream : IThroughput
    {
        #region Private variables

        private Cryptool.Plugins.SubbyteArrayCryptoolStream.SubByteArrayCryptoolStreamSettings settings;
        private byte[] inputDataBytes;

        private byte[] outputData;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        public SubByteArrayCryptoolStream()
        {
            this.settings = new SubByteArrayCryptoolStreamSettings();
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SubByteArrayCryptoolStreamSettings)value; }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        [PropertyInfo(Direction.InputData, "Input ByteArray", "Input ByteArray", "", true, false, QuickWatchFormat.Hex, null)]
        public byte[] InputDataBytes
        {
            get
            {
                if (inputDataBytes != null)
                {
                    return this.inputDataBytes;
                }
                return null;
            }
            set
            {
                this.inputDataBytes = value;
                OnPropertyChanged("InputDataBytes");
            }
        }

        [PropertyInfo(Direction.InputData, "Start Index of ByteArray", "Start Index of ByteArray", "", false, false, QuickWatchFormat.Text, null)]
        public int Start
        {
            get { return this.settings.Start; }
            set
            {
                this.settings.Start = value;
                this.settings.GetTaskPaneAttributeChanged();
            }
        }
        [PropertyInfo(Direction.InputData, "End Index of ByteArray", "End Index of ByteArray", "", false, false, QuickWatchFormat.Text, null)]
        public int End
        {
            get { return this.settings.End; }
            set
            {
                this.settings.End = value;
                this.settings.GetTaskPaneAttributeChanged();
            }
        }


        [PropertyInfo(Direction.OutputData, "Resulting ByteArray", "Resulting ByteArray", "", false, false, QuickWatchFormat.Hex, null)]
        public byte[] OutputData
        {
            get
            {
                return this.outputData;
            }


            set
            {
                outputData = value;
                OnPropertyChanged("OutputData");
            }
        }


        #region IPlugin Member





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
            if (inputDataBytes != null)
            {
                //Settings.setArrayLength(inputDataBytes.Length);
            }
        }

        public void Execute()
        {
            
                int start = this.settings.Start;
                int end = this.settings.End;
                
                if (this.inputDataBytes != null)
                {
                    if ((end <= this.inputDataBytes.Length - 1) && (start <= this.inputDataBytes.Length - 1) && (end >= start)) // Check for valid Start/End Index
                    {
                        this.outputData = new byte[(end - start)+1];
                       
                        int CurrentOutputIndex = 0;
                        while (start <= end)
                        {

                            this.outputData[CurrentOutputIndex] = this.inputDataBytes[start];
                            start++;
                            CurrentOutputIndex++;
                            
                        }
                        ProgressChanged(1.0, 1.0);
                        OnPropertyChanged("OutputData");
                        settings.GetTaskPaneAttributeChanged();

                    }
                    else
                    {
                        GuiLogMessage("Invalid Start or End Index of ByteArray", NotificationLevel.Warning);
                        settings.Start = 0;
                        settings.End = 0;
                    }
                }
                else
                {
                    GuiLogMessage("Received Null-ByteArray..", NotificationLevel.Error);
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



        #region IPlugin Member

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion
    }
}
