using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{    
    public interface IControlEncryption : IControl
    {
        byte[] Encrypt(byte[] key, int blocksize);
        byte[] Decrypt(byte[] key, int blocksize);
        string getKeyPattern();
        byte[] getKeyFromString(string key);
        event KeyPatternChanged keyPatternChanged;
    }
}
