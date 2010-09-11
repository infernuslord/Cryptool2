using System.ComponentModel;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Worker
{
    public class JobCreationWorker : BackgroundWorker
    {
        private readonly JobListManager jobListManager;
        private readonly DistributedJob jobToDistribute;

        public JobCreationWorker(JobListManager jobListManager, DistributedJob jobToDistribute)
        {
            this.jobListManager = jobListManager;
            this.jobToDistribute = jobToDistribute;

            DoWork += JobCreationWorkerDoWork;
        }

        private void JobCreationWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            jobListManager.AddDistributedJob(jobToDistribute);
        }
    }
}