/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using VigenereAutokeyAnalyser;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;

namespace Cryptool.Plugins.VigenereAutokeyAnalyser
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("VigenereAutokeyAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "VigenereAutokeyAnalyser/DetailedDescription/doc.xml", "VigenereAutokeyAnalyser/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class VigenereAutokeyAnalyser : ICrypComponent
    {
        #region Private Variables

        private readonly VigenereAutokeyAnalyserSettings settings;
        private AutokeyPresentation quickWatchPresentation;         //The Quickwatch to be used

        private String ciphertext = "";                             //The cipher to be analysed
        private int modus;                                          //The modus to work with (Autokey or Vigenere)
        private String alphabet;                                    //The alphabet to be used
        private String key;                                         //One probable key
        private double IC;                                          //One probable index of coincidence
        private String completeplain;                               //One probable mixed up plaintext

        private String textcorpus;                                  //Alternative to the predetermindet Frequencys in Languages

        private int maxkeylength;                                   //The maximum keylength we search for
        private int keylength;                                      //One probable keylength
        private int assumedkeylength;                               //The keylength delivered by the autokorrelation
        private int language;                                       //Frequencys we work with
        private double cSofS;                                       //One probable Ciphercolumn Sum of Squares
        
        //Frequency Section
        private Dictionary<char,double> OF = new Dictionary<char,double>(); //Observed Frequency for a ciphercolumn or completeplain
        private Dictionary<char,double> EF = new Dictionary<char,double>(); //Expected Frequency due to the expected language

        //Reminder Section
        private String finalkey;                                    //The solution
        private double finalIC;                                     //The IC for the solution
        private double sumofsquares;                                //The Sum of Squares for the best solution

        #endregion

        #region Data Properties

        /// <summary>
        /// The input for the ciphertext 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCipherCaption", "InputCipherTooltip", true)]
        public String InputCipher
        {
            get
            {
                return ciphertext;
            }
            set
            {
                this.ciphertext = value;
                OnPropertyChanged("InputCipher");
            }
        }

        /// <summary>
        /// The input for the textcorpus (optional) 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCorpusCaption", "InputCorpusTooltip", false)]
        public String InputCorpus
        {
            get
            {
                return textcorpus;
            }
            set
            {
                this.textcorpus = value;
                OnPropertyChanged("InputCorpus");
            }
        }

        /// <summary>
        /// The assumed keylength from the autokorrelation plugin (optional) 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputKeylengthCaption", "InputKeylengthTooltip", false)]
        public int InputKeylength
        {
            get
            {
                return assumedkeylength;
            }
            set
            {
                this.assumedkeylength = value;
                OnPropertyChanged("InputKeylength");
            }
        }

        /// <summary>
        /// Choose the alphabet letters to work with
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip", false)]
        public string InputAlphabet
        {
            get { return ((VigenereAutokeyAnalyserSettings)this.settings).AlphabetSymbols; }
            set
            {
                if (value != null && value != settings.AlphabetSymbols)
                {
                    ((VigenereAutokeyAnalyserSettings)this.settings).AlphabetSymbols = value;
                    OnPropertyChanged("InputAlphabet");
                }
            }
        }

        /// <summary>
        /// The output for the key 
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip")]
        public String OutputKey
        {
            get
            {
                return finalkey;
            }
            set
            {
                this.finalkey = value;
                OnPropertyChanged("OutputKey");
            }
        }     

        #endregion

        #region IPlugin Members

        public VigenereAutokeyAnalyser()
        {
            settings = new VigenereAutokeyAnalyserSettings();
            quickWatchPresentation = new AutokeyPresentation();
            quickWatchPresentation.SelectedIndexChanged += new MouseButtonEventHandler(quickWatchPresentation_SelectedIndexChanged);
        }

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return quickWatchPresentation; }            
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
//START------------------------------------------------------------------------------------------------------------
//Preparations for the Analyse-------------------------------------------------------------------------------------                    
         
            if (InputCipher != null)
            {
                ProgressChanged(0, 1);

                quickWatchPresentation.Clear();
           
                alphabet = settings.AlphabetSymbols;                //initialising the alphabet as given by the user       

                ciphertext = InputCipher;                           //initialising the ciphertext
                ciphertext = prepareForAnalysis(ciphertext);        //and prepare it for the analysis (-> see private methods section)
                                   

                modus = settings.Modus;                             //initialise which modus is used
                language = settings.Language;                       //initialise which language frequencys are expected
                finalIC = 0.0;                                      //initialise the highest index of coincidence to be found among all tests

                if (textcorpus != null)                             //1)  if there's a textcorpus given use it to calculate the expected frequency...
                {                                                   //    (-> see private methods section)
                    textcorpus = prepareForAnalysis(textcorpus);
                    EF = observedFrequency(textcorpus);
                }
                else                                                //OR
                {
                    EF = expectedFrequency(language);               //2) just use the expected frequency from the guessed language
                }                                                   //    (-> see private methods section)

//-----------------------------------------------------------------------------------------------------------------
//Analyse----------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------

                if (InputKeylength != 0)                            //1) if the autokorrelation function has provided an assumed
                {                                                   //   keylength break the AutokeyCipher with it... 
                    lock (this)                                     //   IMPORTANT: This is a critical Area and has to be used by only one thread 
                    {
                        breakVigenere(InputKeylength);
                    }
                }
                else                                                //OR
                {
                    maxkeylength = (ciphertext.Length / 40) + 1;    //2) Brute force the keylength up to (ciphertext.length / 40)
                    for (int d = 1; d <= maxkeylength; d++)
                        breakVigenere(d);                           //"BREAK VIGENERE (KEYLENGTH)" IS THE MAIN METHODE IN FINDING THE KEY FOR A GIVEN KEYLENGTH
                }

                quickWatchPresentation.selectIndex((finalkey.Length) - 1);

                OutputKey = finalkey;                               //sending the key via output
                OnPropertyChanged("OutputKey");
            }


            ProgressChanged(1, 1);
        
        }
