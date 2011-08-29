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
using System.Threading;
using System.Windows.Media.Animation;

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

            //We need this, because for a strange reason, the OnP2PConnectionStateChangeOccurred event is not triggered when reconnecting:
            P2PManager.P2PBase.OnSystemJoined += delegate
                                                        {
                                                            HandleChangedPeerToPeerConnectionState
                                                                (null, true);
                                                        };

            InitializeComponent();

            UpdateDisplay();

            this.Connect.P2PEditorPresentation = this;
            this.GetNewCertificate.P2PEditorPresentation = this;
            this.JobCreation.P2PEditorPresentation = this;
            this.JobDisplay.P2PEditorPresentation = this;

            if (this.IsP2PConnecting || !this.IsP2PConnected)
            {
                ShowConnectView();
            }
            else
            {
                ShowActiveJobsView();
            }
        }

        private void HandleChangedPeerToPeerConnectionState(object sender, bool newState)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateConnectionState));

            if (P2PManager.IsConnected)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowActiveJobsView));
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowConnectView));
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(JobDisplay.UpdateJobList));
        }

        internal void UpdateConnectionState()
        {
            IsP2PConnected = P2PManager.IsConnected;
            IsP2PConnecting = P2PManager.IsConnecting;
        }


        internal void hideAllViews()
        {
            if (JobCreation != null)
            {
                JobCreation.Visibility = Visibility.Hidden;
            }
            if (JobDisplay != null)
            {
                JobDisplay.Visibility = Visibility.Hidden;
            }
            if (Connect != null)
            {
                Connect.Visibility = Visibility.Hidden;
            }
            if (GetNewCertificate != null)
            {
                GetNewCertificate.Visibility = Visibility.Hidden;
            }
            if (ActivateEmailView != null)
            {
                ActivateEmailView.Visibility = Visibility.Hidden;
            }
            if (ForgotPasswordView != null)
            {
                ForgotPasswordView.Visibility = Visibility.Hidden;
            }
            if (VerifyPasswordResetView != null)
            {
                VerifyPasswordResetView.Visibility = Visibility.Hidden;
            }
        }

        internal void ShowGetNewCertificateView()
        {
            if (GetNewCertificate == null)
            {
                return;
            }
            hideAllViews();
            GetNewCertificate.Visibility = Visibility.Visible;            
        }

        internal void ShowActivateEmailView()
        {
            if (ActivateEmailView == null)
            {
                return;
            }
            hideAllViews();
            ActivateEmailView.Visibility = Visibility.Visible;
        }

        internal void ShowJobCreationView()
        {
            if (JobCreation == null)
            {
                return;
            }
            hideAllViews();
            JobCreation.Visibility = Visibility.Visible;
        }

        internal void ShowActiveJobsView()
        {
            if (JobDisplay == null)
            {
                return;
            }
            hideAllViews();
            JobDisplay.Visibility = Visibility.Visible;
            UpdateDisplay();
        }

        internal void ShowConnectView()
        {
            if (Connect == null)
            {
                return;
            }
            hideAllViews();
            Connect.Visibility = Visibility.Visible;         
        }

        internal void ShowForgotPasswordView()
        {
            if (ForgotPasswordView == null)
            {
                return;
            }
            hideAllViews();
            ForgotPasswordView.Visibility = Visibility.Visible;
        }

        internal void ShowVerifyPasswordResetView()
        {
            if (VerifyPasswordResetView == null)
            {
                return;
            }
            hideAllViews();
            VerifyPasswordResetView.Visibility = Visibility.Visible;
        }

        internal void ShowHelp()
        {         
            //to be implemented
        }

        private void P2PEditorControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsP2PConnecting || !this.IsP2PConnected)
            {
                ShowConnectView();
            }
            else
            {
               ShowActiveJobsView();
            }
        }
    }
}