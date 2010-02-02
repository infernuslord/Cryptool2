using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    /// <summary>
    /// Reactive Job stack, which creates the next element not before it is requested. 
    /// However the whole stack count is determined!
    /// </summary>
    public interface IJobStack<JobType>
    {
        /// <summary>
        /// Processes a new Subjob, if possible, otherwise returns null
        /// </summary>
        /// <returns>a new Subjob, if possible, otherwise returns null</returns>
        JobType Pop();
        /// <summary>
        /// Adds an existing Subjob to the JobStack, if it doesn't exists there, otherwise throws an Exception!
        /// </summary>
        /// <param name="jobPart">an existing Subjob, which doesn't exist yet in the JobStack</param>
        void Push(JobType jobPart);
        long Count();
        /// <summary>
        /// Checks the existence of a jobPart.
        /// </summary>
        /// <param name="jobPart"></param>
        /// <returns></returns>
        bool Contains(JobType jobPart);
    }
}
