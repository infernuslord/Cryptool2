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
    public abstract class JobPart<JobType> : IJobPart
    {
        public JobPart(BigInteger jobId)
        {
            this.JobId = jobId;
        }

        public abstract byte[] Serialize();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            return this == (JobPart<JobType>)obj;
        }

        public static bool operator ==(JobPart<JobType> left, JobPart<JobType> right)
        {
            if ((object)left == (object)right)
                return true;

            if ((object)left == null || (object)right == null)
                return false;

            if (left.JobId == right.JobId)
                return true;

            return false;
        }

        public static bool operator !=(JobPart<JobType> left, JobPart<JobType> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return this.JobId.GetHashCode();
        }

        #region IJobPart Members

        private BigInteger jobId;
        public BigInteger JobId
        {
            get { return this.jobId; }
            set { this.jobId = value; }
        }

        #endregion
    }
}
