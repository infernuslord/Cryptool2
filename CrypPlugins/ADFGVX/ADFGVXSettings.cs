/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using Cryptool.PluginBase.Validation;
using System.IO;
using System.Collections;
using System.ComponentModel;

namespace Cryptool.ADFGVX
{
    public class ADFGVXSettings : ISettings
    {
        #region Public ADFGVX specific interface

        /// <summary>
        /// We us this delegate to send messages from the settings class to the ADFGVX plugin
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void AdfgvxLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Remove = 0, Replace = 1 };

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event AdfgvxLogMessage LogMessage;

        public bool CaseSensitiveAlphabet
        {
            get
            {
                if (caseSensitiveAlphabet == 0)
                    return false;
                else
                    return true;
            }
            set { }
        }

        public string DefaultAlphabet
        {
            get { return this.lowerAlphabet; }
            set { }
                
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region Private variables

        private bool hasChanges;
        private int selectedAction = 0;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        private string substitutionMatrix = "abcdefghijklmnopqrstuvwxyz0123456789";
        private string transPass = string.Empty;
        private string cleanTransPass = string.Empty;
        private string substitutionPass = string.Empty;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Replace;
        private int caseSensitiveAlphabet = 1; //0=upper, 1=lower
        
        #endregion

        #region private methods

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i] == value[j]) || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }

            return value;
        }

        private string removeNonAlphabetChar(string value)
        {
            int length = value.Length;
            for (int i = 0; i < length; i++)
            {
                if (!SubstitutionMatrix.Contains(value[i]))
                {
                    value = value.Remove(i, 1);
                    i--;
                    length--;
                }
            }
            return value;
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [TaskPane("SubstitutionMatrixCaption", "SubstitutionMatrixTooltip", null, 2, false, ControlType.TextBox, "")]
        public string SubstitutionMatrix
        {
            get{return this.substitutionMatrix;}
            set
            {
                if (value != substitutionMatrix) HasChanges = true;
                substitutionMatrix = value;
                LogMessage("Changing alphabet to: \"" + SubstitutionMatrix + "\" (" + SubstitutionMatrix.Length.ToString() + " Symbols)", NotificationLevel.Info);
                OnPropertyChanged("SubstitutionMatrix");
            }
        }

        [TaskPane("StandardMatrixCaption", "StandardMatrixTooltip", null, 3, false, ControlType.Button, "")]
        public void StandardMatrix()
        {
            if (SubstitutionMatrix != lowerAlphabet)
            {
                SubstitutionMatrix = lowerAlphabet;
                TranspositionPass = string.Empty;
                substitutionPass = string.Empty;
                OnPropertyChanged("SubstitutionPass");
            }
        }

        [TaskPane("RandomMatrixCaption", "RandomMatrixTooltip", null, 4, false, ControlType.Button, "")]
        public void RandomMatrix()
        {
            Random rand = new Random();
            StringBuilder sb = new StringBuilder(string.Empty);
            string defaultAlph;

            if (!CaseSensitiveAlphabet)
                defaultAlph = upperAlphabet;
            else
                defaultAlph = lowerAlphabet;   

            while (defaultAlph.Length != 0)
            {
                int pos = rand.Next(defaultAlph.Length);
                sb.Append(defaultAlph[pos].ToString());
                defaultAlph = defaultAlph.Remove(pos, 1);
            }
            SubstitutionMatrix = sb.ToString();
            TranspositionPass = string.Empty;
            substitutionPass = string.Empty;
        }

        [TaskPane("SubstitutionPassCaption", "SubstitutionPassTooltip", null, 5, false, ControlType.TextBox, "")]
        public string SubstitutionPass
        {
            get { return this.substitutionPass; }
            set
            {
                if (value != substitutionPass) HasChanges = true;
                substitutionPass = removeNonAlphabetChar(removeEqualChars(value));
                SubstitutionMatrix = removeEqualChars(substitutionPass + SubstitutionMatrix);
                OnPropertyChanged("SubstitutionPass");
            }
        }

        [TaskPane("TranspositionPassCaption", "TranspositionPassTooltip", null, 6, false, ControlType.TextBox, "")]
        public string TranspositionPass
        {
            get { return this.transPass; }
            set
            {
                if (value != transPass) HasChanges = true;
                transPass = value;
                CleanTranspositionPass = removeNonAlphabetChar(removeEqualChars(value));
                OnPropertyChanged("TranspositionPass");
            }
        }

        [TaskPane("CleanTranspositionPassCaption", "CleanTranspositionPassTooltip", null, 7, false, ControlType.TextBoxReadOnly, "")]
        public string CleanTranspositionPass
        {
            get { return this.cleanTransPass; }
            set
            {
                this.cleanTransPass = value;
                OnPropertyChanged("CleanTranspositionPass");
            }
        }

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 8, ContextMenuControlType.ComboBox, null, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 8, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2" })]
        public int UnknownSymbolHandling
        {
            get { return (int)this.unknownSymbolHandling; }
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknownSymbolHandling) HasChanges = true;
                this.unknownSymbolHandling = (UnknownSymbolHandlingMode)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [ContextMenu("AlphabetCaseCaption", "AlphabetCaseTooltip", 9, ContextMenuControlType.ComboBox, null, new string[] { "AlphabetCaseList1", "AlphabetCaseList2" })]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", null, 9, false, ControlType.ComboBox, new string[] { "AlphabetCaseList1", "AlphabetCaseList2" })]
        public int AlphabetCase
        {
            get { return this.caseSensitiveAlphabet; }
            set
            {
                if (value != caseSensitiveAlphabet) HasChanges = true;
                if (value == 0)
                {
                    string subPass = SubstitutionPass.ToUpper();
                    SubstitutionMatrix = upperAlphabet;
                    SubstitutionPass = subPass;
                }
                else
                {
                    string subPass = SubstitutionPass.ToLower();
                    SubstitutionMatrix = lowerAlphabet;
                    SubstitutionPass = subPass;
                }
                this.caseSensitiveAlphabet = value;
                OnPropertyChanged("AlphabetCase");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
