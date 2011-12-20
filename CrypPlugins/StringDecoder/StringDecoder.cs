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
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.Convertor
{
    // Converts a given string into a stream by using different encodings.
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Plugins.Convertor.Properties.Resources", "PluginCaption", "PluginTooltip", "StringDecoder/DetailedDescription/doc.xml", "StringDecoder/t2s-icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StringDecoder : ICrypComponent
    {
        #region Public interface

        /// <summary>
        /// Returns the settings object, or sets it
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (StringDecoderSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get 
            {
                return outputStream;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBytesCaption", "OutputBytesTooltip", true)]
        public byte[] OutputBytes
        {
            get
            {
                return outputBytes;
            }
        }

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get { return this.inputString;  }
            set 
            {
                if (inputString != value)
                {
                    inputString = value;
                    OnPropertyChanged("InputText");
                }
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

        public void Initialize()
        {
            this.settings.SetVisibilityOfEncoding();
        }

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
        private StringDecoderSettings settings = new StringDecoderSettings();
        private CStreamWriter outputStream = null;
        private byte[] outputBytes = null;
        private string inputString;
        #endregion

        #region Private methods

        private byte[] GetBytesForEncoding(string s, StringDecoderSettings.EncodingTypes encoding)
        {
            if (s == null) return null;
            
            switch (encoding)
            {
                case StringDecoderSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetBytes(s);

                case StringDecoderSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetBytes(s);

                default:
                    return Encoding.Default.GetBytes(s);
            }
        }

        private void processInput(string value)
        {
                ShowProgress(50, 100);

                ShowStatusBarMessage("Converting input ...", NotificationLevel.Debug);
                
                // here conversion happens
                switch (settings.PresentationFormatSetting)
                {
                    case StringDecoderSettings.PresentationFormat.Base64:
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

                    case StringDecoderSettings.PresentationFormat.Hex:
                        try
                        {
                            outputBytes = convertHexStringToByteArray(value);
                            outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid hex string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;

                    case StringDecoderSettings.PresentationFormat.Octal:
                        try
                        {
                            outputBytes = convertOctalStringToByteArray(value);
                            outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid octal string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;

                    case StringDecoderSettings.PresentationFormat.Decimal:
                        try
                        {
                            outputBytes = convertDecimalStringToByteArray(value);
                            outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid decimal string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;

                    case StringDecoderSettings.PresentationFormat.Binary:
                        try
                        {
                            outputBytes = convertBinaryStringToByteArray(value);
                            outputStream = new CStreamWriter(outputBytes);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusBarMessage("Error converting input! Not a valid binary string (" + ex.Message + ")", NotificationLevel.Error);
                            return;
                        }
                        break;

                    case StringDecoderSettings.PresentationFormat.Text:
                    default:
                        outputBytes = GetBytesForEncoding(value, settings.Encoding);
                        outputStream = new CStreamWriter(outputBytes);
                        break;
                }

                ShowStatusBarMessage("Input converted.", NotificationLevel.Debug);

                ShowProgress(100, 100);

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
                if (char.IsDigit(c) && c != '8' && c != '9')
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

        private byte[] convertDecimalStringToByteArray(String decimalString)
        {
            MatchCollection matches = new Regex(@"\d+").Matches(decimalString);

            byte[] bytes = new byte[matches.Count];
            int i = 0;

            foreach (Match match in matches)
            {
                int value = Convert.ToInt32(match.Value);
                if (value < 0 || value >= 256) return new byte[0];
                bytes[i++] = (byte)value;
            }

            return bytes;
        }

        private byte[] convertBinaryStringToByteArray(String binaryString)
        {
            MatchCollection matches = new Regex(@"[01]+").Matches(binaryString);

            byte[] bytes = new byte[matches.Count];
            int i = 0;

            foreach (Match match in matches)
            {
                int value = Convert.ToInt32(match.Value,2);
                if (value < 0 || value >= 256) return new byte[0];
                bytes[i++] = (byte)value;
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
            if (!string.IsNullOrEmpty(InputText))
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
