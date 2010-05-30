/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Cryptool.P2P;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.GUI
{
    /// <summary>
    /// Interaction logic for P2PEditorPresentation.xaml
    /// </summary>
    public partial class P2PEditorPresentation
    {
        public static readonly DependencyProperty JobsProperty = DependencyProperty.Register("Jobs",
                                                                                             typeof (
                                                                                                 List<DistributedJob>),
                                                                                             typeof (
                                                                                                 P2PEditorPresentation));

        public static readonly DependencyProperty IsP2PConnectedProperty =
            DependencyProperty.Register("IsP2PConnected",
                                        typeof (
                                            Boolean),
                                        typeof (
                                            P2PEditorPresentation), new PropertyMetadata(false));


        public P2PEditorPresentation(P2PEditor p2PEditor, JobListManager jobListManager)
        {
            P2PEditor = p2PEditor;
            JobListManager = jobListManager;
            P2PEditorPresentation = this;

            P2PManager.OnP2PConnectionStateChangeOccurred += P2PManager_OnP2PConnectionStateChangeOccurred;

            InitializeComponent();

            UpdateDisplay();
        }

        private void P2PManager_OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            IsP2PConnected = newState;
        }

        public List<DistributedJob> Jobs
        {
            get { return (List<DistributedJob>) GetValue(JobsProperty); }
            set { SetValue(JobsProperty, value); }
        }

        public Boolean IsP2PConnected
        {
            get { return (Boolean) GetValue(IsP2PConnectedProperty); }
            set { SetValue(IsP2PConnectedProperty, value); }
        }


        internal void ConnectionWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateDisplay));
        }

        private void UpdateDisplay()
        {
            IsP2PConnected = P2PManager.Instance.P2PConnected();
            Jobs = JobListManager.JobList();
        }

        internal void ShowJobCreation()
        {
            JobTabControl.SelectedItem = JobCreationTab;
        }

        internal void ShowActiveJobs()
        {
            UpdateDisplay();
            JobTabControl.SelectedItem = ActiveJobsTab;
        }
    }
}