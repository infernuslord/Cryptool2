using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace PKCS1.Library
{
    class GuiLogMsgHandOff
    {
        #region singleton
        private static GuiLogMsgHandOff instance = null;

        private GuiLogMsgHandOff() { }

        public static GuiLogMsgHandOff getInstance()
        {
            if (null == instance)
            {
                instance = new GuiLogMsgHandOff();
            }
            return instance;
        }
        #endregion

        public event GuiLogHandler OnGuiLogMsgSend;

        // Klassen, welche GuiLogMessages schicken wollen, müssen hier ihr GuiLogHandler reingeben
        public void registerAt(ref GuiLogHandler guiLogEvent)
        {
            guiLogEvent += handleGuiLogMsgSent;
        }

        // hier wird die Msg weitergereicht
        private void handleGuiLogMsgSent(string message, NotificationLevel logLevel)
        {
            this.SendGuiLogMsg(message, logLevel); // weiterreichen
        }

        private void SendGuiLogMsg(string message, NotificationLevel logLevel)
        {
            if (null != OnGuiLogMsgSend)
            {
                OnGuiLogMsgSend(message, logLevel);
            }
        }
    }
}
