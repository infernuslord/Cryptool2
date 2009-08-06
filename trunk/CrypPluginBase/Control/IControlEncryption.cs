using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
  public interface IControlEncryption : IControl
  {
    byte[] Encrypt(byte[] key, byte[] data, byte[] iv);
    byte[] Decrypt(byte[] key, byte[] data, byte[] iv);
  }
}
