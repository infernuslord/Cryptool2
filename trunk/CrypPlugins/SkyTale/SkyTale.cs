using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.SkyTale
{
	class SkyTale : IEncryption
	{
	    private SkyTaleSettings settings;
        private string inputString;
        private string outputString;

	    public event PropertyChangedEventHandler PropertyChanged;
	    public event StatusChangedEventHandler OnPluginStatusChanged;
	    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
	    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public SkyTale()
        {
            this.settings = new SkyTaleSettings();
        }

        [PropertyInfo(Direction.Input, "Text input", "Input a string to be processed by the SkyTale cipher", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != inputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        [PropertyInfo(Direction.Output, "Text output", "The string after processing with the SkyTale cipher", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

	    public void PreExecution()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void Execute()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void PostExecution()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void Pause()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void Stop()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void Initialize()
	    {
	        throw new System.NotImplementedException();
	    }

	    public void Dispose()
	    {
	        throw new System.NotImplementedException();
	    }

	    public ISettings Settings
	    {
            get { return this.settings; }
            set { this.settings = (SkyTaleSettings) value; }
	    }

	    public UserControl Presentation
	    {
	        get { throw new System.NotImplementedException(); }
	    }

	    public UserControl QuickWatchPresentation
	    {
	        get { throw new System.NotImplementedException(); }
	    }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
	}
}
