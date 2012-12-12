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
using System.Timers;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Plugins.CodeScanner
{
    [Author("Mirko Sartorius", "mirkosartorius@web.de", "University of Kassel", "")]
    [PluginInfo("CodeScanner", "Caputre Image from Webcam", "CodeScanner/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class CodeScanner : ICrypComponent
    {

        #region Private Variables

        private readonly CodeScannerSettings settings = new CodeScannerSettings();
        private readonly CodeScannerPresentation presentation = new CodeScannerPresentation();
        private bool isRunning = false;
        private readonly WebCam wCam = new WebCam();
        private System.Timers.Timer t1 = null;
        private DateTime lastExecuted = DateTime.Now;

        #endregion


        #region Helper Functions

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }


        void t1_Tick(object sender, EventArgs e)
        {
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    //  presentation.Attach();
                    presentation.setImage(wCam.GetCurrentImage());
                    if (lastExecuted.AddMilliseconds(settings.SendPicture) < DateTime.Now)
                    {
                        PictureOutPut = ImageToByte(wCam.GetCurrentImage());
                        OnPropertyChanged("PictureOutPut");
                        lastExecuted = DateTime.Now;
                    }

                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                }
            }), null);
        }
        #endregion

        

        #region Data Properties



        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public byte[] PictureOutPut
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
        public System.Windows.Controls.UserControl Presentation
        {
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {

            if (settings.FrameRate > settings.SendPicture)
            {
                GuiLogMessage("Frame Rate muss kleiner als Send Picture sein", NotificationLevel.Error);
                PostExecution();
            }
    
         //   isRunning = true;
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    wCam.OpenConnection();

                 //   presentation.Attach();
                  
                }
                catch
                {
                }
            }), null);
                

        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            t1 = new System.Timers.Timer();

            t1.Interval = settings.FrameRate; // Intervall festlegen, hier 100 ms
            t1.Elapsed += new ElapsedEventHandler(t1_Tick); // Eventhandler ezeugen der beim Timerablauf aufgerufen wird
            t1.Start(); // Timer starten



            ProgressChanged(1, 1);

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            t1.Stop();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
          //  isRunning = false;
            wCam.Dispose();
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
