using System;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;

namespace Cryptool.CaesarAnalysisHelper
{
    class FrequencyObject
    {
        public char Char { get; set; }
        public int Frequency { get; set; }
    }

    [Author("Fabian Enkler", "", "", "")]
    [PluginInfo(false, "CaesarAnalysisHelper", "This plugin is designed in order to make a cryptanalysis of a caesarcipher based on the frequency test.", "", "CaesarAnalysisHelper/icon.png")]
    public class CaesarAnalysisHelper : IThroughput
    {
        private readonly CaesarAnalysisHelperSettings settings;
        private string inputList = string.Empty;
        private int key;

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public CaesarAnalysisHelper()
        {
            this.settings = new CaesarAnalysisHelperSettings();
        }

        public ISettings Settings
        {
            get { return settings; }
        } 

        [PropertyInfo(Direction.Input, "List Input", "This is the analysis input from the frequency test.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text,
            null)]
        public string InputList
        {
            get { return inputList; }
            set
            {
                inputList = value;
                OnPropertyChanged("InputList");
                ProcessList();
            }
        }

        [PropertyInfo(Direction.Output, "Key", "This is the estimated key.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public int Key
        {
            get
            {
                return key;
            }
        }

        private void ProcessList()
        {
            GuiNotification(inputList);
            if (!string.IsNullOrEmpty(inputList))
            {
                try
                {
                    int HighestCount = 0;
                    foreach (var s in inputList.Split(new[] { "\r\n" }, StringSplitOptions.None))
                    {
                        string[] tmpArr = s.Split(new[] { ':' });
                        if (tmpArr.Length > 0)
                        {
                            int Count;
                            int.TryParse(tmpArr[1], out Count);
                            if (Count > HighestCount)
                            {
                                HighestCount = Count;
                                key = tmpArr[0][0]-settings.FrequentChar;
                                GuiNotification(string.Format("New highest count: {0}", tmpArr[0][0]));
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    GuiNotification("Wrong input format!");
                }
                
            }
            OnPropertyChanged("Key");
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {

        }

        public void Execute()
        {

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

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void GuiNotification(string text)
        {
            GuiNotification(text,NotificationLevel.Debug);
        }

        private void GuiNotification(string text, NotificationLevel Level)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(text, this, Level));
        }
    }
}
