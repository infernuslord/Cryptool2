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
    [PluginInfo("Cryptool.Plugins.Convertor.Properties.Resources", "PluginCaption", "PluginTooltip", "StreamToStringConverter/DetailedDescription/doc.xml", "StreamToStringConverter/s2t-icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StreamToStringConverter : ICrypComponent
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

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
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

        public void Initialize() { }

        public void Dispose()
        {
            inputStream = null;
        }


        public void Stop() { }

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

        private string GetStringForEncoding(byte[] buffer, StreamToStringConverterSettings.EncodingTypes encoding)
        {
            if (buffer == null) return null;
            
            switch (encoding)
            {
                case StreamToStringConverterSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetString(buffer);

                case StreamToStringConverterSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetString(buffer);

                case StreamToStringConverterSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetString(buffer);

                case StreamToStringConverterSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetString(buffer);

                case StreamToStringConverterSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetString(buffer);
                    
                case StreamToStringConverterSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetString(buffer);

                case StreamToStringConverterSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetString(buffer);

                default:
                    return Encoding.Default.GetString(buffer);
            }
        }

        string GetPresentation( byte[] buffer, StreamToStringConverterSettings.PresentationFormat presentation )
        {
            if (buffer == null) return null;

            switch (presentation)
            {
                case StreamToStringConverterSettings.PresentationFormat.Base64:
                    return Convert.ToBase64String(buffer);

                case StreamToStringConverterSettings.PresentationFormat.Binary:
                    return string.Join(" ", Array.ConvertAll(buffer, x => Convert.ToString(x, 2).PadLeft(8, '0')));

                case StreamToStringConverterSettings.PresentationFormat.Hex:
                    return string.Join(" ", Array.ConvertAll(buffer, x => x.ToString("X2")));

                case StreamToStringConverterSettings.PresentationFormat.Octal:
                    return string.Join(" ", Array.ConvertAll(buffer, x => Convert.ToString(x, 8).PadLeft(3, '0')));

                case StreamToStringConverterSettings.PresentationFormat.Decimal:
                    return string.Join(" ", Array.ConvertAll(buffer, x => x.ToString()));

                case StreamToStringConverterSettings.PresentationFormat.Text:
                default:
                    return GetStringForEncoding(buffer, settings.Encoding);
            }
        }

        private void processInput(CStreamReader value)
        {
            ShowProgress(50, 100);

            ShowStatusBarMessage("Converting input ...", NotificationLevel.Debug);

            value.WaitEof();
            if (value.Length > settings.MaxLength)
                ShowStatusBarMessage("WARNING - Input stream is too large (" + (value.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);

            byte[] buffer = new byte[Math.Min(value.Length,settings.MaxLength)];
            value.Seek(0, SeekOrigin.Begin);
            value.Read(buffer, 0, buffer.Length);

            outputString = GetPresentation(buffer, settings.PresentationFormatSetting);

            ShowStatusBarMessage("Input converted.", NotificationLevel.Debug);

            ShowProgress(100, 100);

            OnPropertyChanged("InputStream");
            OnPropertyChanged("OutputString");
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

        #endregion
    }
}
