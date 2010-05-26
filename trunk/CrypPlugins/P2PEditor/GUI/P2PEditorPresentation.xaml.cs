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
using Cryptool.P2P;
using Cryptool.P2P.Helper;
using Cryptool.P2P.Worker;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;

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

        public static readonly DependencyProperty DisplayLevelProperty =
            DependencyProperty.Register("DisplayLevel", typeof (DisplayLevel), typeof (P2PEditorPresentation),
                                        new UIPropertyMetadata(DisplayLevel.Expert));

        private readonly P2PEditor _p2PEditor;

        public P2PEditorPresentation(P2PEditor p2PEditor)
        {
            _p2PEditor = p2PEditor;

            InitializeComponent();

            UpdateDisplay();
            PrepareSettings();
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


        public DisplayLevel DisplayLevel
        {
            get { return (DisplayLevel) GetValue(DisplayLevelProperty); }
            set { SetValue(DisplayLevelProperty, value); }
        }

        private void PrepareSettings()
        {
            userNameTextBox.Text = P2PManager.Instance.P2PSettings.PeerName;
            worldNameTextBox.Text = P2PManager.Instance.P2PSettings.WorldName;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            P2PManager.Instance.P2PSettings.PeerName = userNameTextBox.Text;
            P2PManager.Instance.P2PSettings.WorldName = worldNameTextBox.Text;
            PAPCertificate.CheckAndInstallPAPCertificates();

            _p2PEditor.GuiLogMessage("P2P settings saved.", NotificationLevel.Info);
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            var connectionWorker = new ConnectionWorker(P2PManager.Instance.P2PBase, P2PManager.Instance.P2PSettings);
            connectionWorker.BackgroundWorker.RunWorkerCompleted += ConnectionWorkerCompleted;
            ChangeStatusControls(true);
            connectionWorker.Start();
        }

        private void ConnectionWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateDisplay();
            ChangeStatusControls(false);
            Jobs = JobListManager.JobList();
        }

        private void ChangeStatusControls(bool connecting)
        {
            connectButton.IsEnabled = !connecting;
            connectProgressBar.IsIndeterminate = connecting;
        }

        private void UpdateDisplay()
        {
            if (P2PManager.Instance.P2PConnected())
            {
                connectionStatus.Content = "Connected";
                peerID.Content = P2PManager.Instance.UserInfo();
                connectButton.Content = "Disconnect";
                IsP2PConnected = true;
            }
            else
            {
                connectionStatus.Content = "Disconnected";
                peerID.Content = "-";
                connectButton.Content = "Connect";
                IsP2PConnected = false;
            }
        }
    }
}