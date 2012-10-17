/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using DimCodeEncoder.DimCodes;
using ZXing;
using ZXing.Common;
using System;
using System.Drawing.Imaging;

namespace Cryptool.Plugins.DimCodeEncoder
{
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("DimCodeEncoder", "Subtract one number from another", "DimCodeEncoder/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class DimCodeEncoder : ICrypComponent
    {
       

        #region Const / Variables

        private const int STREAM_BLOCK_SIZE = 8;
        private Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode> codeTypeHandler = new Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode>();
        private readonly DimCodeEncoderSettings settings = new DimCodeEncoderSettings();
        
        #endregion
        
        public DimCodeEncoder()
        {
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.EAN8, new EAN8());
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "IncommingData", "IncommingDataTooltip")]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "PictureBytesOutput", "PictureBytesOutputTooltip")]
        public byte[] PictureBytes
        {
            get;
            private set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            List<byte> allBytes = new List<byte>();
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                var buffer = new byte[1]; //may be a bottleneck so consider bigger buffer if too slow or make it kinda dynamic
                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                   allBytes.Add(buffer[0]); // little hack to store all recieved bytes
                   PictureBytes = codeTypeHandler[settings.EncodingType].Encode(allBytes.ToArray(), settings);
                   OnPropertyChanged("PictureBytes");
                }
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

      

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
