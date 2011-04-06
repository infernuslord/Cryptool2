/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

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
using System.IO;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;

namespace Cryptool.Plugins.Convertor
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Plugins.Convertor.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "StreamToStringConverter/s2t-icon.png")]
    public class StreamToStringConverter : IThroughput
    {
        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamToStringConverter()
        {
            this.settings = new StreamToStringConverterSettings();
        }


        /// <summary>
        /// Returns the settings object, or sets it
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (StreamToStringConverterSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        public object InputStreamQuickWatchConverter(string PropertyNameToConvert)
        {
            return outputString;
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", "", true, false, QuickWatchFormat.Text, "InputStreamQuickWatchConverter")]
        public ICryptoolStream InputStream
        {
            get
            {
                return inputStream;
            }
            set
            {
                inputStream = value;
                // processInput(value); This should be done in execute method, because PlayMode causes 
                // errors state (yellow/red markers) to be flushed on execute. So if input is processed
                // here before execute method the plugin element will not be colored correctly if 
                // errors/warnings occur.
                OnPropertyChanged("InputStream");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Feuern, wenn sich sich eine Änderung des Fortschrittsbalkens ergibt 
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Initialize() { }

        public void Dispose()
        {
            inputStream = null;
        }


        public void Stop() { }

        public bool HasChanges
        {
            get { return settings.HasChanges; }
            set { settings.HasChanges = value; }
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            outputString = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private variables
        private StreamToStringConverterSettings settings;
        private ICryptoolStream inputStream = null;
        private string outputString;
        #endregion

        #region Private methods

        private void processInput(CStreamReader value)
        {
            ShowProgress(50, 100);
            ShowStatusBarMessage("Converting input ...", NotificationLevel.Debug);
            value.WaitEof();
            if (value.Length > settings.MaxLength)
                ShowStatusBarMessage("WARNING - Input stream is too large (" + (value.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);

            byte[] byteValues = new byte[settings.MaxLength];
            int bytesRead;
            value.Seek(0, SeekOrigin.Begin);
            bytesRead = value.Read(byteValues, 0, byteValues.Length);

            // here conversion happens
            switch (settings.Encoding)
            {
                case StreamToStringConverterSettings.EncodingTypes.Default:
                    outputString = Encoding.Default.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.Base64Binary:
                    outputString = Convert.ToBase64String(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.HexStringBinary:
                    outputString = convertBytesToHexString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.OctalStringBinary:
                    outputString = convertBytesToOctalString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.Unicode:
                    outputString = Encoding.Unicode.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.UTF7:
                    outputString = Encoding.UTF7.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.UTF8:
                    outputString = Encoding.UTF8.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.UTF32:
                    outputString = Encoding.UTF32.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.ASCII:
                    outputString = Encoding.ASCII.GetString(byteValues, 0, bytesRead);
                    break;
                case StreamToStringConverterSettings.EncodingTypes.BigEndianUnicode:
                    outputString = Encoding.BigEndianUnicode.GetString(byteValues, 0, bytesRead);
                    break;
                default:
                    outputString = Encoding.Default.GetString(byteValues, 0, bytesRead);
                    break;
            }

            ShowStatusBarMessage("Input converted.", NotificationLevel.Debug);
            ShowProgress(100, 100);
            OnPropertyChanged("InputStream");
            OnPropertyChanged("OutputString");
        }

        private string convertBytesToHexString(byte[] array, int start, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < (start + count); i++)
            {
                sb.Append(array[i].ToString("X2"));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        private string convertBytesToOctalString(byte[] array, int start, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < (start + count); i++)
            {
                string val = String.Format("{0,3}", Convert.ToString(array[i], 8));
                sb.Append(val.Replace(' ', '0'));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        private void ShowStatusBarMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Execute()
        {
            if (InputStream != null)
            {
                using (CStreamReader reader = InputStream.CreateReader())
                {
                    processInput(reader);
                }
            }
            else
            {
                ShowStatusBarMessage("Stream input is null. Nothing to convert.", NotificationLevel.Warning);
            }
        }

        public void Pause()
        {

        }

        #endregion
    }
}
