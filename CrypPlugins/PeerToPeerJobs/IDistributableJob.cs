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
    public interface IDistributableJob
    {
        /// <summary>
        /// pops a new serialized JobPart and the JobParts' JobId.
        /// Check the returned byte array. If it is null, there
        /// are no more jobParts left on the JobPart-Stack
        /// </summary>
        /// <param name="jobId">the JobParts' JobId</param>
        /// <returns>if this is null, there are no more JobParts left, 
        /// otherwise the serialized JobPart</returns>
        byte[] Pop(out BigInteger jobId);
        /// <summary>
        /// Pushes an already distributed JobPart back
        /// to the stack (e.g. a Worker, who was
        /// processing the JobPart actually leaves the network)
        /// </summary>
        /// <param name="jobId"></param>
        void Push(BigInteger jobId);
        double ProcessProgress();
        BigInteger TotalAmount{ get; set; }
        BigInteger AllocatedAmount{ get; set; }
        BigInteger FinishedAmount{ get; set; }
        void JobAccepted(BigInteger jobId);
        void JobDeclined(BigInteger jobId);
        /// <summary>
        /// If a result was received from a working peer, set the result here.
        /// The job will be removed from the job stack and the result will be processed.
        /// </summary>
        /// <param name="jobId">JobId of an already allocated job</param>
        /// <param name="result">a byte representation of the job result</param>
        /// <returns>the TimeSpan of processing the job by the worker</returns>
        TimeSpan SetResult(BigInteger jobId, byte[] result);
    }
}
