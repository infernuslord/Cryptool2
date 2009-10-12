using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.ArrayIndexer
{
    [Author("Christian Arnold", "christian.arnold@stud.uni-due.de", "Uni Duisburg-Essen", "")]
    [PluginInfo(true, "ArrayIndexer", "Content of the chosen index of the array", "", "ArrayIndexer/arrayindexer.png")]

    public class ArrayIndexer : IThroughput
    {
        #region IPlugin Members

        private ArrayIndexerSettings settings = new ArrayIndexerSettings();

        private Array objInput = null;
        private int arrayIndex = 0;
        private object objOutput = null;

        #region In and Out properties

        [PropertyInfo(Direction.InputData, "Array Input", "The input object has to be an array type", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public Array ObjInput
        {
            get
            {
                return objInput;
            }
            set
            {
                objInput = value;
                OnPropertyChanged("ObjInput");
            }
        }

        [PropertyInfo(Direction.InputData, "Index of Array", "Indexes of an array begin always with 0. Example: If you have an array of the length 8, you can index the values 0 to 7", "", DisplayLevel.Beginner)]
        public int ArrayIndex
        {
            get
            {
                return this.arrayIndex;
            }
            set
            {
                this.arrayIndex = value;
                settings.ArrayIndex = value;
                OnPropertyChanged("ArrayIndex");
            }
        }

        [PropertyInfo(Direction.OutputData, "Content of the chosen index of the array", "Content with the array-specific data type", "", DisplayLevel.Beginner)]
        public object ObjOutput
        {
            get
            {
                return this.objOutput;
            }
            set
            {
                this.objOutput = value;
                OnPropertyChanged("ObjOutput");
            }
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (ArrayIndexerSettings)value; }
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
            if (ObjInput != null)
            {
                // error case, if array index is greater than the length of the array
                if (ObjInput.Length <= settings.ArrayIndex)
                {
                    GuiLogMessage("Array Index is greater than the length of the array", NotificationLevel.Error);
                    return;
                }

                if (ObjInput is string[])
                {
                    ObjOutput = ((string[])ObjInput)[settings.ArrayIndex];
                }
                else if (ObjInput is byte[])
                {
                    ObjOutput = ((byte[])ObjInput)[settings.ArrayIndex];
                }
                else if (ObjInput is int[])
                {
                    ObjOutput = ((int[])ObjInput)[settings.ArrayIndex];
                }
                else
                {
                    GuiLogMessage("Type of array not supported", NotificationLevel.Error);
                    return;
                }
                GuiLogMessage("Array type is " + ObjInput.GetType().ToString() + " with value: " + ObjOutput.ToString(), NotificationLevel.Info);
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

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

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
