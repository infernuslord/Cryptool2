using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.Substring
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Substring", "Generating Substring", "", "Substring/icon.png")]
    
    class Substring : IThroughput
    {
        #region IPlugin Members

        private SubstringSettings settings = new SubstringSettings();
        private String inputString = "";
        private int inputPos = 0;
        private int inputLength = 0;
        private String outputString = "";

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SubstringSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            
        }

        public void Execute()
        {
            if (inputString != null)
            {
                outputString = inputString.Substring(inputPos, inputLength);
            }
            
        }

        public void PostExecution()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region SubstringInOut

        [PropertyInfo(Direction.InputData, "String Input", "Input your String here", "", DisplayLevel.Beginner)]
        public String InputString
        {
            get
            {
                return inputString;
            }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "Position Input", "Input your Position here", "", DisplayLevel.Beginner)]
        public int InputPos
        {
            get
            {
                return inputPos;
            }
            set
            {
                this.inputPos = value;
                OnPropertyChanged("InputPosition");
            }
        }

        [PropertyInfo(Direction.InputData, "Length Input", "Input your Length here", "", DisplayLevel.Beginner)]
        public int InputLength
        {
            get
            {
                return inputLength;
            }
            set
            {
                this.inputLength = value;
                OnPropertyChanged("InputLength");
            }
        }

        [PropertyInfo(Direction.OutputData, "String Output", "Your Substring will be send here", "", DisplayLevel.Beginner)]
        public String OutputString
        {
            get
            {
                return outputString;
            }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members



        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        

        #endregion

        
    }
}
