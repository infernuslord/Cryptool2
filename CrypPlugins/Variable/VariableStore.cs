using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.Collections;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.Variable
{
    delegate void StoreVariable(string variable, object input);
    
    [Author("Sven Rech", "sven.rech@cryptool.com", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "VariableStore", "Variable Store", null, "Variable/storeIcon.png")]
    class VariableStore : IOutput
    {
        static public event StoreVariable OnVariableStore;

        #region Private variables
        private VariableSettings settings;        
        private List<CryptoolStream> listCryptoolStreamOut = new List<CryptoolStream>();        
        #endregion

        public VariableStore()
        {
            this.settings = new VariableSettings();
        }

        public ISettings Settings
        {
            get { return settings; }
            set { settings = (VariableSettings)value; }
        }

        #region Properties
        
        private Object storeObject = null;
        [PropertyInfo(Direction.InputData, "Variable Store Object", "Object to be stored to the corresponding variable", "", DisplayLevel.Beginner)]
        public Object VariableStoreObject
        {
            get
            {
                return storeObject;
            }
            set
            {
                storeObject = value;
                OnPropertyChanged("VariableStoreObject");
            }
        }
        
        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

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
            if (settings.VariableName == "" || storeObject == null)
                return;

            ProgressChanged(0.5, 1.0);
            if (storeObject is CryptoolStream)
            {
                CryptoolStream cs = new CryptoolStream();
                cs.OpenRead((storeObject as CryptoolStream).FileName);
                listCryptoolStreamOut.Add(cs);
                if (OnVariableStore != null)
                    OnVariableStore(settings.VariableName, cs);
            }
            else
            {
                if (OnVariableStore != null)
                    OnVariableStore(settings.VariableName, storeObject);
            }
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
            foreach (CryptoolStream cs in listCryptoolStreamOut)
                cs.Close();
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
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
