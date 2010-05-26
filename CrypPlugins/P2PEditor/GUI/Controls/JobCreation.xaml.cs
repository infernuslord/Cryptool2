using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.GUI.Controls
{
    /// <summary>
    /// Interaction logic for JobCreation.xaml
    /// </summary>
    public partial class JobCreation
    {
        private BackgroundWorker _backgroundCreationWorker;
        private DistributedJob _newDistributedJob;

        public JobCreation()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new DistributedJob();
        }

        private void BrowseFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ((DistributedJob) DataContext).LocalFilePath = dialog.FileName;
                    }
                    catch (FileNotFoundException)
                    {
                        P2PEditor.Instance.GuiLogMessage("File not found.", NotificationLevel.Error);
                    }
                }
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            _newDistributedJob = (DistributedJob) DataContext;

            if (_newDistributedJob.JobDescription == null || _newDistributedJob.JobName == null)
            {
                P2PEditor.Instance.GuiLogMessage("Please fill all fields.", NotificationLevel.Error);
                return;
            }

            if (!File.Exists(_newDistributedJob.LocalFilePath))
            {
                // TODO validate that selected file contains a workspace
                P2PEditor.Instance.GuiLogMessage("Selected workspace does not exist.", NotificationLevel.Error);
                return;
            }

            _backgroundCreationWorker = new BackgroundWorker();
            _backgroundCreationWorker.DoWork += BackgroundCreationWorkerDoWork;
            _backgroundCreationWorker.RunWorkerCompleted += BackgroundCreationWorkerCompleted;
            _backgroundCreationWorker.RunWorkerAsync();
        }

        private void BackgroundCreationWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PEditor.Instance.GuiLogMessage("Distributed job " + _newDistributedJob.JobGuid, NotificationLevel.Debug);
            DataContext = new DistributedJob();
        }

        private void BackgroundCreationWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            JobListManager.AddDistributedJob(_newDistributedJob);
        }
    }
}