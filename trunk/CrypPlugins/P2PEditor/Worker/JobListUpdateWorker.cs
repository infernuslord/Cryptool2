using System.Collections.Generic;
using System.ComponentModel;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Worker
{
    public class JobListUpdateWorker : BackgroundWorker
    {
        private readonly JobListManager _jobListManager;
        public List<DistributedJob> RefreshedJobList;

        public JobListUpdateWorker(JobListManager jobListManager)
        {
            _jobListManager = jobListManager;

            DoWork += JobCreationWorker_DoWork;
        }

        private void JobCreationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshedJobList = _jobListManager.JobList();
        }
    }
}