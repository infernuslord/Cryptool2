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
using System.IO;
using System.Reflection;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public abstract class JobResult<JobResultType> : IJobResult<JobResultType>
    {
        #region Variables

        private BigInteger jobId;
        public BigInteger JobId
        {
            get {  return this.jobId; }
            set { this.jobId = value; }
        }
        private bool isIntermediateJobResult = false;
        public bool IsIntermediateResult 
        {
            get { return this.isIntermediateJobResult;}
            set { this.isIntermediateJobResult = value; }
        }
        private TimeSpan processingTime;
        public TimeSpan ProcessingTime 
        {
            get { return this.processingTime;}
            set { this.processingTime = value; } 
        }
        private JobResultType result;
        public JobResultType Result 
        { 
            get{return this.result;}
            set { this.result = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new JobResult (this is an endresul!)
        /// </summary>
        /// <param name="jobId">jobId for which you have a result</param>
        /// <param name="result">result of the job (e.g. simple conclusion, list, complex data structure)</param>
        /// <param name="processingTime">Timespan between begin and end of processing the job</param>
        public JobResult(BigInteger jobId, JobResultType result, TimeSpan processingTime)
            :this(jobId,result,processingTime,false)
        { } 

        /// <summary>
        /// Creates a new JobResult (you can choose if this is only an intermediate result or the endresult)
        /// </summary>
        /// <param name="jobId">jobId for which you have a result</param>
        /// <param name="result">result of the job (e.g. simple conclusion, list, complex data structure)</param>
        /// <param name="processingTime">Timespan between begin and end of processing the job</param>
        /// <param name="isIntermediateResult">Is this is only an intermediate result, set this parameter to true, otherwise choose false</param>
        public JobResult(BigInteger jobId, JobResultType result, TimeSpan processingTime, bool isIntermediateResult)
        {
            this.JobId = jobId;
            this.Result = result;
            this.ProcessingTime = processingTime;
            this.IsIntermediateResult = isIntermediateResult;
        }

        // TODO: Test this constructor, if it works fine, delete Deserialize Method
        /// <summary>
        /// If you have a serialized JobResult class, you can deserialize it by using this constructor!
        /// </summary>
        /// <param name="serializedJobResult">serialized JobResult class as a byte[]</param>
        public JobResult(byte[] serializedJobResult)
        {
            BigInteger temp_jobId;
            JobResultType temp_result;
            bool temp_isIntermediateResult;
            TimeSpan temp_processingTime;

            MemoryStream memStream = new MemoryStream(serializedJobResult, false);
            try
            {
                Int32 testValue = 3000;
                byte[] readInt = BitConverter.GetBytes(testValue);

                /* Deserialize JobId */
                memStream.Read(readInt, 0, readInt.Length);
                int jobIdLen = BitConverter.ToInt32(readInt, 0);
                byte[] jobIdByte = new byte[jobIdLen];
                memStream.Read(jobIdByte, 0, jobIdByte.Length);
                temp_jobId = new BigInteger(jobIdByte);

                /* Deserialize Job result data */
                //memStream.Read(readInt, 0, readInt.Length);
                //int serializedDataLen = BitConverter.ToInt32(readInt, 0);
                //byte[] serializedJobResultByte = new byte[serializedDataLen];
                //memStream.Read(serializedJobResultByte, 0, serializedJobResultByte.Length);
                //temp_result = (JobResultType)GetDeserializationViaReflection(serializedJobResultByte, this.Result);

                // right for bool???
                temp_isIntermediateResult = Convert.ToBoolean(memStream.ReadByte());

                memStream.Read(readInt, 0, readInt.Length);
                int days = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int hours = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int minutes = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int seconds = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int millisec = BitConverter.ToInt32(readInt, 0);
                temp_processingTime = new TimeSpan(days, hours, minutes, seconds, millisec);

                // read the rest of the byte[]-stream, this is the specialized JobResulType
                byte[] serializedJobResultByte = new byte[memStream.Length - memStream.Position];
                memStream.Read(serializedJobResultByte, 0, serializedJobResultByte.Length);
                temp_result = DeserializeResultType(serializedJobResultByte);

                this.JobId = temp_jobId;
                this.IsIntermediateResult = temp_isIntermediateResult;
                this.ProcessingTime = temp_processingTime;
                this.Result = temp_result;
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

        #region comparing methods

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            return this == (JobResult<JobResultType>)obj;
        }

        public static bool operator ==(JobResult<JobResultType> left, JobResult<JobResultType> right)
        {
            if ((object)left == (object)right)
                return true;

            if ((object)left == null || (object)right == null)
                return false;

            if (left.jobId == right.JobId)
                return true;

            return false;
        }

        public static bool operator !=(JobResult<JobResultType> left, JobResult<JobResultType> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return this.JobId.GetHashCode();
        }

        #endregion

        #region Serialization methods

        protected abstract byte[] SerializeResultType();

        protected abstract JobResultType DeserializeResultType(byte[] serializedResultType);

        /* 4 Bytes: serialized JobId
         * 4 Bytes: serialized Job Result length
         * y Bytes: serialized Job Result data
         * 4 Bytes: isIntermedResult
         * 4 Bytes: procTime.Days
         * 4 Bytes: procTime.Hours
         * 4 Bytes: procTime.Minutes
         * 4 Bytes: procTime.Seconds
         * 4 Bytes: procTime.Milliseconds */
        /// <summary>
        /// Serializes the whole class, so you can recreate this instance elsewhere by dint of this byte[].
        /// HINT: You can Deserialize a byte[] representation of this class by using the constructor with
        /// the byte[] parameter!
        /// </summary>
        /// <returns>serialized class as an byte[]</returns>
        public byte[] Serialize()
        {
            byte[] ret = null;
            MemoryStream memStream = new MemoryStream();
            try
            {
                /* Serialize jobId */
                byte[] jobIdByte = this.JobId.ToByteArray();
                byte[] jobIdLen = BitConverter.GetBytes(jobIdByte.Length);
                memStream.Write(jobIdLen, 0, jobIdLen.Length);
                memStream.Write(jobIdByte, 0, jobIdByte.Length);

                /* Serialize job result via Reflection */
                //byte[] serializedJobResult = GetSerializationViaReflection(this.Result);
                //byte[] byJobResultLen = BitConverter.GetBytes(serializedJobResult.Length);
                //memStream.Write(byJobResultLen, 0, byJobResultLen.Length);
                //memStream.Write(serializedJobResult, 0, serializedJobResult.Length);

                byte[] intResultBytes = BitConverter.GetBytes(this.isIntermediateJobResult);
                memStream.Write(intResultBytes,0,intResultBytes.Length);
                /* Storing processingTimeSpan */
                byte[] daysBytes = BitConverter.GetBytes(this.processingTime.Days);
                memStream.Write(daysBytes,0,daysBytes.Length);
                byte[] hoursBytes = (BitConverter.GetBytes(this.processingTime.Hours));
                memStream.Write(hoursBytes,0,hoursBytes.Length);
                byte[] minutesBytes = BitConverter.GetBytes(this.processingTime.Minutes);
                memStream.Write(minutesBytes,0,minutesBytes.Length);
                byte[] secondsBytes = BitConverter.GetBytes(this.processingTime.Seconds);
                memStream.Write(secondsBytes,0,secondsBytes.Length);
                byte[] msecondsBytes = BitConverter.GetBytes(this.processingTime.Milliseconds);
                memStream.Write(msecondsBytes,0,msecondsBytes.Length);


                byte[] serializedJobResult = SerializeResultType();
                memStream.Write(serializedJobResult, 0, serializedJobResult.Length);


                ret = memStream.ToArray();
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
            return ret;
        }

        #endregion

        //#region Reflection methods

        //private byte[] GetSerializationViaReflection(object objectToSerialize)
        //{
        //    byte[] serializedBytes = null;
        //    try
        //    {
        //        MethodInfo methInfo = objectToSerialize.GetType().GetMethod("Serialize");
        //        ParameterInfo[] paramInfo = methInfo.GetParameters();
        //        ParameterInfo returnParam = methInfo.ReturnParameter;
        //        Type returnType = methInfo.ReturnType;

        //        serializedBytes = methInfo.Invoke(objectToSerialize, null) as byte[];
        //        if (serializedBytes == null)
        //            throw (new Exception("Serializing " + objectToSerialize.GetType().ToString() + " canceled!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (new Exception("Invocing method 'Serialize' of '"
        //            + objectToSerialize.GetType().ToString() + "' wasn't possible. " + ex.ToString()));
        //    }
        //    return serializedBytes;
        //}

        //private object GetDeserializationViaReflection(object serializedData, object returnType)
        //{
        //    try
        //    {
        //        MethodInfo methInfo = returnType.GetType().GetMethod("Deserialize", new[] { serializedData.GetType() });
        //        object deserializedData = methInfo.Invoke(returnType, new object[] { serializedData });
        //        if (deserializedData == null)
        //            throw (new Exception("Deserializing " + returnType.ToString() + " canceled!"));
        //        return deserializedData;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (new Exception("Invocing method 'Deserialize' of '"
        //            + returnType.ToString() + "' wasn't possible. " + ex.ToString()));
        //    }
        //}

        //#endregion
    }
}
