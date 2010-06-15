using System;
using System.ComponentModel;
using System.Windows.Threading;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Worker
{
    public class JobParticipationWorker : BackgroundWorker
    {
        private readonly P2PEditor _p2PEditor;
        private readonly JobListManager _jobListManager;
        private readonly DistributedJob _jobToParticipateIn;
        private readonly Dispatcher _dispatcher;

        public JobParticipationWorker(P2PEditor p2PEditor, JobListManager jobListManager, DistributedJob jobToParticipateIn, Dispatcher dispatcher)
        {
            _p2PEditor = p2PEditor;
            _jobListManager = jobListManager;
            _jobToParticipateIn = jobToParticipateIn;
            _dispatcher = dispatcher;

            DoWork += JobParticipationWorker_DoWork;
        }

        private void JobParticipationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _jobListManager.CompleteDistributedJob(_jobToParticipateIn);
            } catch(Exception ex)
            {
                _p2PEditor.GuiLogMessage("Error completing job: " + ex.Message, NotificationLevel.Error);
                return;
            }

            _p2PEditor.GuiLogMessage("Local workspace: " + _jobToParticipateIn.LocalFilePath, NotificationLevel.Debug);
            _p2PEditor.GuiLogMessage(
                string.Format("Workspace {0} ready to participate, dispatching with CrypTool...",
                              _jobToParticipateIn.JobName),
                NotificationLevel.Info);

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(DispatchOpenFileEvent));
        }

        private void DispatchOpenFileEvent()
        {
            _p2PEditor.SendOpenProjectFileEvent(_jobToParticipateIn.LocalFilePath);
        }
    }
}