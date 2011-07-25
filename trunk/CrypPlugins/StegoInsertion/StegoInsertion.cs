/* 
   Copyright 2011 Corinna John

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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.IO;

namespace Cryptool.Plugins.StegoInsertion
{
    [Author("Corinna John", "coco@steganografie.eu", "", "http://www.steganografie.eu")]
    [PluginInfo("StegoInsertion.Properties.Resources", false, "PluginCaption", "PluginTooltip", "StegoInsertion/DetailedDescription/Description.xaml", "StegoInsertion/Images/StegoInsertion.png")]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class StegoInsertion : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly StegoInsertionSettings settings = new StegoInsertionSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", null)]
        public ICryptoolStream InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputCarrierCaption", "InputCarrierTooltip", null)]
        public ICryptoolStream InputCarrier
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", null)]
        public String OutputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCarrierCaption", "OutputCarrierTooltip", null)]
        public ICryptoolStream OutputCarrier
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            // no progress, yet
            ProgressChanged(0, 1);

            using (CStreamReader midiReader = InputCarrier.CreateReader())
            {
                    MemoryStream secretStream;
                    MemoryStream outputStream = new MemoryStream();
                    MidiFileHandler midiHandler = new MidiFileHandler();
                    
                    switch (settings.Action)
                    {
                        case 0: // Encryption
                            using (CStreamReader messageReader = InputData.CreateReader())
                            {
                                secretStream = new MemoryStream(messageReader.ReadFully());
                                midiHandler.HideOrExtract(midiReader, secretStream, new MemoryStream(), outputStream, (byte)(settings.MaxMessageBytesPerCarrierUnit*2), false);
                            }
                            OutputCarrier = new CStreamWriter(outputStream.ToArray());
                            OnPropertyChanged("OutputCarrier");
                            break;
                        case 1: // Decryption
                            secretStream = new MemoryStream();
                            midiHandler.HideOrExtract(midiReader, secretStream, new MemoryStream(), outputStream, 1, true);
                            //OutputData = new CStreamWriter(secretStream.GetBuffer());
                                
                            secretStream.Position = 0;
                            using (StreamReader secretReader = new StreamReader(secretStream))
                            {
                                OutputText = secretReader.ReadToEnd();
                                OnPropertyChanged("OutputText");
                            }
                            break;
                    }
            }

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //OnPropertyChanged("Difference");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            //if (settings.Subtrahend < 0)
            //    GuiLogMessage("Subtrahend is negative", NotificationLevel.Debug);

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
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
            settings.UpdateTaskPaneVisibility();
        }

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
