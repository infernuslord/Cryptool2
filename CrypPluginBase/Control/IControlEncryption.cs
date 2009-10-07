using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
    public interface IControlEncryption : IControl
    {
        byte[] Encrypt(byte[] key);
        byte[] Decrypt(byte[] key);
        string getKeyPattern();
        byte[] getKeyFromString(string key);
    }
}
