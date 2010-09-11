using System;
using System.ComponentModel;
using System.Windows.Threading;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Worker
{
    public class JobParticipationWorker : BackgroundWorker
    {
        private readonly P2PEditor p2PEditor;
        private readonly JobListManager jobListManager;
        private readonly DistributedJob jobToParticipateIn;
        private readonly Dispatcher dispatcher;

        public JobParticipationWorker(P2PEditor p2PEditor, JobListManager jobListManager, DistributedJob jobToParticipateIn, Dispatcher dispatcher)
        {
            this.p2PEditor = p2PEditor;
            this.jobListManager = jobListManager;
            this.jobToParticipateIn = jobToParticipateIn;
            this.dispatcher = dispatcher;

            DoWork += JobParticipationWorkerDoWork;
        }

        private void JobParticipationWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                jobListManager.CompleteDistributedJob(jobToParticipateIn);
            }
            catch (Exception ex)
            {
                p2PEditor.GuiLogMessage("Error completing job: " + ex.Message, NotificationLevel.Error);
                return;
            }

            p2PEditor.GuiLogMessage("Local workspace: " + jobToParticipateIn.LocalFilePath, NotificationLevel.Debug);
            p2PEditor.GuiLogMessage(
                string.Format("Workspace {0} ready to participate, dispatching with CrypTool...",
                              jobToParticipateIn.Name),
                NotificationLevel.Info);

            jobListManager.IncreaseDownloadCount(jobToParticipateIn);

            dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(DispatchOpenFileEvent));
        }

        private void DispatchOpenFileEvent()
        {
            p2PEditor.SendOpenProjectFileEvent(jobToParticipateIn.LocalFilePath);
        }
    }
}