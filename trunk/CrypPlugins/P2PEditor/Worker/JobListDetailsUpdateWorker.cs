﻿using System.Collections.Generic;
using System.ComponentModel;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Worker
{
    class JobListDetailsUpdateWorker : BackgroundWorker
    {
        private readonly ICollection<DistributedJob> distributedJobs;
        private readonly JobListManager jobListManager;

        public JobListDetailsUpdateWorker(ICollection<DistributedJob> distributedJobs, JobListManager jobListManager)
        {
            this.distributedJobs = distributedJobs;
            this.jobListManager = jobListManager;

            DoWork += JobListDetailsUpdateWorker_DoWork;
        }

        void JobListDetailsUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var distributedJob in distributedJobs)
            {
                if (CancellationPending) return;

                jobListManager.RetrieveDownloadCount(distributedJob);
                jobListManager.RetrieveCurrentStatus(distributedJob);
            }
        }
    }
}
