using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace PKCS1.Library
{
    interface IGuiLogMsg
    {
        event GuiLogHandler OnGuiLogMsgSend;

        void registerHandOff();

        void SendGuiLogMsg(string message, NotificationLevel logLevel);
    }
}
