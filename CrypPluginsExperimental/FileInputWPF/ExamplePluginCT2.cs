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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using FileInputWPF;
using FileInputWPF.Helper;
using HexBox;
using System.IO;

namespace Cryptool.Plugins.FileInputWPF
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Example Plugin", "Subtract one number from another", "FileInputWPF/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class FileInputWPF : ICrypComponent
    {
        private FileInputWPFPresentation fwpfp;

        public FileInputWPF()
        {
            fwpfp = new FileInputWPFPresentation(new HexBox.HexBox(),this);
            Presentation = fwpfp;

            settings = new ExamplePluginCT2Settings();
            settings.PropertyChanged += settings_PropertyChanged;

            
            
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "OpenFilename" )
            {
                string fileName = settings.OpenFilename;

                if (File.Exists(fileName))
                {
                    fwpfp.CloseFile();
                    fwpfp.OpenFile(settings.OpenFilename);
                    FileSize = (int)new FileInfo(fileName).Length;
                    GuiLogMessage("Opened file: " + settings.OpenFilename , NotificationLevel.Info);
                }
                else if (e.PropertyName == "OpenFilename" && fileName == null)
                {

                    fwpfp.CloseFile();
                    FileSize = 0;
                }
                NotifyPropertyChange();
            }
        }

        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        public readonly ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip", true)]
        public ICryptoolStream StreamOutput
        {
            get
            {
                return cstreamWriter;
            }
            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "FileSizeCaption", "FileSizeTooltip")]
        public int FileSize { get; private set; }


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
            get; private set; }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            //fwpfp.CloseFileToGetFileStreamForExecution();

            DispatcherHelper.ExecuteMethod(fwpfp.Dispatcher,  fwpfp, "CloseFileToGetFileStreamForExecution", null);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        /// 
        private CStreamWriter cstreamWriter;

        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            if (string.IsNullOrWhiteSpace(settings.OpenFilename))
            {
                GuiLogMessage("No input file selected, can't proceed", NotificationLevel.Error);
                return;
            }

            cstreamWriter = new CStreamWriter(settings.OpenFilename, true);

            NotifyPropertyChange();

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }

        public void NotifyPropertyChange()
        {
            OnPropertyChanged("StreamOutput");
            OnPropertyChanged("FileSize");
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            

            DispatcherHelper.ExecuteMethod(fwpfp.Dispatcher, fwpfp, "ReopenClosedFile", null);
            
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
            fwpfp.dispose();
            cstreamWriter.Dispose();
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
