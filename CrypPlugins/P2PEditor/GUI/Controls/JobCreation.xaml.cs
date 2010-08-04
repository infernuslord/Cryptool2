using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Worker;
using Cryptool.PluginBase;
using Clipboard = System.Windows.Clipboard;
using TextDataFormat = System.Windows.TextDataFormat;

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class JobCreation
    {
        private DistributedJob newDistributedJob;

        public JobCreation()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new DistributedJob();

            try
            {
                if (!Clipboard.ContainsText(TextDataFormat.Text)) return;

                var clipboardData = Clipboard.GetText(TextDataFormat.Text);
                if (clipboardData.EndsWith("-status"))
                {
                    ((DistributedJob) DataContext).StatusKey = clipboardData;
                }
            }
            catch (OutOfMemoryException)
            {
                // If clipboard content is to large, no status key is available.
            }
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
            newDistributedJob = (DistributedJob) DataContext;

            if (newDistributedJob.Description == null || newDistributedJob.Name == null)
            {
                P2PEditor.GuiLogMessage("Please fill all fields.", NotificationLevel.Error);
                return;
            }

            if (!File.Exists(newDistributedJob.LocalFilePath))
            {
                // TODO validate that selected file contains a workspace
                P2PEditor.GuiLogMessage("Selected workspace does not exist.", NotificationLevel.Error);
                return;
            }

            var backgroundCreationWorker = new JobCreationWorker(JobListManager, newDistributedJob);
            backgroundCreationWorker.RunWorkerCompleted += BackgroundCreationWorkerCompleted;
            backgroundCreationWorker.RunWorkerAsync();
        }

        private void BackgroundCreationWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PEditor.GuiLogMessage("Distributed job " + newDistributedJob.Guid, NotificationLevel.Debug);
            DataContext = new DistributedJob();

            P2PEditorPresentation.ShowActiveJobs();
            P2PEditorPresentation.ActiveJobsControl.JobListBox.SelectedIndex = 0;
        }
    }
}