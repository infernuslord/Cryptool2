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

        private int countOfInputs = 1;
        [TaskPane("Number of inputs", "How many inputs do you need?", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
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
            DisplayLevel.Experienced,
            ContextMenuControlType.CheckBox,
            null,
            "")]
        [TaskPane("CubeAttack mode",
            "Extend the BFP for use as a slave for Cube Attack.",
            null,
            1,
            false,
            DisplayLevel.Experienced,
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
