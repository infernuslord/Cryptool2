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
using Cryptool.PluginBase.Analysis;

namespace Cryptool.Plugins.VigenereAutokeyAnalyser
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "VigenereAutokeyAnalyser", "Ciphertext-only attack on VigenereAutoKey encryption", null, "VigenereAutokeyAnalyser/icon.png")]

    public class VigenereAutokeyAnalyser : IStatistic//IIOMisc
    {
        #region Private Variables

        private readonly VigenereAutokeyAnalyserSettings settings = new VigenereAutokeyAnalyserSettings();
        private String ciphertext = "";                             //The cipher to be analysed
        private String alphabet;                                    //The alphabet to be used
        private String key;                                         //One probable key
        private double IC;                                          //One probable index of coincidence
        private String completeplain;                               //One probable mixed up plaintext

        private String textkorpus;                                  //Alternative to the predetermindet Frequencys in Languages

        private int maxkeylength;                                   //The maximum keylength we search for
        private int keylength;                                      //One probable keylength
        private int language;                                       //Frequencys we work with
        private double cSofS;                                       //One probable Ciphercolumn Sum of Squares
        private int maxfactor;                                      //One probable multiple of the keylength
        
        //Frequency Section
        private double[] OF = new double[255];                      //Observed Frequency for a ciphercolumn or completeplain
        private double[] EF;                                        //Expected Frequency due to the expected language

        //Reminder Section
        private String finalkey;                                    //The solution
        private double finalIC = 0.0;                               //The IC for the solution
        private double sumofsquares;                                //The Sum of Squares for the best solution

        #endregion

        #region Data Properties


        [PropertyInfo(Direction.InputData, "Cipher Input", "Enter your cipher here", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Textkorpus Input", "Enter your sample text here or choose a language from the task-pane", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputKorpus
        {
            get
            {
                return textkorpus;
            }
            set
            {
                this.textkorpus = value;
                OnPropertyChanged("InputKorpus");
            }
        }


        [PropertyInfo(Direction.OutputData, "Key Output", "The most probable autokey for the analysed ciphertext", "", DisplayLevel.Beginner)]
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
        }

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

//Preparations for the Analyse-------------------------------------------------------------------------------------

            alphabet = settings.AlphabetSymbols;

            ciphertext = InputCipher;

            ciphertext = prepareForAnalyse(ciphertext);

            maxkeylength = (ciphertext.Length / 40) + 1;
            
            language = settings.Language;

            if (textkorpus != null)
            {
                textkorpus = prepareForAnalyse(textkorpus);
                EF = observedFrequency(textkorpus);
            }
            else
            {
                EF = expectedFrequency(language);
            }

//-----------------------------------------------------------------------------------------------------------------
//Analyse----------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------

            for (int d = 1; d <= maxkeylength; d++)
            {
                completeplain = "";
                key = "";
                keylength = d;

                maxfactor = ciphertext.Length / keylength;

                for (int column = 0; column < keylength; column++)
                {
                    String ciphercolumn = "";
                    char probablekeychar = 'A';
                    sumofsquares = 99999999999.99999999999;

                    for (int i = 0; i <= maxfactor; i++)
                    {
                        if (column + i * keylength < ciphertext.Length)
                        {
                            ciphercolumn = ciphercolumn + ciphertext[column + i * keylength];
                        }
                    }

                    ciphercolumn = ciphercolumn.ToUpper();

                    for (int shift = 0; shift < alphabet.Length; shift++)
                    {
                        cSofS = getSumOfSquares(ciphercolumn, shift);


                        if (cSofS < sumofsquares)
                        {
                            sumofsquares = cSofS;
                            probablekeychar = alphabet[shift];
                        }


                    }

                    completeplain = getShift(ciphercolumn, getPos(probablekeychar)); //merkt sich die entciphertecolumn
                    key = key + probablekeychar;



                }
                          

                IC = getIC(completeplain);

                if (IC > finalIC)
                {
                    finalIC = IC;
                    finalkey = key;
                }

                ProgressChanged((((double)d) / maxkeylength), 1);
            
            }//This keylength checked...next one



            OutputKey = finalkey;
            OnPropertyChanged("OutputKey");

//-----------------------------------------------------------------------------------------------------------------

            ProgressChanged(1, 1);
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
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

//GET IC---------------------------------------------------------------------------------------------------------------------------------

        private double getIC(String completeplain)
        {
            OF = observedFrequency(completeplain);
            IC = 0;

            for (int x = 0; x < alphabet.Length; x++)
            {
                IC = IC + (OF[alphabet[x]] * OF[alphabet[x]]) / (1.0 / alphabet.Length);
            }


            return IC;
        }


//---------------------------------------------------------------------------------------------------------------------------------------
//PREPARE PART---------------------------------------------------------------------------------------------------------------------------

        private String prepareForAnalyse(String c)
        {
            String prepared = "";

            c = c.ToUpper();

            for (int x = 0; x < c.Length; x++)
            {
                if (getPos(c[x]) != -1)
                {
                    prepared = prepared + c[x];
                }
            }
            return prepared;
        }



//---------------------------------------------------------------------------------------------------------------------------------------
//SUM OF SQUARES-------------------------------------------------------------------------------------------------------------------------

        private double getSumOfSquares(String c, int s)
        {
            String shifted = "";
            //Shiften....
            shifted = getShift(c, s);

            //frequency ermitteln
            OF = observedFrequency(shifted);


            double sum = 0;
            double help;
            //Sum ermitteln
            for (int letter = 0; letter < alphabet.Length; letter++)
            {
                help = (EF[alphabet[letter]] / 100) - OF[alphabet[letter]];

                sum = sum + (help * help);

            }

            return sum;
        }

//---------------------------------------------------------------------------------------------------------------------------------------
//LETTER TO NUMBER----------------------------------------------------------------------------------------------------------------------

        private int getPos(char c)
        {
            int pos = -1;
            for (int i = 0; i < alphabet.Length; i++)
            {
                if (alphabet[i] == c)
                {
                    pos = i;
                }
            }
            return pos;
        }

//---------------------------------------------------------------------------------------------------------------------------------------
//SHIFT PART-----------------------------------------------------------------------------------------------------------------------------

        private String getShift(String c, int s)
        {
            String shifted = "";
            int gotcha = s;

            for (int x = 0; x < c.Length; x++)
            {

                gotcha = (getPos(c[x]) - gotcha + 26) % 26;
                shifted = shifted + alphabet[gotcha];
            }

            return shifted;

        }


//---------------------------------------------------------------------------------------------------------------------------------------	
//FREQUENCY ANALYSE PHASE----------------------------------------------------------------------------------------------------------------
       
        
        private double[] observedFrequency(String c)
        {
            double[] book = new double[255];    //ASCII Buch merkt sich Anzahl

            for (int x = 0; x < c.Length; x++)	//für jeden buchstaben in er ciphercolumn
            {
                book[(int)c[x]]++;
            }

            for (int y = 0; y < book.Length; y++)  //die frequenz ermitteln
            {
                book[y] = book[y] / c.Length;
            }

            return book;
        }


        private double[] expectedFrequency(int l)
        {
            double[] book = new double[255];    //ASCII Buch merkt sich Anzahl
            double[] languagefrequency;

            switch (l)
            {
                case 0: //English
                    languagefrequency = new double[] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 };
                    break;
                case 1: //German
                    languagefrequency = new double[] { 6.51, 1.89, 3.06, 5.08, 17.40, 1.66, 3.01, 4.76, 7.55, 0.27, 1.21, 3.44, 2.53, 9.78, 2.51, 0.79, 0.02, 7.00, 7.27, 6.15, 4.35, 0.67, 1.89, 0.03, 0.04, 1.13 };
                    break;
                case 2: //French
                    languagefrequency = new double[] { 7.636, 0.901, 3.260, 3.669, 14.715, 1.066, 0.866, 0.737, 7.529, 0.545, 0.049, 5.456, 2.968, 7.095, 5.378, 3.021, 1.362, 6.553, 7.948, 7.244, 6.311, 1.628, 0.114, 0.387, 0.308, 0.136 };
                    break;
                case 3: //Spain
                    languagefrequency = new double[] { 12.53, 1.42, 4.68, 5.86, 13.68, 0.69, 1.01, 0.70, 6.25, 0.44, 0.01, 4.97, 3.15, 6.71, 8.68, 2.51, 0.88, 6.87, 7.98, 4.63, 3.93, 0.90, 0.02, 0.22, 0.90, 0.52 };
                    break;
                default: //English
                    languagefrequency = new double[] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 };
                    break;
            }

            for (int c = 0; c < 26; c++)
            {
                book[(int)alphabet[c]] = languagefrequency[c];
            }

            return book;
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
