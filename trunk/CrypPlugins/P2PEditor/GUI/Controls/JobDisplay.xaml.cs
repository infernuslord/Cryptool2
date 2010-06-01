using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public JobDisplay()
        {
            InitializeComponent();
        }

        public List<DistributedJob> Jobs
        {
            get { return (List<DistributedJob>) GetValue(JobsProperty); }
            set { SetValue(JobsProperty, value); }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var updateListWorker = new BackgroundWorker();
            updateListWorker.DoWork += UpdateJobListInWorker;
            updateListWorker.RunWorkerAsync();
        }

        void UpdateJobListInWorker(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateJobList));
            
        }

        public void UpdateJobList()
        {
            Jobs = JobListManager.JobList();
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