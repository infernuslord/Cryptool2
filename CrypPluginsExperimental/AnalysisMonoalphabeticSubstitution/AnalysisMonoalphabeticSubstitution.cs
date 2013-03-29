﻿/*
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("AnalysisMonoalphabeticSubstitution.Properties.Resources","PluginCaption", "PluginTooltip", null, "CrypWin/images/default.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Frequencies langFreq = null;
        private LanguageDictionary langDic = null;
        private Text cText = null;
        private Boolean inputOK = true;

        // Input change properties
        private Boolean ciphertext_has_changed = true;
        private Boolean ciphertext_alphabet_has_changed = true;
        private Boolean plaintext_alphabet_has_changed = true;
        private Boolean reference_text_has_changed = true;
        private Boolean language_dictionary_has_changed = true;

        // Input property variables
        private ICryptoolStream ciphertext;
        private String ciphertext_alphabet;
        private String plaintext_alphabet;
        private ICryptoolStream reference_text;
        private ICryptoolStream language_dictionary;

        // Output property variables
        private String plaintext; 

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Ciphertext", "Encrypted text", true)]
        public ICryptoolStream Ciphertext
        {
            get { return this.ciphertext; }
            set {
                this.ciphertext = value;
                this.ciphertext_has_changed = true;
                OnPropertyChanged("Ciphertext");
            }
        }

        [PropertyInfo(Direction.InputData, "Ciphertext Alphabet", "Alphabet of the ciphertext", false)]
        public String Ciphertext_Alphabet
        {
            get { return this.ciphertext_alphabet; }
            set {
                this.ciphertext_alphabet = value;
                this.ciphertext_alphabet_has_changed = true;
            }
        }

        [PropertyInfo(Direction.InputData, "Plaintext Alphabet", "Assumed alphabet of the plaintext", false)]
        public String Plaintext_Alphabet
        {
            get { return this.plaintext_alphabet; }
            set{
                this.plaintext_alphabet = value;
                this.plaintext_alphabet_has_changed = true;
            }
        }

        [PropertyInfo(Direction.InputData, "Reference Text", "Sample text to extract letter frequencies of the assumed plaintext language", false)]
        public ICryptoolStream Reference_Text
        {
            get { return this.reference_text; }
            set {
                this.reference_text = value;
                this.reference_text_has_changed = true;
            }
        }

        [PropertyInfo(Direction.InputData, "Dictionary", "Dictionary of assumed plaintext language", false)]
        public ICryptoolStream Language_Dictionary
        {
            get { return this.language_dictionary; }
            set {
                this.language_dictionary = value;
                this.language_dictionary_has_changed = true;
            }
        }

        [PropertyInfo(Direction.OutputData, "Plaintext", "Decrypted text", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
            set { this.plaintext = value; }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Text RText = null;
            String helper = null;

            // Prepare the cryptanalysis of the ciphertext

            // Set ciphertext alphabet, plaintext alphabet 
            if (settings.SeparateAlphabets == false)
            {
                // Set ciphertext and plaintext alphabet
                if (settings.boAlphabet == 0 || settings.boAlphabet == 1)
                {
                    if (settings.bo_alphabet_has_changed == true)
                    {
                        settings.bo_alphabet_has_changed = false;
                        this.ptAlphabet = new Alphabet(settings.boTextAlphabet, 1);
                        this.ctAlphabet = new Alphabet(settings.boTextAlphabet, 1);
                    }
                }
                else if (settings.boAlphabet == 2)
                {
                    //this.PTAlphabet = this.plainAlphabet;
                    //this.CTAlphabet = this.cipherAlphabet;
                }
            }
            else if (settings.SeparateAlphabets == true)
            {
                // Set plaintext alphabet
                if (settings.ptAlphabet == 0 || settings.ptAlphabet == 1)
                {
                    if (settings.pt_alphabet_has_changed == true)
                    {
                        settings.pt_alphabet_has_changed = false;
                        this.ptAlphabet = new Alphabet(settings.ptTextAlphabet, 1);
                    }
                }
                else if (settings.ptAlphabet == 2)
                {
                    //this.PTAlphabet = this.plainAlphabet;  
                }
                
                // Set ciphertext alphabet
                if (settings.ctAlphabet == 0 || settings.ctAlphabet == 1)
                {
                    if (settings.ct_alphabet_has_changed == true)
                    {
                        settings.ct_alphabet_has_changed = false;
                        this.ctAlphabet = new Alphabet(settings.ctTextAlphabet, 1);
                    }
                }
                else if (settings.ctAlphabet == 2)
                {
                    // this.CTAlphabet = this.cipherAlphabet;
                }
            }
            
            // Reference text 
            // Check reference text input first
            if (this.reference_text != null)
            {
                if (this.reference_text_has_changed == true)
                {
                    this.reference_text_has_changed = false;
                    try
                    {
                        helper = returnStreamContent(this.reference_text);
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining reference text stream.",NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        RText = new Text(helper, this.ptAlphabet);
                    }
                }
            }
            // Check standard files second
            else 
            { 
                if (settings.SeparateAlphabets==true)
                {
                    if (settings.boAlphabet == 0)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_ref_eng.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining English reference text file.", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            RText = new Text(helper, this.ptAlphabet);
                        }
                    }
                    else if (settings.boAlphabet == 1)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_ref_ger.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining German reference text file.", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            RText = new Text(helper, this.ptAlphabet);
                        }
                    }
                    else if (settings.boAlphabet == 2)
                    {
                        RText = null;
                    }
                }
                else if (settings.SeparateAlphabets == false)
                {
                    if (settings.ptAlphabet == 0)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_ref_eng.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining English reference text file.", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            RText = new Text(helper, this.ptAlphabet);
                        }
                    }
                    else if (settings.ptAlphabet == 1)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_ref_ger.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining German reference text file.", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            RText = new Text(helper, this.ptAlphabet);
                        }
                    }
                    else if (settings.ptAlphabet == 2)
                    {
                        RText = null;
                    }
                }
            }

            helper = null;

            // Optional dictionary
            // Check dictionary input first
            if (this.language_dictionary != null)
            {
                if (this.language_dictionary_has_changed == true)
                {
                    this.language_dictionary_has_changed = false;
                    try
                    {
                        helper = returnStreamContent(this.language_dictionary);
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining language dictionary stream.",NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        this.langDic = new LanguageDictionary(helper, ' ');
                    }
                }
            }
            // Check standard files second
            else
            {
                if (settings.SeparateAlphabets == true)
                {
                    if (settings.boAlphabet == 0)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_dic_eng.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                        }
                        if ( helper != null )
                        {
                            this.langDic = new LanguageDictionary(helper, ' ');
                        }
                    }
                    else if (settings.boAlphabet == 1)
                    {
                        try 
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_dic_ger.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining German language dictionary file.", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            this.langDic = new LanguageDictionary(helper, ' ');
                        }
                    }
                    else if (settings.boAlphabet == 2)
                    {
                        this.langDic = null;
                    }
                }
                else if (settings.SeparateAlphabets == false)
                {
                    if (settings.ptAlphabet == 0)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_dic_eng.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            this.langDic = new LanguageDictionary(helper, ' ');
                        }
                    }
                    else if (settings.ptAlphabet == 1)
                    {
                        try
                        {
                            helper = returnFileContent("AnalysisMonoalphabeticSubstitution_dic_eng.txt");
                        }
                        catch
                        {
                            GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                        }
                        if (helper != null)
                        {
                            this.langDic = new LanguageDictionary(helper, ' ');
                        }
                    }
                    else if (settings.ptAlphabet == 2)
                    {
                        this.langDic = null;
                    }
                }
            }

            helper = null;

            // Set ciphertext
            if (this.ciphertext_has_changed == true)
            {
                this.ciphertext_has_changed = false;
                try
                {
                    helper = returnStreamContent(this.ciphertext);
                }
                catch
                {
                    GuiLogMessage("Error while obtaining ciphertext.", NotificationLevel.Error);
                }
                if (helper != null)
                {
                    this.cText = new Text(helper, this.ctAlphabet);
                }
                else 
                {
                    this.cText = null;
                }
            }

            // PTAlphabet correct?
            if (this.ptAlphabet == null)
            {
                GuiLogMessage("No plaintext alphabet is set", NotificationLevel.Error);
                this.inputOK = false;
            }
            // CTAlphabet correct?
            if (this.ctAlphabet == null)
            {
                GuiLogMessage("No ciphertext alphabet is set", NotificationLevel.Error);
                this.inputOK = false;
            }
            // Ciphertext correct?
            if (this.ciphertext == null)
            {
                GuiLogMessage("No ciphertext is set", NotificationLevel.Error);
                this.inputOK = false;
            }
            // Reference text correct?
            if (RText == null)
            {
                this.langFreq = null;
                GuiLogMessage("No reference text is set", NotificationLevel.Warning);
            }
            else
            {
                this.langFreq = new Frequencies(this.ptAlphabet);
                this.langFreq.updateFrequenciesProbabilities(RText);
            }
            // Dictionary correct?
            if (this.langDic == null)
            {
                GuiLogMessage("No language dictionary is set", NotificationLevel.Warning);
            }
            // Language frequencies and dictionary mustn't be null
            if (this.langFreq == null && this.langDic == null)
            {
                GuiLogMessage("Language frequencies or a language dictionary is needed.", NotificationLevel.Error);
                this.inputOK = false;
            }
            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length != this.ptAlphabet.Length)
            {
                GuiLogMessage("Length of ciphertext alphabet and plaintext alphabet is different.", NotificationLevel.Error);
                this.inputOK = false;
            }
        }

        public void Execute()
        {
            // If input incorrect return
            if (this.inputOK == false)
            {
                this.inputOK = true;
                return;
            }

            // Create new analyzer
            Analyzer analyzer = new Analyzer();
            // Set progress to 0
            ProgressChanged(0, 52);
            // Initialize analyzer
            analyzer.Ciphertext = this.cText;
            analyzer.Ciphertext_Alphabet = this.ctAlphabet;
            analyzer.Plaintext_Alphabet = this.ptAlphabet;
            analyzer.Language_Frequencies = this.langFreq;
            analyzer.Language_Dictionary = this.langDic;
            
            // Start analysis
            analyzer.StartAnalysis();
            
            // Process several generations
            ProgressChanged(1, 52); 
            for (int i = 0; i < 50; i++)
            {
                analyzer.NextStep();
                ProgressChanged(i+1, 52);
            }

            // Take best key of last generation
            analyzer.LastStep();
            ProgressChanged(51, 52);

            this.plaintext = analyzer.Plaintext.ToString(this.ptAlphabet);
            OnPropertyChanged("Plaintext");
       

            ProgressChanged(52, 52);
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            settings.Initialize();
        }

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

        #region Helper Functions

        private string returnFileContent(String filename)
        {
            string res = "";

            using (TextReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename)))
            {
                res = reader.ReadToEnd();
            }
            return res;
        }

        private string returnStreamContent(ICryptoolStream stream)
        {
            string res = "";

            if (stream == null)
            {
                return "";
            }

            using (CStreamReader reader = stream.CreateReader())
            {
                res = Encoding.Unicode.GetString(reader.ReadFully());
            }

            return res;
        }

        #endregion
    }
}