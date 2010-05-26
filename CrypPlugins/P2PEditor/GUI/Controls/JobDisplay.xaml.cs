using System;
using System.Collections.Generic;
using System.Windows;
using Cryptool.P2PEditor.Distributed;

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
            Jobs = JobListManager.JobList();
        }
    }
}