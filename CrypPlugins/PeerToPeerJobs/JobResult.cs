﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public class JobResult<JobResultType> : IJobResult<JobResultType>
    {
        #region Variables

        private int jobId;
        public int JobId
        {
            get {  return this.jobId; }
            set { this.jobId = value; }
        }
        private bool isIntermediateJobResult;
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
        public JobResult(int jobId, JobResultType result, TimeSpan processingTime)
            :this(jobId,result,processingTime,false)
        { } 

        /// <summary>
        /// Creates a new JobResult (you can choose if this is only an intermediate result or the endresult)
        /// </summary>
        /// <param name="jobId">jobId for which you have a result</param>
        /// <param name="result">result of the job (e.g. simple conclusion, list, complex data structure)</param>
        /// <param name="processingTime">Timespan between begin and end of processing the job</param>
        /// <param name="isIntermediateResult">Is this is only an intermediate result, set this parameter to true, otherwise choose false</param>
        public JobResult(int jobId, JobResultType result, TimeSpan processingTime, bool isIntermediateResult)
        {
            this.JobId = jobId;
            this.Result = result;
            this.ProcessingTime = processingTime;
            this.IsIntermediateResult = isIntermediateResult;
        }

        #region Serialization methods

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
        /// Serializes the whole class, so you can recreate this instance elsewhere by dint of this byte[]
        /// </summary>
        /// <returns>serialized class as an byte[]</returns>
        public byte[] Serialize()
        {
            byte[] ret = null;
            MemoryStream memStream = new MemoryStream();
            try
            {
                /* Serialize jobId */
                byte[] intJobId = BitConverter.GetBytes(this.JobId);
                memStream.Write(intJobId, 0, intJobId.Length);

                /* Serialize job result via Reflection */
                byte[] serializedJobResult = GetSerializationViaReflection(this.Result);
                byte[] byJobResultLen = BitConverter.GetBytes(serializedJobResult.Length);
                memStream.Write(byJobResultLen, 0, byJobResultLen.Length);
                memStream.Write(serializedJobResult, 0, serializedJobResult.Length);

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

        /// <summary>
        /// ATTENTION: Not only deserializes the DistributableJobResult, but additionally recreates the whole class
        /// by dint of the byte[]. So all this class information will be overwritten.
        /// </summary>
        /// <param name="serializedJobResult">a valid DistributableJobResult serialization</param>
        public bool Deserialize(byte[] serializedJobResult)
        {
            bool ret = false;
            MemoryStream memStream = new MemoryStream(serializedJobResult,false);
            try
            {
                /* Deserialize JobId */
                byte[] readInt = new byte[4];
                memStream.Read(readInt, 0, readInt.Length);
                this.JobId = BitConverter.ToInt32(readInt,0);

                /* Deserialize Job result data */
                memStream.Read(readInt,0,readInt.Length);
                int serializedDataLen = BitConverter.ToInt32(readInt,0);
                byte[] serializedJobResultByte = new byte[serializedDataLen];
                memStream.Read(serializedJobResultByte, 0, serializedJobResultByte.Length);
                this.Result = (JobResultType)GetDeserializationViaReflection(serializedJobResultByte, this.Result);

                // right for bool???
                memStream.Read(readInt, 0, readInt.Length);
                this.isIntermediateJobResult = BitConverter.ToBoolean(readInt,0);

                memStream.Read(readInt, 0, readInt.Length);
                int days = BitConverter.ToInt32(readInt,0);
                memStream.Read(readInt, 0, readInt.Length);
                int hours = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int minutes = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int seconds = BitConverter.ToInt32(readInt, 0);
                memStream.Read(readInt, 0, readInt.Length);
                int millisec = BitConverter.ToInt32(readInt, 0);
                this.processingTime = new TimeSpan(days, hours, minutes, seconds, millisec);

                ret = true;
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

        #region Reflection methods

        private byte[] GetSerializationViaReflection(object objectToSerialize)
        {
            byte[] serializedBytes = null;
            try
            {
                MethodInfo methInfo = objectToSerialize.GetType().GetMethod("Serialize");
                ParameterInfo[] paramInfo = methInfo.GetParameters();
                ParameterInfo returnParam = methInfo.ReturnParameter;
                Type returnType = methInfo.ReturnType;

                serializedBytes = methInfo.Invoke(objectToSerialize, null) as byte[];
                if (serializedBytes == null)
                    throw (new Exception("Serializing " + objectToSerialize.GetType().ToString() + " canceled!"));
            }
            catch (Exception ex)
            {
                throw (new Exception("Invocing method 'Serialize' of '"
                    + objectToSerialize.GetType().ToString() + "' wasn't possible. " + ex.ToString()));
            }
            return serializedBytes;
        }

        private object GetDeserializationViaReflection(object serializedData, object returnType)
        {
            try
            {
                MethodInfo methInfo = returnType.GetType().GetMethod("Deserialize", new[] { serializedData.GetType() });
                object deserializedData = methInfo.Invoke(returnType, new object[] { serializedData });
                if (deserializedData == null)
                    throw (new Exception("Deserializing " + returnType.ToString() + " canceled!"));
                return deserializedData;
            }
            catch (Exception ex)
            {
                throw (new Exception("Invocing method 'Deserialize' of '"
                    + returnType.ToString() + "' wasn't possible. " + ex.ToString()));
            }
        }

        #endregion
    }
}
