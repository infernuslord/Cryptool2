/*
   Copyright 2009 Christian Arnold, Universität Duisburg-Essen

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
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.KeySearcher_IControl
{
    public class KeySearcher_IControlSettings : ISettings
    {
        private KeySearcher_IControl keysearcher;
        private int coresUsed;

        public KeySearcher_IControlSettings(KeySearcher_IControl ks)
        {
            keysearcher = ks;
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;
        }

        //private string key;
        //[TaskPane("Key", "Key pattern used to bruteforce", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        //public String Key
        //{
        //    get
        //    {
        //        return key;
        //    }
        //    set
        //    {
        //        key = value;
        //        OnPropertyChanged("Key");
        //        if (!(keysearcher.Pattern != null && keysearcher.Pattern.testWildcardKey(value)))
        //            keysearcher.GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
        //    }
        //}

        //[TaskPane("Reset", "Reset Key", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        //public void Reset()
        //{

        //    Key = keysearcher.Pattern.giveInputPattern();
        //}
        
        [TaskPane("CoresUsed", "Choose how many cores should be used", null, 3, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
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
        
        #region ISettings Members

        private bool hasChanges;

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
