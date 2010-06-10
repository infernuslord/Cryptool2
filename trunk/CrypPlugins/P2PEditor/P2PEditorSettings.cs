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

using System.ComponentModel;
using Cryptool.P2P;
using Cryptool.P2P.Worker;
using Cryptool.P2PEditor.GUI;
using Cryptool.PluginBase;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.P2PEditor
{
    internal class P2PEditorSettings : ISettings
    {
        private readonly P2PEditor _p2PEditor;
        private readonly P2PSettings _settings;

        private const string GroupExperienced = "experienced_settings";
        private const string GroupExpert = "expert_settings";

        public P2PEditorSettings(P2PEditor p2PEditor)
        {
            _p2PEditor = p2PEditor;
            _settings = P2PSettings.Default;
        }

        #region ISettings Members

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges { get; set; }

        #endregion

        #region Settings

        [TaskPane("username_caption",
            "username_tooltip"
            , null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string PeerName
        {
            get { return _settings.PeerName; }
            set
            {
                if (value != _settings.PeerName)
                {
                    _settings.PeerName = value;
                    OnPropertyChanged("PeerName");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("worldname_caption", "worldname_tooltip", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string WorldName
        {
            get { return _settings.WorldName; }
            set
            {
                if (value != _settings.WorldName)
                {
                    _settings.WorldName = value;
                    OnPropertyChanged("WorldName");
                    HasChanges = true;
                }
            }
        }

        // TODO New ControlType needed to choose dialogs? OpenFileDialog not fitting.
        [TaskPane("workspacePath_caption", "workspacePath_tooltip", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox)]
        public string WorkspacePath
        {
            get { return _settings.WorkspacePath; }
            set
            {
                if (value != _settings.WorkspacePath)
                {
                    _settings.WorkspacePath = value;
                    OnPropertyChanged("WorkspacePath");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("start_caption", "start_tooltip", null, 3, true, DisplayLevel.Beginner, ControlType.Button)]
        public void ButtonStart()
        {
            if (!P2PManager.IsConnected)
            {
                P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += ((P2PEditorPresentation)_p2PEditor.Presentation).ConnectionWorkerCompleted;
                P2PManager.Connect();
                OnPropertyChanged("ButtonStart");
                _p2PEditor.GuiLogMessage(Resources.Attributes.start_launched, NotificationLevel.Info);
            } else
            {
                _p2PEditor.GuiLogMessage(Resources.Attributes.start_failed, NotificationLevel.Warning);
            }
        }

        [TaskPane("stop_caption", "stop_tooltip", null, 4, true, DisplayLevel.Beginner, ControlType.Button)]
        public void ButtonStop()
        {
            if (P2PManager.IsConnected)
            {
                P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += ((P2PEditorPresentation)_p2PEditor.Presentation).ConnectionWorkerCompleted;
                P2PManager.Disconnect();
                OnPropertyChanged("ButtonStop");
                _p2PEditor.GuiLogMessage(Resources.Attributes.stop_launched, NotificationLevel.Info);
            }
            else
            {
                _p2PEditor.GuiLogMessage(Resources.Attributes.stop_failed, NotificationLevel.Warning);
            }
        }

        [TaskPane("distributedJobListRefreshInterval_caption", "distributedJobListRefreshInterval_tooltip", GroupExperienced, 0, false, DisplayLevel.Experienced,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int DistributedJobListRefreshInterval
        {
            get { return _settings.DistributedJobListRefreshInterval; }
            set
            {
                if (value != _settings.DistributedJobListRefreshInterval)
                {
                    _settings.DistributedJobListRefreshInterval = value;
                    OnPropertyChanged("DistributedJobListRefreshInterval");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("connectOnStartup_caption", "connectOnStartup_tooltip", GroupExperienced, 1, true, DisplayLevel.Experienced,
            ControlType.CheckBox)]
        public bool ConnectOnStartup
        {
            get { return _settings.ConnectOnStartup; }
            set
            {
                if (value != _settings.ConnectOnStartup)
                {
                    _settings.ConnectOnStartup = value;
                    OnPropertyChanged("ConnectOnStartup");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("linkmanager_caption", "linkmanager_tooltip", GroupExpert, 0, false, DisplayLevel.Expert,
            ControlType.ComboBox, new[] {"Snal"})]
        public int LinkManager
        {
            get { return (int) _settings.LinkManager; }
            set
            {
                if ((P2PLinkManagerType) value != _settings.LinkManager)
                {
                    _settings.LinkManager = (P2PLinkManagerType) value;
                    OnPropertyChanged("LinkManager");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("bootstrapper_caption", "bootstrapper_tooltip", GroupExpert, 1, false, DisplayLevel.Expert
            , ControlType.ComboBox, new[] {"LocalMachineBootstrapper", "IrcBootstrapper"})]
        public int Bootstrapper
        {
            get { return (int) _settings.Bootstrapper; }
            set
            {
                if ((P2PBootstrapperType) value != _settings.Bootstrapper)
                {
                    _settings.Bootstrapper = (P2PBootstrapperType) value;
                    OnPropertyChanged("Bootstrapper");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("overlay_caption", "overlay_tooltip", GroupExpert, 2, false, DisplayLevel.Expert,
            ControlType.ComboBox, new[] {"FullMeshOverlay"})]
        public int Overlay
        {
            get { return (int) _settings.Overlay; }
            set
            {
                if ((P2POverlayType) value != _settings.Overlay)
                {
                    _settings.Overlay = (P2POverlayType) value;
                    OnPropertyChanged("Overlay");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("dht_caption", "dht_tooltip", GroupExpert, 3, false, DisplayLevel.Expert,
            ControlType.ComboBox, new[] {"FullMeshDHT"})]
        public int Dht
        {
            get { return (int) _settings.Dht; }
            set
            {
                if ((P2PDHTType) value != _settings.Dht)
                {
                    _settings.Dht = (P2PDHTType) value;
                    OnPropertyChanged("Dht");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("transportprotocol_caption", "transportprotocol_tooltip", GroupExpert, 3, false, DisplayLevel.Expert,
            ControlType.ComboBox, new[] { "TCP", "TCP_UDP", "UDP" })]
        public int TransportProtocol
        {
            get { return (int)_settings.TransportProtocol; }
            set
            {
                if ((P2PTransportProtocol)value != _settings.TransportProtocol)
                {
                    _settings.TransportProtocol = (P2PTransportProtocol)value;
                    OnPropertyChanged("TransportProtocol");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("localPort_caption", "localPort_tooltip", GroupExpert, 4, false, DisplayLevel.Expert,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 65535)]
        public int LocalPort
        {
            get { return _settings.LocalReceivingPort; }
            set
            {
                if (value != _settings.LocalReceivingPort)
                {
                    _settings.LocalReceivingPort = value;
                    OnPropertyChanged("LocalPort");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("useLocalAddressDetection_caption", "useLocalAddressDetection_tooltip", GroupExpert, 5, false, DisplayLevel.Expert,
            ControlType.CheckBox)]
        public bool UseLocalAddressDetection
        {
            get { return _settings.UseLocalAddressDetection; }
            set
            {
                if (value != _settings.UseLocalAddressDetection)
                {
                    _settings.UseLocalAddressDetection = value;
                    OnPropertyChanged("UseLocalAddressDetection");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("log2monitor_caption", "log2monitor_tooltip", GroupExpert, 6, false, DisplayLevel.Expert,
            ControlType.CheckBox)]
        public bool Log2Monitor
        {
            get { return _settings.Log2Monitor; }
            set
            {
                if (value != _settings.Log2Monitor)
                {
                    _settings.Log2Monitor = value;
                    OnPropertyChanged("Log2Monitor");
                    HasChanges = true;
                }
            }
        }

        #endregion

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }

            P2PSettings.Default.Save();
        }
    }
}