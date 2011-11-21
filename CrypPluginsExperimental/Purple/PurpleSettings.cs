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

        private string alphabet = "AEIOUYBCDFGHJKLMNPQRSTVWXZ";
        private int motion = 123;
        private int selectedAction = 0;
        public int sechserPos = 0;
        public int zwanzigerPos1 = 0;
        public int zwanzigerPos2 = 0;
        public int zwanzigerPos3 = 0; 
 

        public string hardcodedAlphabet="AEIOUYBCDFGHJKLMNPQRSTVWXZ";

        #endregion

        #region TaskPane Settings
        [PropertySaveOrder(1)]
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
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");

                    //if (ReExecute != null) ReExecute();   
                }
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
                if (alphabet != value)
                {
                    alphabet = value;
                    OnPropertyChanged("Alphabet");
                }
            }
        }
        #endregion

        #region Position Setting
        [TaskPaneAttribute("PositionSixesCaption", "StartwertTooltip", "PositionCaption", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{6}$")]
        public int Sixes
        {
            get
            {
                return sechserPos;
            }
            set
            {
                if (sechserPos != value)
                {
                    sechserPos = value;
                    OnPropertyChanged("Sixes");
                }
            }
        }
        
        
        [TaskPaneAttribute("PositionTwentiesCaption1", "StartwertTooltip", "PositionCaption", 6, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{6}$")]
        public int Twenties
        {
            get
            {
                return zwanzigerPos1;
            }
            set
            {
                if (zwanzigerPos1 != value)
                {
                    zwanzigerPos1 = value;
                    OnPropertyChanged("Twenties");
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption2", "StartwertTooltip", "PositionCaption", 7, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{6}$")]
        public int Twenties2
        {
            get
            {
                return zwanzigerPos2;
            }
            set
            {
                if (zwanzigerPos2 != value)
                {
                    zwanzigerPos2 = value;
                    OnPropertyChanged("Twenties2");
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption3", "StartwertTooltip", "PositionCaption", 8, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{6}$")]
        public int Twenties3
        {
            get
            {
                return zwanzigerPos3;
            }
            set
            {
                if (zwanzigerPos3 != value)
                {
                    zwanzigerPos3 = value;
                    OnPropertyChanged("Twenties3");
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
                if (motion != value)
                {
                    motion = value;
                    OnPropertyChanged("Motion");
                }
            }
        }

        #endregion
      
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
