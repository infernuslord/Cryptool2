using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace RegularExpressions
{
    public class RegularExpressionReplaceSettings :ISettings
    {

        private bool hasChanges = false;

        #region taskpane
        
        private String patternValue;
        [TaskPane("RegexPattern", "Pattern to be replaced.", null, 0, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public String PatternValue
        {
            get { return this.patternValue; }
            set
            {
                if (value != this.patternValue)
                {
                    this.patternValue = value;
                    OnPropertyChanges("PatternValue");
                    HasChanges = true;
                }
            }
        }

        private String replaceValue;
        [TaskPane("Replacement", "Word to replace the pattern.", null, 0, false, ControlType.TextBox)]
        public String ReplaceValue
        {
            get { return this.replaceValue; }
            set
            {
                if (value != this.replaceValue)
                {
                    this.replaceValue = value;
                    OnPropertyChanges("ReplaceValue");
                    HasChanges = true;
                }
            }
        }

        private void OnPropertyChanges(string p)
        {
//dieses ding hier existiert nur weil ich den "stub erschaffen habe. aus testgründen
        }

        #endregion


        #region ISettings Member
       
        public bool HasChanges
        {
            get
            {
                return this.hasChanges;
                           }
            set
            {
                this.hasChanges = value;
                }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
