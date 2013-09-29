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
using System.Collections;
using System;
using System.Collections.Generic;

namespace Cryptool.Plugins.SpanishStripCipher
{
    // HOWTO: Change author name, email address, organization and URL.
   [Author("Prof. Christof Paar, Prof. Gregor Leander, Luis Alberto Benthin Sanguino", "Luis.BenthinSanguino@rub.de", "Ruhr-Universität Bochum - Chair for Embedded Security", "http://www.emsec.rub.de/chair/home/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
   [PluginInfo("SpanishStripCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "SpanishStripCipher/DetailedDescription/doc.xml", new[] { "SpanishStripCipher/Images/SpanishStripCipher.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class SpanishStripCipher : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly SpanishStripCipherSettings settings = new SpanishStripCipherSettings();
        private List<List<string>> homophones = new List<List<string>>();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SpanishStripCipher()
        {
            this.settings.LogMessage += GuiLogMessage;
        }

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        private string _imputString;
        [PropertyInfo(Direction.InputData, "input", "InputTooltip", true)]
        public string SomeInput
        {
            get { return _imputString; }
            set { _imputString = value; }
        }
        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "output", "OutputTooltip")]
        public string SomeOutput
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

            homophones = settings.getHomophones();
        }
        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {

            StringBuilder output = new StringBuilder();
            string orderedAlphabet = settings.OrderedAlphabet;
            string unorderedAlphabet = settings.unorderedAlphabet;
            int index = 0;
            int errorType = 0;

            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            if (string.IsNullOrEmpty(SomeInput))
            {
                errorType = 1;
            }
            else if (string.IsNullOrEmpty(settings.Keyword))
            {
                errorType = 2;
            }
            else if (!checkKeyword(settings.Keyword.ToUpper()))
            {
                errorType = 3;
            }
            switch (errorType)
            {
                case 0:
                    int index2 = 0;
                    string number = "";
                    int l = 0;
                    string temp;
                    char currentChar;
                    bool space = false;
                    for (int i = 0; i < SomeInput.Length + 1; i++)
                    {
                        switch (settings.Action)
                        {
                            case SpanishStripCipherSettings.CipherMode.Encrypt:
                                if (i < SomeInput.Length)
                                {
                                    temp = "0";
                                    SomeInput = SomeInput.ToUpper();
                                    //TODO if Alpahebt 29 selected letter[i] == C or L || letter[i] == H or L
                                    currentChar = SomeInput[i];
                                    if (settings.Alphabets == 0)
                                    {
                                        index = unorderedAlphabet.IndexOf(currentChar);
                                    }
                                    else
                                    {
                                        if (SomeInput.Length != i+1)
                                        {
                                            if (currentChar == 'C' && SomeInput[i + 1] == 'H')
                                            {
                                                index = unorderedAlphabet.IndexOf("ß"); //CH<-ß encoded
                                                i++;
                                            }
                                            else if (currentChar == 'L' && SomeInput[i + 1] == 'L')
                                            {
                                                index = unorderedAlphabet.IndexOf("Ä"); //LL<-Ä encoded
                                                i++;
                                            }
                                            else
                                            {
                                                index = unorderedAlphabet.IndexOf(currentChar);
                                            }
                                        }
                                        else
                                        {
                                            index = unorderedAlphabet.IndexOf(currentChar);
                                        }
                                    }
                                    if (index != -1)
                                    {
                                        output = output.Append(homophones[index][0]);
                                        temp = homophones[index][0];
                                        int j = 0;
                                        for (j = 0; j < homophones[index].Count - 1; j++){
                                            homophones[index][j] = homophones[index][j + 1];
                                        }
                                        homophones[index][j] = temp;
                                    }
                                    /*else{
                                        output = output.Append(" ");
                                    }*/
                                }
                            break;
                            case SpanishStripCipherSettings.CipherMode.Decrypt:
                                if (i < SomeInput.Length)
                                {
                                    if (SomeInput[i].ToString() == " ")
                                    {
                                        space = true;
                                    }
                                    else
                                    {
                                        if (l == 2)
                                        {
                                            index2 = findIndex(number);
                                            if (unorderedAlphabet[index2] =='Ä')
                                            {
                                                output = output.Append("LL"); //Ä<-LL encoded
                                            }
                                            else if (unorderedAlphabet[index2] == 'ß')
                                            {
                                                output = output.Append("CH"); //ß<-CH encoded
                                            }
                                            else
                                            {
                                                output = output.Append(unorderedAlphabet[index2]);
                                            }
                                            if (space)
                                            {
                                                output = output.Append(" ");
                                                space = false;
                                            }
                                            l = 0;
                                            number = "";
                                        }
                                        number = number + SomeInput[i];
                                        l++;
                                    }
                                }
                                else
                                {
                                    index2 = findIndex(number);
                                    output = output.Append(unorderedAlphabet[index2]);
                                }
                            break;
                        }
                        //Show the progress.
                        ProgressChanged(i, SomeInput.Length - 1);
                    }
                    SomeOutput = output.ToString();
                    OnPropertyChanged("SomeOutput");
                break;
                case 1:
                    ProgressChanged(100, SomeInput.Length - 1);
                    if (settings.Action == SpanishStripCipherSettings.CipherMode.Encrypt)
                    {
                        GuiLogMessage("Please enter plaintext to be encrypted.", NotificationLevel.Info);
                    }
                    else
                    {
                        GuiLogMessage("Please enter ciphertext to be decrypted.", NotificationLevel.Info);
                    }
                    SomeOutput = " ";
                    OnPropertyChanged("SomeOutput");
                break;
                case 2:
                    ProgressChanged(100, SomeInput.Length - 1);
                    GuiLogMessage("The parameter \"keyword\" cannot be left empty.", NotificationLevel.Error);
                    SomeOutput = " ";
                    OnPropertyChanged("SomeOutput");
                break;
                case 3:
                    ProgressChanged(100, SomeInput.Length - 1);
                    GuiLogMessage("The parameter \"keyword\" must only contain letters of the fixed alphabet", NotificationLevel.Error);
                    SomeOutput = " ";
                    OnPropertyChanged("SomeOutput");
                break;
            }
            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
        }
        public bool checkKeyword(string keyword)
        {
            bool result = true;
            for (int i = 0; i < keyword.Length; i++)
            {
                if (settings.OrderedAlphabet.IndexOf(keyword[i]) == -1)
                {
                    result = false;
                    i = keyword.Length;
                }
            }
            return result;
        }
        public int findIndex(string number)
        {
            int index = 0;
            for (int i = 0; i < homophones.Count; i++)
            {
                for (int j = 0; j < homophones[i].Count; j++)
                {
                    if (homophones[i][j].ToString() == number)
                    {
                        index = i;
                    }
                }
            }
            return index;
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