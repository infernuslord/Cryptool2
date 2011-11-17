/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace TextOutput
{
    [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("TextOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "TextOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class TextOutput : DependencyObject, ICrypComponent
    {
        #region Fields and properties

        /// <summary>
        /// This dic is used to store error messages while properties are set in PlayMode. The messages
        /// will be send in the execute method. 
        /// The editor flushes plugin color markers before calling the execute method. 
        /// So this messages would would still appear in LogWindow, but the color marker of the 
        /// plugin(red/yellow) would be lost if sending the messages right on property set.
        /// </summary>
        private Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();
        private TextOutputPresentation textOutputPresentation;

        private TextOutputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (TextOutputSettings)value; }
        }

        private string _currentText;

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public object InputOne
        {
            get
            {
                return _currentText;
            }
            set
            {
                try
                {
                    // special handling for certain data types
                    _currentText = ParseInput(value);

                    if (_currentText != null)
                    {
                        // cut long input text (TODO: this shall be part of ParseInput)
                        if (_currentText.Length > settings.MaxLength)
                            _currentText = _currentText.Substring(0, settings.MaxLength);

                        // add to presentation
                        ShowInPresentation(_currentText);
                    }

                    OnPropertyChanged("CurrentText");
                }
                catch(Exception ex)
                {
                    AddMessage(ex.Message, NotificationLevel.Error);
                }
            }
        }

        #endregion

        #region Event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PresentationFormatSetting" && _currentText != null)
            {
                ShowInPresentation(_currentText);
            }
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion events

        #region Constructor and implementation

        public TextOutput()
        {
            textOutputPresentation = new TextOutputPresentation();
            settings = new TextOutputSettings(this);
            settings.PropertyChanged += settings_PropertyChanged;
        }

        private string ParseInput(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is bool)
                return (settings.BooleanAsNumeric) ? Convert.ToInt32(value).ToString() : ((bool)value).ToString();

            if (value is ICryptoolStream)
            {
                using (CStreamReader reader = ((ICryptoolStream)value).CreateReader())
                {
                    reader.WaitEof(); // does not support chunked streaming

                    if (reader.Length > settings.MaxLength)
                        AddMessage("WARNING - Stream is too large (" + (reader.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                    byte[] byteValues = new byte[Math.Min(settings.MaxLength, reader.Length)];
                    int bytesRead;
                    reader.Seek(0, SeekOrigin.Begin);
                    bytesRead = reader.ReadFully(byteValues, 0, byteValues.Length);
                    return GetStringForSelectedEncoding(byteValues);
                }
            }

            if (value is byte[])
            {
                byte[] byteArray = value as byte[];
                if (byteArray.Length > settings.MaxLength)
                {
                    AddMessage("WARNING - byte array is too large (" + (byteArray.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                }

                long size = byteArray.Length;
                if (size > settings.MaxLength)
                {
                    size = settings.MaxLength;
                }
                byte[] sizedArray = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    sizedArray[i] = byteArray[i];
                }
                return GetStringForSelectedEncoding(sizedArray);
            }

            if (value is Array)
            {
                Array array = (Array)value;
                StringBuilder sb = new StringBuilder();

                foreach (object obj in array)
                {
                    sb.AppendLine(obj == null ? "null" : obj.ToString());
                }
                return sb.ToString();
            }

            return value.ToString();
        }

        private void ShowInPresentation(string fillValue)
        {
            int bytes = 0;
            if (fillValue != null)
            {
                bytes = Encoding.UTF8.GetBytes(fillValue.ToCharArray()).Length;

                // Presentation format conversion
                switch (settings.Presentation)
                {
                    case TextOutputSettings.PresentationFormat.Text:
                        // nothin to do here)
                        break;
                    case TextOutputSettings.PresentationFormat.Hex:
                        //byte[] byteValues = Encoding.UTF8.GetBytes(fillValue.ToCharArray());
                        byte[] byteValues = Encoding.Default.GetBytes(fillValue.ToCharArray());
                        fillValue = BitConverter.ToString(byteValues, 0, byteValues.Length).Replace("-", "");
                        break;
                    case TextOutputSettings.PresentationFormat.Base64:
                        //fillValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(fillValue.ToCharArray()));
                        fillValue = Convert.ToBase64String(Encoding.Default.GetBytes(fillValue.ToCharArray()));
                        break;
                    case TextOutputSettings.PresentationFormat.Decimal:
                        //byte[] decValues = Encoding.UTF8.GetBytes(fillValue.ToCharArray());
                        byte[] decValues = Encoding.Default.GetBytes(fillValue.ToCharArray());
                        StringBuilder sb = new StringBuilder();
                        if (decValues.Length > 0)
                        {
                            sb.Append(decValues[0]);
                            for (int i = 1; i < decValues.Length; i++)
                            {
                                sb.Append(" ");
                                sb.Append(decValues[i]);
                            }
                        }
                        fillValue = sb.ToString();
                        break;
                    default:
                        break;
                }


                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (settings.Append)
                    {
                        if (textOutputPresentation.textBox.Text.Length > settings.MaxLength)
                        {
                            GuiLogMessage("Text exceeds size limit. Deleting text...", NotificationLevel.Debug);
                            textOutputPresentation.textBox.Text = string.Empty;
                            textOutputPresentation.textBox.Tag = 0;
                        }

                        // append line breaks only if not first line
                        if (!string.IsNullOrEmpty(textOutputPresentation.textBox.Text))
                        {
                            for (int i = 0; i < settings.AppendBreaks; i++)
                            {
                                if (settings.Presentation == TextOutputSettings.PresentationFormat.Text)
                                {
                                    int newlineSize = Encoding.UTF8.GetBytes("\n".ToCharArray()).Length;
                                    textOutputPresentation.textBox.Tag = (int)textOutputPresentation.textBox.Tag + newlineSize;
                                }
                                textOutputPresentation.textBox.AppendText("\n");
                            }
                        }
                        textOutputPresentation.textBox.AppendText(fillValue);
                        textOutputPresentation.textBox.Tag = (int)textOutputPresentation.textBox.Tag + bytes;

                        textOutputPresentation.textBox.ScrollToEnd();
                    }
                    else
                    {
                        textOutputPresentation.textBox.Text = fillValue;
                        textOutputPresentation.textBox.Tag = bytes;
                    }
                    if (settings.BooleanAsNumeric)
                    {
                        textOutputPresentation.labelBytes.Content = string.Format("{0:0,0}", Encoding.UTF8.GetBytes(textOutputPresentation.textBox.Text.ToCharArray()).Length) + " Bits";
                    }
                    else
                    {
                        textOutputPresentation.labelBytes.Content = string.Format("{0:0,0}", (int)textOutputPresentation.textBox.Tag) + " Bytes";
                    }
                }, fillValue);
            }
        }

        private string GetStringForSelectedEncoding(byte[] arrByte)
        {
            if (arrByte != null)
            {
                GuiLogMessage("Converting from \"" + settings.Encoding.ToString() + "\"...", NotificationLevel.Debug);
                string returnValue;

                // here conversion happens
                switch (settings.Encoding)
                {
                    case TextOutputSettings.EncodingTypes.Unicode:
                        returnValue = Encoding.Unicode.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.UTF7:
                        returnValue = Encoding.UTF7.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.UTF8:
                        returnValue = Encoding.UTF8.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.UTF32:
                        returnValue = Encoding.UTF32.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.ASCII:
                        returnValue = Encoding.ASCII.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.BigEndianUnicode:
                        returnValue = Encoding.BigEndianUnicode.GetString(arrByte, 0, arrByte.Length);
                        break;
                    case TextOutputSettings.EncodingTypes.Default:
                    default:
                        returnValue = Encoding.Default.GetString(arrByte, 0, arrByte.Length);
                        break;
                }
                return returnValue;
            }
            return null;
        }

        private void AddMessage(string message, NotificationLevel level)
        {
            if (!dicWarningsAndErros.ContainsKey(message))
                dicWarningsAndErros.Add(message, level);
        }

        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get { return textOutputPresentation; }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {
            textOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                textOutputPresentation.textBox.Tag = 0;
                textOutputPresentation.textBox.Text = null;
            }, null);
        }

        public void PostExecution()
        {
        }

        public void Execute()
        {
            Progress(100, 100);
            foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
            {
                GuiLogMessage(kvp.Key, kvp.Value);
            }
            dicWarningsAndErros.Clear();
        }

        #endregion
    }
}
