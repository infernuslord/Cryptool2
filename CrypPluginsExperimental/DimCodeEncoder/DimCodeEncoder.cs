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
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.DimCodeEncoder.DimCodes;

namespace Cryptool.Plugins.DimCodeEncoder
{
    [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("DimCodeEncoder.Properties.Resources", "DimCodeEncoderCaption", "DimCodeEncoderTooltip", "DimCodeEncoder/userdoc.xml", new[] { "DimCodeEncoder/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class DimCodeEncoder : ICrypComponent
    {
        #region Const / Variables


        private Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode> codeTypeHandler = new Dictionary<DimCodeEncoderSettings.DimCodeType, DimCode>();
        private readonly DimCodeEncoderSettings settings;
        private DimCodeEncoderPresentation presentation = new DimCodeEncoderPresentation();
        
        #endregion
        
        public DimCodeEncoder()
        {
            settings = new DimCodeEncoderSettings(this);

            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.EAN8, new DimCodes.EAN8(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.EAN13, new DimCodes.EAN13(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.Code39, new DimCodes.Code39(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.Code128, new DimCodes.Code128(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.QRCode, new DimCodes.QRCode(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.DataMatrix, new DimCodes.DataMatrix(this));
            codeTypeHandler.Add(DimCodeEncoderSettings.DimCodeType.PDF417, new DimCodes.PDF417(this));

        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "IncommingData", "IncommingDataTooltip")]
        public byte[] InputStream
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
            Benchmark.Start();

            if (InputStream != null && InputStream.Length >= 1)
            {
                var dimCode = codeTypeHandler[settings.EncodingType].Encode(InputStream, settings);
                if (dimCode != null) //input is valid
                {
                    //update Presentation
                    presentation.SetImages(dimCode.PresentationBitmap, dimCode.PureBitmap,false); 
                    presentation.SetList(dimCode.Legend);
    
                    //update output
                    PictureBytes = dimCode.PureBitmap;
                    OnPropertyChanged("PictureBytes");
                }
            }

            Benchmark.End();
            Console.WriteLine("-GeneratePresentationBitmap:{0}", Benchmark.GetSeconds());
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
