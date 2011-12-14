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

namespace Cryptool.Plugins.Purple
{
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    public class PurpleSettings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;
        private string alphabet = "AEIOUYBCDFGHJKLMNPQRSTVWXZ";
        private int motion = 123;
        private int selectedAction = 0;
        public int sechserPos = 0;
        public int zwanzigerPos1 = 0;
        public int zwanzigerPos2 = 0;
        public int zwanzigerPos3 = 0;
        private int unknownSymbolHandling = 0; // 0=ignore, leave unmodified
        private int caseHandling = 0; // 0=preserve, 1, convert all to upper, 2= convert all to lower
        public string hardcodedAlphabet="AEIOUYBCDFGHJKLMNPQRSTVWXZ";

        #endregion

        #region TaskPane Settings
        [PropertySaveOrder(1)]
        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "EncryptCaption", "DecryptCaption")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "EncryptCaption", "DecryptCaption" })]
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

                //if (ReExecute != null) ReExecute();
            }
        }

        #region PlugBoard Setting
        [TaskPane("PlugBoardCaption", "PlugBoardTooltip", "PlugBoardCaption", 2, true, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get { return hardcodedAlphabet; }
            set { }
        }

        [TaskPaneAttribute("OutputCaption", "OutputTooltip", "PlugBoardCaption", 3, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{26}$")]
        public string Alphabet
        {
            get
            {
                return alphabet;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (alphabet != value)
                {
                    alphabet = value;
                    hasChanges = true;
                }
            }
        }
        #endregion

        #region Position Setting
        [TaskPaneAttribute("PositionSixesCaption", "StartwertTooltip", "PositionCaption", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Sixes
        {
            get
            {
                return sechserPos;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (sechserPos != value)
                {
                    sechserPos = value;
                    hasChanges = true;
                }
            }
        }
        
        
        [TaskPaneAttribute("PositionTwentiesCaption1", "StartwertTooltip", "PositionCaption", 6, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties
        {
            get
            {
                return zwanzigerPos1;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos1 != value)
                {
                    zwanzigerPos1 = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption2", "StartwertTooltip", "PositionCaption", 7, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties2
        {
            get
            {
                return zwanzigerPos2;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos2 != value)
                {
                    zwanzigerPos2 = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption3", "StartwertTooltip", "PositionCaption", 8, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties3
        {
            get
            {
                return zwanzigerPos3;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos3 != value)
                {
                    zwanzigerPos3 = value;
                    hasChanges = true;
                }
            }
        }


        #endregion

        #region Motion Setting
        [TaskPaneAttribute("MotionCaption", "MotionTooltip", "MotionCaption",9, true, ControlType.TextBox, ValidationType.RegEx, "^[1-3]{3}$")]
        public int Motion
        {
            get
            {
                return motion;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (motion != value)
                {
                    motion = value;
                    hasChanges = true;
                }
            }
        }

        #endregion
      

        #endregion

        #region Text options

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            3, ContextMenuControlType.ComboBox, null,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            "TextOptionsGroup", 10, false, ControlType.ComboBox,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if ((int)value != unknownSymbolHandling) HasChanges = true;
                this.unknownSymbolHandling = (int)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [ContextMenu("CaseHandlingCaption", "CaseHandlingTooltip",
            4, ContextMenuControlType.ComboBox, null,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip",
            "TextOptionsGroup", 4, false, ControlType.ComboBox,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return this.caseHandling; }
            set
            {
                if ((int)value != caseHandling) HasChanges = true;
                this.caseHandling = (int)value;
                OnPropertyChanged("CaseHandling");
            }
        }

        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
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
