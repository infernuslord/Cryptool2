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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Sigaba
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Julian Weyers", "weyers@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Sigaba.Properties.Resources", "PluginCaption", "PluginToolTip", "Enigma/DetailedDescription/doc.xml",
      "Sigaba/Images/Icon.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class Sigaba : ICrypComponent
    {
        #region Constructor 
        public Sigaba()
        {
            
            SigabaPresentation sigpa = new SigabaPresentation(this,_settings);
            Presentation = sigpa;
            this._settings.PropertyChanged += sigpa.settings_OnPropertyChange;
            
            _core = new SigabaCore(this, sigpa);
            _keys = new string[5];

            this._settings.PropertyChanged += _core.settings_OnPropertyChange;
        }

        #endregion

        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly SigabaSettings _settings = new SigabaSettings();
        private readonly SigabaCore _core;
        private string[] _keys;

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "(De)Cipher", "Sets the (De)Cipher as input")]
        public string InputString
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "(De)Cipher", "Sets the (De)Cipher as output")]
        public string OutputString
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
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _core.SetInternalConfig();
            _keys[0] = _settings.CipherKey;
            _keys[1] = _settings.IndexKey;
            _keys[2] = _settings.ControlKey;

        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            _settings.CipherKey = _keys[0].ToUpper();
            _settings.IndexKey = _keys[1].ToUpper();
            _settings.ControlKey = _keys[2].ToUpper();
            
            _core.SetKeys();
            if(!Presentation.IsVisible)
            {
                OutputString = postFormatOutput(_core.Encrypt(preFormatInput(InputString)));
            }
            else
            {
                OutputString = postFormatOutput(_core.EncryptPresentation(preFormatInput(InputString)));
            }
            OnPropertyChanged("OutputString");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _settings.CipherKey = _keys[0].ToUpper();
            _settings.IndexKey = _keys[1].ToUpper();
            _settings.ControlKey = _keys[2].ToUpper();

             _core.stop();
            ((SigabaPresentation)Presentation).ClearPresentation();

        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _core.stop();   
            ((SigabaPresentation)Presentation).ClearPresentation();
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

        #region ConnectorProperties
        
        #endregion

        #region Tools 
        
        internal class UnknownToken
        {
            internal string text;
            internal int position;

            internal UnknownToken(char c, int position)
            {
                this.text = char.ToString(c);
                this.position = position;
            }

            public override string ToString()
            {
                return "[" + text + "," + position + "]";
            }
        }

        IList<UnknownToken> unknownList = new List<UnknownToken>();
        IList<UnknownToken> lowerList = new List<UnknownToken>();

        private string preFormatInput(string text)
        {
            StringBuilder result = new StringBuilder();
            bool newToken = true;
            unknownList.Clear();
            lowerList.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                if (_settings.Alphabet.Contains(char.ToUpper(text[i])))
                {
                    newToken = true;
                    if (text[i] == char.ToLower(text[i])) //Solution for preserve FIXME underconstruction
                    {
                        if (_settings.UnknownSymbolHandling == 1)
                        {
                            lowerList.Add(new UnknownToken(text[i], result.Length));
                        }
                        else
                        {
                            lowerList.Add(new UnknownToken(text[i], i));
                        }

                    }                                      //underconstruction end
                    result.Append(char.ToUpper(text[i])); // FIXME: shall save positions of lowercase letters
                }
                else if (_settings.UnknownSymbolHandling != 1) // 1 := remove
                {
                    // 0 := preserve, 2 := replace by X
                    char symbol = _settings.UnknownSymbolHandling == 0 ? text[i] : 'X';

                    if (newToken)
                    {
                        unknownList.Add(new UnknownToken(symbol, i));
                        newToken = false;
                    }
                    else
                    {
                        unknownList.Last().text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();

        }

        private string postFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in unknownList)
            {
                workstring.Insert(token.position, token.text);
            }

            foreach (UnknownToken token in lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token.position];
                workstring.Remove(token.position, 1);
                workstring.Insert(token.position, char.ToLower(help));
            }                                           //underconstruction end

            switch (_settings.CaseHandling)
            {
                default:
                case 0: // preserve
                    // FIXME: shall restore lowercase letters
                    return workstring.ToString();
                case 1: // upper
                    return workstring.ToString().ToUpper();
                case 2: // lower
                    return workstring.ToString().ToLower();
            }
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
