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

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    /// <summary>
    /// use this ENUM only in the context with JobMessages!
    /// </summary>
    public enum MessageJobType
    {
        /// <summary>
        /// indicates that this message contains a JobAccepted/JobDeclined Information
        /// </summary>
        JobAcceptanceInfo = 200,
        /// <summary>
        /// indicates that this message contains a JobResult
        /// </summary>
        JobResult = 201,
        /// <summary>
        /// indicates, that it has capacities to process a new job (use it only 
        /// for messages FROM the P2PJobAdmin TO the Manager)
        /// </summary>
        Free = 202,
        /// <summary>
        /// indicates that this message contains a JobPart (use it only for
        /// messages FROM the Manager TO the P2PJobAdmin)
        /// </summary>
        JobPart = 203
    }

    public static class JobMessages
    {
        #region Detection and Type-Request of JobMessages

        /// <summary>
        /// returns true, when the first byte indicates, that this is a JobMessage
        /// </summary>
        /// <param name="firstByte">first byte of a message</param>
        /// <returns></returns>
        public static bool IsJobMessageType(byte firstByte)
        {
            foreach (MessageJobType msgJobType in Enum.GetValues(typeof(MessageJobType)))
            {
                if ((MessageJobType)firstByte == msgJobType)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// use this method only, when you are sure, that the first byte indicates
        /// a JobMessage (use IsJobMessageType first), then it will return the 
        /// special MessageJobType
        /// </summary>
        /// <param name="firstByte">First byte of the message</param>
        /// <returns></returns>
        public static MessageJobType GetMessageJobType(byte firstByte)
        {
            foreach (MessageJobType msgJobType in Enum.GetValues(typeof(MessageJobType)))
            {
                if ((MessageJobType)firstByte == msgJobType)
                    return msgJobType;
            }
            throw (new Exception("No other JobMessageTypes are available!"));
        }

        #endregion

        /// <summary>
        /// integrates the serialized JobPart into the DistributableJob-specific
        /// communication protocol - returns a completely compatible DistributableJob-Message
        /// </summary>
        /// <param name="jobId">the jobId of the actual JobPart</param>
        /// <param name="data">the already serialized JobPart-data</param>
        /// <returns></returns>
        public static byte[] CreateJobPartMessage(BigInteger jobId, byte[] data)
        {
            byte[] jobIdBytes = SerializeJobId(jobId);

            byte[] result = new byte[jobIdBytes.Length + data.Length + 1];
            result[0] = (byte)MessageJobType.JobPart;

            Buffer.BlockCopy(jobIdBytes, 0, result, 1, jobIdBytes.Length);
            Buffer.BlockCopy(data, 0, result, jobIdBytes.Length + 1, data.Length);

            return result;
        }

        /// <summary>
        /// Fetches the serialized JobPart data and the JobId of the JobPart out of 
        /// the DistributableJob-specific message. If the message doesn't fit to the
        /// communication protocol, this method returns null, otherwise the raw serialized
        /// JobPart data!
        /// </summary>
        /// <param name="data">DistributableJob-specific message, which was proved that it is a JobPartMessage before</param>
        /// <param name="jobId">catches the JobId out of the raw byte data and returns it, too</param>
        /// <returns></returns>
        public static byte[] GetJobPartMessage(byte[] data, out BigInteger jobId)
        {
            byte[] serializedJobPart = null;
            jobId = null;
            if ((MessageJobType)data[0] == MessageJobType.JobPart)
            {
                byte[] tailBytes = new byte[data.Length - 1];

                Buffer.BlockCopy(data, 1, tailBytes, 0, tailBytes.Length);
                int bytesLeft;
                jobId = DeserializeJobId(tailBytes, out bytesLeft);

                serializedJobPart = new byte[bytesLeft];
                Buffer.BlockCopy(tailBytes, tailBytes.Length - bytesLeft, serializedJobPart, 0, serializedJobPart.Length);
            }
            return serializedJobPart;
        }

        /* Byte representation of a Job Result Message:
         * 1 Byte:  MessageJobType
         * n Bytes: serialized job id (incl. byteLen; use DeserializeBigInt!)
         * n Bytes: serialized job result */
        /// <summary>
        /// integrates the serialized JobResult into the DistributableJob-specific
        /// communication protocol - returns a completely compatible DistributableJob-Message
        /// </summary>
        /// <param name="jobId">the jobId of the actual JobPart</param>
        /// <param name="data">the already serialized JobResult-data</param>
        /// <returns></returns>
        public static byte[] CreateJobResultMessage(BigInteger jobId, byte[] data)
        {
            byte[] jobIdBytes = SerializeJobId(jobId);

            byte[] result = new byte[jobIdBytes.Length + data.Length + 1];
            result[0] = (byte)MessageJobType.JobResult;

            Buffer.BlockCopy(jobIdBytes, 0, result, 1, jobIdBytes.Length);
            Buffer.BlockCopy(data, 0, result, jobIdBytes.Length + 1, data.Length);

            return result;
        }

        /// <summary>
        /// Fetches the serialized JobResult data and the JobId of the JobResult out of 
        /// the DistributableJob-specific message. If the message doesn't fit to the
        /// communication protocol, this method returns null, otherwise the raw serialized
        /// JobResult data!
        /// </summary>
        /// <param name="data">DistributableJob-specific message, which was proved that it is a JobResultMessage before</param>
        /// <param name="jobId">catches the JobId out of the raw byte data and returns it, too</param>
        /// <returns></returns>
        public static byte[] GetJobResult(byte[] data, out BigInteger jobId)
        {
            byte[] serializedJobResult = null;
            jobId = null;
            if ((MessageJobType)data[0] == MessageJobType.JobResult)
            {
                byte[] tailBytes = new byte[data.Length - 1];

                Buffer.BlockCopy(data, 1, tailBytes, 0, tailBytes.Length);
                int bytesLeft;
                jobId = DeserializeJobId(tailBytes, out bytesLeft);
                serializedJobResult = new byte[bytesLeft];
                Buffer.BlockCopy(tailBytes, tailBytes.Length - bytesLeft, serializedJobResult, 0, serializedJobResult.Length);
            }
            return serializedJobResult;
        }

        /* Byte representation of a job acceptance Message:
         * 1 Byte:  MessageJobType
         * n Bytes: bool JobAcceptance
         * n Bytes: serialized Job Id (incl. length, use DeserializeJobId!)*/
        /// <summary>
        /// creates a DistributableJob-specific message, which contains information
        /// about the job acceptance (Accepted or Declined)
        /// </summary>
        /// <param name="jobId">the jobId of the actual JobPart</param>
        /// <param name="jobAccepted">true, when JobPart was accepted by the P2PJobAdmin, otherwise false</param>
        /// <returns></returns>
        public static byte[] CreateJobAcceptanceMessage(BigInteger jobId, bool jobAccepted)
        {
            byte[] serializedJobId = SerializeJobId(jobId);

            byte jobAcceptedByte = Convert.ToByte(jobAccepted);

            byte[] serializedData = new byte[serializedJobId.Length + 2];
            serializedData[0] = (byte)MessageJobType.JobAcceptanceInfo;
            serializedData[1] = jobAcceptedByte;
            Buffer.BlockCopy(serializedJobId, 0, serializedData, 2, serializedJobId.Length);

            return serializedData;
        }

        /// <summary>
        /// Fetches the JobAcceptance-Result out of the DistributableJob-specific
        /// message. Returns true, if JobPart was accepted by the P2PJobAdmin,
        /// otherwise false. Additionally it returns the JobId of the accepted/declined
        /// JobPart.
        /// </summary>
        /// <param name="data">DistributableJob-specific message</param>
        /// <param name="jobId">catches the JobId out of the raw byte data and returns it, too</param>
        /// <returns></returns>
        public static bool GetJobAcceptanceMessage(byte[] data, out BigInteger jobId)
        {
            bool result;
            if ((MessageJobType)data[0] == MessageJobType.JobAcceptanceInfo)
            {
                result = Convert.ToBoolean(data[1]);
                byte[] jobIdBytes = new byte[data.Length - 2];
                Buffer.BlockCopy(data, 2, jobIdBytes, 0, jobIdBytes.Length);
                int neverMind;
                jobId = DeserializeJobId(jobIdBytes, out neverMind);
            }
            else
            {
                throw (new Exception("byte[] representation wasn't a JobAcceptance Message!"));
            }
            return result;
        }

        /// <summary>
        /// When a P2PJobAdmin is ready to get a new JobPart for processing,
        /// it have to send a "Free Worker"-Status-Message. This method returns
        /// a DistributableJob-specific message.
        /// </summary>
        /// <param name="free">true, if worker is ready for processing new, incoming JobParts.</param>
        /// <returns></returns>
        public static byte[] CreateFreeWorkerStatusMessage(bool free)
        {
            byte[] retValue = new byte[2];
            retValue[0] = (byte)MessageJobType.Free;
            if (free)
                retValue[1] = 1;
            else
                retValue[1] = 0;
            return retValue;
        }

        /// <summary>
        /// When a P2PJobAdmin is ready for processing new incoming JobParts
        /// it sends a "Free Worker"-Status-Message. This method returns true
        /// when Worker is ready (so send a new JobPart to it), otherwise false
        /// </summary>
        /// <param name="msg">the whole, DistributableJob-specific message</param>
        /// <returns></returns>
        public static bool GetFreeWorkerStatusMessage(byte[] msg)
        {
            bool retValue = false;
            if ((MessageJobType)msg[0] == MessageJobType.Free && msg.Length == 2)
            {
                if (msg[1] == 0)
                    retValue = false;
                else
                    retValue = true;
            }
            return retValue;
        }

        #region (De-)Serialization of JobId

        public static byte[] SerializeJobId(BigInteger jobId)
        {
            byte[] resultByte = null;
            if (jobId != null)
            {
                // Note, there is a Bug in BigInt: BigInt b = 256; => b.dataLength = 1 -- it should be 2!
                // As a workarround rely on getBytes().Length (the null bytes for the BigInt 0 should be fixed now)
                byte[] jobIdBytes = jobId.getBytes();
                byte[] jobIdBytesLen = BitConverter.GetBytes(jobIdBytes.Length);
              
                resultByte = new byte[jobIdBytes.Length + jobIdBytesLen.Length];
                
                Buffer.BlockCopy(jobIdBytesLen, 0, resultByte, 0, jobIdBytesLen.Length);
                Buffer.BlockCopy(jobIdBytes, 0, resultByte, jobIdBytesLen.Length, jobIdBytes.Length);
            }
            return resultByte;
        }

        /// <summary>
        /// Deserialized a jobId from any byte[] array. Requirement: The byte[] has
        /// to start with a four byte (int32) JobId-Length-Information and have than enough
        /// bytes left to deserialize the BigInteger Value. If any bytes left after
        /// deserializing the JobId, the out value will specify this amount.
        /// </summary>
        /// <param name="serializedJobId">The byte[] has to start with a four bytes (int32) 
        /// JobId-Length-Information and have than enough bytes left to deserialize 
        /// the BigInteger Value</param>
        /// <param name="bytesLeft">If any bytes left after
        /// deserializing the JobId, the out value will specify this amount.</param>
        /// <returns></returns>
        public static BigInteger DeserializeJobId(byte[] serializedJobId, out int bytesLeft)
        {
            // byte length of Int32
            int iInt32 = 4;

            BigInteger result = null;
            bytesLeft = serializedJobId.Length;
            if (serializedJobId != null && serializedJobId.Length > iInt32)
            {
                byte[] bigIntByteLen = new byte[iInt32];
                Buffer.BlockCopy(serializedJobId, 0, bigIntByteLen, 0, bigIntByteLen.Length);
                int bigIntLen = BitConverter.ToInt32(bigIntByteLen, 0);
                byte[] bigIntByte = new byte[bigIntLen];
                Buffer.BlockCopy(serializedJobId, bigIntByteLen.Length, bigIntByte, 0, bigIntByte.Length);
                result = new BigInteger(bigIntByte, bigIntByte.Length);
                bytesLeft = serializedJobId.Length - iInt32 - bigIntByte.Length;
            }
            return result;
        }
#endregion
    }
}
