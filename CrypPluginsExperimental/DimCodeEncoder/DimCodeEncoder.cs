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

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.DimCodeEncoder.DimCodes;
using DimCodeEncoder;

namespace Cryptool.Plugins.DimCodeEncoder
{
    [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("DimCodeEncoderCaption", "DimCodeEncoderTooltip", "DimCodeEncoder/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class DimCodeEncoder : ICrypComponent
    {
        #region Const / Variables


        private Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode> codeTypeHandler = new Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode>();
        private readonly DimCodeEncoderSettings settings = new DimCodeEncoderSettings();
        private DimCodeEncoderPresentation presentation = new DimCodeEncoderPresentation();
        
        #endregion
        
        public DimCodeEncoder()
        {
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.EAN8, new EAN8(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.EAN13, new EAN13(this));

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
        #region std functions 
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
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }
        #endregion
        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            var allBytes = new List<byte>();
            using (CStreamReader reader = InputStream.CreateReader())
            {
                var buffer = new byte[1]; //may be a bottleneck so consider bigger buffer if too slow or make it kinda dynamic
                while (reader.Read(buffer) > 0)
                {
                    allBytes.Add(buffer[0]); //store all recieved bytes

                    //handle input
                    var dimCode = codeTypeHandler[settings.EncodingType].Encode(allBytes.ToArray(), settings);
                    if (dimCode != null) //input is valid
                    {
                        //update Presentation
                        presentation.SetImages(dimCode.PresentationBitmap, dimCode.PureBitmap);
                        presentation.SetList(dimCode.Legend);

                        //update output
                        PictureBytes = dimCode.PureBitmap;
                        OnPropertyChanged("PictureBytes");
                    }
                }
            }
            ProgressChanged(1, 1);
        }
        #region std functions
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
            settings.EncodingType = DimCodeEncoderSettings.DimCodeType.EAN13;
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;


        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;
       
        public void GuiLogMessage(string message, NotificationLevel logLevel)
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
