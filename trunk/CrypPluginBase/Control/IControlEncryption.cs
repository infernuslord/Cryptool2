/*
   Copyright 2009 Thomas Schmid

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
    public interface IControlEncryption : IControl, IDisposable
    {
        byte[] Encrypt(byte[] key, int blocksize);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV);
        string getKeyPattern();
        byte[] getKeyFromString(string key);
        void changeSettings(string setting, object value);
        IControlEncryption clone();
        event KeyPatternChanged keyPatternChanged;
    }
}
