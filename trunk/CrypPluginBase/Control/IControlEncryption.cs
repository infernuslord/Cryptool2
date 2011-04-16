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
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse);

        /// <summary>
        /// Returns the number of bytes of a block that is fixed for the cipher or being currently configured.
        /// </summary>
        /// <returns></returns>
        int GetBlockSize();

        /// <summary>
        /// Returns the pattern that the corresponding encryption plugin expects for the abstract key.
        /// </summary>
        /// <returns>The pattern</returns>
        string GetKeyPattern();

        /// <summary>
        /// Returns the KeyTranslator which can be used to map abstract keys to concrete key for this encryption plugin.
        /// </summary>
        /// <returns>An implementation of IKeyTranslator.</returns>
        IKeyTranslator GetKeyTranslator();

        /// <summary>
        /// Returns OpenCL code for this encryption plugin.
        /// </summary>
        /// <param name="decryptionLength">Indicates how many bytes should be decrypted. Important for speed.</param>
        /// <param name="iv">The IV vector (for CBC)</param>
        /// <returns>The OpenCL code.</returns>
        string GetOpenCLCode(int decryptionLength, byte[] iv);

        void changeSettings(string setting, object value);
        IControlEncryption clone();
        event KeyPatternChanged keyPatternChanged;
    }
}
