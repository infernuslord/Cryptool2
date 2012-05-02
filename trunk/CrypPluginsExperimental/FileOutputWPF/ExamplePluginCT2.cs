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
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using FileOutput.Helper;
using FileOutputWPF;
using FileOutputWPF.Helper;
using HexBox;




namespace Cryptool.Plugins.FileOutputWPF
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("FileOutputWPF", "Subtract one number from another", "FileOutputWPF/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class FileOutputWPF : ICrypComponent
    {
        private FileOutputWPFPresentation fwpfp;

        public FileOutputWPF()
        {
            fwpfp = new FileOutputWPFPresentation(new HexBox.HexBox(),this);
            Presentation = fwpfp;

            settings = new ExamplePluginCT2Settings();
            settings.PropertyChanged += settings_PropertyChanged;

            
            
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();

        #endregion

        # region Properties

        [PropertyInfo(Direction.InputData, "StreamInputCaption", "StreamInputTooltip", true)]
        public ICryptoolStream StreamInput
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
                get; private set; 
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

        public string InputFile { get; set; }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);
            if (StreamInput == null)
            {
                GuiLogMessage("Received null value for ICryptoolStream.", NotificationLevel.Warning);
                return;
            }

            using (CStreamReader reader = StreamInput.CreateReader())
            {
                // If target file was selected we have to copy the input to target. 
                
                # region copyToTarget
                if (settings.TargetFilename != null)
                {
                    InputFile = settings.TargetFilename;
                    try
                    {
                        fwpfp.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            fwpfp.CloseFileToGetFileStreamForExecution();
                        }, null);

                        FileStream fs;
                        if (!settings.Append)
                        {
                            fs = FileHelper.GetFileStream(settings.TargetFilename, FileMode.Create);
                        }
                        else
                        {
                            fs = FileHelper.GetFileStream(settings.TargetFilename, FileMode.Append);
                            for (int i = 0; i < settings.AppendBreaks; i++)
                            {
                                const string nl = "\n";
                                fs.Write(Encoding.ASCII.GetBytes(nl), 0, Encoding.ASCII.GetByteCount(nl));
                            }
                        }

                        byte[] byteValues = new byte[1024];
                        int byteRead;

                        long position = fs.Position;
                        GuiLogMessage("Start writing to target file now: " + settings.TargetFilename, NotificationLevel.Debug);
                        while ((byteRead = reader.Read(byteValues, 0, byteValues.Length)) != 0)
                        {
                            fs.Write(byteValues, 0, byteRead);
                            if (OnPluginProgressChanged != null && reader.Length > 0 &&
                                (int)(reader.Position * 100 / reader.Length) > position)
                            {
                                position = (int)(reader.Position * 100 / reader.Length);
                                ProgressChanged(reader.Position, reader.Length);
                            }
                        }
                        fs.Flush();
                        fs.Close();

                        GuiLogMessage("Finished writing: " + settings.TargetFilename, NotificationLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        settings.TargetFilename = null;
                    }
                }
                # endregion copyToTarget

                fwpfp.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    fwpfp.ReopenClosedFile();
                }, null);
                ProgressChanged(1.0, 1.0);
            }
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
