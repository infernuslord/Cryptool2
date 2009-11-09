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
        private int selectedBlackBox = 0;
        private int selectedAction = 0;
        private int publicVar;
        private int secretVar;
        private int maxcube;
        private int constTest = 50;
        private int linTest = 50;
        private string setPublicBits = "0*00*";
        private int outputBit;
        private bool readSuperpolysFromFile;
        private string openFilename;
        private bool enableLogMessages = false;

        private string saveOutputSuperpoly;
        private Matrix saveSuperpolyMatrix;
        private List<List<int>> saveListCubeIndexes;
        private int[] saveOutputBitIndex;
        private int saveCountSuperpoly;
        private Matrix saveMatrixCheckLinearitySuperpolys;

        #endregion


        #region Algorithm settings properties (visible in the settings pane)

        /*[PropertySaveOrder(1)]
        [ContextMenu("Black Box",
            "Select the black box",
            1,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            null,
            "Boolean function parser",
            "Trivium")]
        [TaskPane("Black Box",
            "Select the black box",
            "",
            1,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new string[] { "Boolean function parser", "Trivium" })]
        public int BlackBox
        {
            get { return this.selectedBlackBox; }
            set
            {
                if (value != selectedBlackBox) HasChanges = true;
                this.selectedBlackBox = value;
                OnPropertyChanged("BlackBox");
            }
        }*/

        [PropertySaveOrder(2)]
        [ContextMenu("Action", 
            "Select the cube attack modi", 
            2, 
            DisplayLevel.Beginner, 
            ContextMenuControlType.ComboBox, 
            null, 
            "Preprocessing",
            "Online",
            "Manual Public Bit Input")]
        [TaskPane("Action",
            "Select the cube attack modi", 
            "", 
            2, 
            false, 
            DisplayLevel.Beginner, 
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
        
        [PropertySaveOrder(3)]
        [TaskPane("Public Bit Size",
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
        [TaskPane("Secret Bit Size",
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
        [TaskPane("Max Cube Size",
            "Maxmium size of the summation cube.",
            null,
            5,
            true,
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
            true,
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
        [TaskPane("Linearity Test",
            "Number of linearity tests to check if the superpoly is linear or not.", 
            null, 
            7, 
            true, 
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
        [TaskPane("Output Bit",
            "Chooses the output bit of the black box, which should be evaluated.",
            null,
            8,
            true,
            DisplayLevel.Beginner,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int TriviumOutputBit
        {
            get { return outputBit; }
            set
            {
                if (value != this.outputBit) HasChanges = true;
                outputBit = value;
                OnPropertyChanged("TriviumOutputBit");
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("Manual Public Bit Input",
            "Possible inputs '0' (set bit to value 0), '1' (set bit to value 1) and '*' (sum the 0/1 value of the bit).",
            null,
            9,
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

        [PropertySaveOrder(10)]
        [ContextMenu("Read superpolys from File",
            "With this checkbox enabled, superpolys will be loaded from the selected File and can be evaluated in the online phase.",
            10,
            DisplayLevel.Experienced,
            ContextMenuControlType.CheckBox,
            null,
            new string[] { "Read superpolys from File" })]
        [TaskPane("Read superpolys from File", 
            "With this checkbox enabled, superpolys will be loaded from the selected File and can be evaluated in the online phase.",
            null,
            10,
            false,
            DisplayLevel.Beginner,
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

        [PropertySaveOrder(11)]
        [TaskPane("Filename", "Select the file you want to open.", 
            null, 
            11, 
            false, 
            DisplayLevel.Beginner, 
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

        [PropertySaveOrder(12)]
        [ContextMenu("Enable log messages",
            "With this checkbox enabled, log messages will be showed.", 
            12, 
            DisplayLevel.Experienced, 
            ContextMenuControlType.CheckBox, 
            null,
            new string[] { "Enable log messages?" })]
        [TaskPane("Enable log messages",
            "With this checkbox enabled, a lot of log messages will be showed during preprocessing.", 
            null, 
            12, 
            false, 
            DisplayLevel.Beginner, 
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
