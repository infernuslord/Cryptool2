using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public interface IJobPart<JobType>
    {
        byte[] Serialize();
        JobType Deserialize(byte[] serializedJobPart);
    }
}
