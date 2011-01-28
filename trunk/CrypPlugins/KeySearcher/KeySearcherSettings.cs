using System;
using System.Collections.Generic;
using System.Windows;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;
using KeySearcher.P2P.Storage;
using KeySearcher.Properties;
using OpenCLNet;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private readonly KeySearcher keysearcher;
        private int coresUsed;
        private string csvPath = "";

        public class OpenCLDeviceSettings
        {
            public string name;
            public int index;
            public bool useDevice;
            public int mode;
        }

        private List<OpenCLDeviceSettings> deviceSettings = new List<OpenCLDeviceSettings>();
        public List<OpenCLDeviceSettings> DeviceSettings
        {
            get
            {
                return deviceSettings;
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public KeySearcherSettings(KeySearcher ks, OpenCLManager oclManager)
        {
            keysearcher = ks;

            devicesAvailable.Clear();
            int c = 0;
            if (oclManager != null)
                foreach (var device in oclManager.Context.Devices)
                {
                    string deviceName = device.Vendor + ":" + device.Name;
                    deviceSettings.Add(new OpenCLDeviceSettings() {name = deviceName, index = c, mode = 1, useDevice = false});
                    devicesAvailable.Add(deviceName);
                    c++;
                }

            CoresAvailable.Clear();
            for (int i = -1; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;

            chunkSize = 21;
        }

        public void Initialize()
        {
            OpenCLGroupVisiblity();
            Settings.Default.PropertyChanged += delegate
                                                    {
                                                        OpenCLGroupVisiblity();
                                                    };
        }

        private string key;
        [TaskPane("KeySettings", "KeySettingsDesc", null, 1, false, ControlType.TextBox)]
        public String Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                OnPropertyChanged("Key");
                if (!(keysearcher.Pattern != null && keysearcher.Pattern.testWildcardKey(value)))
                    keysearcher.GuiLogMessage(Resources.Wrong_key_pattern_, NotificationLevel.Error);
                HasChanges = true;
            }
        }

        [TaskPane("ResetSettings", "ResetSettingsDesc", null, 2, false, ControlType.Button)]
        public void Reset()
        {
            Key = keysearcher.Pattern.giveInputPattern();
        }
        
        [TaskPane("CoresUsedSettings", "CoresUsedSettingsDesc", null, 3, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                    HasChanges = true;
                }
            }
        }

        private bool usePeerToPeer;
        [TaskPane("UseP2PSettings", "UseP2PSettingsDesc", "GroupPeerToPeer", 0, false,
            ControlType.CheckBox)]
        public bool UsePeerToPeer
        {
            get { return usePeerToPeer; }
            set
            {
                if (value != usePeerToPeer)
                {
                    usePeerToPeer = value;
                    OnPropertyChanged("UsePeerToPeer");
                    HasChanges = true;
                }
            }
        }

        private bool autoconnectPeerToPeer;
        [TaskPane("AutoconnectSettings", "AutoconnectSettingsDesc", "GroupPeerToPeer", 1, false,
            ControlType.CheckBox)]
        public bool AutoconnectPeerToPeer
        {
            get { return autoconnectPeerToPeer; }
            set
            {
                if (value != autoconnectPeerToPeer)
                {
                    autoconnectPeerToPeer = value;
                    OnPropertyChanged("AutoconnectPeerToPeer");
                    HasChanges = true;
                }
            }
        }

        private bool verbosePeerToPeerDisplay;
        [TaskPane("DisplayVerboseInformationSettings", "DisplayVerboseInformationSettingsDesc", "GroupPeerToPeer", 2, true,
            ControlType.CheckBox)]
        public bool VerbosePeerToPeerDisplay
        {
            get { return verbosePeerToPeerDisplay; }
            set
            {
                if (value != verbosePeerToPeerDisplay)
                {
                    verbosePeerToPeerDisplay = value;
                    OnPropertyChanged("VerbosePeerToPeerDisplay");
                    HasChanges = true;
                }
            }
        }

        private int chunkSize;
        [TaskPane("ChunkSizeSettings", "ChunkSizeSettingsDesc", "GroupPeerToPeer", 3, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 1000)]
        public int ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value != chunkSize)
                {
                    chunkSize = value;
                    OnPropertyChanged("ChunkSize");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("CopyStatusKeySettings", "CopyStatusKeySettingsDesc", "GroupPeerToPeer", 4, true, ControlType.Button)]
        public void StatusKeyButton()
        {
            if (!keysearcher.IsKeySearcherRunning)
            {
                keysearcher.GuiLogMessage(Resources.KeySearcher_must_be_running_to_copy_the_status_key_, NotificationLevel.Error);
                return;
            }

            var generator = new StorageKeyGenerator(keysearcher, this);
            var statusKey = generator.GenerateStatusKey();

            Clipboard.SetDataObject(statusKey, true);
            keysearcher.GuiLogMessage(string.Format(Resources.Status_key___0___has_been_copied_to_clipboard_, statusKey),
                                      NotificationLevel.Info);
        }

        private string evaluationHost;
        [TaskPane("HostSettings", "HostSettingsDesc", "GroupEvaluation", 0, false, ControlType.TextBox)]
        public String EvaluationHost
        {
            get
            {
                return evaluationHost;
            }
            set
            {
                if (value != evaluationHost)
                {
                    evaluationHost = value;
                    OnPropertyChanged("EvaluationHost");
                    HasChanges = true;
                }
            }
        }

        private string evaluationUser;
        [TaskPane("UserSettings", "UserSettingsDesc", "GroupEvaluation", 1, false, ControlType.TextBox)]
        public String EvaluationUser
        {
            get
            {
                return evaluationUser;
            }
            set
            {
                if (value != evaluationUser)
                {
                    evaluationUser = value;
                    OnPropertyChanged("EvaluationUser");
                    HasChanges = true;
                }
            }
        }

        private string evaluationPassword;
        [TaskPane("PasswordSettings", "PasswordSettingsDesc", "GroupEvaluation", 2, false, ControlType.TextBox)]
        public String EvaluationPassword
        {
            get
            {
                return evaluationPassword;
            }
            set
            {
                if (value != evaluationPassword)
                {
                    evaluationPassword = value;
                    OnPropertyChanged("EvaluationPassword");
                    HasChanges = true;
                }
            }
        }

        private string evaluationDatabase;
        [TaskPane("DatabaseSettings", "DatabaseSettingsDesc", "GroupEvaluation", 3, false, ControlType.TextBox)]
        public String EvaluationDatabase
        {
            get
            {
                return evaluationDatabase;
            }
            set
            {
                if (value != evaluationDatabase)
                {
                    evaluationDatabase = value;
                    OnPropertyChanged("EvaluationDatabase");
                    HasChanges = true;
                }
            }
        }

        #region OpenCL

        [TaskPane("NoOpenCLSettings", null, "GroupOpenCL", 1, false, ControlType.TextBoxReadOnly)]
        public string NoOpenCL
        {
            get { return Resources.No_OpenCL_Device_available_; }
            set {}
        }

        private void OpenCLGroupVisiblity()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (!Settings.Default.UseOpenCL)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Collapsed)));
            }
            else
            {
                if (DevicesAvailable.Count == 0)
                {
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Visible)));
                }
                else
                {
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Collapsed)));
                }
            }
        }

        private int openCLDevice;
        [TaskPane("OpenCLDeviceSettings", "OpenCLDeviceSettingsDesc", "GroupOpenCL", 1, false, ControlType.DynamicComboBox, new string[] { "DevicesAvailable" })]
        public int OpenCLDevice
        {
            get { return this.openCLDevice; }
            set
            {
                if (value != this.openCLDevice)
                {
                    this.openCLDevice = value;
                    UseOpenCL = deviceSettings[value].useDevice;
                    OpenCLMode = deviceSettings[value].mode;
                    OnPropertyChanged("OpenCLDevice");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("UseSelectedDeviceSettings", "UseSelectedDeviceSettingsDesc",
            "GroupOpenCL", 2, false, ControlType.CheckBox)]
        public bool UseOpenCL
        {
            get
            {
                if (deviceSettings.Count > OpenCLDevice)
                    return deviceSettings[OpenCLDevice].useDevice;
                else
                    return false;
            }
            set
            {
                if ((deviceSettings.Count > OpenCLDevice) && (value != deviceSettings[OpenCLDevice].useDevice))
                {
                    deviceSettings[OpenCLDevice].useDevice = value;
                    hasChanges = true;
                    OnPropertyChanged("UseOpenCL");
                }
            }
        }

        [TaskPane("OpenCLModeSettings", "OpenCLModeSettingsDesc", "GroupOpenCL", 3, false, ControlType.RadioButton, new string[] { "Low Load", "Normal Load", "High Load (use with caution)" })]
        public int OpenCLMode
        {
            get
            {
                if (deviceSettings.Count > OpenCLDevice)
                    return deviceSettings[OpenCLDevice].mode;
                else
                    return 0;
            }
            set
            {
                if ((deviceSettings.Count > OpenCLDevice) && (value != deviceSettings[OpenCLDevice].mode))
                {
                    if (Settings.Default.EnableHighLoad || value != 2)
                        deviceSettings[OpenCLDevice].mode = value;
                    else
                        keysearcher.GuiLogMessage(
                            "Using \"High Load\" is disabled. Please check your CrypTool 2.0 settings.", NotificationLevel.Error);

                    OnPropertyChanged("OpenCLMode");
                    HasChanges = true;
                }
            }
        }

        private ObservableCollection<string> devicesAvailable = new ObservableCollection<string>();
        public ObservableCollection<string> DevicesAvailable
        {
            get { return devicesAvailable; }
            set
            {
                if (value != devicesAvailable)
                {
                    devicesAvailable = value;
                }
                OnPropertyChanged("DevicesAvailable");
            }
        }

        #endregion

        #region external client

        private bool useExternalClient = false;
        [TaskPane("UseExternalClientSettings", "UseExternalClientSettingsDesc", 
            "GroupExternalClient", 1, false, ControlType.CheckBox)]
        public bool UseExternalClient
        {
            get { return useExternalClient; }
            set
            {
                if (value != useExternalClient)
                {
                    useExternalClient = value;
                    hasChanges = true;
                    OnPropertyChanged("UseExternalClient");
                }
            }
        }

        private int port = 6234;
        [TaskPane("PortSettings", "PortSettingsDesc", "GroupExternalClient", 2, false, ControlType.TextBox)]
        public int Port
        {
            get { return port; }
            set
            {
                if (value != port)
                {
                    port = value;
                    hasChanges = true;
                    OnPropertyChanged("Port");
                }
            }
        }

        private String externalClientPassword = "";
        [TaskPane("ExternalClientPasswordSettings", "ExternalClientPasswordSettingsDesc", "GroupExternalClient", 3, false, ControlType.TextBoxHidden)]
        public String ExternalClientPassword
        {
            get { return externalClientPassword; }
            set
            {
                if (value != externalClientPassword)
                {
                    externalClientPassword = value;
                    hasChanges = true;
                    OnPropertyChanged("ExternalClientPassword");
                }
            }
        }
        #endregion

        #region csv path
        /// <summary>
        /// Getter/Setter for the csv file
        /// </summary>
        [TaskPane("CSVPathSettings", "CSVPathSettings", "GroupStatisticPath", 1, false, ControlType.SaveFileDialog, FileExtension = "Comma Seperated Values (*.csv)|*.csv")]
        public string CsvPath
        {
            get { return csvPath; }
            set
            {
                if (value != csvPath)
                {
                    csvPath = value;
                    HasChanges = true;
                    OnPropertyChanged("CsvPath");
                }
            }
        }

        /// <summary>
        /// Button to "reset" the csv file. That means it will not appear any more in the text field
        /// </summary>
        [TaskPane("DefaultPathSettings", "DefaultPathSettingsDesc", "GroupStatisticPath", 2, false, ControlType.Button)]
        public void DefaultPath()
        {
            csvPath = "";
            OnPropertyChanged("CsvPath");
        }
        #endregion

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                }
                OnPropertyChanged("CoresAvailable");
            }
        }

        #region ISettings Members

        private bool hasChanges;

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
