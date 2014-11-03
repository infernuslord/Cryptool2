using System.Collections.Generic;

namespace Cryptool.P2PEditor.Distributed
{
    public class JobListManager
    {
        private readonly P2PEditor p2PEditor;
        public JobListManager(P2PEditor p2PEditor)
        {
            this.p2PEditor = p2PEditor;
        }

        public ICollection<DistributedJob> JobList()
        {
            return new List<DistributedJob>();
        }

        public void AddDistributedJob(DistributedJob distributedJob)
        {
       
        }

        public void DeleteDistributedJob(DistributedJob distributedJobToDelete)
        {
          
        }

        public void CompleteDistributedJob(DistributedJob distributedJob)
        {
           
        }

        public void RetrieveDownloadCount(DistributedJob distributedJob)
        {
          
        }

        public void RetrieveCurrentStatus(DistributedJob distributedJob)
        {
      
        }

        public void IncreaseDownloadCount(DistributedJob distributedJob)
        {
            
        }

    }
}