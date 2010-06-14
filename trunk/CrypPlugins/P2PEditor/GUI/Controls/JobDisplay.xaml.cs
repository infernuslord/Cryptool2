using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Cryptool.P2P;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Worker;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.GUI.Controls
{
    /// <summary>
    /// Interaction logic for JobDisplay.xaml
    /// </summary>
    public partial class JobDisplay
    {
        public static DependencyProperty JobsProperty = DependencyProperty.Register("Jobs",
                                                                                    typeof (List<DistributedJob>),
                                                                                    typeof (JobDisplay));

        private Timer _refreshListTimer;

        public JobDisplay()
        {
            InitializeComponent();
            UpdateRefreshTimerSettings(P2PManager.IsConnected);

            P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += P2PManager_OnP2PConnectionStateChangeOccurred;
        }

        void P2PManager_OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            UpdateRefreshTimerSettings(newState);
        }

        private void UpdateRefreshTimerSettings(bool isConnected)
        {
            if (P2PSettings.Default.DistributedJobListRefreshInterval == 0)
            {
                return;
            }

            if (_refreshListTimer == null)
            {
                _refreshListTimer = new Timer(P2PSettings.Default.DistributedJobListRefreshInterval * 1000);
                _refreshListTimer.Elapsed += RefreshListTimerElapsed;
            }

            if (isConnected)
            {
                _refreshListTimer.Start();
            }
            else 
            {
                _refreshListTimer.Stop();
            }
        }

        void RefreshListTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateJobList));
        }

        public List<DistributedJob> Jobs
        {
            get { return (List<DistributedJob>) GetValue(JobsProperty); }
            set { SetValue(JobsProperty, value); }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateJobList();
        }

        public void UpdateJobList()
        {
            if (!P2PManager.IsConnected)
            {
                return;
            }

            P2PEditor.GuiLogMessage("Requesting new job list...", NotificationLevel.Debug);
            var updateWorker = new JobListUpdateWorker(JobListManager);
            updateWorker.RunWorkerCompleted += HandleRefreshedJobList;
            updateWorker.RunWorkerAsync();
        }

        void HandleRefreshedJobList(object sender, RunWorkerCompletedEventArgs e)
        {
            var updateWorker = sender as JobListUpdateWorker;
            if (updateWorker == null)
            {
                return;
            }

            P2PEditor.GuiLogMessage("Received new job list...", NotificationLevel.Debug);
            Jobs = updateWorker.RefreshedJobList;
            Jobs.Reverse();
        }

        private void ParticipateButton_Click(object sender, RoutedEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateItemHandler(object sender, MouseButtonEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateInSelectedJob()
        {
            var jobToParticipateIn = (DistributedJob) JobListBox.SelectedItem;

            if (jobToParticipateIn == null)
            {
                return;
            }

            P2PEditor.GuiLogMessage(
                string.Format("Participating in job {0} ({1}).", jobToParticipateIn.JobName, jobToParticipateIn.JobGuid),
                NotificationLevel.Info);
            new JobParticipationWorker(P2PEditor, JobListManager, jobToParticipateIn, Dispatcher).RunWorkerAsync();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedJob();
        }

        private void JobListBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ParticipateInSelectedJob();
                    break;
                case Key.Delete:
                    DeleteSelectedJob();
                    break;
            }
            
        }

        private void DeleteSelectedJob()
        {
            var jobToDelete = (DistributedJob)JobListBox.SelectedItem;

            if (jobToDelete == null || jobToDelete.JobOwner != P2PSettings.Default.PeerName)
            {
                return;
            }

            P2PEditor.GuiLogMessage(
                string.Format("Deleting job {0} ({1}).", jobToDelete.JobName, jobToDelete.JobGuid),
                NotificationLevel.Info);

            var backgroundCreationWorker = new JobDeletionWorker(JobListManager, jobToDelete);
            backgroundCreationWorker.RunWorkerCompleted += BackgroundDeletionWorkerCompleted;
            backgroundCreationWorker.RunWorkerAsync();
        }

        private void BackgroundDeletionWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PEditorPresentation.ShowActiveJobs();
        }
    }
}