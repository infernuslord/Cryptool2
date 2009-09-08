using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.BigNumber
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "BigNumberInput", "Big Number Input", null, "BigNumber/inputIcon.png")]
    class BigNumberInput : IInput
    {

        public BigNumberInput()
        {
            settings = new BigNumberInputSettings();
        }

        #region Properties

        private BigInteger numberOutput = null;
        [PropertyInfo(Direction.OutputData, "Number Output", "Number Output", "", DisplayLevel.Beginner)]
        public BigInteger NumberOutput
        {
            get
            {
                return numberOutput;
            }
            set
            {
                numberOutput = value;
                OnPropertyChanged("NumberOutput");
                OnPropertyChanged("StringOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "String Output", "String Output", "", DisplayLevel.Beginner)]
        public String StringOutput
        {
            get
            {
                if (numberOutput is object)
                    return numberOutput.ToString();
                else
                    return "";
            }
            set {} //readonly
        }
        #endregion

        #region IPlugin Members

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private BigNumberInputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (BigNumberInputSettings)value; }
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
            Dispose();
        }

        public void Execute()
        {
            BigInteger bi;
            try
            {
                bi = new BigInteger(settings.Number, 10);
            }
            catch (Exception)
            {
                GuiLogMessage("Invalid input", NotificationLevel.Error);
                return;
            }
            NumberOutput = bi;
            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {
            Dispose();
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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
