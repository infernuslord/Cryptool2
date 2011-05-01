using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;

namespace BitCommitmentScheme
{
    [Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.vs.uni-duisburg-essen.de")]
    [PluginInfo("BitCommitmentScheme.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "BitCommitmentScheme/Images/icon.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class BitCommitmentScheme : IAnalysisMisc
    {
        private readonly BitCommitmentSchemeSettings _settings = new BitCommitmentSchemeSettings();
        private string _logMessage;
        private ISAPCommitmentScheme.Wrapper _ISAPalgorithmWrapper = new ISAPCommitmentScheme.Wrapper();

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        [PropertyInfo(Direction.OutputData, "LogMessageCaption", "LogMessageTooltip", "")]
        public String LogMessage
        {
            get
            {
                return _logMessage;
            }
            set
            {
                _logMessage = value;
                OnPropertyChanged("LogMessage");
            }
        }

        [PropertyInfo(Direction.InputData, "InputBitCaption", "InputBitTooltip", "")]
        public bool InputBit
        {
            set
            {
                try
                {
                    LogMessage = _ISAPalgorithmWrapper.Run(value);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("ISAP algorithm failed: {0}", ex.Message), NotificationLevel.Error);
                }
                OnPropertyChanged("InputBit");
            }
        }

        public ISettings Settings
        {
            get { return _settings; }
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

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
