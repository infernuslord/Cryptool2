using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    public class DESSettings : ISettings
    {
        private int action = 0; //0=encrypt, 1=decrypt
        private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7", 3="ANSIX923", 4="ISO10126"
        private bool tripleDES = false;

        [ContextMenu( "ActionCaption", "ActionTooltip",1, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "ActionList1", "ActionList2")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [ContextMenu("ModeCaption", "ModeTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "ModeList1", "ModeList2", "ModeList3" })]
        [TaskPane("ModeTPCaption", "ModeTPTooltip", null, 2, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3" })]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                if (((int)value) != mode)
                {
                    this.mode = (int)value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 3, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5")]
        [TaskPane("PaddingTPCaption", "PaddingTPTooltip", "", 3, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding)
                {
                    this.padding = (int)value;
                    OnPropertyChanged("Padding");
                }
            }
        }

        [ContextMenu("TripleDESCaption", "TripleDESTooltip", 3, ContextMenuControlType.CheckBox, null)]
        [TaskPane("TripleDESCaption", "TripleDESTooltip", "", 3, false, ControlType.CheckBox, null)]
        public bool TripleDES
        {
            get { return this.tripleDES; }
            set
            {
                if (((bool)value) != tripleDES)
                {
                    this.tripleDES = (bool)value;
                    OnPropertyChanged("TripleDES");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

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
