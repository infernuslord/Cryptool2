using Cryptool.PluginBase;
using LatticeCrypto.Views;

namespace LatticeCrypto
{
    [Author("Eric Schmeck", "eric.schmeck@gmx.de", "Universität Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("LatticeCrypto.Properties.Languages", "PluginCaption", "PluginTooltip", "LatticeCrypto/DetailedDescription/doc.xml", "LatticeCrypto/LatticeCryptoPlugin.png")]
    
    public class LatticeCryptoPlugin : ICrypTutorial
    {
        private LatticeCryptoMain latticeCryptoMain;

        //public LatticeCryptoPlugin()
        //{
        //    //GuiLogMsgHandOff.getInstance().OnGuiLogMsgSend += GuiLogMessage; // bei weiterleitung registrieren
        //}

        #region EventHandler

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        //private void GuiLogMessage(string message, NotificationLevel logLevel)
        //{            
        //    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, message, logLevel); 
        //}

        #region IPlugin Member

        public void Dispose()
        {
            //if (latticeCryptoPlugin != null)
            //    latticeCryptoPlugin.Dispose();
        }

        public void Execute()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return latticeCryptoMain ?? (latticeCryptoMain = new LatticeCryptoMain()); }
        }

        public ISettings Settings
        {
            get { return null; }
        }

        #endregion
    }
}
