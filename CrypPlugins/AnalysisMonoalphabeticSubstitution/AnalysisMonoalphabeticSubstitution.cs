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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using AnalysisMonoalphabeticSubstitution.Properties;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String key_string, String plaintext_string);
    delegate double CalculateFitness(Text plaintext);
    delegate void UpdateKeyDisplay(KeyCandidate keyCan);

    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("AnalysisMonoalphabeticSubstitution.Properties.Resources", "PluginCaption", "PluginTooltip", "AnalysisMonoalphabeticSubstitution/Documentation/doc.xml", "AnalysisMonoalphabeticSubstitution/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // StopFlag
        StopFlag stopFlag = new StopFlag();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Frequencies langFreq = null;
        private Dictionary langDic = null;
        private Text cText = null;
        private List<KeyCandidate> keyCandidates;

        // Statistics
        private TimeSpan total_time = new TimeSpan();
        private TimeSpan currun_time;

        // Input property variables
        private String ciphertext;
      
        // Output property variables
        private String plaintext;
        private String plaintext_alphabet_output;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();
        private DateTime startTime;
        private DateTime endTime;

        // Alphabet constants
        private const String English = "abcdefghijklmnopqrstuvwxyz";

        // Attackers
        private DictionaryAttacker dicAttacker;
        private GeneticAttacker genAttacker;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PropCiphertextCaption", "PropCiphertextTooltip", true)]
        public String Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
        }
       
        [PropertyInfo(Direction.OutputData, "PropPlaintextCaption", "PropPlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
            set { }
        }

        [PropertyInfo(Direction.OutputData, "PropPlaintextalphabetoutputCaption", "PropPlaintextalphabetoutputTooltip", true)]
        public String Plaintext_Alphabet_Output
        {
            get { return this.plaintext_alphabet_output; }
            set { }
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
            get { return this.masPresentation; }
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

        }

        public void Execute()
        {
            this.genAttacker = new GeneticAttacker();
            this.dicAttacker = new DictionaryAttacker();

            String alpha = "";
            Boolean inputOK = true;

            // Clear presentation
            ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).entries.Clear();
            }, null);

            // Prepare the cryptanalysis of the ciphertext

            // Set alphabet 
            alpha = detAlphabet(settings.Alphabet);
            this.ptAlphabet = new Alphabet(alpha, 1, settings.Alphabet);
            this.ctAlphabet = new Alphabet(alpha, 1, settings.Alphabet);

            // N-gram probabilities    
            String helper = IdentifyNGramFile(settings.Alphabet);
            if (helper != null)
            {
                this.langFreq = new Frequencies(this.ptAlphabet);
                this.langFreq.ReadProbabilitiesFromNGramFile(helper);
            }
            else
            {
                GuiLogMessage(Resources.no_ngram_file, NotificationLevel.Error);
            }
             
            // Dictionary
            if (settings.Alphabet == 0)
            {
                try
                {
                    this.langDic = new Dictionary("en-small.dic");
                }
                catch
                {
                    GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
                }
            }
            // Add new case for another language
            // elseif (settings.Alphabet == 1)
            // {
            // ......
            // }

            // Set ciphertext
            String helper1 = null;
            try
            {
                //helper1 = returnStreamContent(this.ciphertext);
                if (this.ciphertext.Length != 0)
                {
                    helper1 = this.ciphertext;
                }
            }
            catch
            {
                GuiLogMessage(Resources.error_ciphertext, NotificationLevel.Error);
            }
            if (helper1 != null)
            {
                this.cText = new Text(helper1, this.ctAlphabet, settings.TreatmentInvalidChars);
            }
            else
            {
                this.cText = null;
            }



            // PTAlphabet correct?
            if (this.ptAlphabet == null)
            {
                GuiLogMessage(Resources.no_plaintext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }
            // CTAlphabet correct?
            if (this.ctAlphabet == null)
            {
                GuiLogMessage(Resources.no_ciphertext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }
            // Ciphertext correct?
            if (this.cText == null)
            {
                GuiLogMessage(Resources.no_ciphertext, NotificationLevel.Error);
                inputOK = false;
            }
            // Dictionary correct?
            if (this.langDic == null)
            {
                GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
            }
            // Language frequencies
            if (this.langFreq == null)
            {
                GuiLogMessage(Resources.no_lang_freq, NotificationLevel.Error);
                inputOK = false;
            }
            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length != this.ptAlphabet.Length)
            {
                GuiLogMessage(Resources.error_alphabet_length, NotificationLevel.Error);
                inputOK = false;
            }

            // If input incorrect return otherwise execute analysis
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            
            if (inputOK == false)
            {
                inputOK = true;
                return;
            }
            else
            {
                this.UpdateDisplayStart();
                //this.masPresentation.DisableGUI();
                this.masPresentation.UpdateOutputFromUserChoice = this.UpdateOutput;
                this.keyCandidates = new List<KeyCandidate>();
                if (this.langDic == null)
                {
                    AnalyzeGenetic();
                }
                else
                {
                    AnalyzeDictionary();
                    AnalyzeGenetic();
                }
                //this.masPresentation.EnableGUI();
                this.UpdateDisplayEnd();
            }
        }

        public void PostExecution()
        {
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = false;
            }
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            this.dicAttacker.StopFlag = true;
            this.genAttacker.StopFlag = true;
            this.langDic.StopFlag = true;
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = true;
            }
        }

        public void Initialize()
        {
            this.settings.Initialize();
        }

        public void Dispose()
        {
        }

        private void AnalyzeDictionary()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ////////////////////// Create keys with dictionary attacker
            // Initialize dictionary attacker
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
                //this.dicAttacker = new DictionaryAttacker();
            this.dicAttacker.ciphertext = this.cText;
            this.dicAttacker.languageDictionary = this.langDic;
            this.dicAttacker.frequencies = this.langFreq;
            this.dicAttacker.ciphertext_alphabet = this.ctAlphabet;
            this.dicAttacker.plaintext_alphabet = this.ptAlphabet;
            this.dicAttacker.PluginProgressCallback = this.ProgressChanged;
            this.dicAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            

            // Prepare text
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            this.dicAttacker.PrepareAttack();
            

            // Deterministic search
            // Try to find full solution with all words enabled
     
                this.dicAttacker.SolveDeterministicFull();

                // Try to find solution with disabled words
                if (!this.dicAttacker.CompleteKey)
                {
                    this.dicAttacker.SolveDeterministicWithDisabledWords();

                    // Randomized search;
                    if (!this.dicAttacker.PartialKey)
                    {
                        this.dicAttacker.SolveRandomized();
                    }
                }
                  

            watch.Stop();

            string curTime = String.Format("{0:00}:{1:00}:{2:00}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
            GuiLogMessage(Resources.dic_attack_finished + curTime, NotificationLevel.Info);
        }

        private void AnalyzeGenetic()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ////////////////// Create keys with genetic attacker
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }

            // Initialize analyzer
            this.genAttacker.Ciphertext = this.cText;
            this.genAttacker.Ciphertext_Alphabet = this.ctAlphabet;
            this.genAttacker.Plaintext_Alphabet = this.ptAlphabet;
            this.genAttacker.Language_Frequencies = this.langFreq;
            this.genAttacker.PluginProgressCallback = this.ProgressChanged;
            this.genAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            

            // Start attack
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            this.genAttacker.Analyze();
            
            
            watch.Stop();

            string curTime = String.Format("{0:00}:{1:00}:{2:00}", watch.Elapsed.Hours ,watch.Elapsed.Minutes, watch.Elapsed.Seconds);
            GuiLogMessage(Resources.gen_attack_finished + curTime, NotificationLevel.Info);
            GuiLogMessage(Resources.gen_attack_testedkeys + genAttacker.Currun_Keys.ToString("#,##0"), NotificationLevel.Info);
        }

        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            bool update = false;

            // Add key if key does not already exists
            if (!this.keyCandidates.Contains(keyCan))
            {
                this.keyCandidates.Add(keyCan);
                this.keyCandidates.Sort(new KeyCandidateComparer());

                if (this.keyCandidates.Count > 20)
                {
                    this.keyCandidates.RemoveAt(this.keyCandidates.Count - 1);
                }
                update = true;
            }
            else
            {
                int index = this.keyCandidates.IndexOf(keyCan);
                KeyCandidate keyCanAlreadyInList = this.keyCandidates[index];
                if (keyCan.DicAttack == true)
                {
                    if (keyCanAlreadyInList.DicAttack == false)
                    {
                        keyCanAlreadyInList.DicAttack = true;
                        update = true;
                    }
                }
                if (keyCan.GenAttack == true)
                {
                    if (keyCanAlreadyInList.GenAttack == false)
                    {
                        keyCanAlreadyInList.GenAttack = true;
                        update = true;
                    }
                }
            }

            // Display output
            if (update)
            {
                this.plaintext = this.keyCandidates[0].Plaintext;
                OnPropertyChanged("Plaintext");

                this.plaintext_alphabet_output = CreateAlphabetOutput(this.keyCandidates[0].Key, this.ctAlphabet);
                OnPropertyChanged("Plaintext_Alphabet_Output");

                ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((AssignmentPresentation)Presentation).entries.Clear();

                    for (int i = 0; i < this.keyCandidates.Count; i++)
                    {
                        KeyCandidate keyCandidate = this.keyCandidates[i];

                        ResultEntry entry = new ResultEntry();
                        entry.Ranking = i.ToString();
                        entry.Text = keyCandidate.Plaintext;
                        entry.Key = keyCandidate.Key_string;

                        if (keyCandidate.GenAttack == true && keyCandidate.DicAttack == false)
                        {
                            entry.Attack = Resources.GenAttackDisplay;
                        }
                        else if (keyCandidate.DicAttack == true && keyCandidate.GenAttack == false)
                        {
                            entry.Attack = Resources.DicAttackDisplay;
                        }
                        else if (keyCandidate.GenAttack == true && keyCandidate.DicAttack == true)
                        {
                            entry.Attack = Resources.GenAttackDisplay + ", " + Resources.DicAttackDisplay;
                        }

                        double f = Math.Log10(Math.Abs(keyCandidate.Fitness));
                        entry.Value = string.Format("{0:0.00000} ", f);
                        ((AssignmentPresentation)Presentation).entries.Add(entry);

                    }
                }, null);
            }
        }  
        

        private void UpdateDisplayStart()
        {
             ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
                 this.startTime = DateTime.Now;
                 ((AssignmentPresentation)Presentation).startTime.Content = "" + startTime;
                 ((AssignmentPresentation)Presentation).endTime.Content = "";
                 ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
              }, null);
        }

        private void UpdateDisplayEnd()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.endTime = DateTime.Now;
                TimeSpan elapsedtime = this.endTime.Subtract(this.startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + this.endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;

            }, null);
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
                return null;
            }

            using (CStreamReader reader = stream.CreateReader())
            {
                res = Encoding.Default.GetString(reader.ReadFully());
               
               
                if (res.Length == 0)
                {
                    return null;
                }
            }

            return res;
        }

        private string detAlphabet(int lang)
        {
            String alpha = "";
            // English
            if (lang == 0)
            {
                alpha = AnalysisMonoalphabeticSubstitution.English;
            }
            // Add another case for a new language
            //else if ( lang == 1)
            //{
            //
            //}

            return alpha;
        }

        private string IdentifyNGramFile(int alpha_nr)
        {
            bool cs = false;
            string name = "";
            string lang = "";
            string casesen = "";

            if (alpha_nr == 0)
            {
                lang = "en";
            }
            // Add another case for a new language
            //else if (alpha_nr == 1)
            //{
            //    lang = "xx";
            //}

            if (cs == false)
            {
                casesen = "nocs";
            } else
            {
                casesen = "cs";
            }

            // It is always looked for a 4-gram file at first. If the 4-gram file is not found the 3-gram file is choosen
            for (int i = 4; i > 2; i--)
            {
                name = lang + "-" + i.ToString() + "gram-" + casesen + ".lm";
                if (File.Exists(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, name)))
                {
                    return name;
                }
            }
            return null;
        }

        private void UpdateOutput(String key_string, String plaintext_string)
        {
            // Plaintext
            this.plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");

            // Alphabet
            this.plaintext_alphabet_output = key_string;
            OnPropertyChanged("Plaintext_Alphabet_Output");
        }

        private String CreateAlphabetOutput(int[] key, Alphabet ciphertext_alphabet)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < key.Length; i++)
            {
                sb.Append(ciphertext_alphabet.GetLetterFromPosition(key[i]));
            }

            return sb.ToString();
        }

        #endregion
    }

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Attack { get; set; }
    }

    public class StopFlag
    {
        public Boolean Stop { get; set; }
    }
}
