/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using System.Windows.Documents;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace SHA
{
  [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "SHA", "SHA hash functions", "", "SHA/SHA.png")]    
    public class SHA : ICryptographicHash
    {
        private SHASettings settings;
        private ICryptoolStream inputData;
        private byte[] outputData;        
        

        public SHA()
        {
            this.settings = new SHASettings();
        }

        #region IHashAlgorithm Members

        public void Execute()
        {
            Progress(0.5, 1.0);

            HashAlgorithm hash = GetHashAlgorithm(settings.SHAFunction);

            if (inputData == null)
              {
                GuiLogMessage("Received null value for ICryptoolStream.", NotificationLevel.Warning);
              }              
            else if (hash == null)
            {
                GuiLogMessage("No valid SHA algorithm instance.", NotificationLevel.Error);
            }
            else
            {                            
                using (CStreamReader reader = inputData.CreateReader())
                {
                    OutputData = hash.ComputeHash(reader);
                    GuiLogMessage("Hash created.", NotificationLevel.Info);
            }
            }
            Progress(1.0, 1.0);
        }

        private HashAlgorithm GetHashAlgorithm(int shaType)
        {
            switch ((SHASettings.ShaFunction)shaType)
            {
                case SHASettings.ShaFunction.SHA1:
                    return new SHA1Managed();
                case SHASettings.ShaFunction.SHA256:
                    return new SHA1Managed();
                case SHASettings.ShaFunction.SHA384:
                    return new SHA384Managed();
                case SHASettings.ShaFunction.SHA512:
                    return new SHA512Managed();
            }

            return null;
        }

        public ISettings Settings
        {
          get { return (ISettings)this.settings; }
            set { this.settings = (SHASettings)value; }
        }

        #endregion

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream InputData
        {
            get 
            {
                return inputData;
              }
            set 
            {
              if (value != inputData)
              {
                inputData = value;
              }              
            }
        }

        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as Stream", "", true, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream OutputDataStream
        {
            get
            {
                if (outputData != null)
                {
                    return new CStreamWriter(outputData);
                }
                return null;
            }
            set { } //readonly
        }

        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as byte array", "", true, false, QuickWatchFormat.Hex, null)]
        public byte[] OutputData
        {
            get { return this.outputData; }
            set
            {
              if (outputData != value)
              {
                this.outputData = value;
                OnPropertyChanged("OutputData");
                OnPropertyChanged("OutputDataStream");                
              }
            }
        }

        #region IPlugin Members

        public void Dispose()
        {
          if (inputData != null)
          {
            inputData = null;
          }
          }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public System.Windows.Controls.UserControl Presentation
        {
          get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
          get { return null; }
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
          Dispose();
        }

        public void PreExecution()
        {
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IHashAlgorithm Members

        public FlowDocument DetailedDescription
        {
            get { return null; }
        }

        #endregion

        private void Progress(double value, double max)
        {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #region IPlugin Members

        public void Pause()
        {
          
        }

        #endregion
    }
}
