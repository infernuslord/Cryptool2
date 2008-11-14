using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.Security.Cryptography;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.WEP
{
    /// <summary>
    /// Settings for the WEP plugins.
    /// You can choose between encryption and decryption, between saving to file and not,
    /// and you can set the number how many packets are going to be saved.
    /// </summary>
    public class WEPSettings : ISettings
    {
        #region Private variables

        private bool hasChanges = false;
        private int action = 0;

        /// <summary>
        /// Encryption (=0) or decryption (=1)?
        /// </summary>
        [ContextMenu("Action",
            "Do you want to encrypt or decrypt data?",
            1,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            new int[] { 1, 2 },
            "Encrypt",
            "Decrypt")]
        [TaskPane("Action",
            "Do you want to encrypt or decrypt data?",
            "",
            1,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new String[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if ((int)value != action)
                {
                    hasChanges = true;
                }
                this.action = (int)value;
                OnPropertyChanged("Action");
            }
        }

        /*/// <summary>
        /// 0 = get from other plugin, 1 = load from file
        /// </summary>
        [ContextMenu("Input",
            "Do you want to load packets from a file or get them from other plugins?",
            2,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            new int[] { 1, 2 },
            "Get from other plugin",
            "Load from file")]
        [TaskPane("Input",
            "Dou you want to load packets from a file or get them from other plugins?",
            "",
            2,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new String[]{"Get from other plugin", "Load from file"})]
        public int InputMode
        {
            get { return this.inputMode; }
            set
            {
                if ((int)value != inputMode)
                {
                    hasChanges = true;
                }
                this.inputMode = (int)value;
                if (this.inputMode == 0)
                {
                    LoadFileName = string.Empty;
                }
                OnPropertyChanged("InputMode");
            }
        }

        [TaskPane("Load Filename",
            "File to load data from.",
            null,
            3,
            false,
            DisplayLevel.Beginner,
            ControlType.OpenFileDialog,
            "All Files (*.*)|*.*")]
        public string LoadFileName
        {
            get { return loadFileName; }
            set
            {
                loadFileName = value;
                OnPropertyChanged("LoadFileName");
            }
        }

        /*[TaskPane("Close file",
            "Close file",
            null,
            4,
            false,
            DisplayLevel.Beginner,
            ControlType.Button)]
        public void CloseLoadFile()
        {
            LoadFileName = string.Empty;
        }

        /// <summary>
        /// Don't save to file (=0) or save to file (=1).
        /// </summary>
        [ContextMenu("Output",
            "Do you want to apply the packets for further plugins or save them to a file?",
            4,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            new int[] { 1, 2 },
            "For further plugins",
            "Save to file")]
        [TaskPane("Output",
            "Do you want to apply the packets for further plugins or save them to a file?",
            "",
            4,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new String[] { "For further plugins", "Save to file" })]
        public int OutputMode
        {
            get { return this.outputMode; }
            set
            {
                if ((int)value != outputMode)
                {
                    hasChanges = true;
                }
                this.outputMode = (int)value;
                if (this.outputMode == 0)
                {
                    SaveFileName = string.Empty;
                }
                OnPropertyChanged("OutputMode");
            }
        }

        [TaskPane("Target Filename",
            "File to write data into.",
            null,
            5,
            false,
            DisplayLevel.Beginner,
            ControlType.SaveFileDialog,
            "All Files (*.*)|*.*")]
        public string SaveFileName
        {
            get { return saveFileName; }
            set
            {
                saveFileName = value;
                OnPropertyChanged("SaveFileName");
            }
        }

        /*[TaskPane("Close file",
            "Close file",
            null,
            7,
            false,
            DisplayLevel.Beginner,
            ControlType.Button)]
        public void CloseSaveFile()
        {
            SaveFileName = string.Empty;
        }*/

        #endregion


        #region ISettings Member

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(Icon));
            }
        }

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
