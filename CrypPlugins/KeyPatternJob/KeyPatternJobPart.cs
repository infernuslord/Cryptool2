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
using Cryptool.Plugins.PeerToPeer.Jobs;
using KeySearcher;
using System.IO;
using Cryptool.PluginBase.Miscellaneous;

/*
 * TODO:
 * - At present to similar methods (byte[] constructor and Deserialize)
 */

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public class KeyPatternJobPart : IJobPart
    {
        #region Properties and Variables

        private BigInteger jobId;
        public BigInteger JobId 
        {
            get { return this.jobId ; }
            set { this.jobId = value; }
        }

        private KeyPattern pattern;
        public KeyPattern Pattern 
        {
            get { return this.pattern; }
            private set 
            {
                if (KeyPattern.testPattern(value.GetPattern()))
                    this.pattern = value;
                else
                    throw (new Exception("KeyPattern isn't valid"));
            }
        }
        private byte[] encryptData;
        public byte[] EncryptData 
        {
            get { return this.encryptData;}
            private set { this.encryptData = value; }
        }
        private byte[] initVector;
        public byte[] InitVector 
        {
            get { return this.initVector; }
            private set { this.initVector = value; }
        }

        #endregion

        /// <summary>
        /// standard constructor
        /// </summary>
        /// <param name="jobId">a unique jobId</param>
        /// <param name="pattern">a valid KeyPattern</param>
        /// <param name="encryptData">serialized encryptedData</param>
        /// <param name="initVector">serialized initVector</param>
        public KeyPatternJobPart(BigInteger jobId, KeyPattern pattern, byte[] encryptData, byte[] initVector)
        {
            this.JobId = jobId;
            this.Pattern = pattern;
            this.EncryptData = encryptData;
            this.InitVector = initVector;
        }

        public KeyPatternJobPart(byte[] serializedKeyPatternJobPart)
        {
            /* So i always have the same byte length for int32 values */
            int iTest = 500;
            int int32ByteLen = BitConverter.GetBytes(iTest).Length;

            MemoryStream memStream = new MemoryStream(serializedKeyPatternJobPart);
            try
            {
                byte[] intBuffer = new byte[int32ByteLen];
                // deserialize JobId
                memStream.Read(intBuffer, 0, intBuffer.Length);
                byte[] jobIdByte = new byte[BitConverter.ToInt32(intBuffer, 0)];
                memStream.Read(jobIdByte, 0, jobIdByte.Length);
                BigInteger temp_jobId = new BigInteger(jobIdByte, jobIdByte.Length);
                
                // deserialize KeyPattern
                memStream.Read(intBuffer, 0, intBuffer.Length);
                int byteLen = BitConverter.ToInt32(intBuffer, 0);
                byte[] temp_serializedPattern = new byte[byteLen];
                memStream.Read(temp_serializedPattern, 0, temp_serializedPattern.Length);
                KeyPattern temp_pattern = new KeyPattern(temp_serializedPattern);

                // deserialize EncryptData
                memStream.Read(intBuffer, 0, intBuffer.Length);
                byteLen = BitConverter.ToInt32(intBuffer, 0);
                byte[] temp_encryptedData = new byte[byteLen];
                memStream.Read(temp_encryptedData, 0, temp_encryptedData.Length);

                // deserialize InitVector
                memStream.Read(intBuffer, 0, intBuffer.Length);
                byteLen = BitConverter.ToInt32(intBuffer, 0);
                byte[] temp_initVector = new byte[byteLen];
                memStream.Read(temp_initVector, 0, temp_initVector.Length);

                //return new KeyPatternJobPart(temp_jobId, temp_pattern, temp_encryptedData, temp_initVector);
                this.JobId = temp_jobId;
                this.Pattern = temp_pattern;
                this.EncryptData = temp_encryptedData;
                this.InitVector = temp_initVector;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                memStream.Flush();
                memStream.Close();
                memStream.Dispose();
            }
        }

        #region IJobPart<KeyPattern> Members

        public KeyPatternJobPart Job()
        {
            return this;
        }

        public bool IsDivisable()
        {
            throw new NotImplementedException();
        }

        /* 4 Bytes: Length of serializedPattern
         * x Bytes: serialized Pattern
         * 4 Bytes: Length of encrypted Data
         * x Bytes: encrypted Data
         * 4 Bytes: Length of initialization value
         * x Bytes: initialization value */
        public byte[] Serialize()
        {
            MemoryStream memStream = new MemoryStream();
            try
            {
                // serialize JobId
                byte[] byteJobId = this.JobId.getBytes();
                byte[] jobIdLen = BitConverter.GetBytes(byteJobId.Length);
                memStream.Write(jobIdLen, 0, jobIdLen.Length);
                memStream.Write(byteJobId, 0, byteJobId.Length);

                // serialize KeyPattern
                byte[] serializedPattern = this.Pattern.Serialize();
                byte[] serializedPatternLen = BitConverter.GetBytes(serializedPattern.Length);
                memStream.Write(serializedPatternLen, 0, serializedPatternLen.Length);
                memStream.Write(serializedPattern, 0, serializedPattern.Length);

                // serialize EncryptData
                byte[] encryptDataLen = BitConverter.GetBytes(this.EncryptData.Length);
                memStream.Write(encryptDataLen, 0, encryptDataLen.Length);
                memStream.Write(this.EncryptData, 0, this.EncryptData.Length);

                // serialize InitVector
                byte[] initValueLen = BitConverter.GetBytes(this.InitVector.Length);
                memStream.Write(initValueLen, 0, initValueLen.Length);
                memStream.Write(this.InitVector, 0, this.InitVector.Length);

                return memStream.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                memStream.Flush();
                memStream.Close();
                memStream.Dispose();
            }
            return null;
        }

        /* This method have to be static, so I can call it without creating an "test instance" before */
        /// <summary>
        /// Deserializes a valid byte[] representation of the KeyPatternJobPart class
        /// </summary>
        /// <param name="serializedKeyPatternJobPart">a valid byte[] representation of the KeyPatternJobPart class</param>
        /// <returns>a new KeyPatternJobPart instance</returns>
        public static KeyPatternJobPart Deserialize(byte[] serializedKeyPatternJobPart)
        {
            return new KeyPatternJobPart(serializedKeyPatternJobPart);
        }

        ///* 4 Bytes: Length of serializedPattern
        // * x Bytes: serialized Pattern
        // * 4 Bytes: Length of encrypted Data
        // * x Bytes: encrypted Data
        // * 4 Bytes: Length of initialization value
        // * x Bytes: initialization value */
        //public KeyPatternJobPart Deserialize(byte[] serializedJobPart)
        //{
        //    MemoryStream memStream = new MemoryStream(serializedJobPart);
        //    try
        //    {
        //        byte[] intBuffer = new byte[int32ByteLen];
        //        // deserialize JobId
        //        memStream.Read(intBuffer, 0, intBuffer.Length);
        //        int temp_jobId = BitConverter.ToInt32(intBuffer);

        //        // deserialize KeyPattern
        //        memStream.Read(intBuffer, 0, intBuffer.Length);
        //        int byteLen = BitConverter.ToInt32(intBuffer, 0);
        //        byte[] temp_serializedPattern = new byte[byteLen];
        //        memStream.Read(temp_serializedPattern, 0, temp_serializedPattern.Length);
        //        KeyPattern temp_pattern = this.Pattern.Deserialize(temp_serializedPattern);

        //        // deserialize EncryptData
        //        memStream.Read(intBuffer, 0, intBuffer.Length);
        //        byteLen = BitConverter.ToInt32(intBuffer, 0);
        //        byte[] temp_encryptedData = new byte[byteLen];
        //        memStream.Read(temp_encryptedData, 0, temp_encryptedData.Length);

        //        // deserialize InitVector
        //        memStream.Read(intBuffer, 0, intBuffer.Length);
        //        byteLen = BitConverter.ToInt32(intBuffer, 0);
        //        byte[] temp_initVector = new byte[byteLen];
        //        memStream.Read(temp_initVector, 0, temp_initVector.Length);

        //        return new KeyPatternJobPart(temp_jobId, temp_pattern, temp_encryptedData, temp_initVector);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        memStream.Flush();
        //        memStream.Close();
        //        memStream.Dispose();
        //    }
        //    return null;
        //}

        #endregion
    }
}
