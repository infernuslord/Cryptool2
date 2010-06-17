using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using PKCS1.WpfVisualization;
using PKCS1.Library;

namespace PKCS1
{
    [Author("Jens Schomburg", "mail@escobar.de", "Universität Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "PKCS #1 / Bleichenbacher Angriff", "PKCS1/Bleichenbacher Angriff", "PKCS1/DetailedDescription/Description.xaml", "PKCS1/PKCS1.png")]
    //[PluginInfo(false, "PKCS #1 / Bleichenbacher Angriff", "PKCS #1 / Bleichenbacher Angriff", "MD5/DetailedDescription/Description.xaml", "PKCS1/PKCS1.png")]
    //[PluginInfo(Cryptool.PKCS1., false, "PKCS #1 / Bleichenbacher Angriff", "PKCS #1 / Bleichenbacher Angriff", "", "PKCS1/PKCS1.png")] 

    public class PKCS1Plugin : ITool
    {
        private Pkcs1Control m_Pkcs1Plugin = null;
        private PKCS1Settings settings;

        public PKCS1Plugin()
        {
            this.settings = new PKCS1Settings();
            GuiLogMsgHandOff.getInstance().OnGuiLogMsgSend += GuiLogMessage; // bei weiterleitung registrieren
        }

        #region EventHandler

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        #region IPlugin Member

        public void Dispose()
        {
            //throw new NotImplementedException();
            //TODO: dispose von erstelltem UserControl aufrufen!?
            if (m_Pkcs1Plugin == null) m_Pkcs1Plugin.Dispose();
        }

        public void Execute()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void PreExecution()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get 
            {
                if (m_Pkcs1Plugin == null) m_Pkcs1Plugin = new Pkcs1Control();
                return m_Pkcs1Plugin;
            }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (PKCS1Settings)value; }
        }

        #endregion
    }
}
