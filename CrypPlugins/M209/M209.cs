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

namespace Cryptool.Plugins.M209
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.M209.Properties.Resources", false, "PluginCaption", "PluginTooltip", "M209/DetailedDescription/Description.xaml",
      "M209/Images/M-209.jpg", "M209/Images/encrypt.png", "M209/Images/decrypt.png")]
    
    // HOWTO: Change interface to one that fits to your plugin (see CrypPluginBase).
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class M209 : ICrypComponent
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
        ///         

        private string[] rotoren =  new String[6] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",    // no W
            "ABCDEFGHIJKLMNOPQRSTUVX",      // no WYZ
            "ABCDEFGHIJKLMNOPQRSTU",        // no V-Z
            "ABCDEFGHIJKLMNOPQRS",          // no T-Z
            "ABCDEFGHIJKLMNOPQ"             // no R-Z
        };

        private bool[,] pins = new Boolean[6, 27];
        private int[,] StangeSchieber = new int[27, 2];

        int[,] rotorpos = new int[6, 26];
        int[] rotorofs = new int[6] {15,14,13,12,11,10};  // position of 'active' pin wrt upper pin 

        public M209()
        {
            // invert array 'rotoren' for faster access
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < rotoren[r].Length; c++)
                    rotorpos[r, rotoren[r][c] - 'A'] = c;
        }

        //  Pfeil in Programmiersprache Eingabe
        [PropertyInfo(Direction.InputData, "TextCaption", "TextTooltip", null, true, false, QuickWatchFormat.Text, null)]
        public String Text
        {
            get;
            set;
        }

        // Pfeil in Programmiersprache Ausgabe
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public String OutputString
        {
            get;
            set;
        }
        
       /*
            Diese Methode Verschlüsselt einen einzelnen Buchstaben
        
            In Cipher mode, the output is printed in groups of five letters. 
            Use the letter Z in the plain text to replace spaces between words. 
         
            In Decipher mode, the output is printed continuously. If the deciphered plaintext letter is
            Z, a space is printed.
        */

 
        public char calculateOffset(string extKey, char c)
        {
            bool[] aktiveArme = new bool[6];

            // Durch alle Rotoren
            for (int r = 0; r < 6; r++)
            {
                int i = getRotorPostion(extKey[r], r);
                i = (i + rotorofs[r]) % rotoren[r].Length;   // position of 'active' pin
                aktiveArme[r] = pins[r,i];
            }

            int verschiebung = (countStangen(aktiveArme) + ('Z' - c)) % 26;
            return (char)( 'A' + verschiebung );
        }
        
        //Zähle die Verschiebung (wo aktive Arme und Schieber)
        public int countStangen(bool[] active)
        {
            int count = 0;

            for (int i = 0; i < 27; i++)
                for (int c = 0; c < 6; c++)
                {
                    if (active[c])
                        if ( (StangeSchieber[i, 0] == (c+1)) || (StangeSchieber[i, 1] == (c+1)) ) {
                            count++; 
                            break;  // bar is set, leave inner loop
                        }
                }
 
            return count;
        }

        // Schieber von der GUI in Array
        public void setBars()
        {
            clearBars();
            
            for (int i = 0; i < 27; i++)
            {
                string temp = settings.bar[i];

                // Wir müssen den String splitten und in int umwandeln
                if (temp != null && temp.Length > 0)
                {
                    StangeSchieber[i, 0] = Convert.ToInt32(temp[0].ToString());
                    if (temp.Length > 1)    // Falls 2 Zahlen --> splitten, konvertieren und speichern 
                        StangeSchieber[i, 1] = Convert.ToInt32(temp[1].ToString());
                }
            }
        }

        // Benutzereingaben werden in Pin Array reingeschrieben
        public void setPins()
        {
     
            clearPins();
            
            if (settings.Rotor1 != null)
            {
                for (int i = 0; i < settings.Rotor1.Length; i++)
                {
                    pins[0, getRotorPostion(settings.Rotor1[i], 0)] = true;   
                }
            }
            if (settings.Rotor2 != null)
            {
                for (int i = 0; i < settings.Rotor2.Length; i++)
                {
                    pins[1, getRotorPostion(settings.Rotor2[i], 1)] = true;
                }
            }
            if (settings.Rotor3 != null)
            {
                for (int i = 0; i < settings.Rotor3.Length; i++)
                {
                    pins[2, getRotorPostion(settings.Rotor3[i], 2)] = true;
                }
            }
            if (settings.Rotor4 != null)
            {
                for (int i = 0; i < settings.Rotor4.Length; i++)
                {
                    pins[3, getRotorPostion(settings.Rotor4[i], 3)] = true;
                }
            }
            if (settings.Rotor5 != null)
            {
                for (int i = 0; i < settings.Rotor5.Length; i++)
                {
                    pins[4, getRotorPostion(settings.Rotor5[i], 4)] = true;
                }
            }
            if (settings.Rotor6 != null)
            {
                for (int i = 0; i < settings.Rotor6.Length; i++)
                {
                    pins[5, getRotorPostion(settings.Rotor6[i], 5)] = true;
                }
            }
        }

        // Pins werden genullt
        public void clearPins()
        {
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
        
        // Sucht zum Buchstaben passenden Index
        public int getRotorPostion(char c,int rotor)
        {
            return rotorpos[rotor, c - 'A'];
        }

        // Sammelt die verschlüsselten Buchstaben zu einem String und passt den Strin an (cipher /decipher)
        public void cipherText()
        {
            string aktuellerWert = settings.Startwert;
            string tempOutput="";

            Text = Text.Replace(" ", "");

            if (!cipher) //Dechiffrieren
                Text = Text.Replace(Environment.NewLine, "");

            Text = Text.ToUpper();

            Regex objNotNaturalPattern = new Regex("[A-Z]");

            for (int i = 0; i < Text.Length; i++)
            {
                if (objNotNaturalPattern.IsMatch(Text[i].ToString()))
                {
                    tempOutput += calculateOffset(aktuellerWert, Text[i]);
                    aktuellerWert = rotateWheel(aktuellerWert);
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
            char[] neuePos = new char[6];

            //Durch alle Rotoren
            for (int r = 0; r < 6; r++)
            {
                neuePos[r] = rotoren[r][ ( getRotorPostion(pos[r],r) + 1 ) % rotoren[r].Length ];
            }

            return new String(neuePos);
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

            if (Text == null)
            {
                GuiLogMessage("Input is not set. Execution is stopped!", NotificationLevel.Warning);
                return;
            }

            setPins();
            setBars();

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
