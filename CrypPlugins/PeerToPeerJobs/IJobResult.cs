using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public interface IJobResult<JobResultType>
    {
        /// <summary>
        /// in this context a unique identifier for the job, whose 
        /// Result is represented by this class
        /// </summary>
        int JobId { get; set; }
        /// <summary>
        /// You can allow intermediate results by using this flag
        /// </summary>
        bool IsIntermediateResult { get; set; }
        /// <summary>
        /// Timespan between start and end processing
        /// </summary>
        TimeSpan ProcessingTime { get; set; }
        JobResultType Result { get; set; }
        /// <summary>
        /// serializes this class, so you can recover all class information by deserializing
        /// </summary>
        /// <returns>serialized byte[] representation of this class</returns>
        byte[] Serialize();
        /// <summary>
        /// Deserializes a valid byte[] representation of a IJobResult.
        /// ATTENTION: Not only deserializes, but additionally recreates the whole class
        /// by dint of the byte[]. So all this class information will be overwritten.
        /// </summary>
        /// <param name="serializedJobResult">a valid byte[] representation of a IJobResult</param>
        /// <returns></returns>
        bool Deserialize(byte[] serializedJobResult);
    }
}
