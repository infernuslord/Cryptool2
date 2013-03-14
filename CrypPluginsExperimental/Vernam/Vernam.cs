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
using Cryptool.PluginBase.Miscellaneous;
using System.Text;

namespace Cryptool.Plugins.Vernam
{
   
    [Author("Benedict Beuscher", "benedict.beuscher@hotmail.com", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("Vernam Cipher", "Combines any alphanumeric plaintext with a keytext to get a vernam-encrypted ciphertext", "Vernam/userdoc.xml", new[] { "Vernam/Images/Vernam.png" })]

    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Vernam : ICrypComponent
    {
        #region Private Variables

        private readonly VernamSettings settings = new VernamSettings();
    
        #endregion

        #region Data Properties

        private string inputString;
        private string outputString;
        private string keyString;


        [PropertyInfo(Direction.InputData, "Key input", "KeyTooltip", false)]
        public string KeyString
        {
            get { return this.keyString; }
            set
            {
                if (value != keyString)
                {
                    this.keyString = value;
                    OnPropertyChanged("newKeyString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be encrypted by the Vernam Cipher",true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != InputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("newInputString");
                }
            }
        }

        

        [PropertyInfo(Direction.OutputData, "Text output", "The string encrypted by the Vernam Cipher",false)]
        public string OutputString
        {
            get { return this.outputString; }
            set { this.outputString = value; }
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
            get { return null; }
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
        public void Execute()
        {
            ProgressChanged(0, 1);
            StringBuilder newOutputString = new StringBuilder();
            string alphabet = settings.alphabet;
            
            if (!string.IsNullOrEmpty(InputString))
            {
                for (int i = 0; i < InputString.Length; i++)
                {
                    char currentChar = InputString[i];
                    char currentKeyChar = KeyString[i%KeyString.Length];
                    int currentCharPosition = alphabet.IndexOf(currentChar);
                    int currentKeyCharPosition = alphabet.IndexOf(currentKeyChar);
                    int cipherCharPosition = 0;
                    if (currentCharPosition >= 0)
                    {

                        if (settings.Action == VernamSettings.CipherMode.Encrypt)
                        {
                            cipherCharPosition = (currentCharPosition + currentKeyCharPosition) % alphabet.Length;
                        }
                        else if (settings.Action == VernamSettings.CipherMode.Decrypt)
                        {
                            cipherCharPosition = (currentCharPosition - currentKeyCharPosition + alphabet.Length) % alphabet.Length;
                        }
                        newOutputString.Append(alphabet[cipherCharPosition]);
                    }
                    else
                    {
                        if (settings.UnknownSymbolHandling == VernamSettings.UnknownSymbolHandlingMode.Ignore)
                        {
                            newOutputString.Append(currentKeyChar);
                        }
                        else if (settings.UnknownSymbolHandling == VernamSettings.UnknownSymbolHandlingMode.Replace)
                        {
                            newOutputString.Append("#");
                        }
                    }

                    

                }


            }



            ProgressChanged(1, 1);
            OutputString = newOutputString.ToString();
            OnPropertyChanged("OutputString");


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
