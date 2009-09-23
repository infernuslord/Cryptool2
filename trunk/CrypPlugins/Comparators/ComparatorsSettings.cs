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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Comparators
{
    public class ComparatorsSettings : ISettings
    {
        #region private variables
        private int comparator = 0; // 0 =, 1 !=, 2 <, 3 >, 4 <=, 5 >=
        private bool hasChanges;
        #endregion

        #region taskpane
        [TaskPane("Comparator", "Choose the Comperator", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "=", "!=", "<", ">", "<=", ">=" })]
        public int Comparator
        {
            get { return this.comparator; }
            set
            {
                if (value != this.comparator)
                {
                    this.comparator = value;
                    OnPropertyChanged("Comparator");
                    HasChanges = true;

                    ChangePluginIcon(comparator);
                }
            }
        }
        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
        }
        #endregion
    }
}
