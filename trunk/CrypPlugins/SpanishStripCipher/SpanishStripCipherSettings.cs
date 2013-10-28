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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;

namespace Cryptool.Plugins.SpanishStripCipher
{
    public class SpanishStripCipherSettings : ISettings
    {
        #region Public Variables

        public enum CipherMode { Encrypt = 0, Decrypt = 1 };
        /// <summary>
        /// We use this delegate to send log messages from the settings class to the SpanishStripCipher plugin
        /// </summary>
        public delegate void SpanishStripCipherLogMessage(string msg, NotificationLevel loglevel);
        public event SpanishStripCipherLogMessage LogMessage;

        #endregion 

        #region Private Variables

        private CipherMode selectedAction = CipherMode.Encrypt;
        private int selectedHomophonesTables = 0;
        private int selectedAlphabet = 0;
        private int selectedLetter1 = 0;
        private int selectedLetter2 = 0;
        private int selectedNumberLetter = 0;
        private string selectedLetter1String ="A";
        private string selectedLetter2String ="A";
        private static string[] alpha = new string[] {"A", "B"};
        private string orderedAlphabet = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
        private string orderedAlphabet27Letters = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
        private string orderedAlphabet29Letters = "ABCßDEFGHIJKLÄMNÑOPQRSTUVWXYZ"; // ß<-CH and Ä<-LL encoded 
        private string orderedAlphabet27LettersPanel = "A B C D E F G H I J K L M N Ñ O P Q R S T U V W X Y Z";
        private string orderedAlphabet29LettersPanel = "A B C CH D E F G H I J K L LL M N Ñ O P Q R S T U V W X Y Z"; // ß<-CH and Ä<-LL encoded  
        private string orderedAlphabetPanel = "A B C D E F G H I J K L M N Ñ O P Q R S T U V W X Y Z";
        public string unorderedAlphabet = " ";
        private string unorderedAlphabetPanel = " ";
        private string keyword = "";
        private List<List<string>> homophones = new List<List<string>>();
        //private List<int> numbers = new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 22, 33, 44, 55, 66, 77, 88, 99, 10, 12, 23, 34, 93, 94, 45, 56, 67, 78, 89, 20, 21, 13, 24, 25, 26, 27, 28, 29, 30, 31, 32, 14, 35, 36, 37, 38, 39, 40, 70, 71, 72, 41, 42, 43, 15, 46, 47, 48, 49, 50, 51, 52, 53, 54, 16, 57, 58, 59, 60, 61, 62, 63, 64, 65, 17, 68, 69, 95, 96, 97, 98, 73, 74, 75, 76, 18, 79, 80, 81, 82, 83, 84, 85, 86, 87, 19, 90, 91, 92 };
        private List<string> numbersRandomTable = new List<string>() { "01", "02", "03", "04", "05", "06", "07", "08", "09", "11", "22", "33", "44", "55", "66", "77", "88", "99", "10", "12", "23", "34", "93", "94", "45", "56", "67", "78", "89", "20", "21", "13", "24", "25", "26", "27", "28", "29", "30", "31", "32", "14", "35", "36", "37", "38", "39", "40", "70", "71", "72", "41", "42", "43", "15", "46", "47", "48", "49", "50", "51", "52", "53", "54", "16", "57", "58", "59", "60", "61", "62", "63", "64", "65", "17", "68", "69", "95", "96", "97", "98", "73", "74", "75", "76", "18", "79", "80", "81", "82", "83", "84", "85", "86", "87", "19", "90", "91", "92" };
        private List<string> numbersTableOne = new List<string>() { "10", "37", "61", "81", "12", "56", "99", "20", "44", "55", "82", "32", "54", "77", "36", "45", "60", "95", "30", "59", "68", "86", "11", "38", "78", "21", "53", "62", "18", "46", "75", "88", "31", "74", "80", "17", "39", "57", "96", "23", "63", "83", "13", "47", "76", "97", "33", "64", "94", "19", "40", "87", "22", "65", "58", "28", "48", "73", "15", "51", "93", "26", "49", "85", "16", "41", "89", "24", "66", "72", "29", "50", "90", "34", "42", "84", "25", "67", "71", "92", "35", "70", "98", "27", "52", "79", "14", "43", "69", "91" };
        private List<string> numbersTableTwo = new List<string>() { "10", "42", "57", "97", "23", "51", "75", "99", "07", "41", "65", "87", "14", "45", "68", "93", "01", "40", "69", "78", "28", "55", "74", "06", "30", "63", "92", "02", "32", "66", "17", "34", "62", "11", "29", "60", "98", "18", "26", "54", "84", "09", "35", "53", "90", "08", "38", "64", "91", "13", "44", "72", "19", "43", "86", "22", "48", "82", "15", "36", "71", "96", "05", "33", "61", "81", "27", "59", "73", "94", "16", "39", "88", "24", "52", "70", "89", "21", "49", "67", "85", "04", "31", "50", "79", "20", "37", "56", "80", "12", "46", "77", "95", "00", "25", "58", "76", "03", "47", "83" };
        private List<int> amountOfnumbersPerColRandomTableAlphabet27 = new List<int>() { 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        private List<int> amountOfnumbersPerColTableOneAlphabet27 = new List<int>() { 4, 3, 4, 3, 4, 4, 3, 3, 4, 3, 4, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 3, 3, 4 };
        private List<int> amountOfnumbersPerColTableTwoAlphabet27 = new List<int>() { 4, 4, 4, 4, 4, 3, 4, 3, 3, 4, 4, 4, 4, 3, 3, 3, 4, 4, 4, 3, 4, 4, 4, 4, 4, 4, 3 };
        private List<int> amountOfnumbersPerColRandomTableAlphabet29 = new List<int>() { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        private List<int> amountOfnumbersPerColTableOneAlphabet29 = new List<int>() { 3, 4, 3, 3, 4, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
        private List<int> amountOfnumbersPerColTableTwoAlphabet29 = new List<int>() { 3, 4, 3, 4, 4, 3, 4, 3, 4, 3, 3, 3, 3, 4, 4, 3, 4, 3, 3, 3, 4, 3, 4, 3, 3, 3, 3, 4, 4 };
        private string homophoneString = "";
        private Boolean flag = true;

        #endregion

        #region functions

        public List<List<string>> getHomophones()
        { return homophones; }
        public void GeneratetHomopheneTable()
        {
            homophones.Clear();
            int j;
            int numberPossition = 0;
            numbersRandomTable = ShuffleArrayList(numbersRandomTable);
            amountOfnumbersPerColRandomTableAlphabet27 = ShuffleArrayListInt(amountOfnumbersPerColRandomTableAlphabet27);
            amountOfnumbersPerColRandomTableAlphabet29 = ShuffleArrayListInt(amountOfnumbersPerColRandomTableAlphabet29);
            List<string> numbers = numbersRandomTable; ;
            List<int> amountOfnumbersPerCol;
            if (selectedAlphabet==0)
            {
                amountOfnumbersPerCol = amountOfnumbersPerColRandomTableAlphabet27;
            }
            else
            {
                amountOfnumbersPerCol = amountOfnumbersPerColRandomTableAlphabet29;
            }
            switch (selectedHomophonesTables)
            {
                case 1:
                    numbers = numbersTableOne;
                    if (selectedAlphabet == 0)
                    {
                        amountOfnumbersPerCol = amountOfnumbersPerColTableOneAlphabet27;
                    }
                    else
                    {
                        amountOfnumbersPerCol = amountOfnumbersPerColTableOneAlphabet29;
                    }
                break;
                case 2:
                    numbers = numbersTableTwo;
                    if (selectedAlphabet == 0)
                    {
                        amountOfnumbersPerCol = amountOfnumbersPerColTableTwoAlphabet27;
                    }
                    else
                    {
                        amountOfnumbersPerCol = amountOfnumbersPerColTableTwoAlphabet29;
                    }
                break;
            }
            for (int i = 0; i < orderedAlphabet.Length; i++)
            {
                j = amountOfnumbersPerCol[i];
                homophones.Add(new List<string>());
                for (int k = 0; k < j; k++)
                {
                    homophones[i].Add(numbers[numberPossition]);
                    numberPossition++;
                }
            }
        }
        public void GenerateRandomAlphabet()
        {
            int alphabetLenth = orderedAlphabet.Length;
            //string charKey;
            //remove repeated letters from keyword
            for (int i = 0; i < keyword.Length; i++)
            {
                //charKey = keyword[i].ToString();
                if (unorderedAlphabet.Contains(keyword[i].ToString()) == false)
                {
                    unorderedAlphabet = unorderedAlphabet + keyword[i].ToString();
                }
            }
            int keywordWithoutRepeatedLettersLength = unorderedAlphabet.Length;
            //remove repeated letters from ordered alphabet
            unorderedAlphabet = unorderedAlphabet.ToUpper();
            for (int i = 0; i < orderedAlphabet.Length; i++)
            {
                //string charkey = orderedAlphabet[i].ToString();
                if (unorderedAlphabet.Contains(orderedAlphabet[i].ToString()) == false)
                {
                    unorderedAlphabet = unorderedAlphabet + orderedAlphabet[i].ToString();
                }
            }
            string unorderedAlphabetFinal = "";
            int index = 0;
            int col = 0;
            for (int i = 0; i < unorderedAlphabet.Length; i++)
            {
                if (i != 0)
                {
                    index = index + keywordWithoutRepeatedLettersLength;
                    if (index > alphabetLenth-1)
                    {
                        col = col + 1;
                        index = col;
                    }
                }
                unorderedAlphabetFinal = unorderedAlphabetFinal + unorderedAlphabet[index];
            }
            string unorderedAlphabetFinalShifted = "";
            int shiftingIndex = 0;
            int orderedIndex = orderedAlphabet.IndexOf(orderedAlphabet[selectedLetter2]);
            int unorderedIndex = 0;
            unorderedIndex = unorderedAlphabetFinal.IndexOf(orderedAlphabet[selectedLetter1]);
            int index2 = 0;
            if (unorderedIndex > orderedIndex)
            {
                shiftingIndex = unorderedIndex - orderedIndex;
                for (int i = 0; i < unorderedAlphabetFinal.Length; i++)
                {
                    index2 = (i + shiftingIndex) % alphabetLenth;
                    unorderedAlphabetFinalShifted = unorderedAlphabetFinalShifted + unorderedAlphabetFinal[index2];
                }
            }
            else
            {
                shiftingIndex = orderedIndex - unorderedIndex;
                for (int i = 0; i < unorderedAlphabetFinal.Length; i++)
                {
                    index2 = (alphabetLenth - shiftingIndex + i) % alphabetLenth;
                    unorderedAlphabetFinalShifted = unorderedAlphabetFinalShifted + unorderedAlphabetFinal[index2];
                }
            }
            unorderedAlphabet = unorderedAlphabetFinalShifted;
            if (flag)
            {
                GeneratetHomopheneTable();
                flag = false;
            }
            homophoneString = "";
            for (int i = 0; i < homophones.Count; i++)
            {
                if(unorderedAlphabet[i] == 'Ä')
                {
                    homophoneString = homophoneString + "(LL)" + "=";
                }
                else if (unorderedAlphabet[i] == 'ß')
                {
                    homophoneString = homophoneString + "(CH)" + "=";
                }
                else
                {
                    homophoneString = homophoneString + "(" + unorderedAlphabet[i] + ")" + "=";
                }
                for (int j = 0; j < homophones[i].Count; j++)
                {
                   
                    homophoneString = homophoneString + homophones[i][j].ToString();
                    if (j < homophones[i].Count - 1)
                    {
                        homophoneString = homophoneString + " ";
                    }
                }
                if(i<homophones.Count-1)
                {
                    homophoneString = homophoneString + ", ";
                }
            }
            //Set Unordered Alphabet Panel
            for (int i = 0; i < unorderedAlphabet.Length; i++)
            {
                if (unorderedAlphabet[i] == 'ß')
                {
                    unorderedAlphabetPanel = unorderedAlphabetPanel + "CH" + " ";
                }
                else if (unorderedAlphabet[i] == 'Ä')
                {
                    unorderedAlphabetPanel = unorderedAlphabetPanel + "LL" + " ";
                }
                else
                {
                    unorderedAlphabetPanel = unorderedAlphabetPanel + unorderedAlphabet[i] + " ";
                }
            }
            OnPropertyChanged("Homophones");
            OnPropertyChanged("UnorderedAlphabetPanel");
            OnPropertyChanged("Keyword");
        }
        public List<string> ShuffleArrayList(List<string> source)
        {
            List<string> sortedList = new List<string>();
            Random generator = new Random();
            while (source.Count > 0)
            {
                int position = generator.Next(source.Count);
                sortedList.Add(source[position]);
                source.RemoveAt(position);
            }
            return sortedList;
        }
        public List<int> ShuffleArrayListInt(List<int> source)
        {
            List<int> sortedList = new List<int>();
            Random generator = new Random();
            while (source.Count > 0)
            {
                int position = generator.Next(source.Count);
                sortedList.Add(source[position]);
                source.RemoveAt(position);
            }
            return sortedList;

        }
        public bool checkInitialPositionLetter(string letter)
        {
            bool result=false;
            if (letter == "CH" && orderedAlphabet.Length == 29)
            {
                letter = "ß";
                result = true;
            }
            else if (letter == "LL" && orderedAlphabet.Length == 29)
            {
                letter = "Ä";
                result = true;
            }
            else if (this.orderedAlphabet.IndexOf(letter) != -1) 
            {
                result = true;
            }
            if (true)
            {
                selectedNumberLetter = orderedAlphabet.IndexOf(letter); 
            }
            return result;
        }
        #endregion

        #region TaskPane Settings

        [TaskPane("Action", "ActionTPTooltip", null, 1, true, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public CipherMode Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != this.selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }
        [TaskPane("Alphabets", "AlphabetsTooltip", null, 2, false, ControlType.ComboBox, new string[] { "Letters27", "Letters29" })]
        public int Alphabets
        {
            get { return this.selectedAlphabet; }
            set
            {
                if (value != this.selectedAlphabet)
                {
                    selectedAlphabet = value;
                    Position1 = "A";
                    Position2 = "A";
                    if (selectedAlphabet == 0)
                    { 
                        orderedAlphabetPanel = orderedAlphabet27LettersPanel;
                        orderedAlphabet = orderedAlphabet27Letters;
                    }
                    else 
                    { 
                        orderedAlphabetPanel = orderedAlphabet29LettersPanel;
                        orderedAlphabet = orderedAlphabet29Letters;
                    }
                    OnPropertyChanged("OrderedAlphabet");
                   // OnPropertyChanged("UnorderedAlphabetPanel");
                    OnPropertyChanged("Aphabets");
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        flag = true;
                        unorderedAlphabet = "";
                        unorderedAlphabetPanel = "";
                        GenerateRandomAlphabet();
                    }
                }
            }
        }
        [TaskPane("HomophonesTables", "HomophonesTablesTooltip", null, 2, false, ControlType.ComboBox, new string[] { "Random", "TableOne", "TableTwo" })]
        public int HomophonesTables
        {
            get { return this.selectedHomophonesTables; }
            set
            {
                if (value != this.selectedHomophonesTables)
                {
                    selectedHomophonesTables = value;
                    OnPropertyChanged("HomophonesTables");
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        flag = true;
                        unorderedAlphabet = "";
                        unorderedAlphabetPanel = "";
                        GenerateRandomAlphabet();
                    }
                }
            }
        }
        [TaskPane("Keyword", "KeywordTooltip", null, 3, false, ControlType.TextBox, "")]
        public string Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    unorderedAlphabet = "";
                    unorderedAlphabetPanel = "";
                    GenerateRandomAlphabet();
                }
            }
        }
        [TaskPane("Position1", "Position1Tooltip", null, 4, false, ControlType.TextBox)]
        public string Position1
        {
            get { return selectedLetter1String; }
            set
            {
                if(value !=this.selectedLetter1String)
                {
                    if (checkInitialPositionLetter(value.ToUpper()))
                    {
                        selectedLetter1String = value.ToUpper();
                        this.selectedLetter1 = selectedNumberLetter;
                    }
                    else
                    {
                        OnLogMessage(value+ " is not a valid entry. Please enter only letters of the selected alphabet", NotificationLevel.Error);
                        selectedLetter1 = 0;
                        selectedLetter1String = "A";
                        OnLogMessage("Character "+value+ " was replaced by letter A", NotificationLevel.Warning);
                    }
                    OnPropertyChanged("Position1");
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        unorderedAlphabet = "";
                        unorderedAlphabetPanel = "";
                        GenerateRandomAlphabet();
                    }
                }
            }
        }

        [TaskPane("Position2", "Position2Tooltip", null, 5, false, ControlType.TextBox)]
        public string Position2
        {
            get { return selectedLetter2String; }
            set
            {
                if (value != this.selectedLetter2String)
                {
                    selectedLetter2String = "";
                    if (checkInitialPositionLetter(value.ToUpper()))
                    {
                        selectedLetter2String = value.ToUpper();
                        this.selectedLetter2 = selectedNumberLetter;
                    }
                    else
                    {
                        OnLogMessage(value + " is not a valid entry. Please enter only letters of the selected alphabet", NotificationLevel.Error);
                        selectedLetter2 = 0;
                        selectedLetter2String = "A";
                        OnLogMessage("The entry " + value + " was replaced by the letter A", NotificationLevel.Warning);
                    }
                    OnPropertyChanged("Position2");
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        unorderedAlphabet = "";
                        unorderedAlphabetPanel = "";
                        GenerateRandomAlphabet();
                    }
                }
            }
        }
        [TaskPane("OrderedAlphabet", "OrderedAlphabetTooltip", null, 6, false, ControlType.TextBox, "")]
        public string OrderedAlphabet
        {
            get { return orderedAlphabetPanel; }
            set
            {
                if (orderedAlphabetPanel != value)
                {
                    orderedAlphabetPanel = value;
                    OnPropertyChanged("OrderedAlphabet");
                }
            }
        }
        [TaskPane("UnorderedAlphabetPanel", "UnorderedAlphabetPanelTooltip", null, 7, false, ControlType.TextBox)]
        public string UnorderedAlphabetPanel
        {
            get { return unorderedAlphabetPanel; }
            set
            {
                if (unorderedAlphabetPanel != value)
                {
                    //for (int i = 0; i < keyword.Length; i++)
                    //{
                      //  string charKey = keyword[i].ToString();
                        //if (unorderedAlphabet.Contains(charKey) == false)
                        //{
                            //unorderedAlphabet = unorderedAlphabet + charKey;
                        //}

                    //}                   
                    OnPropertyChanged("UnorderedAlphabetPanel");
               }
            }
        }
        [TaskPane("Homophones", "HomophonesTooltip", null, 8, false, ControlType.TextBox)]
        public string Homophones
        {
            get { return homophoneString; }
            set
            {
                if (homophoneString != value)
                {
                    OnPropertyChanged("Homophones");
                }
            }
        }

        #endregion

        #region

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
        #endregion
    }
}
