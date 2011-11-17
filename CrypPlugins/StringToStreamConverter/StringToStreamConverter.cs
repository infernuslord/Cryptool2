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
    // Converts a given string into a stream by using different encodings.
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Plugins.Convertor.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "StringToStreamConverter/t2s-icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StringToStreamConverter : ICrypComponent
    {
        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public StringToStreamConverter()
        {
            this.settings = new StringToStreamConverterSettings();
        }


        /// <summary>
        /// Returns the settings object, or sets it
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (StringToStreamConverterSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get 
            {
                if (outputStream == null)
              return null;

                return outputStream;
            }
            set
            {
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBytesCaption", "OutputBytesTooltip", true)]
        public byte[] OutputBytes
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (outputBytes == null)
                    return null;

                    return outputBytes;
                }
            set
            {
            }
        }

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get { return this.inputString;  }
            set 
            {
              inputString = value;
              // processInput(value); This should be done in execute method, because PlayMode causes 
              // errors state (yellow/red markers) to be flushed on execute. So if input is processed
              // here before execute method the plugin element will not be colored correctly if 
              // errors/warnings occur.
              OnPropertyChanged("InputText");
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
            if (outputStream != null)
            {
                outputStream.Flush();
                outputStream.Close();
                outputStream.Dispose();
                outputStream = null;
            }
            }


        public void Stop() { }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private variables
        private StringToStreamConverterSettings settings;
        private CStreamWriter outputStream = null;
        private byte[] outputBytes = null;
        private string inputString;
        #endregion

        #region Private methods

        private void processInput(string value)
        {
                ShowProgress(50, 100);
                ShowStatusBarMessage("Converting input ...", NotificationLevel.Debug);
                
                // here conversion happens
                switch (settings.Encoding)
                {
                    case StringToStreamConverterSettings.EncodingTypes.Default:
                        outputBytes = Encoding.Default.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.Base64Binary:
                        try
                        {
                            outputBytes = Convert.FromBase64String(value);
                        outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid base64 string (" + ex.Message + ")", NotificationLevel.Error);
                            // outputStream = null;
                            return;
                        }
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.HexStringBinary:
                        try
                        {
                            outputBytes = convertHexStringToByteArray(value);
                        outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid hex-string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.OctalStringBinary:
                        try
                        {
                            outputBytes = convertOctalStringToByteArray(value);
                        outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid octal-string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.Unicode:
                        outputBytes = Encoding.Unicode.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.UTF7:
                        outputBytes = Encoding.UTF7.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.UTF8:
                        outputBytes = Encoding.UTF8.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.UTF32:
                        outputBytes = Encoding.UTF32.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.ASCII:
                        outputBytes = Encoding.ASCII.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    case StringToStreamConverterSettings.EncodingTypes.BigEndianUnicode:
                        outputBytes = Encoding.BigEndianUnicode.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                    default:
                        outputBytes = Encoding.Default.GetBytes(value);
                    outputStream = new CStreamWriter(outputBytes);
                        break;
                }
                                
                ShowStatusBarMessage("Input converted.", NotificationLevel.Info);
                ShowProgress(100, 100);
                OnPropertyChanged("InputString");
                OnPropertyChanged("OutputBytes");
                OnPropertyChanged("OutputStream");
            }
        

        private byte[] convertHexStringToByteArray(String hexString)
        {
            if (null == hexString)
                return new byte[0];
            
            StringBuilder cleanHexString = new StringBuilder();

            //cleanup the input
            foreach (char c in hexString)
            {
                if (Uri.IsHexDigit(c))
                    cleanHexString.Append(c);
            }

            int numberChars = cleanHexString.Length;

            if (numberChars < 2) // Need at least 2 chars to make one byte
                return new byte[0];

            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(cleanHexString.ToString().Substring(i, 2), 16);
            }
            return bytes;
        }


        private byte[] convertOctalStringToByteArray(String octalString)
        {
            if (null == octalString)
                return new byte[0];

            StringBuilder cleanOctalString = new StringBuilder();

            //cleanup the input
            foreach (char c in octalString)
            {
                if (char.IsDigit(c) && c!='8' && c!='9')
                    cleanOctalString.Append(c);
            }

            int numberChars = cleanOctalString.Length;

            if (numberChars < 3) // Need at least 3 chars to make one byte
                return new byte[0];

            byte[] bytes = new byte[numberChars / 3];

            for (int i = 0; i < numberChars; i += 3)
            {
                bytes[i / 3] = Convert.ToByte(cleanOctalString.ToString().Substring(i, 3), 8);
            }
            return bytes;
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
            if ((InputText != null) && (InputText.Length != 0))
            {
          processInput(InputText);
        }
            else
            {
                ShowStatusBarMessage("String input is empty. Nothing to convert.", NotificationLevel.Warning);
            }
        }

        #endregion
    }
}
