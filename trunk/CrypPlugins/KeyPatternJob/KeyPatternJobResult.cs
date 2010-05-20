/* Copyright 2010 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Miscellaneous;
using System.IO;
using System.Numerics;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public class KeyPatternJobResult : JobResult<LinkedList<KeySearcher.KeySearcher.ValueKey>>
    {
        private Encoding enc = Encoding.UTF8;

        public KeyPatternJobResult(byte[] serializedKeyPatternJobResult) : base(serializedKeyPatternJobResult)
        { }

        public KeyPatternJobResult(BigInteger jobId, LinkedList<KeySearcher.KeySearcher.ValueKey> resultList, 
            TimeSpan processingTime) 
            : this(jobId, resultList, processingTime, false) { }

        public KeyPatternJobResult(BigInteger jobId, LinkedList<KeySearcher.KeySearcher.ValueKey> resultList, 
            TimeSpan processingTime, bool isIntermediateResult)
            : base(jobId,resultList,processingTime, isIntermediateResult)
        {
            this.ResultList = resultList;
        }

        #region access methods on result elements

        LinkedList<KeySearcher.KeySearcher.ValueKey> resultList;
        public LinkedList<KeySearcher.KeySearcher.ValueKey> ResultList
        {
            get { return this.resultList; }
            set { this.resultList = value; }
        }

        public KeySearcher.KeySearcher.ValueKey GetElement(int position)
        {
            if (this.ResultList != null)
            {
                return this.resultList.ElementAt<KeySearcher.KeySearcher.ValueKey>(position);
            }
            throw (new Exception("ResultList is empty or the choosen position doesn't exists in this context"));
        }
        public KeySearcher.KeySearcher.ValueKey GetFirstElement()
        {
            if (this.ResultList != null)
            {
                return this.resultList.First.Value;
            }
            throw (new Exception("ResultList is empty or the first position doesn't exists in this context"));
        }


        #endregion

        #region Serialization methods - 

        /* serialization information: 3 fields per data set in the following order: 
         *  -------------------------------------------------------------------------------------------------------------------------------
         * | Dataset count | double-value (8 byte) | string len (1 byte) | string-data (n bytes) | byte len (1 byte) | byte-data (n bytes) |
         *  ------------------------------------------------------------------------------------------------------------------------------- */
        protected override byte[] SerializeResultType()
        {
            MemoryStream memStream = new MemoryStream();
            byte[] resultCountByte = BitConverter.GetBytes(resultList.Count);
            memStream.Write(resultCountByte, 0, resultCountByte.Length);

            foreach (KeySearcher.KeySearcher.ValueKey  valKey in resultList)
            {
                // serialize value (double)
                byte[] valueByte = BitConverter.GetBytes(valKey.value);
                byte[] valueByteLen = BitConverter.GetBytes(valueByte.Length);
                memStream.Write(valueByteLen, 0, valueByteLen.Length);
                memStream.Write(valueByte, 0, valueByte.Length);

                // serialize key (string)
                byte[] keyByte = enc.GetBytes(valKey.key);
                byte[] keyByteLen = BitConverter.GetBytes(keyByte.Length);
                memStream.Write(keyByteLen, 0, keyByteLen.Length);
                memStream.Write(keyByte, 0, keyByte.Length);

                byte[] decryptionByteLen;
                // serialize decryption (byte[]) - not more than 256 byte
                if (valKey.decryption.Length >= 255)
                {
                    decryptionByteLen = BitConverter.GetBytes(255);
                    memStream.Write(decryptionByteLen, 0, decryptionByteLen.Length);
                    memStream.Write(valKey.decryption, 0, 254);
                }
                else if (valKey.decryption.Length > 0 && valKey.decryption.Length < 255)
                {
                    decryptionByteLen = BitConverter.GetBytes(valKey.decryption.Length);
                    memStream.Write(decryptionByteLen, 0, decryptionByteLen.Length);
                    memStream.Write(valKey.decryption, 0, valKey.decryption.Length);
                }
                else // if no decryption result exists
                {
                    decryptionByteLen = BitConverter.GetBytes((int)0);
                    memStream.Write(decryptionByteLen, 0, decryptionByteLen.Length);
                }
            } // end foreach

            return memStream.ToArray();
        }

        protected override LinkedList<KeySearcher.KeySearcher.ValueKey> DeserializeResultType(byte[] serializedResultType)
        {
            LinkedList<KeySearcher.KeySearcher.ValueKey> lstRet = new LinkedList<KeySearcher.KeySearcher.ValueKey>();
            MemoryStream memStream = new MemoryStream(serializedResultType);

            // byte[] length of Int32
            int iInt32 = 4;

            int valueLen = 0;
            int keyLen = 0;
            int decryptionLen = 0;

            byte[] byteDatasetCount = new byte[iInt32];
            memStream.Read(byteDatasetCount, 0, byteDatasetCount.Length);
            int iDatasetCount = BitConverter.ToInt32(byteDatasetCount, 0);

            KeySearcher.KeySearcher.ValueKey valKey = new KeySearcher.KeySearcher.ValueKey();
            for (int i = 0; i < iDatasetCount; i++)
            {
                // deserialize value (double)
                byte[] valueByteLen = new byte[iInt32];
                memStream.Read(valueByteLen, 0, valueByteLen.Length);
                valueLen = BitConverter.ToInt32(valueByteLen, 0);
                byte[] valueByte = new byte[valueLen];
                memStream.Read(valueByte, 0, valueByte.Length);
                
                valKey.value = BitConverter.ToDouble(valueByte, 0);

                // deserialize key (string)
                byte[] keyByteLen = new byte[iInt32];
                memStream.Read(keyByteLen, 0, keyByteLen.Length);
                keyLen = BitConverter.ToInt32(keyByteLen, 0);
                byte[] keyByte = new byte[keyLen];
                memStream.Read(keyByte, 0, keyByte.Length);

                valKey.key = enc.GetString(keyByte, 0, keyByte.Length);

                // deserialize decryption (byte[])
                byte[] decryptByteLen = new byte[iInt32];
                memStream.Read(decryptByteLen, 0, decryptByteLen.Length);
                decryptionLen = BitConverter.ToInt32(decryptByteLen, 0);
                byte[] decryptionByte = new byte[decryptionLen];
                memStream.Read(decryptionByte, 0, decryptionByte.Length);

                valKey.decryption = decryptionByte;

                // add new, deserialize ValueKey to LinkedList
                lstRet.AddLast(valKey);
            }
            if ((memStream.Length - memStream.Position) > 0)
            {
                throw(new Exception("The serializedResultType wasn't deserialized correctly, because " 
                    + "after deserializing there are already some bytes (remaining Length of byte[]: " 
                    + memStream.Length + ")"));
            }
            this.ResultList = lstRet;

            return lstRet;
        }

        #endregion
    }
}
