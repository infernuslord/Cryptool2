using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Cryptool.AESTeaching
{  
    class AESSettings : IEncryptionAlgorithmSettings
    {
        private EncryptionAlgorithmAction selectedAction = EncryptionAlgorithmAction.Encrypt;
        
        private int keylength = 128;
        private string key;
        private bool visualizationenabled = false;

        [TaskPaneSettings("Key length",
            "Select the required key length",
            0, true, DisplayLevel.Beginner, ControlType.ComboBox, "", new string[] { "128","192","256" })]
        public string KeyLength
        {
            get { return this.keylength.ToString(); }
            set { this.keylength = int.Parse(value); }
        }

        [TaskPaneSettings("Key",
            "Enter the key in hex-notation",
            0, true, DisplayLevel.Beginner, ControlType.TextBox, "", new string[] { })]
        public string Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        [TaskPaneSettings("Visualization",
            "Determines whether to enable the visualization",
            0, true, DisplayLevel.Beginner, ControlType.ComboBox , "", new string[] {"Yes", "No" })]
        public int VisualizationEnabled
        {
            get
            {
                if (visualizationenabled)
                    return 0;
                else
                    return 1;
            }
            set
            {
                this.visualizationenabled = (value == 0);
            }
        }

        [TaskPaneSettings("Action",
            "Select the Algorithm action",
            Int32.MaxValue, true, DisplayLevel.Beginner, ControlType.ComboBox, "", new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get
            {
                return (int)this.selectedAction;
            }
            set
            {
                this.selectedAction = (EncryptionAlgorithmAction)value;
            }
        }

    }
}
