using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.BigNumber
{
    [Author("Sven Rech", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "BigNumberOperation", "Big Number Operation", null, "BigNumber/operationIcon.png")]
    class BigNumberOperation : IThroughput
    {
        #region Properties

        private BigInteger input1 = null;
        [PropertyInfo(Direction.InputData, "Input1", "Number Input 1", "", DisplayLevel.Beginner)]
        public BigInteger Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;
                OnPropertyChanged("Input1");
            }
        }

        private BigInteger input2 = null;
        [PropertyInfo(Direction.InputData, "Input2", "Number Input 2", "", DisplayLevel.Beginner)]
        public BigInteger Input2
        {
            get
            {
                return input2;
            }
            set
            {
                input2 = value;
                OnPropertyChanged("Input2");
            }
        }

        private BigInteger output = null;
        [PropertyInfo(Direction.OutputData, "Output", "Number Output", "", DisplayLevel.Beginner)]
        public BigInteger Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                OnPropertyChanged("Output");
                OnPropertyChanged("StringOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "StringOutput", "Number String Output", "", DisplayLevel.Beginner)]
        public string StringOutput
        {
            get
            {
                if (output is object)
                    return output.ToString();
                else
                    return "";
            }
            set {}
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

        private BigNumberOperationSettings settings = new BigNumberOperationSettings();
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (BigNumberOperationSettings)value; }
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
            if (input1 is object && input2 is object)
            {
                ProgressChanged(0.5, 1.0);
                try
                {
                    Output = Input1 * Input2;
                }
                catch (Exception)
                {

                    GuiLogMessage("Error multiplying big numbers.", NotificationLevel.Error);
                    return;
                }
                ProgressChanged(1.0, 1.0);
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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
