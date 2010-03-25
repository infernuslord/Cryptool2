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
using System.IO;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.ANDBinary
{
    public class ANDBinarySettings : ISettings
    {
        #region Public Xor specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Xor plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void XorLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event XorLogMessage LogMessage;
        
        /// <summary>
        /// Returns true if some settigns have been changed. This value should be set
        /// externally to false e.g. when a project was saved.
        /// </summary>
        [PropertySaveOrder(0)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        bool flagInputOne;
        [ContextMenu("Declare Flag I1 clean", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set Flag of Input One to clean at the beginning of the chain.")]
        [TaskPaneAttribute("Declare Flag I1 clean", " yes / no ", "", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, null)]
        public bool FlagInputOne
        {
            get { return this.flagInputOne; }
            set
            {
                this.flagInputOne = value;
                OnPropertyChanged("FlagInputOne");
                HasChanges = true;
            }

        }

        bool flagInputTwo;
        [ContextMenu("Declare Flag I2 clean", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set Flag of Input Two to clean at the beginning of the chain.")]
        [TaskPaneAttribute("Declare Flag I2 clean", " yes / no ", "", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, null)]
        public bool FlagInputTwo
        {
            get { return this.flagInputTwo; }
            set
            {
                this.flagInputTwo = value;
                OnPropertyChanged("FlagInputTwo");
                HasChanges = true;
            }

        }

        bool invertInputOne;
        [ContextMenu("Invert I1", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Invert Input 1.")]
        [TaskPaneAttribute("Invert I1", " yes / no ", "", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, null)]
        public bool InvertInputOne
        {
            get { return this.invertInputOne; }
            set
            {
                this.invertInputOne = value;
                OnPropertyChanged("InvertInputOne");
                HasChanges = true;
            }

        }

        #endregion

        #region Private variables

        private bool hasChanges;

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
