using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

// Currently this PlugIn accepts every Array-Type, string and byte. In other cases a GuiMessage.Error is thrown.
namespace Cryptool.Plugins.LengthOf
{
    [Author("Christian Arnold", "christian.arnold@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "LengthOf", "Operates the Length of an input", "", "LengthOf/LenOf.png")]
    public class LengthOf : IThroughput
    {
        private LengthOfSettings settings = new LengthOfSettings();

        private object objInput = null;
        private int outputLen = 0;


        #region IPlugin Members

        public void Execute()
        {
            if (ObjInput != null)
            {
                Type typeInput = ObjInput.GetType();
                if (typeInput.IsArray)
                {
                    OutputLen = (ObjInput as Array).Length;
                    GuiLogMessage("Object is an array. Length: " + OutputLen, NotificationLevel.Info);
                }
                else //no array
                {
                    if (ObjInput is string)
                    {
                        string sInput = objInput as String;
                        OutputLen = sInput.Length;
                    }
                    else if (ObjInput is byte)
                    {
                        string sInput2 = ((byte)objInput).ToString();
                        OutputLen = sInput2.Length;
                    }
                    else
                    {
                        GuiLogMessage("Inputtype not handled!", NotificationLevel.Error);
                    }
                    //switch (typeInput.ToString())
                    //{
                    //    case "System.String":
                    //        string sInput = objInput as String;
                    //        OutputLen = sInput.Length;
                    //        break;
                    //    case "System.Byte":
                    //        string sInput2 = ((byte)objInput).ToString();
                    //        OutputLen = sInput2.Length;
                    //        break;
                    //    default:
                    //        GuiLogMessage("Inputtype not handled!", NotificationLevel.Error);
                    //        //throw new Exception("Inputtype not handled!");
                    //        break;
                    //}
                    GuiLogMessage("Object isn't an array. Length: " + OutputLen, NotificationLevel.Info);
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Object Input", "Input your Object here", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object ObjInput
        {
            get
            {
                return objInput;
            }
            set
            {
                this.objInput = value;
                OnPropertyChanged("ObjInput");
            }
        }

        [PropertyInfo(Direction.OutputData, "Integer Output", "The Length of your Object will be send here", "", DisplayLevel.Beginner)]
        public int OutputLen
        {
            get
            {
                return outputLen;
            }
            set
            {
                this.outputLen = value;
                OnPropertyChanged("OutputLen");
            }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Pause()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (LengthOfSettings)value; }
        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }
        #endregion
    }
}
