using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.CubeAttack
{
    public class CubeAttackSettings : ISettings
    {
        #region Public CubeAttack specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the CubeAttack plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void CubeAttackLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event CubeAttackLogMessage LogMessage;
        
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

        #endregion

        #region Private variables

        private bool hasChanges;
        private int selectedAction = 0;
        bool onlinePhase = false;
        private int publicVar;
        private int secretVar;
        private int maxcube;
        private int constTest = 50;
        private int linTest = 50;
        private string setPublicBits = "0*00*";
        
        #endregion


        #region Algorithm settings properties (visible in the settings pane)

        [PropertySaveOrder(1)]
        [ContextMenu("Action", 
            "Select the Algorithm action", 
            1, 
            DisplayLevel.Beginner, 
            ContextMenuControlType.ComboBox, 
            null, 
            "Preprocessing/Online Phase",
            "Input public bits")]
        [TaskPane("Action", 
            "Select the phase", 
            "", 
            1, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.ComboBox,
            new string[] { "Preprocessing/Online Phase", "Input public bits" })]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if(value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [PropertySaveOrder(2)]
        [ContextMenu("Try to discover key", 
            "Online phase of the cube attack which tries to solve the system of linear maxterm equations to discover the secret key.", 
            2, 
            DisplayLevel.Beginner, 
            ContextMenuControlType.CheckBox, 
            null, 
            "")]
        [TaskPane("Try to discover key", 
            "Online phase of the cube attack which tries to solve the system of linear maxterm equations to discover the secret key.", 
            null, 
            2, 
            false, 
            DisplayLevel.Expert, 
            ControlType.CheckBox, "")]
        public bool OnlinePhase
        {
            get { return this.onlinePhase; }
            set
            {
                if (value != this.onlinePhase) HasChanges = true;
                this.onlinePhase = value;
                OnPropertyChanged("OnlinePhase");
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("Public bit size",
            "Public input bits (IV or plaintext) of the attacked cryptosystem.", 
            null, 
            3, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.NumericUpDown, 
            ValidationType.RangeInteger, 
            1, 
            10000)]
        public int PublicVar
        {
            get { return publicVar; }
            set
            {
                if (value != this.publicVar) HasChanges = true;
                publicVar = value;
                OnPropertyChanged("PublicVar");
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("Secret bit size",
            "Key size or key length  of the attacked cryptosystem.", 
            null, 
            4, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.NumericUpDown, 
            ValidationType.RangeInteger, 
            1, 
            10000)]
        public int SecretVar
        {
            get { return secretVar; }
            set
            {
                if (value != this.secretVar) HasChanges = true;
                secretVar = value;
                OnPropertyChanged("SecretVar");
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("Max cube size",
            "Maxmium size of the summation cube.",
            null,
            5,
            false,
            DisplayLevel.Beginner,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int MaxCube
        {
            get { return maxcube; }
            set
            {
                if (value != this.maxcube) HasChanges = true;
                maxcube = value;
                OnPropertyChanged("MaxCube");
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("Constant Test",
            "Number of tests to check if the superpoly is a constant value or not.",
            null,
            6,
            false,
            DisplayLevel.Beginner,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            0,
            100000)]
        public int ConstTest
        {
            get { return constTest; }
            set
            {
                if (value != this.constTest) HasChanges = true;
                constTest = value;
                OnPropertyChanged("ConstTest");
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("Linearity tests",
            "Number of linearity tests to check if the superpoly is linear or not.", 
            null, 
            7, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.NumericUpDown, 
            ValidationType.RangeInteger, 
            0, 
            100000)]
        public int LinTest
        {
            get { return linTest; }
            set
            {
                if (value != this.linTest) HasChanges = true;
                linTest = value;
                OnPropertyChanged("LinTest");
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("Input public bits", 
            "Manual input of public bits.", 
            null, 
            8, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.TextBox,
            null)]
        public string SetPublicBits
        {
            get 
            {
                if (setPublicBits != null)
                    return setPublicBits;
                else
                    return "";
            }
            set
            {
                if (value != this.setPublicBits) HasChanges = true;
                setPublicBits = value;
                OnPropertyChanged("SetPublicBits");
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
