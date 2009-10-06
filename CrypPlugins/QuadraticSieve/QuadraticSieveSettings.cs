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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections.ObjectModel;

namespace QuadraticSieve
{
    class QuadraticSieveSettings : ISettings
    {
        #region private variables
        private int coresUsed;
        private bool hasChanges = false;
        #endregion

        public QuadraticSieveSettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i+1).ToString());
            CoresUsed = Environment.ProcessorCount-1;
        }

        #region taskpane
        [TaskPane("CoresUsed", "Choose how many cores should be used", null, 1, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                    HasChanges = true;
                }
            }
        }

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                }
                OnPropertyChanged("CoresAvailable");
            }
        }

        private bool deleteCache;
        [TaskPane("Delete cache", "If checked, this plugin will delete the old cache file before it starts sieving.", null, 2, false, DisplayLevel.Expert, ControlType.CheckBox, "", null)]
        public bool DeleteCache
        {
            get { return deleteCache; }
            set
            {
                if (value != deleteCache)
                {
                    deleteCache = value;
                    hasChanges = true;
                    OnPropertyChanged("DeleteCache");
                }
            }
        }

        #endregion

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

        

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }


        #endregion

    }
}