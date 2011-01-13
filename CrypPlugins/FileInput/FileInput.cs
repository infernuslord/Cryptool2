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
    [PluginInfo(true, "FileInput", "File based input", "", "FileInput/Images/FileInput.png")]
    public class FileInputClass : IInput
    {
        #region Private variables
        private const int MAX_BYTE_ARRAY_SIZE = 10485760; // 20MB
        private FileInputPresentation fileInputPresentation;
        private FileInputSettings settings;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
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

        [PropertyInfo(Direction.OutputData, "Stream Output", "Selected file as stream.", "", true, false, QuickWatchFormat.Hex, null)]
        public CryptoolStream StreamOutput
        {
            get
            {
                try
                {
                    Progress(0.5, 1.0);
                    if (File.Exists(settings.OpenFilename))
                    {
                        CryptoolStream cryptoolStream = new CryptoolStream();
                        cryptoolStream.OpenRead(settings.OpenFilename);
                        listCryptoolStreamsOut.Add(cryptoolStream);

                        Progress(1.0, 1.0);
                        return cryptoolStream;
                    }
                    return null;
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                    return null;
                }
            }
            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "File Size", "Size of the selected file", "")]
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

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

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
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
                stream.Close();

            listCryptoolStreamsOut.Clear();
            fileInputPresentation.CloseFileToGetFileStreamForExecution();
        }

        public void Stop()
        {

        }

        public void PreExecution()
        {
            DispatcherHelper.ExecuteMethod(fileInputPresentation.Dispatcher,
              fileInputPresentation, "CloseFileToGetFileStreamForExecution", null);
            Dispose();
        }

        public void PostExecution()
        {
            Dispose();
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
            NotifyPropertyChange();
        }

        public void Pause()
        {
        }

        #endregion
    }
}
