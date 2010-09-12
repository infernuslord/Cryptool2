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

        public static readonly DependencyProperty IsP2PConnectingProperty =
            DependencyProperty.Register("IsP2PConnecting",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            P2PEditorPresentation), new PropertyMetadata(false));

        public P2PEditorPresentation(P2PEditor p2PEditor, JobListManager jobListManager)
        {
            P2PEditor = p2PEditor;
            JobListManager = jobListManager;
            P2PEditorPresentation = this;

            P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += HandleChangedPeerToPeerConnectionState;

            InitializeComponent();

            UpdateDisplay();

            if (!P2PManager.IsConnected)
            {
                ShowConnectTab();
            }
        }

        private void HandleChangedPeerToPeerConnectionState(object sender, bool newState)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateConnectionState));

            if (P2PManager.IsConnected)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowActiveJobs));
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowConnectTab));
            }
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

        public Boolean IsP2PConnecting
        {
            get { return (Boolean)GetValue(IsP2PConnectingProperty); }
            set { SetValue(IsP2PConnectingProperty, value); }
        }

        private void UpdateDisplay()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateConnectionState));
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ActiveJobsControl.UpdateJobList));
        }

        internal void UpdateConnectionState()
        {
            IsP2PConnected = P2PManager.IsConnected;
            IsP2PConnecting = P2PManager.IsConnecting;
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

        internal void ShowConnectTab()
        {
            JobTabControl.SelectedItem = ConnectTab;
        }

        internal void ShowHelp()
        {
            JobTabControl.SelectedItem = AboutTab;
        }
    }
}