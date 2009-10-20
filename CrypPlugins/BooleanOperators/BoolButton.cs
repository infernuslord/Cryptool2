using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;

using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.BooleanOperators;
using Cryptool.PluginBase.IO;

using BooleanOperators;





namespace Cryptool.Plugins.BoolButton
{
    [Author("Julian Weyers", "julian.weyers@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(true, "Boolean Button", "Boolean Button", "BooleanOperators/DetailedDescription/Description.xaml", "BooleanOperators/icons/false.png", "BooleanOperators/icons/true.png")]

    public class BoolButton : IInput
    {
        private BoolButtonSettings settings;
        private Boolean output = true;
        private ButtonInputPresentation myButton;
        
        public BoolButton()
        {
            this.settings = new BoolButtonSettings();            
           // this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
            this.settings.PropertyChanged += settings_PropertyChanged;
            myButton = new ButtonInputPresentation();
            Presentation = myButton;
            this.settings.PropertyChanged += myButton.ExecuteThisMethodWhenButtonIsClicked;
       }

       void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (myButton.c == true)
            {
                Output = false;
                // as the setting is not changeable in play mode, there is no need to update Output property
                //Output = (settings.Value == 1);

                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
            }
            else 
            {
                Output = true;
                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        
        public Boolean Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChange("Output");
            }
        }

        #region IPlugin Member
        
        

        public void Dispose()
        {
            //throw new NotImplementedException();  
        }

        public void Execute()
        {
            
            Output = true;
            if (myButton.c == true)
            {
                Output = false;
                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            }
            if (myButton.c == false)
            {
                Output = true;
                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
            }
            //Output = (settings.Value == 1);
            
            //settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));

            ProgressChanged(1, 1);
        }

        public void Initialize()
        {
            Output = true;
            if (myButton.c == true)
            {
                Output = false;
            }
            if (myButton.c == false)
            {
                Output = true;
            }
            // not working, see ticket #80
           // settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));

        }

        public void Pause()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public UserControl Presentation 
        { 
            get; 
            private set; 
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            
            get { return Presentation; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BoolButtonSettings)value; }
        }

        public void Stop()
        {
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
       
    }
}
