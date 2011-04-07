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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Cryptography;


namespace Cryptool.Plugins.m209
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "m209", "pluginToolTip", "m209/DetailedDescription/Description.xaml",
      "m209/Images/M-209.jpg", "m209/Images/encrypt.png", "m209/Images/decrypt.png")]
    
    // HOWTO: Change interface to one that fits to your plugin (see CrypPluginBase).
    [EncryptionType(EncryptionType.Classic)]
    public class m209 : IEncryption
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        /// 
        string[,] rotoren =  new String[6,27]{

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
         "U", "V", "W", "X", "Y", "Z", null},

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
         "U", "V", "X", "Y", "Z", null, null},

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
         "U", "V", "X", null, null, null, null},

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
         "U", null, null, null, null, null, null},

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", "R", "S", null,
         null, null, null, null, null, null, null},

        {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", "N", "O", "P", "Q", null, null, null,
         null, null, null, null, null, null, null},
        };

        bool[,] pins = new Boolean[6, 27]{

        {true, true, false, true, false, false, false, true, true, false,
         true, false, true, true, false, false, false, false, true, true,
         false, true, true, false, false, false, false},

        {true, false, false, true, true, false, true, false, false, true,
         true, true, false, false, true, false, false, true, true, false,
         true, false, true, false, false, false, false},

        {true, true, false, false, false, false, true, true, false, true,
         false, true, true, true, false, false, false, true, true, true,
         true, false, true, false, false, false, false},

        {false, false, true, false, true, true, false, true, true, false,
         false, false, true, true, false, true, false, false, true, true,
         true, false, false, false, false, false, false},

        {false, true, false, true, true, true, false, true, true, false,
         false, false, true, true, false, true, false, false, true, false,
         false, false, false, false, false, false, false},

        {true, true, false, true, false, false, false, true, false, false,
         true, false, false, true, true, false, true, false, false, false,
         false, false, false, false, false, false, false},
        };

        string[,] rotorenersatz = new String[6, 27]{

        {"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
         "Z", "A", "B", "C", "D", "E", "F", "G", "H", "I",
         "J", "K", "L", "M", "N", "O", null},

        {"O", "P", "Q", "R", "S", "T", "U", "V", "X", "Y",
         "Z", "A", "B", "C", "D", "E", "F", "G", "H", "I",
         "J", "K", "L", "M", "N", null, null},

        {"N", "O", "P", "Q", "R", "S", "T", "U", "V", "X",
         "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
         "K", "L", "M", null, null, null, null},

        {"M", "N", "O", "P", "Q", "R", "S", "T", "U", "A",
         "B", "C", "D", "E", "F", "G", "H", "I", "J", "K",
         "L", null, null, null, null, null, null},

        {"L", "M", "N", "O", "P", "Q", "R", "S", "A", "B",
         "C", "D", "E", "F", "G", "H", "I", "J", "K", null,
         null, null, null, null, null, null, null},

        {"K", "L", "M", "N", "O", "P", "Q", "A", "B", "C",
         "D", "E", "F", "G", "H", "I", "J", null, null, null,
         null, null, null, null, null, null, null},
        };

        int[,] StangeSchieber = new int[27, 2] {
        {3,6},{0,6},{1,6},{1,5},{4,5},{0,4},{0,4},{0,4},{0,4},
        {2,0},{2,0},{2,0},{2,0},{2,0},{2,0},{2,0},{2,0},{2,0},
        {2,0},{2,5},{2,5},{0,5},{0,5},{0,5},{0,5},{0,5},{0,5}
        };

        
        string Alphabet= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string Schluesselalphabet = "ZYXWVUTSRQPONMLKJIHGFEDCBA";
        [PropertyInfo(Direction.InputData, "Texteingabe", "Input Text", null, DisplayLevel.Beginner)]
        public String Text
        {
            get;
            set;
        }



        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Text output", "The string after processing with the Caesar cipher", "", false, false, QuickWatchFormat.Text, null)]
        public String OutputString
        {
            get;
            set;
        }
        
        // Ersetze Alphabet durch Schlüsselalphabet
        public string Substitutionscipher(string Text ){
           char[] alles = new Char[Text.Length]; 
           for(int i = 0; i < Text.Length; i++){ 
               for(int j=0; j < Alphabet.Length; j++){
                    if(Text[i].Equals(Alphabet[j])){
                      alles[i] = Schluesselalphabet[j];
                   }
               }
           }
        String ausgabe = new String(alles);
        return ausgabe;
        }

        //Verschiebung eines Buchstaben berechnen
        public String calculateOffset(string extKey, char c){

            int cnum;
            // Position des Buchstaben 0...25
            cnum = c - 'A';
            bool[] aktiveArme = new bool[6];
            string temp = "";
            // Durch alle Rotoren
            for (int i = 0; i < 6; i++)
            {
                // Durch alle Buchstaben
                for (int j = 0; j < 27; j++)
                {
                    if (rotoren[i,j]!=null && rotoren[i,j] == extKey[i].ToString())
                    {

                        temp += rotorenersatz[i,j];
                        // Hier wurde "AAAAAA" in "PONMLK" umgewandelt (bestimme Testkonfiguration)
                        aktiveArme[i] = pins[i,getRotorPostion(rotorenersatz[i,j],i)];
                    }
                }
            }
            int verschiebung = 0;

            int[] alleSchieber = new int[6];
            for (int i = 0; i < 6; i++)
            {
                if (aktiveArme[i])
                {
                    alleSchieber[i] = i + 1;
                    //verschiebung+=countStangen(i+1);
                }
            }

            verschiebung = countStangen(alleSchieber);

            GuiLogMessage("verschiebung " + verschiebung, NotificationLevel.Info);
            char back;
            cnum -= verschiebung;
            while (cnum < 0)
            {
                cnum += 26;
            }
            cnum = cnum % 26; 
             
             
            int nrZ = 'Z';
            cnum = nrZ-cnum;
            back = (char)cnum;
           
            return ""+back;
        }
        //Gebe Position zurück die zu P gefunden wurde. Schaut in Rotoren welche Position P hat.
        public int countStangen(int[] number)
        {
            int temp = 0;
            bool raus = false;
            for (int i = 0; i < 27; i++)
            {
               
                    for (int j = 0; j < 2 &&!raus; j++)
                    {
                        //Vergleiche alle Zahlen mit jeden der Zwei Schieber

                        for (int c = 0; c < 6; c++)
                        {
                            if (number[c] == StangeSchieber[i, j] && number[c] != 0)
                            {
                                GuiLogMessage("number[c]" + number[c] + "i=" + i + " j=" + j, NotificationLevel.Info);
                                temp++;
                                raus = true;
                                break;
                            }
                        }
                    } raus = false;
            }
            return temp;
        }
        
        
        public int getRotorPostion(string c,int rotor)
        {
            int temp=0;
           
                for (int j = 0; j < 27; j++)
                {
                    if (c == rotoren[rotor, j])
                        temp = j;
                }
            
            return temp;
        }

        public ICryptoolStream OutputStream
        {
            get;
            private set;
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
            char c;
            c = Text[0];
            //GuiLogMessage("c" + c, NotificationLevel.Info);
            OutputString = calculateOffset("AAAAAA", c).ToString(); 
            //ProgressChanged(1, 10);
            OnPropertyChanged("OutputString");
            CStreamWriter writer = new CStreamWriter();
            writer.Close();
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
