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
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private SHASettings settings;
        private CryptoolStream inputData;
        private byte[] outputData;        
        

        public SHA()
        {
            this.settings = new SHASettings();
        }

        #region IHashAlgorithm Members

        public void Execute()
        {
            Progress(0.5, 1.0);
            if (inputData != null)
            {
              byte[] data = null;

              switch (settings.SHAFunction)
              {
                case (int)SHASettings.ShaFunction.SHA1:
                  SHA1Managed sha1Hash = new SHA1Managed();
                  data = sha1Hash.ComputeHash((Stream)inputData);
                  break;
                case (int)SHASettings.ShaFunction.SHA256:
                  SHA256Managed sha256Hash = new SHA256Managed();
                  data = sha256Hash.ComputeHash((Stream)inputData);
                  break;
                case (int)SHASettings.ShaFunction.SHA384:
                  SHA384Managed sha384Hash = new SHA384Managed();
                  data = sha384Hash.ComputeHash((Stream)inputData);
                  break;
                case (int)SHASettings.ShaFunction.SHA512:
                  SHA512Managed sha512Hash = new SHA512Managed();
                  data = sha512Hash.ComputeHash((Stream)inputData);
                  break;
              }              
              
              GuiLogMessage("Hash created.", NotificationLevel.Info);              
              Progress(1, 1);

              OutputData = data;
            }
            else
            {                            
              GuiLogMessage("Received null value for ICryptoolStream.", NotificationLevel.Warning);
            }
            Progress(1.0, 1.0);
        }

        public ISettings Settings
        {
          get { return (ISettings)this.settings; }
            set { this.settings = (SHASettings)value; }
        }

        #endregion

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputData
        {
            get 
            {
              if (inputData != null)
              {
                CryptoolStream cs = new CryptoolStream();
                listCryptoolStreamsOut.Add(cs);
                cs.OpenRead(inputData.FileName);
                return cs;
              }
              return null;
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
        public CryptoolStream OutputDataStream
        {
            get
            {
                CryptoolStream outputDataStream = null;
                if (outputData != null)
                {
                  outputDataStream = new CryptoolStream();
                  outputDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, outputData);
                  listCryptoolStreamsOut.Add(outputDataStream);
                }
                return outputDataStream;
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
            inputData.Close();
            inputData = null;
          }
          foreach (CryptoolStream cryptoolStream in listCryptoolStreamsOut)
          {
            cryptoolStream.Close();
          }
          listCryptoolStreamsOut.Clear();
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