//EXECUTE END------------------------------------------------------------------------------------------------------------

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

//GET IC---------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calculate the index of coincidence which is required for the sum of squares analysis
        /// </summary>
        private double getIC(String completeplain)
        {
            OF = observedFrequency(completeplain);
            double IC = 0;

            foreach (char c in alphabet)
                IC += OF[c] * OF[c];

            return IC * alphabet.Length;
        }


//---------------------------------------------------------------------------------------------------------------------------------------
//PREPARE PART---------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove spaces and symbols not provided by the alphabet from the text
        /// </summary>
        private String prepareForAnalysis(String s)
        {
            String prepared = "";

            foreach (char c in s.ToUpper())
                if (alphabet.IndexOf(c) != -1)
                    prepared += c;

            return prepared;
        }


//---------------------------------------------------------------------------------------------------------------------------------------
//SUM OF SQUARES-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The shifted column with the least sum of squares is most probably shifted by the correct letter.
        /// This letter is part of the key we want to find.
        /// </summary>
        private double getSumOfSquares(String col, int s)
        {
            String shifted = getShift(col, s);                  //"autokey shift" the whole column by the probable key-letter 
           
            OF = observedFrequency(shifted);                    // calculate the observed frequency of the shift

            double sum = 0;
            double diff;

            foreach (char c in alphabet)                        //Calculate the sum of squares
            {
                diff = EF[c] - OF[c];
                sum += diff * diff;
            }

            return sum;
        }

//---------------------------------------------------------------------------------------------------------------------------------------
//LETTER TO NUMBER----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert the letter to an int-value that corresponds to its position in the given alphabet
        /// </summary>
        private int getPos(char c)
        {
            return alphabet.IndexOf(c);
        }

//---------------------------------------------------------------------------------------------------------------------------------------
//SHIFT PART-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Choose the decryption rule
        /// </summary>
        private String getShift(String c, int s)
        {
            return (modus == 1) ? getCaesarShift(c, s) : getAutoShift(c, s);
        }

        /// <summary>
        /// "Autokey shift" the given column by the probable key-letter 
        /// </summary>
        private String getAutoShift(String col, int s)
        {
            String shifted = "";
            int gotcha = s;

            foreach (char c in col)
            {
                gotcha = (getPos(c) - gotcha + alphabet.Length) % alphabet.Length;
                shifted += alphabet[gotcha];
            }

            return shifted;
        }

        /// <summary>
        /// "Caesar shift" the given column by the probable key-letter (Used for the optional NORMAL-Vigenere Modus) 
        /// </summary>
        private String getCaesarShift(String col, int s)
        {
            String shifted = "";
            int gotcha = 0;

            foreach (char c in col)
            {
                gotcha = (getPos(c) - s + alphabet.Length) % alphabet.Length;
                shifted += alphabet[gotcha];
            }

            return shifted;
        }


