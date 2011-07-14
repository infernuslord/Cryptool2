using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.MonoalphabeticAnalysis
{
    public class MonoalphabeticAnalysisSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        private string proposalKey = "";
        public string ProposalKey
        {
            get { return proposalKey; }
            set {
                  proposalKey = value;
                  //hasChanges = true; 
                }
        }

        private string workKey = "";
        public string WorkKey
        {
            get { return workKey; }
            set
            {
                workKey = value;
                //hasChanges = true; 
            }
        }
        private StringBuilder plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        public void PlugboardRestart()
        {
           
            this.plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            for (int i = 0; i < alphabet.Length; i++)
            {

                
               OnPropertyChanged("PlugBoard" + alphabet[i]);
            }
            OnPropertyChanged("PlugBoard");
            
        }



        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //private bool involutoricPlugBoard = true;
        private bool suggestSubstitutionManually = false;
        
        
        private char[] workKeyCharArray = new char[26];


        private void setPlugBoard(int letterPos, int newIndex)
        {
            if (workKey.Length != 0 && proposalKey.Length!=0)
            {
                if (workKey[letterPos] != proposalKey[letterPos]) //Change has been made. Must be undone
                {
                    hasChanges = true;
                    this.workKeyCharArray = this.workKey.ToCharArray();
                    char currentworkchar = this.workKey[letterPos];
                    this.workKeyCharArray[letterPos] = this.proposalKey[letterPos];
                    int indexofcurrentworkchar = this.proposalKey.IndexOf(currentworkchar);
                    this.workKeyCharArray[indexofcurrentworkchar] = currentworkchar;
                    this.workKey = new string(this.workKeyCharArray);
                    //same change also to plugboard
                    char currentPlugChar = plugBoard[letterPos];
                    char oldPlugChar = plugBoard[indexofcurrentworkchar];
                    plugBoard[indexofcurrentworkchar] = oldPlugChar;
                    plugBoard[letterPos] = oldPlugChar;
                    OnPropertyChanged("PlugBoard");
                    OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                    OnPropertyChanged("PlugBoard" + alphabet[indexofcurrentworkchar]);
                }


               



                hasChanges = true;

                this.workKeyCharArray = this.workKey.ToCharArray();

                char newKeyChar = this.workKey[newIndex];

                char currentKeyChar = this.workKey[letterPos];

                this.workKeyCharArray[letterPos] = newKeyChar;

                this.workKeyCharArray[newIndex] = currentKeyChar;

                this.workKey = new string(workKeyCharArray);

                this.PlugBoard = this.workKey;

                //same chage also to plugboard

                char plugChar = plugBoard[letterPos];
                char newPlugChar = plugBoard[newIndex];
                plugBoard[newIndex] = plugChar;
                plugBoard[letterPos] = newPlugChar;

                OnPropertyChanged("PlugBoard");
                OnPropertyChanged("PlugBoard" + alphabet[newIndex]);
                OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                OnPropertyChanged("PlugBoardDisplay");
            }
        }

        
        private int fastAproach = 0;
        

        
        //[PropertySaveOrder(1)]
        [ContextMenu("Generate digram matrix internally", "When the digram matrix is generated internally, the time for calculating the cost function is significantly reduced. ", 27, ContextMenuControlType.ComboBox, null, new string[] { "Don't generate internally", "Generate internally" })]
        [TaskPane("Digram matrix", "When the digram matrix is generated internally, the time for calculating the cost function is significantly reduced.", "", 27, false, ControlType.ComboBox, new string[] { "Don't generate internally", "Generate internally" })]
        public int FastAproach
        {
            get { return this.fastAproach; }
            set
            {
                if (value != fastAproach)
                {
                    HasChanges = true;
                    fastAproach = value;
                }

                OnPropertyChanged("Fast Aproach");
            }
        }

        


        #region Plugboard settings


        


        [TaskPane("ManualSuggestion", "Once you have a key proposittion you can make fixes according to the decipher results. Use the Letter drop-down boxes to correct the text in substitution decipher output, check 'Manual Suggestion'(Me) and 'play' the chain again. Good Luck! ", "", 28, false, ControlType.CheckBox, "", null)]
        public bool SuggestSubstitutionManually
        {
            get { return suggestSubstitutionManually; }
            set
            {
                if (value != suggestSubstitutionManually)
                {
                    suggestSubstitutionManually = value;
                    hasChanges = true;
                    OnPropertyChanged("ManualSuggestion");
                }
            }
        }


     /*   [TaskPane("Plaintext Alphabet", "Displays the Plaintext alphabet", "", 29, false, ControlType.TextBoxReadOnly)]
        public string PlaintextAlphabet
        {
            get { return alphabet; }
            
            set { }  
        }*/
       

        [TaskPane("Key Proposal", "Displays the current key proposal", "", 30, false, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get {
                return this.workKey; ; 
                }

            set
            {
                hasChanges = true;
                workKey = value;
             
               OnPropertyChanged("Plugboard");
            }
        }
        
   /*     [TaskPane("Involutoric", "The mapping and the inverse mapping are the same. As an example if A mapps to X, also X is mapped to A", "Plugboard", 31, false, ControlType.CheckBox, "", null)]
        public bool Involutoric
        {
            get { return involutoricPlugBoard; }
            set
            {
                if (value != involutoricPlugBoard)
                {
                    involutoricPlugBoard = value;
                    hasChanges = true;
                    OnPropertyChanged("Involutoric");
                }
            }
        }*/

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("A=", "Select the letter for connecting this plug.", "", 40, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(0, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("B=", "Select the letter for connecting this plug.", "", 41, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(1, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("C=", "Select the letter for connecting this plug.", "", 42, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get { return alphabet.IndexOf(this.plugBoard[2]); }
            set { setPlugBoard(2, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("D=", "Select the letter for connecting this plug.", "", 43, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get { return alphabet.IndexOf(this.plugBoard[3]); }
            set { setPlugBoard(3, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("E=", "Select the letter for connecting this plug.", "", 44, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get { return alphabet.IndexOf(this.plugBoard[4]); }
            set { setPlugBoard(4, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("F=", "Select the letter for connecting this plug.", "", 45, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get { return alphabet.IndexOf(this.plugBoard[5]); }
            set { setPlugBoard(5, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("G=", "Select the letter for connecting this plug.", "", 46, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get { return alphabet.IndexOf(this.plugBoard[6]); }
            set { setPlugBoard(6, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("H=", "Select the letter for connecting this plug.", "", 47, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get { return alphabet.IndexOf(this.plugBoard[7]); }
            set { setPlugBoard(7, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("I=", "Select the letter for connecting this plug.", "", 48, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get { return alphabet.IndexOf(this.plugBoard[8]); }
            set { setPlugBoard(8, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("J=", "Select the letter for connecting this plug.", "", 49, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get { return alphabet.IndexOf(this.plugBoard[9]); }
            set { setPlugBoard(9, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("K=", "Select the letter for connecting this plug.", "", 50, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get { return alphabet.IndexOf(this.plugBoard[10]); }
            set { setPlugBoard(10, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("L=", "Select the letter for connecting this plug.", "", 51, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get { return alphabet.IndexOf(this.plugBoard[11]); }
            set { setPlugBoard(11, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("M=", "Select the letter for connecting this plug.", "", 52, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get { return alphabet.IndexOf(this.plugBoard[12]); }
            set { setPlugBoard(12, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("N=", "Select the letter for connecting this plug.", "", 53, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get { return alphabet.IndexOf(this.plugBoard[13]); }
            set { setPlugBoard(13, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("O=", "Select the letter for connecting this plug.", "", 54, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get { return alphabet.IndexOf(this.plugBoard[14]); }
            set { setPlugBoard(14, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("P=", "Select the letter for connecting this plug.", "", 55, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get { return alphabet.IndexOf(this.plugBoard[15]); }
            set { setPlugBoard(15, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("Q=", "Select the letter for connecting this plug.", "", 56, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get { return alphabet.IndexOf(this.plugBoard[16]); }
            set { setPlugBoard(16, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("R=", "Select the letter for connecting this plug.", "", 57, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get { return alphabet.IndexOf(this.plugBoard[17]); }
            set { setPlugBoard(17, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("S=", "Select the letter for connecting this plug.", "", 58, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get { return alphabet.IndexOf(this.plugBoard[18]); }
            set { setPlugBoard(18, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("T=", "Select the letter for connecting this plug.", "", 59, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get { return alphabet.IndexOf(this.plugBoard[19]); }
            set { setPlugBoard(19, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("U=", "Select the letter for connecting this plug.", "", 60, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get { return alphabet.IndexOf(this.plugBoard[20]); }
            set { setPlugBoard(20, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("V=", "Select the letter for connecting this plug.", "", 61, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get { return alphabet.IndexOf(this.plugBoard[21]); }
            set { setPlugBoard(21, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("W=", "Select the letter for connecting this plug.", "", 62, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get { return alphabet.IndexOf(this.plugBoard[22]); }
            set { setPlugBoard(22, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("X=", "Select the letter for connecting this plug.", "", 63, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get { return alphabet.IndexOf(this.plugBoard[23]); }
            set { setPlugBoard(23, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Y=", "Select the letter for connecting this plug.", "", 64, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get { return alphabet.IndexOf(this.plugBoard[24]); }
            set { setPlugBoard(24, value); }
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Z=", "Select the letter for connecting this plug.", "", 65, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardZ
        {
            get { return alphabet.IndexOf(this.plugBoard[25]); }
            set { setPlugBoard(25, value); }
        }


        #endregion


        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
