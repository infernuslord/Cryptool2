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
using System.Windows;

namespace QuadraticSieve
{
    class QuadraticSieveSettings : ISettings
    {
        #region private variables
        private int coresUsed;
        private bool hasChanges = false;
        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        private bool deleteCache;
        private bool usePeer2Peer;
        private string channel;
        #endregion

        #region events
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        #endregion

        #region public

        /// <summary>
        /// Constructs a new QuadraticSieveSettings
        /// 
        /// Also calculates the amount of cores which can be used for the quadratic sieve
        /// </summary>
        public QuadraticSieveSettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i+1).ToString());
            CoresUsed = Environment.ProcessorCount-1;
        }

        /// <summary>
        /// Getter/Setter for the amount of cores which the user wants to have used by the quadratic sieve
        /// </summary>
        [TaskPane("CoresUsed", "Choose how many cores should be used for sieving", null, 1, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
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
        
        /// <summary>
        /// Get the available amount of cores of this pc
        /// </summary>
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

        /// <summary>
        /// Getter / Setter to enable/disable the deletion of the cache
        /// </summary>
        [TaskPane("Delete cache", "If checked, this plugin will delete the old cache file before it starts sieving", null, 2, false, DisplayLevel.Expert, ControlType.CheckBox, "", null)]
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
        
        /// <summary>
        /// Getter / Setter to enable/disable the use of peer2peer
        /// </summary>
        [TaskPane("Use Peer2Peer", "If checked, this plugin will connect to Peer2Peer network to sieve together with other clients", null, 3, false, DisplayLevel.Experienced, ControlType.CheckBox, "", null)]
        public bool UsePeer2Peer
        {
            get { return usePeer2Peer; }
            set
            {
                if (value != usePeer2Peer)
                {
                    usePeer2Peer = value;
                    hasChanges = true;
                    if (usePeer2Peer)
                    {
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Channel", Visibility.Visible)));
                    }
                    else
                    {
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Channel", Visibility.Collapsed)));
                    }
                    OnPropertyChanged("UsePeer2Peer");
                }
            }
        }

        /// <summary>
        /// Channel of the Peer2Peer network
        /// </summary>
        [TaskPane("Channel", "Channel of the Peer2Peer network", null, 4, false, DisplayLevel.Experienced, ControlType.TextBox, "", null)]
        public string Channel
        {
            get { return channel; }
            set
            {
                if (value != channel)
                {
                    channel = value;
                    hasChanges = true;
                    OnPropertyChanged("Channel");
                }
            }
        }

        /// <summary>
        /// Called if the settings have changes
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

        #region private

        /// <summary>
        /// A property changed
        /// </summary>
        /// <param name="name">name</param>
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}