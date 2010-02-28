using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
    public interface IControlPublicKey:IControl

    {
        string publicKey { get; set; }
        void setPublicKey(string publicKey);

    }
}
