/* 
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using Cryptool.MD5.Presentation;
using Cryptool.MD5.Algorithm;
using System.Windows.Threading;

namespace Cryptool.MD5
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "MD5", "MD5 hash function", "MD5/DetailedDescription/Description.xaml", "MD5/MD5.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class MD5 : ICryptographicHash
    {
        #region Private variables

        private MD5Settings settings;
        private ICryptoolStream inputData;
        private byte[] outputData;
        private PresentableMD5 md5;
        private PresentationContainer presentationContainer;

        #endregion

        #region Public interface
        public MD5()
        {
            settings = new MD5Settings();

            md5 = new PresentableMD5();
            md5.AddSkippedState(MD5StateDescription.STARTING_ROUND_STEP);
            md5.AddSkippedState(MD5StateDescription.FINISHING_COMPRESSION);

            presentationContainer = new PresentationContainer(md5);

            md5.StatusChanged += Md5StatusChanged;
        }

        public ISettings Settings
        {
          
            get { return this.settings; }
          
            set { this.settings = (MD5Settings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream InputData
        {
            get 
            {
                return inputData;
              }
          
            set 
            { 
              this.inputData = value;
              OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as Stream", "", false, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream OutputDataStream
        {
          get 
          {            
            if (outputData != null)
            {
              GuiLogMessage("Got request for hash (Stream)...", NotificationLevel.Debug);
                return new CStreamWriter(outputData);
            }
            return null; ;
          }
        }

        [PropertyInfo(Direction.OutputData, "Hashed value", "Output data of the hashed value as byte array", "", false, false, QuickWatchFormat.Hex, null)]
        public byte[] OutputData 
        {
          
            get 
            {
              GuiLogMessage("Got request for hash (Byte Array)...", NotificationLevel.Debug);
              return this.outputData; 
            }
          
            set 
            { 
              outputData = value;              
              OnPropertyChanged("OutputData");
              OnPropertyChanged("OutputDataStream");             
            }
        }

        void Md5StatusChanged()
        {
            if (md5.IsInFinishedState)
            {
                OutputData = md5.HashValueBytes;
                ProgressChanged(1.0, 1.0);
            }
        }
        
        public void Execute()
        {
          ProgressChanged(0.5, 1.0);
          if (inputData != null)
          {
              using (CStreamReader reader = inputData.CreateReader())
              {
              if (Presentation.IsVisible)
              {
                      md5.Initialize(reader);
              }
              else
              {
                  HashAlgorithm builtinMd5 = System.Security.Cryptography.MD5.Create();

                      OutputData = builtinMd5.ComputeHash(reader);

                  ProgressChanged(1.0, 1.0);
              }
          }
          }
          else
          {            
              GuiLogMessage("Received null value for CryptoolStream.", NotificationLevel.Warning);
          }
        }

        
        public void Initialize()
        {            
        }
        
        public void Dispose()
        {
            inputData = null;
          }
        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
          get { return presentationContainer; }
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
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
				public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        
        public void Pause()
        {          
        }

        #endregion
    }
}
