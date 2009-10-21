using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
namespace Cryptool.Plugins.SubbyteArrayCryptoolStream
{
    public class SubByteArrayCryptoolStreamSettings: ISettings
        
    {
        private bool hasChanges = false;
        #region taskPane
        private int start;
        private int end;
        private int maxOffset;
        
        [TaskPane("Start Index of ByteArray","Start",null,0,true,DisplayLevel.Beginner,ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Start
        {
            get { return this.start; }
            set { this.start = (int)value;
            OnPropertyChanged("Start");
            }
        }
        [TaskPane("End Index of ByteArray", "End", null, 1, true, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int End
        {
            get { return this.end; }
            set { this.end = (int)value;
            OnPropertyChanged("End");
            }
        }

        public void setMaxOffset(int max) 
        {
            this.maxOffset = max;
        }

        #endregion
        
        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        #region ISettings Member


        public void setArrayLength(int p)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                this.hasChanges = value;
            }
        }

        #endregion
    }
}