using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;

namespace Gate
{
    [Author("Matthäus Wander", "wander@cryptool.org", "Universität Duisburg-Essen, Fachgebiet Verteilte Systeme", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "Gate", "Control operator", null, "Gate/default_icon.png")]
    public class Gate : IThroughput
    {
        private GateSettings settings = new GateSettings();
        private object input;
        private object output;
        private bool oldControl = false;
        private bool control = false;

        private bool locked = false;

        [PropertyInfo(Direction.InputData, "Input", "Input object of any type", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object InputObject
        {
            get
            {
                return input;
            }
            set
            {
                input = value;
                OnPropertyChanged("InputObject");
            }
        }

        [PropertyInfo(Direction.InputData, "Control", "Controls whether to open gate", null, false, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        public bool Control
        {
            get
            {
                return control;
            }
            set
            {
                oldControl = control;
                locked = false;

                control = value;
                OnPropertyChanged("Control");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output object", null, DisplayLevel.Beginner)]
        public object OutputObject
        {
            get
            {
                return output;
            }
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (GateSettings)value;
            }
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
            if (shallFire())
            {
                output = input;
                locked = true;
                ProgressChanged(1, 1);
                OnPropertyChanged("OutputObject");
            }
            else
            {
                output = null;
                ProgressChanged(0.5, 1);
            }
        }

        private bool shallFire()
        {
            switch (settings.Trigger)
            {
                case Trigger.AlwaysOpen:
                    return true;
                case Trigger.AlwaysClosed:
                    return false;
                case Trigger.TrueValue:
                    return !locked && control;
                case Trigger.AnyEdge:
                    return !locked && control != oldControl;
                case Trigger.PositiveEdge:
                    return !locked && !oldControl && control;
                case Trigger.NegativeEdge:
                    return !locked && oldControl && !control;
                default:
                    return false;
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

        #endregion

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}