//---------------------------------------------------------------------------------------------------------------------------------------	
//FREQUENCY ANALYSE PHASE----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calculate the letter frequency of a provided text
        /// </summary>
        private Dictionary<char,double> observedFrequency(String s)
        {
            Dictionary<char, double> book = new Dictionary<char,double>();  //book assigns the frequency to a letter
            
            // make sure that the alphabet letters appear in the dictionary
            foreach (char c in alphabet)
                book.Add(c, 0);

            //count the symbols and add 1 in their ASCII Position
            foreach (char c in s)
                if(book.ContainsKey(c)) book[c]++; else book.Add(c,0);

            //calculate the frequency by dividing through the textlength
            foreach (char c in book.Keys.ToList())
                book[c] /= s.Length;

            return book;
        }


        /// <summary>
        /// Set the expected letter frequency of a language
        /// </summary>
        private Dictionary<char, double> expectedFrequency(int l)
        {
            Dictionary<char, double> book = new Dictionary<char,double>();                   //"book" remembers the alphabet letters
            double[] languagefrequency;

            //switch to the expected language and set its frequency
            switch (l)
            {
                case 1: //German
                    languagefrequency = new double[] { 6.51, 1.89, 3.06, 5.08, 17.40, 1.66, 3.01, 4.76, 7.55, 0.27, 1.21, 3.44, 2.53, 9.78, 2.51, 0.79, 0.02, 7.00, 7.27, 6.15, 4.35, 0.67, 1.89, 0.03, 0.04, 1.13 };
                    break;
                case 2: //French
                    languagefrequency = new double[] { 7.636, 0.901, 3.260, 3.669, 14.715, 1.066, 0.866, 0.737, 7.529, 0.545, 0.049, 5.456, 2.968, 7.095, 5.378, 3.021, 1.362, 6.553, 7.948, 7.244, 6.311, 1.628, 0.114, 0.387, 0.308, 0.136 };
                    break;
                case 3: //Spanish
                    languagefrequency = new double[] { 12.53, 1.42, 4.68, 5.86, 13.68, 0.69, 1.01, 0.70, 6.25, 0.44, 0.01, 4.97, 3.15, 6.71, 8.68, 2.51, 0.88, 6.87, 7.98, 4.63, 3.93, 0.90, 0.02, 0.22, 0.90, 0.52 };
                    break;
                default: //English
                    languagefrequency = new double[] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 };
                    break;
            }

            //set the frequency for the letters in the alphabet
            for (int c = 0; c < 26; c++)
                book.Add((char)('A' + c), languagefrequency[c] / 100.0);

            // make sure that the alphabet letters appear in the dictionary
            foreach (char c in alphabet)
                if (!book.ContainsKey(c)) book.Add(c, 0);

            return book;
        }

//-----------------------------------------------------------------------------------------------------------------------------------------	
//QUICKWATCH PART--------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Show the results in the Presentation
        /// </summary>
        private void showResult(String key, double IC)
        {
                ResultEntry entry = new ResultEntry();
                entry.Key = key;
                entry.IC = IC.ToString();

                quickWatchPresentation.Add(entry);
        }

//-----------------------------------------------------------------------------------------------------------------------------------------
//PRESENTATION UPDATE----------------------------------------------------------------------------------------------------------------------

        void quickWatchPresentation_SelectedIndexChanged(object sender, MouseButtonEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv != null)
            {
                ResultEntry rse = lv.SelectedItem as ResultEntry;

                if (rse != null)
                {
                    OutputKey = rse.Key;
                    OnPropertyChanged("OutputKey");
                }
            }
        }

//-----------------------------------------------------------------------------------------------------------------------------------------
//CALCULATION PART: BREAK METHOD (MOST IMPORTANT METHOD)-----------------------------------------------------------------------------------

        /// <summary>
        /// Find the key for a given keylength using "Least Sum of Squares" attack
        /// </summary>
        private void breakVigenere(int d)
        {
                completeplain = "";                             //initialising completeplain, 
                key = "";                                       //key, 
                keylength = d;                                  //keylength

                //for all columns in a possible keylength
                for (int column = 0; column < keylength; column++)
                {
                    String ciphercolumn = "";                   //column is reset
                    char probablekeychar = 'A';                 //probablekeychar is reset
                    sumofsquares = Double.MaxValue;             //the sum of squares is reset

                    //A new column is calculated through  c1 , c1 + d , c1 + 2d , c1 + 3d etc.
                    for (int i = column; i < ciphertext.Length; i += keylength)
                        ciphercolumn += ciphertext[i];

                    ciphercolumn = ciphercolumn.ToUpper();

                    //for this column the Sum Of Squares is calculated for each shift key...
                    for (int shift = 0; shift < alphabet.Length; shift++)
                    {
                        cSofS = getSumOfSquares(ciphercolumn, shift);

                        //...and compared so the correct one having the least sum can be found 
                        if (sumofsquares > cSofS)
                        {
                            sumofsquares = cSofS;
                            probablekeychar = alphabet[shift];
                        }
                    }

                    completeplain += getShift(ciphercolumn, getPos(probablekeychar)); //remembers a decrypted cipher
                    key += probablekeychar;                                           //remembers the probable key letter of this decryption
                }

                IC = getIC(completeplain);                              //calculate the IC (index of coincidence)
                showResult(key, IC);                                    //show the results
                                                
                //the decrypted cipher with the highest index of coincidence was decrypted with the correct key
                if (IC > finalIC)
                {
                    finalIC = IC;                                       //remember the IC
                    finalkey = key;                                     //remember the key                                     
                }

                ProgressChanged((((double)d) / maxkeylength), 1);       
        }

//-----------------------------------------------------------------------------------------------------------------------------------------       

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