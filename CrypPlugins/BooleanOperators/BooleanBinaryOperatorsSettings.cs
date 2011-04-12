/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase;


namespace Cryptool.Plugins.BooleanOperators
{
    class BooleanBinaryOperatorsSettings : ISettings
    {
        private int operatorType = 0;
        /* 0 = AND
         * 1 = OR
         * 2 = NAND
         * 3 = NOR
         * 4 = XOR
         */

        private bool hasChanges = false;
        private bool updateOnlyAtBothInputsChanged = true;
        private bool defaultFlagA = false;
        private bool defaultFlagB = false;

        #region ISettings Members
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

        [ContextMenu( "BBO_OperatorTypeCaption", "BBO_OperatorTypeTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        [TaskPane( "BBO_OperatorTypeCaption", "BBO_OperatorTypeTooltip", null, 2, false, ControlType.ComboBox, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        public int OperatorType
        {
            get { return this.operatorType; }
            set
            {
                if (operatorType != value)
                    hasChanges = true;

                this.operatorType = value;
                OnPropertyChanged("OperatorType");
                ChangePluginIcon(value);                
            }
        }


        [TaskPane( "BBO_UpdateOnlyAtBothInputsChangedCaption", "BBO_UpdateOnlyAtBothInputsChangedTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool UpdateOnlyAtBothInputsChanged
        {
            get { return updateOnlyAtBothInputsChanged; }
            set
            {
                if (value != updateOnlyAtBothInputsChanged)
                {
                    updateOnlyAtBothInputsChanged = value;
                    hasChanges = true;
                    OnPropertyChanged("UpdateOnlyAtBothInputsChanged");
                }
            }
        }

        [TaskPane( "BBO_DefaultFlagACaption", "BBO_DefaultFlagATooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool DefaultFlagA
        {
            get { return defaultFlagA; }
            set
            {
                if (value != defaultFlagA)
                {
                    defaultFlagA = value;
                    hasChanges = true;
                    OnPropertyChanged("DefaultFlagA");
                }
            }
        }

        [TaskPane( "BBO_DefaultFlagBCaption", "BBO_DefaultFlagBTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool DefaultFlagB
        {
            get { return defaultFlagB; }
            set
            {
                if (value != defaultFlagB)
                {
                    defaultFlagB = value;
                    hasChanges = true;
                    OnPropertyChanged("DefaultFlagB");
                }
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

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
