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
    public interface IJobResult<JobResultType>
    {
        /// <summary>
        /// in this context a unique identifier for the job, whose 
        /// Result is represented by this class
        /// </summary>
        BigInteger JobId { get; set; }
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
        /// serializes this class, so you can recover all class information by deserializing.
        /// HINT: You can Deserialize a byte[] representation of this class by using the constructor with
        /// the byte[] parameter!
        /// </summary>
        /// <returns>serialized byte[] representation of this class</returns>
        byte[] Serialize();
    }
}
