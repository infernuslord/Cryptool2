using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Worker;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.GUI.Controls
{
    /// <summary>
    /// Interaction logic for JobCreation.xaml
    /// </summary>
    public partial class JobCreation
    {
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
                        P2PEditor.GuiLogMessage("File not found.", NotificationLevel.Error);
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
                P2PEditor.GuiLogMessage("Please fill all fields.", NotificationLevel.Error);
                return;
            }

            if (!File.Exists(_newDistributedJob.LocalFilePath))
            {
                // TODO validate that selected file contains a workspace
                P2PEditor.GuiLogMessage("Selected workspace does not exist.", NotificationLevel.Error);
                return;
            }

            var backgroundCreationWorker = new JobCreationWorker(JobListManager, _newDistributedJob);
            backgroundCreationWorker.RunWorkerCompleted += BackgroundCreationWorkerCompleted;
            backgroundCreationWorker.RunWorkerAsync();
        }

        private void BackgroundCreationWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PEditor.GuiLogMessage("Distributed job " + _newDistributedJob.JobGuid, NotificationLevel.Debug);
            DataContext = new DistributedJob();

            P2PEditorPresentation.ShowActiveJobs();
        }
    }
}