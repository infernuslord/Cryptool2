using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Cryptool.WEPAttacks
{
    /// <summary>
    /// Some settings for the <see cref="WEPAttacks"/> plugin.
    /// </summary>
    public class WEPAttacksSettings : ISettings
    {
        #region Private variables

        /// <summary>
        /// Kind of attack. 0 ==> FMS, 1 ==> KoreK, 2 ==> PTW
        /// </summary>
        private int action = 0;
        private bool fileOrNot = false;
        private bool hasChanges = false;

        /// <summary>
        /// Action. 0 => FMS, 1 => KoreK, 2 => PTW
        /// </summary>
        [ContextMenu("Kind of attack",
            "Kind of attack",
            1,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            new int[] { 1, 2, 3 },
            new string[] { "\"FMS\"", "\"KoreK\"", "\"PTW\"" })]
        [TaskPane("Kind of attack",
            "Kind of attack",
            "",
            1,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new string[] { "\"FMS\"", "\"KoreK\"", "\"PTW\"" })]
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

        /// <summary>
        /// true => source is data, false => source is another plugin (NOT "File Input"!!!)
        /// </summary>
        /*[ContextMenu("Data comes from file",
            "Does the data come from file or not?",
            2,
            DisplayLevel.Beginner,
            ContextMenuControlType.CheckBox,
            new int[] { 1, 2 },
            new string[] { "Data comes from file" })]*/
        [TaskPane("Data comes from file",
            "Does the data come from file or not?",
            "",
            2,
            false,
            DisplayLevel.Beginner,
            ControlType.CheckBox,
            new string[] { "Data comes from file" })]
        public bool FileOrNot
        {
            get
            {
                return this.fileOrNot;
            }
            set
            {
                if ((bool)value != fileOrNot)
                {
                    hasChanges = true;
                }
                this.fileOrNot = (bool)value;
                OnPropertyChanged("FileOrNot");
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
            new String[] { "Get from other plugin", "Load from file" })]
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

        [TaskPane("Load FileName",
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
        public void CloseFile()
        {
            LoadFileName = string.Empty;
        }*/

        #endregion

        #region ISettings Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(Icon));
            }
        }

        public bool HasChanges
        {
            get { return this.hasChanges; }
            set { this.hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Member

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
