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
        private int publicVar;
        private int secretVar;
        private int maxcube;
        private int constTest = 50;
        private int linTest = 50;
        private string setPublicBits = "0*00*";
        private int outputBit = 1;
        private bool readSuperpolysFromFile;
        private string openFilename;
        private bool enableLogMessages = false;

        private string saveOutputSuperpoly;
        private Matrix saveSuperpolyMatrix;
        private List<List<int>> saveListCubeIndexes;
        private int[] saveOutputBitIndex;
        private int saveCountSuperpoly;
        private Matrix saveMatrixCheckLinearitySuperpolys;
        private int savePublicBitSize;
        private int saveSecretBitSize;

        #endregion


        #region Algorithm settings properties (visible in the settings pane)

        [PropertySaveOrder(1)]
        [ContextMenu( "ActionCaption", "ActionTooltip", 
            1, 
            ContextMenuControlType.ComboBox, 
            null, 
            "Preprocessing",
            "Online",
            "Manual Public Bit Input")]
        [TaskPane( "ActionCaption", "ActionTooltip", 
            "", 
            1, 
            false, 
            ControlType.ComboBox,
            new string[] { "Preprocessing", "Online", "Manual Public Bit Input" })]
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
        [TaskPane( "PublicVarCaption", "PublicVarTooltip", 
            null, 
            2, 
            false, 
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

        [PropertySaveOrder(3)]
        [TaskPane( "SecretVarCaption", "SecretVarTooltip", 
            null, 
            3, 
            false, 
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

        [PropertySaveOrder(4)]
        [TaskPane( "MaxCubeCaption", "MaxCubeTooltip",
            null,
            4,
            false,
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

        [PropertySaveOrder(5)]
        [TaskPane( "ConstTestCaption", "ConstTestTooltip",
            null,
            5,
            false,
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

        [PropertySaveOrder(6)]
        [TaskPane( "LinTestCaption", "LinTestTooltip", 
            null, 
            6, 
            false, 
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

        [PropertySaveOrder(7)]
        [TaskPane( "OutputBitCaption", "OutputBitTooltip",
            null,
            7,
            true,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int OutputBit
        {
            get { return outputBit; }
            set
            {
                if (value != this.outputBit) HasChanges = true;
                outputBit = value;
                OnPropertyChanged("OutputBit");
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane( "SetPublicBitsCaption", "SetPublicBitsTooltip",
            null,
            8,
            false,
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

        [PropertySaveOrder(9)]
        [ContextMenu( "ReadSuperpolysFromFileCaption", "ReadSuperpolysFromFileTooltip",
            9,
            ContextMenuControlType.CheckBox,
            null,
            new string[] { "Read Superpolys From File" })]
        [TaskPane( "ReadSuperpolysFromFileTPCaption", "ReadSuperpolysFromFileTPTooltip", 
            null,
            9,
            false,
            ControlType.CheckBox,
            "",
            null)]
        public bool ReadSuperpolysFromFile
        {
            get { return this.readSuperpolysFromFile; }
            set
            {
                this.readSuperpolysFromFile = (bool)value;
                OnPropertyChanged("ReadSuperpolysFromFile");
                HasChanges = true;
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane( "OpenFilenameCaption", "OpenFilenameTooltip",
            null, 
            10, 
            false, 
            ControlType.OpenFileDialog, 
            FileExtension = "All Files (*.*)|*.*")]
        public string OpenFilename
        {
            get { return openFilename; }
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    HasChanges = true;
                    OnPropertyChanged("OpenFilename");
                }
            }
        }

        [PropertySaveOrder(11)]
        [ContextMenu( "EnableLogMessagesCaption", "EnableLogMessagesTooltip",
            11, 
            ContextMenuControlType.CheckBox, 
            null,
            new string[] { "Enable log messages?" })]
        [TaskPane( "EnableLogMessagesTPCaption", "EnableLogMessagesTPTooltip",
            null, 
            11, 
            false, 
            ControlType.CheckBox, 
            "", 
            null)]
        public bool EnableLogMessages
        {
            get { return this.enableLogMessages; }
            set
            {
                this.enableLogMessages = (bool)value;
                OnPropertyChanged("EnableLogMessages");
                HasChanges = true;
            }
        }

        public string SaveOutputSuperpoly
        {
            get { return saveOutputSuperpoly; }
            set
            {
                if (value != saveOutputSuperpoly) hasChanges = true;
                saveOutputSuperpoly = value;
            }
        }

        public Matrix SaveSuperpolyMatrix
        {
            get { return saveSuperpolyMatrix; }
            set
            {
                if (value != saveSuperpolyMatrix) hasChanges = true;
                saveSuperpolyMatrix = value;
            }
        }

        public List<List<int>> SaveListCubeIndexes
        {
            get { return saveListCubeIndexes; }
            set
            {
                if (value != saveListCubeIndexes) hasChanges = true;
                saveListCubeIndexes = value;
            }
        }

        public int[] SaveOutputBitIndex
        {
            get { return saveOutputBitIndex; }
            set
            {
                if (value != saveOutputBitIndex) hasChanges = true;
                saveOutputBitIndex = value;
            }
        }

        public int SaveCountSuperpoly
        {
            get { return saveCountSuperpoly; }
            set
            {
                if (value != saveCountSuperpoly) hasChanges = true;
                saveCountSuperpoly = value;
            }
        }

        public Matrix SaveMatrixCheckLinearitySuperpolys
        {
            get { return saveMatrixCheckLinearitySuperpolys; }
            set
            {
                if (value != saveMatrixCheckLinearitySuperpolys) hasChanges = true;
                saveMatrixCheckLinearitySuperpolys = value;
            }
        }

        public int SavePublicBitSize
        {
            get { return savePublicBitSize; }
            set
            {
                if (value != savePublicBitSize) hasChanges = true;
                savePublicBitSize = value;
            }
        }

        public int SaveSecretBitSize
        {
            get { return saveSecretBitSize; }
            set
            {
                if (value != saveSecretBitSize) hasChanges = true;
                saveSecretBitSize = value;
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
