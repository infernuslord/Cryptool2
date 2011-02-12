/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;

namespace Cryptool.BooleanFunctionParser
{
    class BooleanFunctionParserSettings : ISettings
    {
        #region Private variables

        private bool hasChanges = false;
        private bool useBFPforCube = false;
        private int saveInputsCount = 1;

        #endregion

        #region for Quickwatch

        private string function;
        public string Function
        {
            get { return function; }
            set
            {
                if (value != function) hasChanges = true;
                function = value;
            }
        }

        private string data;
        public string Data
        {
            get { return data; }
            set
            {
                if (value != data) hasChanges = true;
                data = value;
            }
        }

        private string functionCube;
        public string FunctionCube
        {
            get { return functionCube; }
            set
            {
                if (value != functionCube) hasChanges = true;
                functionCube = value;
            }
        }

        private string dataCube;
        public string DataCube
        {
            get { return dataCube; }
            set
            {
                if (value != dataCube) hasChanges = true;
                dataCube = value;
            }
        }

        #endregion

        #region ISettings Members

        public int countOfInputsOld; 
        private int countOfInputs = 1;
        [TaskPane("Number of inputs", "How many inputs do you need?", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int CountOfInputs
        {
            get { return this.countOfInputs; }
            set
            {
                // this handling has to be implemented; doesn't work as expected right now
                /*if (CanChangeProperty)
                {*/
                    this.countOfInputs = value;
                    OnPropertyChanged("CountOfInputs");
                    //saveInputsCount = value;
                    HasChanges = true;
                /*}
                else
                {
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs("Can't change number of boolean inputs while plugin is connected.", null, NotificationLevel.Warning));
                    this.countOfInputs = saveInputsCount;
                }*/
            }
        }

        [ContextMenu("CubeAttack mode",
            "Extend the BFP for use as a slave for Cube Attack.",
            1,
            ContextMenuControlType.CheckBox,
            null,
            "")]
        [TaskPane("CubeAttack mode",
            "Extend the BFP for use as a slave for Cube Attack.",
            null,
            1,
            false,
            ControlType.CheckBox, "")]
        public bool UseBFPforCube
        {
            get { return this.useBFPforCube; }
            set
            {
                if (value != this.useBFPforCube) HasChanges = true;
                this.useBFPforCube = value;
                OnPropertyChanged("UseBFPforCube");
            }
        }

        /*[TaskPane("Evaluate function", "", null, 2, false, ControlType.Button)]
        public void evalFunction()
        {
            OnPropertyChanged("evalFunction");
        }*/

        public bool HasChanges
        {
            get { return hasChanges; }
            set
            {
                if (value != hasChanges)
                {
                    hasChanges = value;
                    OnPropertyChanged("HasChanges");
                }
            }
        }

        public bool CanChangeProperty { get; set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
