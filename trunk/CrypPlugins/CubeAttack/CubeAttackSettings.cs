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
        private int linTest = 100;
        private string setPublicBits = "0*00*000*00***0000*00000000*0000000*0*0*000000*0*0***0*0*00000**0*000000*00000*0";
        
        #endregion


        #region Algorithm settings properties (visible in the settings pane)

        [PropertySaveOrder(1)]
        [ContextMenu("Action", 
            "Select the Algorithm action", 
            1, 
            DisplayLevel.Beginner, 
            ContextMenuControlType.ComboBox, 
            null, 
            "Find maxterms", 
            "Set public bits" )]
        [TaskPane("Action", 
            "Select the phase", 
            "", 
            1, 
            false, 
            DisplayLevel.Beginner, 
            ControlType.ComboBox, 
            new string[] { "Find maxterms", "Set public bits" })]
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
        [ContextMenu("Try to recover key", 
            "Online phase of the cube attack which tries to solve the system of linear maxterm equations to recover the secret key.", 
            2, 
            DisplayLevel.Beginner, 
            ContextMenuControlType.CheckBox, 
            null, 
            "")]
        [TaskPane("Try to recover key", 
            "Online phase of the cube attack which tries to solve the system of linear maxterm equations to recover the secret key.", 
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
        [TaskPane("Public variables", 
            "Number of public variables.", 
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
                publicVar = value;
                OnPropertyChanged("PublicVar");
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("Secret variables", 
            "Number of secret variables.", 
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
                secretVar = value;
                OnPropertyChanged("SecretVar");
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("Max cube size",
            "Maxmium size of cube.",
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
                maxcube = value;
                OnPropertyChanged("MaxCube");
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("Linearity tests", 
            "Number of linearity tests.", 
            null, 
            6, 
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
                linTest = value;
                OnPropertyChanged("LinTest");
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("Set public bits", 
            "Manual input of public bits.", 
            null, 
            6, 
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
