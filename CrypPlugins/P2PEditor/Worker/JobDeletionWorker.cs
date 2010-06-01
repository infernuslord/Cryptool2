using System.ComponentModel;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Worker
{
    public class JobDeletionWorker : BackgroundWorker
    {
        private readonly JobListManager _jobListManager;
        private readonly DistributedJob _jobToDistribute;

        public JobDeletionWorker(JobListManager jobListManager, DistributedJob jobToDistribute)
        {
            _jobListManager = jobListManager;
            _jobToDistribute = jobToDistribute;

            DoWork += JobCreationWorker_DoWork;
        }

        private void JobCreationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _jobListManager.DeleteDistributedJob(_jobToDistribute);
        }
    }
}
