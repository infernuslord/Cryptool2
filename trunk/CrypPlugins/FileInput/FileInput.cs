/*
   Copyright [2008] [Thomas Schmid, University of Siegen]

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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using FileInput.Helper;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace FileInput
{
    [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("FileInput.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "FileInput/Images/FileInput.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class FileInputClass : ICrypComponent
    {
        #region Private variables
        private const int MAX_BYTE_ARRAY_SIZE = 10485760; // 20MB
        private FileInputPresentation fileInputPresentation;
        private FileInputSettings settings;
        private CStreamWriter cstreamWriter;
        #endregion

        public FileInputClass()
        {
            settings = new FileInputSettings();
            settings.PropertyChanged += settings_PropertyChanged;
            fileInputPresentation = new FileInputPresentation(this);

            Presentation = fileInputPresentation;
            fileInputPresentation.UscHexBoc.OnExceptionOccured += UscHexBoc_OnExceptionOccured;
            fileInputPresentation.UscHexBoc.OnInformationOccured += UscHexBoc_OnInformationOccured;
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "OpenFilename")
            {
                string fileName = settings.OpenFilename;

                if (File.Exists(fileName))
                {
                    fileInputPresentation.OpenFile(settings.OpenFilename);
                    FileSize = (int)new FileInfo(fileName).Length;
                    GuiLogMessage("Opened file: " + settings.OpenFilename + " " + FileHelper.FilesizeReadable(settings.OpenFilename), NotificationLevel.Info);
                }
                else if (e.PropertyName == "OpenFilename" && fileName == null)
                {

                    fileInputPresentation.CloseFile();
                    FileSize = 0;
                }
                NotifyPropertyChange();
            }
        }

        #region Properties
        public ISettings Settings
        {
            get { return (ISettings)settings; }
            set { settings = (FileInputSettings)value; }
        }

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

        void UscHexBoc_OnInformationOccured(object sender, Exception e)
        {
            GuiLogMessage(e.Message, NotificationLevel.Info);
        }

        void UscHexBoc_OnExceptionOccured(object sender, Exception e)
        {
            GuiLogMessage(e.Message, NotificationLevel.Error);
        }

        #region IPlugin Members
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation { get; private set; }

        public void Initialize()
        {
            fileInputPresentation.CloseFile();
            if (File.Exists(settings.OpenFilename))
            {
                fileInputPresentation.OpenFile(settings.OpenFilename);
                NotifyPropertyChange();
            }
        }

        /// <summary>
        /// Close open file. Will be called when deleting an element instance from workspace.
        /// </summary>
        public void Dispose()
        {
            if (cstreamWriter != null)
            {
                cstreamWriter.Dispose();
                cstreamWriter = null;
            }

            fileInputPresentation.CloseFileToGetFileStreamForExecution();
        }

        public void Stop()
        {

        }

        public void PreExecution()
        {
            DispatcherHelper.ExecuteMethod(fileInputPresentation.Dispatcher,
              fileInputPresentation, "CloseFileToGetFileStreamForExecution", null);
        }

        public void PostExecution()
        {
            DispatcherHelper.ExecuteMethod(fileInputPresentation.Dispatcher,
              fileInputPresentation, "ReopenClosedFile", null);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region methods
        public void NotifyPropertyChange()
        {
            OnPropertyChanged("StreamOutput");
            OnPropertyChanged("FileSize");
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
        #endregion

        #region IPlugin Members


        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(settings.OpenFilename))
            {
                GuiLogMessage("No input file selected, can't proceed", NotificationLevel.Error);
                return;
            }
            
            cstreamWriter = new CStreamWriter(settings.OpenFilename);
            NotifyPropertyChange();
        }

        #endregion
    }
}
