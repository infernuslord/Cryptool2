/*
   Copyright 2014 Nils Rehwald

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
using System.Collections.Generic;
using System.Text;
using System;
using Cryptool.PluginBase.IO;
using System.IO;

namespace Cryptool.Plugins.M_138
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("M-138.Properties.Resources", "PluginCaption", "PluginCaptionTooltip", "M_138/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class M_138 : ICrypComponent
    {
        #region Private Variables

        private readonly M_138Settings settings = new M_138Settings();
        enum Commands { Encrypt, Decryp };
        private bool _stopped = true;
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private List<string> stripes = new List<string>();
        private int _numberOfStripes = 0;
        private int[] TextNumbers;
        private char _separatorStripes = ',';
        private char _separatorOffset = '/';
        int _offset;
        int[] _stripNumbers;
        private List<int[]> numStripes = new List<int[]>();


        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputDescription")]
        public string TextInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyDescription")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputDescription")]
        public string TextOutput
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
            _stopped = false;
            readStripes();
            setSeparator();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            TextInput = RemoveInvalidChars(TextInput.ToUpper(), alphabet);
            TextNumbers = MapTextIntoNumberSpace(TextInput, alphabet);
            splitKey();
            switch (settings.ModificationType) {
                case (int) Commands.Encrypt:
                    Encrypt();
                    break;
                case (int) Commands.Decryp:
                    Decrypt();
                    break;
                default:
                    GuiLogMessage("Invalid Selection", NotificationLevel.Error);
                    break; 
            }
            OnPropertyChanged("TextOutput");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _stopped = true;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
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

        #region Helpers
        private void readStripes()
        {
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"), FileMode.Open, FileAccess.Read))
            {
                //System.IO.StreamReader file = new System.IO.StreamReader("../../CrypPluginsExperimental/M-138/stripes.txt"); //TODO: Path? Oder lokal?
                using (var file = new StreamReader(fileStream))
                {
                    string line = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        stripes.Add(line);
                    }
                    file.Close();
                }
            }
        }


        private string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (char c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in numbers)
            {
                builder.Append(alphabet[c]);
            }
            return builder.ToString();
        }

        private void Encrypt()
        {
            int _textlen = TextNumbers.Length;
            int[] output = new int[_textlen];
            for (int i = 0; i < _stripNumbers.Length; i++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[i]], alphabet));
            }
            if (_stripNumbers.Length > 25)
            {
                GuiLogMessage("Number of stripes used should not exceed 25", NotificationLevel.Warning);
            }
            for(int i=0; i<_textlen; i++) {
                int _usedStrip = i % _stripNumbers.Length;
                int[] currentStrip = numStripes[_usedStrip];
                int isAt = Array.IndexOf(currentStrip, TextNumbers[i]);
                output[i] = currentStrip[(isAt + _offset) % alphabet.Length];
            }
            TextOutput = MapNumbersIntoTextSpace(output, alphabet);
        }

        private void Decrypt()
        {
            int _textlen = TextNumbers.Length;
            int[] output = new int[_textlen];
            for (int i = 0; i < _stripNumbers.Length; i++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[i]], alphabet));
            }
            if (_stripNumbers.Length > 25)
            {
                GuiLogMessage("Number of stripes used should not exceed 25", NotificationLevel.Warning);
            }
            for (int i = 0; i < _textlen; i++)
            {
                int _usedStrip = i % _stripNumbers.Length;
                int[] currentStrip = numStripes[_usedStrip];
                int isAt = Array.IndexOf(currentStrip, TextNumbers[i]);
                output[i] = currentStrip[(isAt - _offset + alphabet.Length) % alphabet.Length];
            }
            TextOutput = MapNumbersIntoTextSpace(output, alphabet);
        }

        private void setSeparator()
        {
            switch (settings.SeperatorStripChar)
            {
                case 0:
                    _separatorStripes = ',';
                    break;
                case 1:
                    _separatorStripes = '.';
                    break;
                case 2:
                    _separatorStripes = '/';
                    break;
                default:
                    break;
            }

            switch (settings.SeperatorOffChar)
            {
                case 0:
                    _separatorOffset = '/';
                    break;
                case 1:
                    _separatorOffset = ',';
                    break;
                case 2:
                    _separatorOffset = '.';
                    break;
            }
        }

        private void splitKey()
        {
            string[] splitted;
            splitted = Key.Split(_separatorOffset);
            _offset = Convert.ToInt32(splitted[1]);
            string[] s1 = splitted[0].Split(_separatorStripes);
            _stripNumbers = new int[s1.Length];
            for ( int i=0; i < s1.Length; i++) {
                _stripNumbers[i] = Convert.ToInt32(s1[i]) - 1;
            }
        }

        #endregion
    }
}
