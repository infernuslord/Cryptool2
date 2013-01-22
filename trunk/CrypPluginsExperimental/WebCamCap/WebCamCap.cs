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
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using WebCamCap;

namespace Cryptool.Plugins.WebCamCap
{
    //TODO 
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("WebCamCap", "Subtract one number from another", "WebCamCap/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class WebCamCap : ICrypComponent
    {
        #region Private Variables

        private readonly ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();
        private readonly WebCamCapPresentation presentation;
        private DateTime lastExecuted = DateTime.Now;

        public WebCamCap()
        {
            presentation = new WebCamCapPresentation(CaptureImage_Executed);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// </summary>
        [PropertyInfo(Direction.OutputData, "pictureOutputCaption", "pictureOutputCaptionTooltip")] //TODO internationalisierung
        public byte[] PictureOutput
        {
            get;
            set;
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
            get { return presentation; }
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
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    presentation.StartCam((CapDevice.DeviceMonikers.Length > 0) ? CapDevice.DeviceMonikers[0].MonikerString : ""); //TODO select webcam via settings
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
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
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    presentation.StopCam();
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
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

        /// <summary>
        /// uses an Memorystream to convert a given BitmapSource to the byte[] representation of its jpeg encodeing with the given quality (0-100)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static byte[] ImageTojepgByte(BitmapSource bitmap, int quality)
        {
            using (var ms = new MemoryStream())
            {
                var enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmap));
                enc.QualityLevel = quality;
                enc.Save(ms);

                var image = ms.ToArray(); 
                ms.Dispose();
                return image;
            }
        }

        #region Event Handling

        /// <summary>
        /// is invoked when a new picture is received
        /// its running inside of the presentation thread
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void CaptureImage_Executed(object sender, EventArgs e)
        {
            try
            {
                //// Store current image in the webcam
                BitmapSource bitmap = presentation.webcamPlayer.CurrentBitmap;

                if (bitmap != null)
                {
                  
                  if (lastExecuted.AddMilliseconds(100) < DateTime.Now) //TODO refactor to settings
                  {
                    PictureOutput = ImageTojepgByte(bitmap, 50); //todo refactor quality to settings ( or kinda "maximum picture size", "target pricture size") 
                    OnPropertyChanged("PictureOutput");
                    lastExecuted = DateTime.Now;
                   }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

     

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
