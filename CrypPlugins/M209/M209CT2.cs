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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Cryptography;


namespace Cryptool.Plugins.M209
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "M209", "Rotor-Cipher Machine", "M209/DetailedDescription/Description.xaml",
      "M209/Images/M-209.jpg", "M209/Images/encrypt.png", "M209/Images/decrypt.png")]
    
    // HOWTO: Change interface to one that fits to your plugin (see CrypPluginBase).
    [EncryptionType(EncryptionType.Classic)]
    public class M209 : IEncryption
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private M209Settings settings = new M209Settings();
        private bool cipher = true;
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

        bool[,] pins = new Boolean[6, 27];

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

        int[,] StangeSchieber = new int[27, 2];

        
        string Alphabet= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string Schluesselalphabet = "ZYXWVUTSRQPONMLKJIHGFEDCBA";
       
        //  Pfeil in Programmiersprache Eingabe
        [PropertyInfo(Direction.InputData, "Texteingabe", "Input Text", null)]
        public String Text
        {
            get;
            set;
        }

        // Pfeil in Programmiersprache Ausgabe
        [PropertyInfo(Direction.OutputData, "Text output", "The string after processing with the M209 cipher", "", false, false, QuickWatchFormat.Text, null)]
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

       /*Diese Methode Verschlüsselt einen einzelnen Buchstaben
         In Cipher mode, the output is printed in groups of five letters. 
         Use the letter Z in the plain text to replace
         spaces between words. 
         
         In Decipher mode, the output is printed continuously. If the deciphered plaintext letter is
         Z, a space is printed.
         */

 
        public String calculateOffset(string extKey, char c){

            int cnum;
            

                cnum = c - 'A';
                bool[] aktiveArme = new bool[6];
                string temp = "";
                // Durch alle Rotoren
                for (int i = 0; i < 6; i++)
                {
                    // Durch alle Buchstaben
                    for (int j = 0; j < 27; j++)
                    {
                        // Hier wird z.B. "AAAAAA" in "PONMLK" umgewandelt (bestimme Testkonfiguration)
                        if (rotoren[i, j] != null && rotoren[i, j] == extKey[i].ToString())
                        {
                            temp += rotorenersatz[i, j];
                            // 1. A -> P . 
                            // 2. Wo ist das P in den Rotoren und gebe Position zurück
                            // 3. Schau in Pins nach ob false oder true und schreibe in aktiveArme
                            aktiveArme[i] = pins[i, getRotorPostion(rotorenersatz[i, j], i)];
                        }
                    }
                }
                int verschiebung = 0;

                char back;

                // Boolean in Zahlen für aktive arme
                int[] alleSchieber = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    if (aktiveArme[i])
                    {
                        alleSchieber[i] = i + 1;
                    }
                }

                verschiebung = countStangen(alleSchieber);

                // Die Verschiebung wird in einen Buchstaben umgewandelt
                cnum -= verschiebung;
                while (cnum < 0)
                {
                    cnum += 26;
                }
                cnum = cnum % 26;
                int nrZ = 'Z';
                cnum = nrZ - cnum;
                back = (char)cnum;

            

            return ""+back;
        }
        
        //Zähle die Verschiebung (wo aktive Arme und Schieber)
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
                                temp++;
                                raus = true;
                                break;
                            }
                        }
                    } raus = false;
            }
            return temp;
        }

        // Schieber von der GUI in Array
        public void setBar()
        {
            clearBars();
            
            for (int i = 0; i < 27; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    
                    string temp = null;
                    // Abfrage ob Schieber gesetzt sind
                    if(settings.bar[i]!=null)
                    temp = settings.bar[i].ToString();
                    // Wir müssen den String splitten und in int umwandeln
                    if (temp != null)
                    { 
                        // Falls 2 Zahlen --> splitten, konvertieren und speichern 
                        if (temp.Length > 1)
                        {
                            StangeSchieber[i, j] = Convert.ToInt32(temp[0].ToString());
                            StangeSchieber[i, j + 1] = Convert.ToInt32(temp[1].ToString());
                        }
                    }
                }
            }
        }

        // Benutzereingaben werden in Pin Array reingeschrieben
        public void setPins()
        {
     
            clearPins();
            
            if (settings.Rotor1!=null)
            {
                for (int i = 0; i < settings.Rotor1.Length; i++)
                {
                    pins[0, getRotorPostion(settings.Rotor1[i].ToString(), 0)]= true;   
                    
                }
            }
            if (settings.Rotor2 != null)
            {
                for (int i = 0; i < settings.Rotor2.Length; i++)
                {
                    pins[1, getRotorPostion(settings.Rotor2[i].ToString(), 1)] = true;

                }
            }
            if (settings.Rotor3 != null)
            {
                for (int i = 0; i < settings.Rotor3.Length; i++)
                {
                    pins[2, getRotorPostion(settings.Rotor3[i].ToString(), 2)] = true;

                }
            }
            if (settings.Rotor4 != null)
            {
                for (int i = 0; i < settings.Rotor4.Length; i++)
                {
                    pins[3, getRotorPostion(settings.Rotor4[i].ToString(), 3)] = true;

                }
            }
            if (settings.Rotor5 != null)
            {
                for (int i = 0; i < settings.Rotor5.Length; i++)
                {
                    pins[4, getRotorPostion(settings.Rotor5[i].ToString(), 4)] = true;

                }
            }
            if (settings.Rotor6 != null)
            {
                for (int i = 0; i < settings.Rotor6.Length; i++)
                {
                    pins[5, getRotorPostion(settings.Rotor6[i].ToString(), 5)] = true;
                }
            }
        }

        // Pins werden genullt
        public void clearPins() {
          for (int i = 0; i < 6; i++)
            {
                // Durch alle Buchstaben
                for (int j = 0; j < 27; j++)
                {
                    pins[i,j]=false;
                }
            }
        }
        // Schieber werden genullt
        public void clearBars()
        {
            for (int i = 0; i < 27; i++)
            {

                for (int j = 0; j < 2; j++)
                {
                    StangeSchieber[i, j] = 0;
                }
            }
        }
        
        // Sucht zum Buchstaben passenden index
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

        // Sammelt die verschlüsselten Buchstaben zu einem String und passt den Strin an (cipher /decipher)
        public void cipherText()
        {
            string aktuellerWert = settings.Startwert;
            string tempOutput="";
            Text = Text.Replace(" ", "");
            //Dechiffrieren
            if(!cipher){
                Text = Text.Replace(Environment.NewLine, "");
            }

            
            Text = Text.ToUpper();

            Regex objNotNaturalPattern = new Regex("[A-Z]");
           
                for (int i = 0; i < Text.Length; i++)
                {
                    if (objNotNaturalPattern.IsMatch(Text[i].ToString()))
                    {
                        // Beim ersten mal muss das Rad nicht gedreht werden
                        if (i == 0)
                        {
                            tempOutput += calculateOffset(aktuellerWert, Text[i]).ToString();
                        }
                        else
                        {
                            aktuellerWert = rotateWheel(aktuellerWert);
                            tempOutput += calculateOffset(aktuellerWert, Text[i]).ToString();
                        }
                    }
                }

                OutputString = tempOutput;

                if (cipher)
                {   // In fünfergruppen ausgeben
                    for (int i = 5; i < OutputString.Length; i = i + 6)
                    {
                        OutputString = OutputString.Insert(i, " ");
                    }
                }
                else // decipher
                {
                    OutputString = OutputString.Replace("Z", " ");
                }

            
            OnPropertyChanged("OutputString");
        }
        
        // Die Methode wird für jeden Buchstaben aufgerufen um die Rotoren zu drehen

        // Rein AAAAAA Raus BBBBBB                                    
        public string rotateWheel(string pos)
        {
            string tempS = pos;
            char[] neuePos = new char[6];
            //Durch alle Rotoren
            for (int i = 0; i < 6; i++)
            {
                // Durch alle Buchstaben
                for (int j = 0; j < 27; j++)
                {

                    // Falls kein nachfolgender Buchstabe im jeweiligen Rotor
                    // vorhanden ist nehme A
                    if (rotoren[i, j + 1] == null)
                    {
                        neuePos[i] = 'A';
                        break;
                    }
                    else
                    {   // Falls ein die Aktuelle Position gefunden wurde dann wechsle zu nächsten
                        if (tempS[i].ToString() == rotoren[i, j])
                        {
                            string t = rotoren[i, j + 1];
                            neuePos[i]= t[0];
                            break;
                        }
                        
                    }
                }
            }
            

            String back = new String(neuePos);
            return back;
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
            
            setPins();
            setBar();

            switch (settings.Action)
            {
                case 0:
                    cipher=true;
                    break;
                case 1:
                    cipher=false;
                    break;
                default:
                    break;
            }

            cipherText();
          
 
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
