﻿/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cryptool.Core;
using Cryptool.CrypWin.Helper;
using Cryptool.CrypWin.Properties;
using Cryptool.CrypWin.Resources;
using Cryptool.P2P;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using WorkspaceManager;
using CrypWin.Helper;
using DevComponents.WpfRibbon;
using Microsoft.Win32;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Generators.LaTeXGenerator;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Control = System.Windows.Controls.Control;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;
using TabControl = System.Windows.Controls.TabControl;
using ToolTip = System.Windows.Controls.ToolTip;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class MainWindow : DevComponents.WpfRibbon.RibbonWindow
    {

        #region private variables
        private List<NotificationLevel> listFilter = new List<NotificationLevel>();
        private ObservableCollection<LogMessage> collectionLogMessages = new ObservableCollection<LogMessage>();
        private PluginManager pluginManager;
        private Dictionary<string, List<Type>> loadedTypes;
        private int numberOfLoadedTypes = 0;
        private int initCounter;
        private Dictionary<CTTabItem, object> tabToContentMap = new Dictionary<CTTabItem, object>();
        private Dictionary<object, CTTabItem> contentToTabMap = new Dictionary<object, CTTabItem>();
        private Dictionary<object, IEditor> contentToParentMap = new Dictionary<object, IEditor>();
        private TabItem lastTab = null;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool closingCausedMinimization = false;
        private WindowState oldWindowState;
        private bool restart = false;
        private bool shutdown = false;
        private string personalDir;
        private IEditor lastEditor = null;
        private SystemInfos systemInfos = new SystemInfos();
        private LicensesTab licenses = new LicensesTab();
        private System.Windows.Forms.MenuItem playStopMenuItem;
        private EditorTypePanelManager editorTypePanelManager = new EditorTypePanelManager();
        private System.Windows.Forms.Timer hasChangesCheckTimer;
        private X509Certificate serverTlsCertificate1;
        private X509Certificate serverTlsCertificate2;

        private Dictionary<IEditor, string> editorToFileMap = new Dictionary<IEditor, string>();
        private string ProjectFileName
        {
            get
            {
                if (ActiveEditor != null && editorToFileMap.ContainsKey(ActiveEditor))
                    return editorToFileMap[ActiveEditor];
                else
                    return null;
            }
            set
            {
                if (ActiveEditor != null)
                    editorToFileMap[ActiveEditor] = value;
            }
        }

        private bool dragStarted;
        private Splash splashWindow;
        private bool startUpRunning = true;
        private string defaultTemplatesDirectory = "";
        private bool silent = false;
        private List<IPlugin> listPluginsAlreadyInitialized = new List<IPlugin>();
        private string[] interfaceNameList = new string[] {
                typeof(Cryptool.PluginBase.ICrypComponent).FullName,
                typeof(Cryptool.PluginBase.Editor.IEditor).FullName,
                typeof(Cryptool.PluginBase.ICrypTutorial).FullName };
        #endregion

        public IEditor ActiveEditor
        {
            get
            {
                if (MainSplitPanel == null)
                    return null;
                if (MainSplitPanel.Children.Count == 0)
                    return null;
                CTTabItem selectedTab = (CTTabItem)(MainTab.SelectedItem);
                if (selectedTab == null)
                    return null;

                if (tabToContentMap.ContainsKey(selectedTab))
                {
                    if (tabToContentMap[selectedTab] is IEditor)
                    {
                        return (IEditor)(tabToContentMap[selectedTab]);
                    }
                    else if (contentToParentMap.ContainsKey(tabToContentMap[selectedTab]) && (contentToParentMap[tabToContentMap[selectedTab]] != null))
                    {
                        return (IEditor)contentToParentMap[tabToContentMap[selectedTab]];
                    }
                }

                return null;
            }

            set
            {
                AddEditor(value);
            }
        }

        public IPlugin ActivePlugin
        {
            get
            {
                if (MainSplitPanel == null)
                    return null;
                if (MainSplitPanel.Children.Count == 0)
                    return null;
                CTTabItem selectedTab = (CTTabItem)(MainTab.SelectedItem);
                if (selectedTab == null)
                    return null;

                if (tabToContentMap.ContainsKey(selectedTab))
                {
                    if (tabToContentMap[selectedTab] is IPlugin)
                    {
                        return (IPlugin)(tabToContentMap[selectedTab]);
                    }
                }

                return null;
            }
        }

        public static readonly DependencyProperty AvailableEditorsProperty =
            DependencyProperty.Register(
            "AvailableEditors",
            typeof(List<Type>),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(new List<Type>(), FrameworkPropertyMetadataOptions.None, null));

        [TypeConverter(typeof(List<Type>))]
        public List<Type> AvailableEditors
        {
            get
            {
                return (List<Type>)GetValue(AvailableEditorsProperty);
            }
            private set
            {
                SetValue(AvailableEditorsProperty, value);
            }
        }

        public static readonly DependencyProperty VisibilityStartProperty =
              DependencyProperty.Register(
              "VisibilityStart",
              typeof(bool),
              typeof(MainWindow),
              new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(bool))]
        public bool VisibilityStart
        {
            get { return (bool)GetValue(VisibilityStartProperty); }
            set
            {
                SetValue(VisibilityStartProperty, value);
            }
        }

        internal static void SaveSettingsSavely()
        {
            try
            {
                Settings.Default.Save();
            }
            catch(Exception ex1)
            {
                try
                {
                    Settings.Default.Save();
                }
                catch(Exception ex2)
                {
                    //try saving two times, then do not try it again
                } 
            }
        }

        private bool IsUpdaterEnabled
        {
            get { return AssemblyHelper.BuildType != Ct2BuildType.Developer && !IsCommandParameterGiven("-noupdate"); }
            // for testing the autoupdater with the developer edition comment the above line and uncomment the following
            //get { return !IsCommandParameterGiven("-noupdate"); }
        }

        #region Init

        public MainWindow()
        {
            SetLanguage();
            LoadResources();

            if (AssemblyHelper.InstallationType == Ct2InstallationType.ZIP)
            {
                UnblockDLLs();
            }

            // will exit application after doc has been generated
            if (IsCommandParameterGiven("-GenerateDoc"))
            {
                try
                {
                    var generatingDocWindow = new GeneratingWindow();
                    generatingDocWindow.Message.Content = Properties.Resources.GeneratingWaitMessage;
                    generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;
                    generatingDocWindow.Show();
                    var docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                    docGenerator.Generate(DirectoryHelper.BaseDirectory, new HtmlGenerator());
                    generatingDocWindow.Close();
                }
                catch(Exception ex)
                {
                    //wtf?    
                }
                Environment.Exit(0);
            }
            if (IsCommandParameterGiven("-GenerateDocLaTeX"))
            {
                try
                {
                    var noIcons = IsCommandParameterGiven("-NoIcons");
                    var showAuthors = IsCommandParameterGiven("-ShowAuthors");
                    var generatingDocWindow = new GeneratingWindow();
                    generatingDocWindow.Message.Content = Properties.Resources.GeneratingLaTeXWaitMessage;
                    generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;                                                              
                    generatingDocWindow.Show();
                    var docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                    docGenerator.Generate(DirectoryHelper.BaseDirectory, new LaTeXGenerator("de", noIcons, showAuthors));
                    docGenerator.Generate(DirectoryHelper.BaseDirectory, new LaTeXGenerator("en", noIcons, showAuthors));
                    generatingDocWindow.Close();                    
                }
                catch(Exception ex)
                {
                    //wtf?    
                }
                Environment.Exit(0);
            }

            defaultTemplatesDirectory = Path.Combine(DirectoryHelper.BaseDirectory, Settings.Default.SamplesDir);
            if (!Directory.Exists(defaultTemplatesDirectory))
            {
                GuiLogMessage("Directory with project templates not found", NotificationLevel.Debug);
                defaultTemplatesDirectory = personalDir;
            }

            if (IsCommandParameterGiven("-GenerateComponentConnectionStatistics"))
            {
                try
                {
                    var generatingComponentConnectionStatistic = new GeneratingWindow();
                    generatingComponentConnectionStatistic.Message.Content = Properties.Resources.StatisticsWaitMessage;
                    generatingComponentConnectionStatistic.Title = Properties.Resources.Generating_Statistics_Title;
                    generatingComponentConnectionStatistic.Show();
                    TemplatesAnalyzer.GenerateStatisticsFromTemplate(defaultTemplatesDirectory);
                    SaveComponentConnectionStatistics();
                    generatingComponentConnectionStatistic.Close();
                }catch(Exception ex)
                {
                    //wtf?
                }
                Environment.Exit(0);
            }

            if (!EstablishSingleInstance())
            {
                Environment.Exit(0);
            }

            // check whether update is available to be installed
            if (IsUpdaterEnabled
                && CheckCommandProjectFileGiven().Count == 0 // NO project file given as command line argument
                && IsUpdateFileAvailable()) // update file ready for install
            {
                // really start the update process?
                if (Settings.Default.AutoInstall || AskForInstall())
                {
                    // start update and check whether it seems to succeed
                    if (OnUpdate())
                        return; // looking good, exit CrypWin constructor now
                }
            }

            //upgrade the config
            //and fill some defaults

            if (Settings.Default.UpdateFlag)
            {
                Console.WriteLine("Upgrading config ...");
                Settings.Default.Upgrade();                
                Cryptool.PluginBase.Properties.Settings.Default.Upgrade();
                //upgrade p2p settings                    
                Cryptool.P2P.P2PSettings.Default.Upgrade();
                //upgrade WorkspaceManagerModel settings
                WorkspaceManagerModel.Properties.Settings.Default.Upgrade();
                //upgrade Crypwin settings
                Cryptool.CrypWin.Properties.Settings.Default.Upgrade();
                //upgrade Crypcore settings
                Cryptool.Core.Properties.Settings.Default.Upgrade();
                //upgrade MainWindow settings
                Settings.Default.Upgrade();
                //remove UpdateFlag
                Settings.Default.UpdateFlag = false;
            }

            StartCenter.StartcenterEditor.StartupBehaviourChanged += (showOnStartup) =>
            {
                Properties.Settings.Default.ShowStartcenter = showOnStartup;
            };

            try
            {
                personalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrypTool 2 Projects");
                if (!Directory.Exists(personalDir))
                {
                    Directory.CreateDirectory(personalDir);
                }
            }
            catch (Exception ex)
            {
                // minor error, ignore
                GuiLogMessage("Could not create personal dir: " + ex.Message, NotificationLevel.Debug);
            }

            if (string.IsNullOrEmpty(Settings.Default.LastPath) || !Settings.Default.useLastPath || !Directory.Exists(Settings.Default.LastPath))
            {
                Settings.Default.LastPath = personalDir;
            }

            SaveSettingsSavely();

            ComponentConnectionStatistics.OnGuiLogMessageOccured += GuiLogMessage;
            ComponentConnectionStatistics.OnStatisticReset += GenerateStatisticsFromTemplates;

            recentFileList.ListChanged += RecentFileListChanged;

            this.Activated += MainWindow_Activated;
            this.Initialized += MainWindow_Initialized;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            this.demoController = new DemoController(this);
            this.InitializeComponent();

            if ((System.Windows.Visibility)Enum.Parse(typeof(System.Windows.Visibility), Properties.Settings.Default.SettingVisibility) == System.Windows.Visibility.Visible)
            {
                SettingBTN.IsChecked = true;
                dockWindowAlgorithmSettings.Open();
            }
            else
            {
                SettingBTN.IsChecked = false;
                dockWindowAlgorithmSettings.Close();
            }

            if ((System.Windows.Visibility)Enum.Parse(typeof(System.Windows.Visibility), Properties.Settings.Default.PluginVisibility) == System.Windows.Visibility.Visible)
            {
                PluginBTN.IsChecked = true;
                dockWindowNaviPaneAlgorithms.Open();
            }
            else
            {
                PluginBTN.IsChecked = false;
                dockWindowNaviPaneAlgorithms.Close();
            }

            if ((System.Windows.Visibility)Enum.Parse(typeof(System.Windows.Visibility), Properties.Settings.Default.LogVisibility) == System.Windows.Visibility.Visible)
            {
                LogBTN.IsChecked = true;
                dockWindowLogMessages.Open();
            }
            else
            {
                LogBTN.IsChecked = false;
                dockWindowLogMessages.Close();
            }

            if (IsCommandParameterGiven("-demo") || IsCommandParameterGiven("-test"))
            {
                ribbonDemoMode.Visibility = Visibility.Visible;
                PluginExtension.IsTestMode = true;
                LocExtension.OnGuiLogMessageOccured += GuiLogMessage;
            }

            VisibilityStart = true;

            oldWindowState = WindowState;

            RecentFileListChanged();

            CreateNotifyIcon();

            if (IsUpdaterEnabled)
                InitUpdater();
            else
                autoUpdateButton.Visibility = Visibility.Collapsed; // hide update button in ribbon
                

            if (!Settings.Default.ShowRibbonBar)
                AppRibbon.IsEnabled = false;
            if (!Settings.Default.ShowAlgorithmsNavigation)
                splitPanelNaviPaneAlgorithms.Visibility = Visibility.Collapsed;
            if (!Settings.Default.ShowAlgorithmsSettings)
                splitPanelAlgorithmSettings.Visibility = Visibility.Collapsed;

            if (P2PManager.IsP2PSupported)
            {
                InitP2P();
            }

            OnlineHelp.ShowDocPage += ShowHelpPage;

            SettingsPresentation.GetSingleton().OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(OnGuiLogNotificationOccured);
            Settings.Default.PropertyChanged += delegate(Object sender, PropertyChangedEventArgs e)
            {
                //Always save everything immediately:
                SaveSettingsSavely();

                //Set new button image when needed:
                CheckPreferredButton(e);

                //Set lastPath to personal directory when lastPath is disabled:
                if (e.PropertyName == "useLastPath" && !Settings.Default.useLastPath)
                    Settings.Default.LastPath = personalDir;
            };

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            hasChangesCheckTimer = new System.Windows.Forms.Timer(); 
            hasChangesCheckTimer.Tick += new EventHandler(delegate 
 		    { 
 		        if (ActiveEditor != null)
                    ((CTTabItem)(contentToTabMap[ActiveEditor])).HasChanges = ActiveEditor.HasChanges ? true : false; 
 		    }); 
            hasChangesCheckTimer.Interval = 800; 
            hasChangesCheckTimer.Start();

            if (IsCommandParameterGiven("-ResetConfig"))
            {
                GuiLogMessage("\"ResetConfig\" startup parameter set. Resetting configuration of CrypTool 2 to default configuration", NotificationLevel.Info);
                try
                {
                    //Reset all plugins settings
                    Cryptool.PluginBase.Properties.Settings.Default.Reset();
                    //Reset p2p settings                    
                    Cryptool.P2P.P2PSettings.Default.Reset();
                    //Reset WorkspaceManagerModel settings
                    WorkspaceManagerModel.Properties.Settings.Default.Reset();
                    //reset Crypwin settings
                    Cryptool.CrypWin.Properties.Settings.Default.Reset();
                    //reset Crypcore settings
                    Cryptool.Core.Properties.Settings.Default.Reset();
                    //reset MainWindow settings
                    Settings.Default.Reset();                    
                    GuiLogMessage("Settings successfully set to default configuration", NotificationLevel.Info);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error occured while resetting configration: {0}",ex), NotificationLevel.Info);
                }                
            }

            //Load our certificates &
            //Install validation callback
            try
            {
                // 18.12.12, AW: Not needed anymore.
                //serverTlsCertificate1 = new System.Security.Cryptography.X509Certificates.X509Certificate(global::Cryptool.CrypWin.Properties.Resources.www_cryptool_org);
                //serverTlsCertificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate(global::Cryptool.CrypWin.Properties.Resources.old_www_cryptool_org);

                ServicePointManager.ServerCertificateValidationCallback = UpdateServerCertificateValidationCallback;
      
            }
            catch (Exception ex)
            {
                //GuiLogMessage(string.Format("Error occured while loading certificates: {0}", ex), NotificationLevel.Error);
                GuiLogMessage(string.Format("Error while initializing the certificate callback: {0}", ex), NotificationLevel.Error);
            }
        }

        private void LoadIndividualComponentConnectionStatistics()
        {
            //Load component connection statistics if available, or generate them:
            try
            {
                ComponentConnectionStatistics.LoadCurrentStatistics(Path.Combine(DirectoryHelper.DirectoryLocal, "ccs.xml"));
            }
            catch (Exception ex)
            {
                try
                {
                    ComponentConnectionStatistics.LoadCurrentStatistics(Path.Combine(DirectoryHelper.BaseDirectory, "ccs.xml"));
                }
                catch (Exception ex2)
                {
                    GuiLogMessage("No component connection statistics found... Generate them.", NotificationLevel.Info);
                    GenerateStatisticsFromTemplates();
                }
            }
        }

        private void GenerateStatisticsFromTemplates()
        {
            var generatingComponentConnectionStatistic = new GeneratingWindow();
            generatingComponentConnectionStatistic.Message.Content = Properties.Resources.StatisticsWaitMessage;
            generatingComponentConnectionStatistic.Title = Properties.Resources.Generating_Statistics_Title;
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/CrypWin;component/images/statistics_icon.png");
            icon.EndInit();
            generatingComponentConnectionStatistic.Icon.Source = icon;
            generatingComponentConnectionStatistic.ContentRendered += delegate
            {
                TemplatesAnalyzer.GenerateStatisticsFromTemplate(defaultTemplatesDirectory);
                SaveComponentConnectionStatistics();
                GuiLogMessage("Component connection statistics successfully generated from templates.", NotificationLevel.Info);
                generatingComponentConnectionStatistic.Close();
            };
            generatingComponentConnectionStatistic.ShowDialog();
        }

        private bool EstablishSingleInstance()
        {
            try
            {
                bool createdNew;
                MD5 md5 = new MD5CryptoServiceProvider();
                var id =
                    BitConverter.ToInt32(md5.ComputeHash(Encoding.ASCII.GetBytes(Environment.GetCommandLineArgs()[0])),
                                         0);
                var identifyingString = string.Format("Local\\CrypTool 2.0 ID {0}", id);
                _singletonMutex = new Mutex(true, identifyingString, out createdNew);

                if (createdNew)
                {
                    var queueThread = new Thread(QueueThread);
                    queueThread.IsBackground = true;
                    queueThread.Start(identifyingString);
                }
                else
                {
                    //CT2 instance already exists... send files to it and shutdown
                    using (var pipeClient = new NamedPipeClientStream(".", identifyingString, PipeDirection.Out))
                    {
                        pipeClient.Connect();
                        using (var sw = new StreamWriter(pipeClient))
                        {
                            foreach (var file in CheckCommandProjectFileGiven())
                            {
                                sw.WriteLine(file);
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                GuiLogMessage(String.Format("Error during EstablishSingleInstance: {0}",ex.Message),NotificationLevel.Warning);
                return false;
            }
        }

        private void QueueThread(object identifyingString)
        {
            while (true)
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream((string)identifyingString, PipeDirection.In))
                    {
                    
                        pipeServer.WaitForConnection();
                        BringToFront();
                        using (var sr = new StreamReader(pipeServer))
                        {
                            string file;
                            do
                            {
                                file = sr.ReadLine();
                                if (!string.IsNullOrEmpty(file))
                                {
                                    string theFile = file;
                                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        GuiLogMessage(string.Format(Resource.workspace_loading, theFile), NotificationLevel.Info);
                                        OpenProject(theFile, null);
                                    }, null);
                                }
                            } while (!string.IsNullOrEmpty(file));
                        }
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while maintaning single instance: {0}", ex.Message), NotificationLevel.Error);
                }
            }
        }

        private void BringToFront()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                WindowState = WindowState.Normal;
                Activate();
            }, null);
        }

        private bool UpdateServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                GuiLogMessage("CrypWin: Could not validate certificate, as it is null! Aborting connection.", NotificationLevel.Error);
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                GuiLogMessage("CrypWin: Could not validate TLS certificate, as the server did not provide one! Aborting connection.", NotificationLevel.Error);
                return false;
            }


            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                GuiLogMessage("CrypWin: Certificate name mismatch (certificate not for www.cryptool.org?). Aborting connection.", NotificationLevel.Error);
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                GuiLogMessage("CrypWin: Certificate-chain could not be validated (using self-signed certificate?)! Aborting connection.", NotificationLevel.Error);
                return false;
            }

            // Catch any other SSLPoliy errors - should never happen, as all oerror are captured before, but to be on the safe side.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                GuiLogMessage("CrypWin: General SSL/TLS policy error: " + sslPolicyErrors.ToString() + " Aborting connection.", NotificationLevel.Error);
                return false;
            }

            // 18.12.2012, AW: Decided to skip the following check. It is not required, as we use official certifcates with valid chains, hence wrong 
            // certificates should be detected with the checks above.

            // Why do we check this?
            // Check equality of remote and local certificate
            // check for current and new certificate, in case server-certificate is changed
            //if (!(certificate.Equals(this.serverTlsCertificate1) | certificate.Equals(this.serverTlsCertificate2)))
            //{
            //    GuiLogMessage("CrypWin: Received TLS certificate is not the expected certificate: Equality check failed!", NotificationLevel.Error);
            //    return false;
            //}

            GuiLogMessage("CrypWin: The webserver serving the URL " + ((HttpWebRequest)sender).Address.ToString() + " provided a valid SSL/TLS certificate. We trust it." + Environment.NewLine + 
                "Certificate:  " + certificate.Subject + Environment.NewLine +
                "Issuer: " + certificate.Issuer, 
                NotificationLevel.Debug);
            
            return true;
        }

        private void SetLanguage()
        {
            var lang = GetCommandParameter("-lang");    //Check if language parameter is given
            if (lang != null)
            {
                switch (lang.ToLower())
                {
                    case "de":
                        Settings.Default.Culture = CultureInfo.CreateSpecificCulture("de-DE").TextInfo.CultureName;
                        break;
                    case "en":
                        Settings.Default.Culture = CultureInfo.CreateSpecificCulture("en-US").TextInfo.CultureName;
                        break;
                }
            }

            var culture = Settings.Default.Culture;
            if (!string.IsNullOrEmpty(culture))
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                }
                catch (Exception)
                {
                }
            }
        }

        private void CheckPreferredButton(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "useDefaultEditor" || e.PropertyName == "preferredEditor" || e.PropertyName == "defaultEditor")
            {
                string checkEditor;
                if (!Settings.Default.useDefaultEditor)
                {
                    checkEditor = Settings.Default.preferredEditor;
                }
                else
                {
                    checkEditor = Settings.Default.defaultEditor;
                }
                foreach (ButtonDropDown editorButtons in buttonDropDownNew.Items)
                {
                    Type type = (Type)editorButtons.Tag;
                    editorButtons.IsChecked = (type.FullName == checkEditor);
                    if (editorButtons.IsChecked)
                        ((Image)buttonDropDownNew.Image).Source = ((Image)editorButtons.Image).Source;
                }
            }
        }

        private void PlayStopMenuItemClicked(object sender, EventArgs e)
        {
            if (ActiveEditor == null)
                return;

            if (ActiveEditor.CanStop && !(bool)playStopMenuItem.Tag)
            {
                ActiveEditor.Stop();
                playStopMenuItem.Text = "Start";
                playStopMenuItem.Tag = true;
            }
            else if (ActiveEditor.CanExecute && (bool)playStopMenuItem.Tag)
            {
                ActiveEditor.Execute();
                playStopMenuItem.Text = "Stop";
                playStopMenuItem.Tag = false;
            }
        }

        void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            WindowState = oldWindowState;
        }

        private void LoadResources()
        {
            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/CrypWin;Component/Resources/GridViewStyle.xaml", UriKind.Relative)));

            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/CrypWin;Component/Resources/ValidationRules.xaml", UriKind.Relative)));

            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/CrypWin;Component/Resources/BlackTheme.xaml", UriKind.Relative)));

            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/CrypWin;Component/Resources/Expander.xaml", UriKind.Relative)));

            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
            new Uri("/CrypWin;Component/Resources/ToggleButton.xaml", UriKind.Relative)));

        }

        /// <summary>
        /// Called when window goes to foreground.
        /// </summary>
        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (startUpRunning && splashWindow != null)
            {
                splashWindow.Activate();
            }
            else if (!startUpRunning)
            {
                this.Activated -= MainWindow_Activated;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (ActiveEditor != null)
                        ActiveEditor.Presentation.Focus();
                }, null);
            }
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            PluginExtension.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;

            HashSet<string> disabledAssemblies = new HashSet<string>();
            if (Settings.Default.DisabledPlugins != null)
                foreach (PluginInformation disabledPlugin in Settings.Default.DisabledPlugins)
                {
                    disabledAssemblies.Add(disabledPlugin.Assemblyname);
                }
            this.pluginManager = new PluginManager(disabledAssemblies);
            this.pluginManager.OnExceptionOccured += pluginManager_OnExceptionOccured;
            this.pluginManager.OnDebugMessageOccured += pluginManager_OnDebugMessageOccured;
            this.pluginManager.OnPluginLoaded += pluginManager_OnPluginLoaded;

            // Initialize P2PManager
            if (P2PManager.IsP2PSupported)
            {
                ValidateAndSetupPeer2Peer();
            }

            # region GUI stuff without plugin access

            naviPane.SystemText.CollapsedPaneText = Properties.Resources.Classic_Ciphers;
            this.RibbonControl.SystemText.QatPlaceBelowRibbonText = Resource.show_quick_access_toolbar_below_the_ribbon;

            // standard filter
            listViewLogList.ItemsSource = collectionLogMessages;
            listFilter.Add(NotificationLevel.Info);
            listFilter.Add(NotificationLevel.Warning);
            listFilter.Add(NotificationLevel.Error);
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            view.Filter = new Predicate<object>(FilterCallback);

            // Set user view
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (Settings.Default.IsWindowMaximized || Settings.Default.RelWidth >= 1 || Settings.Default.RelHeight >= 1)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                Width = System.Windows.SystemParameters.PrimaryScreenWidth * Settings.Default.RelWidth;
                Height = System.Windows.SystemParameters.PrimaryScreenHeight * Settings.Default.RelHeight;
            }
            dockWindowLogMessages.IsAutoHide = Settings.Default.logWindowAutoHide;

            this.IsEnabled = false;
            splashWindow = new Splash();
            if (!IsCommandParameterGiven("-nosplash"))
            {
                splashWindow.Show();
            }
            # endregion

            AsyncCallback asyncCallback = new AsyncCallback(LoadingPluginsFinished);
            LoadPluginsDelegate loadPluginsDelegate = new LoadPluginsDelegate(this.LoadPlugins);
            loadPluginsDelegate.BeginInvoke(asyncCallback, null);
        }

        private bool IsCommandParameterGiven(string parameter)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].Equals(parameter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        private string GetCommandParameter(string parameter)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length-1; i++)
            {
                if (args[i].Equals(parameter, StringComparison.InvariantCultureIgnoreCase))
                    return args[i+1];
            }

            return null;
        }

        private void InitTypes(Dictionary<string, List<Type>> dicTypeLists)
        {
            // process ICrypComponent (regular editor plugins)
            InitCrypComponents(dicTypeLists[typeof(ICrypComponent).FullName]);

            // process ICrypTutorial (former standalone plugins)
            InitCrypTutorials(dicTypeLists[typeof(ICrypTutorial).FullName]);

            // process IEditor
            InitCrypEditors(dicTypeLists[typeof(IEditor).FullName]);
        }

        private void InitCrypComponents(List<Type> typeList)
        {
            foreach (Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();
                if (pia == null)
                {
                    GuiLogMessage(string.Format(Resource.no_plugin_info_attribute, type.Name), NotificationLevel.Error);
                    continue;
                }

                foreach (ComponentCategoryAttribute attr in type.GetComponentCategoryAttributes())
                {
                    GUIContainerElementsForPlugins cont = null;

                    switch (attr.Category)
                    {
                        case ComponentCategory.CiphersClassic:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemClassic, navListBoxClassic, Properties.Resources.Classic_Ciphers);
                            break;
                        case ComponentCategory.CiphersModernSymmetric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemModernCiphers, navListBoxModernCiphersSymmetric, Properties.Resources.Symmetric);
                            break;
                        case ComponentCategory.CiphersModernAsymmetric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemModernCiphers, navListBoxModernCiphersAsymmetric, Properties.Resources.Asymmetric);
                            break;
                        case ComponentCategory.Steganography:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemSteganography, navListBoxSteganography, Properties.Resources.Steganography);
                            break;
                        case ComponentCategory.HashFunctions:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemHash, navListBoxHashFunctions, Properties.Resources.Hash_Functions_);
                            break;
                        case ComponentCategory.CryptanalysisSpecific:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemCryptanalysis, navListBoxCryptanalysisSpecific, Properties.Resources.Specific);
                            break;
                        case ComponentCategory.CryptanalysisGeneric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemCryptanalysis, navListBoxCryptanalysisGeneric, Properties.Resources.Generic);
                            break;
                        case ComponentCategory.Protocols:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemProtocols, navListBoxProtocols, Properties.Resources.Protocols);
                            break;
                        case ComponentCategory.ToolsBoolean:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsBoolean, Properties.Resources.Boolean);
                            break;
                        case ComponentCategory.ToolsDataflow:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsDataflow, Properties.Resources.Dataflow);
                            break;
                        case ComponentCategory.ToolsDataInputOutput:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsData, Properties.Resources.DataInputOutput);
                            break;
                        case ComponentCategory.ToolsMisc:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsMisc, Properties.Resources.Misc);
                            break;
                        case ComponentCategory.ToolsP2P:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsP2P, Properties.Resources.PeerToPeer);
                            break;
                        default:
                            GuiLogMessage(string.Format("Category {0} of plugin {1} not handled in CrypWin", attr.Category, pia.Caption), NotificationLevel.Error);
                            break;
                    }

                    if (cont != null)
                        AddPluginToNavigationPane(cont);
                }

                SendAddedPluginToGUIMessage(pia.Caption);
            }
        }

        private void InitCrypTutorials(List<Type> typeList)
        {
            if (typeList.Count > 0)
                SetVisibility(ribbonTabView, Visibility.Visible);

            foreach(Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();
                if (pia == null)
                {
                    GuiLogMessage(string.Format(Resource.no_plugin_info_attribute, type.Name), NotificationLevel.Error);
                    continue;
                }

                Type typeClosure = type;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    var button = new ButtonDropDown();
                    button.Header = pia.Caption;
                    button.ToolTip = pia.ToolTip;
                    button.Image = typeClosure.GetImage(0, 64, 40);
                    button.ImageSmall = typeClosure.GetImage(0, 20, 16);
                    button.ImagePosition = eButtonImagePosition.Left;
                    button.Tag = typeClosure;
                    button.Style = (Style)FindResource("AppMenuCommandButton");
                    button.Height = 65;

                    button.Click += buttonTutorial_Click;

                    ribbonBarTutorial.Items.Add(button);
                }, null);

                SendAddedPluginToGUIMessage(pia.Caption);
            }
        }

        // CrypTutorial ribbon bar clicks
        private void buttonTutorial_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonDropDown;
            if (button == null)
                return;

            Type type = button.Tag as Type;
            if (type == null)
                return;

            //CrypTutorials are singletons:
            foreach (var tab in contentToTabMap.Where(x => x.Key.GetType() == type))
            {
                tab.Value.IsSelected = true;
                return;
            }

            var content = type.CreateTutorialInstance();
            if (content == null)
                return;

            OpenTab(content, new TabInfo() { Title = type.GetPluginInfoAttribute().Caption, 
                Icon = content.GetType().GetImage(0).Source,
                Tooltip = new Span(new Run(content.GetPluginInfoAttribute().ToolTip))}, null);
            //content.Presentation.ToolTip = type.GetPluginInfoAttribute().ToolTip;
        }

        private void InitCrypEditors(List<Type> typeList)
        {
            foreach (Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();

                // We dont't display a drop down button while only one editor is available
                if (typeList.Count > 1)
                {
                    var editorInfo = type.GetEditorInfoAttribute();
                    if (editorInfo != null && !editorInfo.ShowAsNewButton)
                        continue;

                    Type typeClosure = type;
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ButtonDropDown btn = new ButtonDropDown();
                        btn.Header = typeClosure.GetPluginInfoAttribute().Caption;
                        btn.ToolTip = typeClosure.GetPluginInfoAttribute().ToolTip;
                        btn.Image = typeClosure.GetImage(0);
                        btn.Tag = typeClosure;
                        btn.IsCheckable = true;
                        if ((Settings.Default.useDefaultEditor && typeClosure.FullName == Settings.Default.defaultEditor)
                            || (!Settings.Default.useDefaultEditor && typeClosure.FullName == Settings.Default.preferredEditor))
                        {
                            btn.IsChecked = true;
                            ((Image)buttonDropDownNew.Image).Source = ((Image)btn.Image).Source;
                        }

                        btn.Click += buttonEditor_Click;
                        //buttonDropDownEditor.Items.Add(btn);
                        buttonDropDownNew.Items.Add(btn);
                        AvailableEditors.Add(typeClosure);
                    }, null);
                }
            }

            if (typeList.Count <= 1)
                SetVisibility(buttonDropDownNew, Visibility.Collapsed);
        }

        private void buttonEditor_Click(object sender, RoutedEventArgs e)
        {
            IEditor editor = AddEditorDispatched(((sender as Control).Tag as Type));
            editor.PluginManager = this.pluginManager;
            Settings.Default.defaultEditor = ((sender as Control).Tag as Type).FullName;
            ButtonDropDown button = sender as ButtonDropDown;

            if (Settings.Default.useDefaultEditor)
            {
                ((Image)buttonDropDownNew.Image).Source = ((Image)button.Image).Source;
                foreach (ButtonDropDown btn in buttonDropDownNew.Items)
                {
                    if (btn != button)
                        btn.IsChecked = false;
                }
            }
            else
            {
                button.IsChecked = (Settings.Default.preferredEditor == ((Type)button.Tag).FullName);
            }
        }

        private void SetVisibility(UIElement element, Visibility vis)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                element.Visibility = vis;
            }, null);
        }

        /// <summary>
        /// Method is invoked after plugin manager has finished loading plugins and 
        /// CrypWin is building the plugin entries. Hence 50% is added to progess here.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        private void SendAddedPluginToGUIMessage(string plugin)
        {
            initCounter++;
            splashWindow.ShowStatus(string.Format(Properties.Resources.Added_plugin___0__, plugin), 50 + ((double)initCounter) / ((double)numberOfLoadedTypes) * 100);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide the native expand button of naviPane, because we use resize/hide functions of SplitPanel Element
            Button naviPaneExpandButton = naviPane.Template.FindName("ExpandButton", naviPane) as Button;
            if (naviPaneExpandButton != null) naviPaneExpandButton.Visibility = Visibility.Collapsed;
        }

        [Conditional("DEBUG")]
        private void InitDebug()
        {
            dockWindowLogMessages.IsAutoHide = false;
        }

        private HashSet<Type> pluginInSearchListBox = new HashSet<Type>();
        private Mutex _singletonMutex;

        private void AddPluginToNavigationPane(GUIContainerElementsForPlugins contElements)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Image image = contElements.Plugin.GetImage(0);
                if (image != null)
                {
                    ListBoxItem navItem = CreateNavItem(contElements, image);
                    if (!pluginInSearchListBox.Contains(contElements.Plugin))
                    {
                        ListBoxItem navItem2 = CreateNavItem(contElements, contElements.Plugin.GetImage(0));
                        navListBoxSearch.Items.Add(navItem2);
                        pluginInSearchListBox.Add(contElements.Plugin);
                    }

                    if (!contElements.PaneItem.IsVisible)
                        contElements.PaneItem.Visibility = Visibility.Visible;
                    contElements.ListBox.Items.Add(navItem);
                }
                else
                {
                    if (contElements.PluginInfo != null)
                        GuiLogMessage(String.Format(Resource.plugin_has_no_icon, contElements.PluginInfo.Caption), NotificationLevel.Error);
                    else if (contElements.PluginInfo == null && contElements.Plugin != null)
                        GuiLogMessage("Missing PluginInfoAttribute on Plugin: " + contElements.Plugin.ToString(), NotificationLevel.Error);
                }
            }, null);
        }

        private ListBoxItem CreateNavItem(GUIContainerElementsForPlugins contElements, Image image)
        {
            image.Margin = new Thickness(16, 0, 5, 0);
            image.Height = 25;
            image.Width = 25;
            TextBlock textBlock = new TextBlock();
            textBlock.FontWeight = FontWeights.DemiBold;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = contElements.PluginInfo.Caption;
            textBlock.Tag = textBlock.Text;

            StackPanel stackPanel = new StackPanel();
            if (CultureInfo.CurrentUICulture.Name != "en")
            {
                var englishCaption = contElements.PluginInfo.EnglishCaption;
                if (englishCaption != textBlock.Text)
                    stackPanel.Tag = englishCaption;
            }
            
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Margin = new Thickness(0, 2, 0, 2);
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);
            ListBoxItem navItem = new ListBoxItem();
            navItem.Content = stackPanel;
            navItem.Tag = contElements.Plugin;
            navItem.ToolTip = contElements.PluginInfo.ToolTip;
            // dragDrop handler
            navItem.PreviewMouseDown += navItem_PreviewMouseDown;
            navItem.PreviewMouseMove += navItem_PreviewMouseMove;
            navItem.MouseDoubleClick += navItem_MouseDoubleClick;
            return navItem;
        }

        private void LoadPlugins()
        {
            Dictionary<string, List<Type>> pluginTypes = new Dictionary<string, List<Type>>();
            foreach (string interfaceName in interfaceNameList)
            {
                pluginTypes.Add(interfaceName, new List<Type>());
            }

            PluginList.AddDisabledPluginsToPluginList(Settings.Default.DisabledPlugins);

            foreach (Type pluginType in this.pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies).Values)
            {
                ComponentInformations.AddPlugin(pluginType);

                if (pluginType.GetInterface("IEditor") == null)
                    PluginList.AddTypeToPluginList(pluginType);

                foreach (string interfaceName in interfaceNameList)
                {
                    if (pluginType.GetInterface(interfaceName) != null)
                    {
                        pluginTypes[interfaceName].Add(pluginType);
                        numberOfLoadedTypes++;
                    }
                }
            }
            foreach (var pluginType in pluginTypes)
            {
                pluginType.Value.Sort((x, y) => x.GetPluginInfoAttribute().Caption.CompareTo(y.GetPluginInfoAttribute().Caption));
            }
            loadedTypes = pluginTypes;
        }

        public void LoadingPluginsFinished(IAsyncResult ar)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                PluginList.Finished();
            }, null);
                        
            try
            {
                AsyncResult asyncResult = ar as AsyncResult;
                LoadPluginsDelegate exe = asyncResult.AsyncDelegate as LoadPluginsDelegate;

                // check if plugin thread threw an exception
                try
                {
                    exe.EndInvoke(ar);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }
                // Init of this stuff has to be done after plugins have been loaded
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ComponentInformations.EditorExtension = GetEditorExtension(loadedTypes[typeof(IEditor).FullName]);
                }, null);
                AsyncCallback asyncCallback = new AsyncCallback(TypeInitFinished);
                InitTypesDelegate initTypesDelegate = new InitTypesDelegate(this.InitTypes);
                initTypesDelegate.BeginInvoke(loadedTypes, asyncCallback, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// CrypWin startup finished, show window stuff.
        /// </summary>
        public void TypeInitFinished(IAsyncResult ar)
        {
            // check if plugin thread threw an exception
            CheckInitTypesException(ar);

            try
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.Visibility = Visibility.Visible;
                    this.Show();

                    #region Gui-Stuff
                    Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    Version version = AssemblyHelper.GetVersion(assembly);
                    OnGuiLogNotificationOccuredTS(this, new GuiLogEventArgs(Resource.crypTool + " " + version.ToString() + Resource.started_and_ready, null, NotificationLevel.Info));

                    this.IsEnabled = true;
                    AppRibbon.Items.Refresh();
                    splashWindow.Close();
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    #endregion Gui-Stuff

                    InitDebug();

                    LoadIndividualComponentConnectionStatistics();

                    // open projects at startup if necessary, return whether any project has been opened
                    bool hasOpenedProject = CheckCommandOpenProject();

                    if (Properties.Settings.Default.ShowStartcenter && CheckCommandProjectFileGiven().Count == 0)
                    {
                        AddEditorDispatched(typeof(StartCenter.StartcenterEditor));
                    }
                    else if (!hasOpenedProject) // neither startcenter shown nor any project opened
                    {
                        ProjectTitleChanged(); // init window title in order to avoid being empty
                    }

                    if (IsCommandParameterGiven("-silent"))
                    {
                        silent = true;
                        statusBarItem.Content = null;
                        dockWindowLogMessages.IsAutoHide = true;
                        dockWindowLogMessages.Visibility = Visibility.Collapsed;
                    }

                    startUpRunning = false;

                }, null);

            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void CheckInitTypesException(IAsyncResult ar)
        {
            AsyncResult asyncResult = ar as AsyncResult;
            if (asyncResult == null)
                return;

            InitTypesDelegate exe = asyncResult.AsyncDelegate as InitTypesDelegate;
            if (exe == null)
                return;
            
            try
            {
                exe.EndInvoke(ar);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private Dictionary<string, Type> GetEditorExtension(List<Type> editorTypes)
        {
            Dictionary<string, Type> editorExtension = new Dictionary<string, Type>();
            foreach (Type type in editorTypes)
            {
                if (type.GetEditorInfoAttribute() != null)
                    editorExtension.Add(type.GetEditorInfoAttribute().DefaultExtension, type);
            }
            return editorExtension;
        }

        /// <summary>
        /// Find workspace parameter and if found, load workspace
        /// </summary>
        /// <returns>project file name or null if none</returns>
        private List<string> CheckCommandProjectFileGiven()
        {
            var res = new List<string>();
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                string currentParameter = args[i];
                if (currentParameter.StartsWith("-"))
                {
                    continue;
                }

                if (File.Exists(currentParameter))
                {
                    res.Add(currentParameter);
                }
            }

            return res;
        }

        /// <summary>
        /// Open projects at startup.
        /// </summary>
        /// <returns>true if at least one project has been opened</returns>
        private bool CheckCommandOpenProject()
        {
            bool hasOpenedProject = false;

            try
            {
                var filesPath = CheckCommandProjectFileGiven();

                if (filesPath.Count == 0)
                {
                    if (Settings.Default.ReopenLastTabs && Settings.Default.LastOpenedTabs != null)
                    {
                        hasOpenedProject = ReopenLastTabs(Settings.Default.LastOpenedTabs);
                    }
                }
                else
                {
                    foreach (var filePath in filesPath)
                    {
                        GuiLogMessage(string.Format(Resource.workspace_loading, filePath), NotificationLevel.Info);
                        OpenProject(filePath, FileLoadedOnStartup);
                        hasOpenedProject = true;
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                if (ex.InnerException != null)
                    GuiLogMessage(ex.InnerException.Message, NotificationLevel.Error);
            }

            return hasOpenedProject;
        }

        private bool ReopenLastTabs(List<StoredTab> lastOpenedTabs)
        {
            var hasOpenedProject = false;

            foreach (var lastOpenedTab in lastOpenedTabs)
            {
                if (lastOpenedTab is EditorFileStoredTab)
                {
                    var file = ((EditorFileStoredTab) lastOpenedTab).Filename;
                    if (File.Exists(file))
                    {
                        this.OpenProject(file, null);
                        OpenTab(ActiveEditor, lastOpenedTab.Info, null);
                        hasOpenedProject = true;
                    }
                    else
                    {
                        GuiLogMessage(string.Format(Properties.Resources.FileLost, file), NotificationLevel.Error);
                    }
                }
                else if (lastOpenedTab is EditorTypeStoredTab)
                {
                    var editorType = ((EditorTypeStoredTab) lastOpenedTab).EditorType;
                    var editor = AddEditorDispatched(editorType);
                    TabInfo info = new TabInfo();

                    try
                    {
                        if (editorType == typeof(P2PEditor.P2PEditor))
                            info.Title = P2PEditor.Properties.Resources.P2PEditor_Tab_Caption;
                        else if (editorType == typeof(WorkspaceManager.WorkspaceManagerClass))
                            info.Title = WorkspaceManager.Properties.Resources.unnamed_project;
                        else
                            info.Title = editorType.GetPluginInfoAttribute().Caption;
                    }
                    catch (Exception ex)
                    {
                        info = lastOpenedTab.Info;
                    }

                    OpenTab(editor, info, null);     //rename
                }
                else if (lastOpenedTab is CommonTypeStoredTab)
                {
                    object tabContent = null;
                    TabInfo info = new TabInfo();

                    var type = ((CommonTypeStoredTab) lastOpenedTab).Type;

                    if (type == typeof(OnlineHelpTab))
                    {
                        tabContent = OnlineHelpTab.GetSingleton(this);
                        info.Title = Properties.Resources.Online_Help;
                    }
                    else if (type == typeof(SettingsPresentation))
                    {
                        tabContent = SettingsPresentation.GetSingleton();
                        info.Title = Properties.Resources.Settings;
                    }
                    else if (type == typeof(UpdaterPresentation))
                    {
                        tabContent = UpdaterPresentation.GetSingleton();
                        info.Title = Properties.Resources.CrypTool_2_0_Update;
                    }
                    else if (type == typeof(SystemInfos))
                    {
                        tabContent = systemInfos;
                        info.Title = Properties.Resources.System_Infos;
                    }
                    else if (type == typeof(LicensesTab))
                    {
                        tabContent = licenses;
                        info.Title = Properties.Resources.Licenses;
                    } 
                    else if (typeof(ICrypTutorial).IsAssignableFrom(type))
                    {
                        var constructorInfo = type.GetConstructor(new Type[0]);
                        if (constructorInfo != null)
                            tabContent = constructorInfo.Invoke(new object[0]);
                        info.Title = type.GetPluginInfoAttribute().Caption;
                        info.Icon = type.GetImage(0).Source;
                        info.Tooltip = new Span(new Run(type.GetPluginInfoAttribute().ToolTip));
                    }
                    else if(type != null)
                    {
                        try
                        {
                            var constructorInfo = type.GetConstructor(new Type[0]);
                            if (constructorInfo != null)
                                tabContent = constructorInfo.Invoke(new object[0]);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(string.Format(Properties.Resources.Couldnt_create_tab_of_Type, type.Name, ex.Message), NotificationLevel.Error);
                        }

                        try
                        {
                            info.Title = type.GetPluginInfoAttribute().Caption;
                        }
                        catch (Exception ex)
                        {
                            info = lastOpenedTab.Info;
                        }
                    }

                    if (tabContent != null && info != null)
                    {
                        OpenTab(tabContent, info, null);
                    }
                }
            }

            return hasOpenedProject;
        }

        private void FileLoadedOnStartup(IEditor editor, string filename)
        {
            // Switch to "Play"-state, if parameter is given
            if (IsCommandParameterGiven("-autostart"))
            {
                PlayProject(editor);
            }
        }

        #endregion Init

        #region Editor

        private IEditor AddEditorDispatched(Type type)
        {
            if (type == null) // sanity check
                return null;

            var editorInfo = type.GetEditorInfoAttribute();
            if (editorInfo != null && editorInfo.Singleton)
            {
                foreach (var e in contentToTabMap.Keys.Where(e => e.GetType() == type))
                {
                    ActiveEditor = (IEditor)e;
                    return (IEditor)e;
                }
            }

            IEditor editor = type.CreateEditorInstance();
            if (editor == null) // sanity check
                return null;

            if (editor.Presentation != null)
            {
                ToolTipService.SetIsEnabled(editor.Presentation, false);
                //editor.Presentation.Tag = type.GetImage(0).Source;   
            }
            editor.SamplesDir = defaultTemplatesDirectory;

            if (editor is StartCenter.StartcenterEditor)
            {
                ((StartCenter.StartcenterEditor) editor).ShowOnStartup = Properties.Settings.Default.ShowStartcenter;
                ((Startcenter.Startcenter)((StartCenter.StartcenterEditor)editor).Presentation).TemplateLoaded += new EventHandler<Startcenter.TemplateOpenEventArgs>(MainWindow_TemplateLoaded);
            }

            if (this.Dispatcher.CheckAccess())
            {
                AddEditor(editor);
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    AddEditor(editor);
                }, null);
            }
            return editor;
        }

        void MainWindow_TemplateLoaded(object sender, Startcenter.TemplateOpenEventArgs e)
        {
            var editor = OpenEditor(e.Type, e.Info);
            editor.Open(e.Info.Filename.FullName);
            OpenTab(editor, e.Info, null);
        }

        private void AddEditor(IEditor editor)
        {
            editor.PluginManager = this.pluginManager;

            TabControl tabs = (TabControl)(MainSplitPanel.Children[0]);
            foreach (TabItem tab in tabs.Items)
            {
                if (tab.Content == editor.Presentation)
                {
                    tabs.SelectedItem = tab;
                    return;
                }
            }

            editor.OnOpenTab += OpenTab;
            editor.OnOpenEditor += OpenEditor;

            editor.OnProjectTitleChanged += EditorProjectTitleChanged;
            
            OpenTab(editor, new TabInfo(){Title = editor.GetType().Name, Icon = editor.GetImage(0).Source}, null);

            editor.Initialize();

            editor.New();
            editor.Presentation.Focusable = true;
            editor.Presentation.Focus();
        }

        private IEditor OpenEditor(Type editorType, TabInfo info)
        {
            var editor = AddEditorDispatched(editorType);
            if (info == null)
                info = new TabInfo();

            if (info.Filename != null)
                this.ProjectFileName = info.Filename.FullName;
            if (info != null)
                OpenTab(editor, info, null);
            return editor;
        }

        private void EditorProjectTitleChanged(IEditor editor, string newprojecttitle)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (!contentToTabMap.ContainsKey(editor))
                    return;

                newprojecttitle = newprojecttitle.Replace("_", "__");
                contentToTabMap[editor].Header = newprojecttitle;
                if (editor == ActiveEditor)
                    ProjectTitleChanged(newprojecttitle);

                SaveSession();
            }, null);
        }

        /// <summary>
        /// Opens a tab with the given content.
        /// If content is of type IEditor, this method behaves a little bit different.
        /// 
        /// If a tab with the given content already exists, only the title of it is changed.
        /// </summary>
        /// <param name="content">The content to be shown in the tab</param>
        /// <param name="title">Title of the tab</param>
        TabItem OpenTab(object content, TabInfo info,IEditor parent)
        {
            if (contentToTabMap.ContainsKey(content))
            {
                var tab = contentToTabMap[content];
                tab.SetTabInfo(info);
                //tab.Header = title.Replace("_", "__");
                tab.IsSelected = true;
                SaveSession();
                return tab;
            }

            TabControl tabs = (TabControl)(MainSplitPanel.Children[0]);
            lastTab = (TabItem) tabs.SelectedItem;
            CTTabItem tabitem = new CTTabItem(info);
            tabitem.RequestDistractionFreeOnOffEvent += new EventHandler(tabitem_RequestDistractionFreeOnOffEvent);
            tabitem.RequestHideMenuOnOffEvent += new EventHandler(tabitem_RequestHideMenuOnOffEvent);
            tabitem.RequestBigViewFrame += handleMaximizeTab;

            var plugin = content as IPlugin;
            if (plugin != null)
            {
                plugin.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                tabitem.Content = plugin.Presentation;
            }
            else
            {
                tabitem.Content = content;
            }

            var editor = content as IEditor;
            if (editor != null)
            {
                tabitem.Editor = editor;
                if (Settings.Default.FixedWorkspace)
                    editor.ReadOnly = true;
            }

            //Create the tab header:
            //StackPanel tabheader = new StackPanel();
            //tabheader.Orientation = Orientation.Horizontal;
            //TextBlock tablabel = new TextBlock();
            //tablabel.Text = title.Replace("_", "__");
            //tablabel.Name = "Text";
            //tabheader.Children.Add(tablabel);

            //Binding bind = new Binding();
            //bind.Source = tabitem.Content;
            //bind.Path = new PropertyPath("Tag");
            //tabitem.SetBinding(CTTabItem.IconProperty, bind);
            //tabitem.Header = tablabel.Text;

            //give the tab header his individual color:
            var colorAttr = Attribute.GetCustomAttribute(content.GetType(), typeof(TabColorAttribute));
            if (colorAttr != null)
            {
                var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(((TabColorAttribute)colorAttr).Brush);
                var color = new Color() { A = 45, B = brush.Color.B, G = brush.Color.G, R = brush.Color.R };
                tabitem.Background = new SolidColorBrush(color);
            }

            tabitem.OnClose += delegate
            {
                CloseTab(content, tabs, tabitem);
            };

            tabs.Items.Add(tabitem);

            tabToContentMap.Add(tabitem, content);
            contentToTabMap.Add(content, tabitem);
            if (parent != null)
                contentToParentMap.Add(content, parent);

            //bind content tooltip to tabitem header tooltip:
            //var headerTooltip = new ToolTip();
            //tabitem.Tag = headerTooltip;
            //if (content is ICrypTutorial)
            //{
            //    headerTooltip.Content = ((ICrypTutorial)content).GetPluginInfoAttribute().ToolTip;
            //}
            //else
            //{
            //    Binding tooltipBinding = new Binding("ToolTip");
            //    tooltipBinding.Source = tabitem.Content;
            //    tooltipBinding.Mode = BindingMode.OneWay;
            //    headerTooltip.SetBinding(ContentProperty, tooltipBinding);
            //}

            tabs.SelectedItem = tabitem;

            SaveSession();
            return tabitem;
        }

        void tabitem_RequestHideMenuOnOffEvent(object sender, EventArgs e)
        {
            AppRibbon.IsMinimized = !AppRibbon.IsMinimized;
        }

        void tabitem_RequestDistractionFreeOnOffEvent(object sender, EventArgs e)
        {
            doHandleMaxTab();
        }

        private void CloseTab(object content, TabControl tabs, CTTabItem tabitem)
        {
            if (Settings.Default.FixedWorkspace)
                return;

            IEditor editor = content as IEditor;

            if (editor != null && SaveChangesIfNecessary(editor) == FileOperationResult.Abort)
                return;

            if (editor != null && contentToParentMap.ContainsValue(editor))
            {
                var children = contentToParentMap.Keys.Where(x => contentToParentMap[x] == editor).ToArray();
                foreach (var child in children)
                {
                    CloseTab(child, tabs, contentToTabMap[child]);
                }
            }

            tabs.Items.Remove(tabitem);
            tabToContentMap.Remove(tabitem);
            contentToTabMap.Remove(content);
            contentToParentMap.Remove(content);
            if(tabitem is CTTabItem)
            {
                ((CTTabItem)tabitem).RequestDistractionFreeOnOffEvent -= tabitem_RequestDistractionFreeOnOffEvent;
                ((CTTabItem)tabitem).RequestHideMenuOnOffEvent -= tabitem_RequestHideMenuOnOffEvent;
            }


            tabitem.Content = null;

            if (editor != null)
            {
                editorToFileMap.Remove(editor);
                if (editor.CanStop)
                {
                    StopProjectExecution(editor);
                }
                editor.OnOpenTab -= OpenTab;
                editor.OnOpenEditor -= OpenEditor;
                editor.OnProjectTitleChanged -= EditorProjectTitleChanged;

                SaveSession();
            }

            IPlugin plugin = content as IPlugin;
            if (plugin != null)
            {
                plugin.Dispose();
            }

            //Jump back to last tab:
            if (lastTab != null && lastTab != tabitem)
            {
                lastTab.IsSelected = true;
            }

            //Open Startcenter if tabcontrol is empty now:
            if (tabs.Items.Count == 0)
            {
                if (Properties.Settings.Default.ShowStartcenter)
                {
                    AddEditorDispatched(typeof(StartCenter.StartcenterEditor));
                }
            }
        }

        private void SaveSession()
        {
            var session = new List<StoredTab>();
            foreach (var c in tabToContentMap.Where(x => Attribute.GetCustomAttribute(x.Value.GetType(), typeof(NotStoredInSessionAttribute)) == null))
            {
                if (c.Value is IEditor && !string.IsNullOrEmpty(((IEditor)c.Value).CurrentFile))
                {
                    session.Add(new EditorFileStoredTab(c.Key.Info, ((IEditor)c.Value).CurrentFile));
                }
                else if (c.Value is IEditor)
                {
                    session.Add(new EditorTypeStoredTab(c.Key.Info, c.Value.GetType()));
                }
                else
                {
                    session.Add(new CommonTypeStoredTab(c.Key.Info, c.Value.GetType()));
                }
            }
            Settings.Default.LastOpenedTabs = session;
        }

        private void SetRibbonControlEnabled(bool enabled)
        {
            ribbonMainControls.IsEnabled = enabled;
            ribbonEditorProcess.IsEnabled = enabled;
            ribbonBarEditor.IsEnabled = enabled;
        }

        public void SetRibbonControlEnabledInGuiThread(bool enabled)
        {
            if (this.Dispatcher.CheckAccess())
            {
                SetRibbonControlEnabled(enabled);
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    SetRibbonControlEnabled(enabled);
                }, null);
            }
        }
        
        private void ProjectTitleChanged(string newProjectTitle = null)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                string windowTitle = AssemblyHelper.ProductName;
                if (!string.IsNullOrEmpty(newProjectTitle))
                    windowTitle += " - " + newProjectTitle; // append project name if not null or empty

                this.Title = windowTitle;
            }, null);
        }

        private void SelectedPluginChanged(object sender, PluginChangedEventArgs pce)
        {
            if (!listPluginsAlreadyInitialized.Contains(pce.SelectedPlugin))
            {
                listPluginsAlreadyInitialized.Add(pce.SelectedPlugin);
                pce.SelectedPlugin.Initialize();
            }
        }

        private Type GetDefaultEditor()
        {
            return GetEditor(Settings.Default.defaultEditor);
        }

        private Type GetEditor(string name)
        {
            foreach (Type editor in this.loadedTypes[typeof(IEditor).FullName])
            {
                if (editor.FullName == name)
                    return editor;
            }
            return null;
        }

        private void SetCurrentEditorAsDefaultEditor()
        {
            Settings.Default.defaultEditor = ActiveEditor.GetType().FullName;
        }
        #endregion Editor

        #region DragDrop, NaviPaneMethods

        private void navPaneItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PaneItem pi = sender as PaneItem;
            if (pi != null)
            {
                naviPane.SystemText.CollapsedPaneText = pi.Title;
            }
        }

        void navItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                GuiLogMessage("Not a valid menu entry.", NotificationLevel.Error);
                return;
            }

            Type type = listBoxItem.Tag as Type;
            if (type == null)
            {
                GuiLogMessage("Not a valid menu entry.", NotificationLevel.Error);
                return;
            }

            try
            {
                if (ActiveEditor != null)
                    ActiveEditor.Add(type);
                else
                    GuiLogMessage("Adding plugin to active workspace not possible!", NotificationLevel.Error);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        void navItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragStarted = true;
            base.OnPreviewMouseDown(e);
        }

        void navItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragStarted)
            {
                dragStarted = false;

                //create data object 
                ButtonDropDown button = sender as ButtonDropDown;
                ListBoxItem listBoxItem = sender as ListBoxItem;
                Type type = null;
                if (button != null) type = button.Tag as Type;
                if (listBoxItem != null) type = listBoxItem.Tag as Type;

                if (type != null)
                {
                    DataObject data = new DataObject(new DragDropDataObject(type.Assembly.FullName, type.FullName, null));
                    //trap mouse events for the list, and perform drag/drop 
                    Mouse.Capture(sender as UIElement);
                    if (button != null)
                        System.Windows.DragDrop.DoDragDrop(button, data, DragDropEffects.Copy);
                    else
                        System.Windows.DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Copy);
                    Mouse.Capture(null);
                }
            }
            dragStarted = false;
            base.OnPreviewMouseMove(e);
        }

        private void naviPane_Collapsed(object sender, RoutedEventArgs e)
        {
            naviPane.IsExpanded = true;
        }
        # endregion OnPluginClicked, DragDrop, NaviPaneMethods

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (RunningWorkspaces() == 0 && ShowInTaskbar && !closedByMenu && !restart && !shutdown && Settings.Default.RunInBackground)
            {
                oldWindowState = WindowState;
                closingCausedMinimization = true;
                WindowState = System.Windows.WindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                if (RunningWorkspaces() > 0 && !restart && !shutdown)
                {
                    MessageBoxResult res;
                    if (RunningWorkspaces() == 1)
                    {
                        res = MessageBox.Show(Properties.Resources.There_is_still_one_running_task, Properties.Resources.Warning, MessageBoxButton.YesNo);
                    }else
                    {
                        res = MessageBox.Show(Properties.Resources.There_are_still_running_tasks__templates_in_Play_mode___Do_you_really_want_to_exit_CrypTool_2__, Properties.Resources.Warning, MessageBoxButton.YesNo);
                    }
                    if (res == MessageBoxResult.Yes)
                    {
                        ClosingRoutine(e);
                    }
                    else
                        e.Cancel = true;
                }
                else
                {
                    ClosingRoutine(e);
                }

                closedByMenu = false;
            }
        }

        private void ClosingRoutine(CancelEventArgs e)
        {
            try
            {
                SaveSession();
                SaveComponentConnectionStatistics();

                if (demoController != null)
                    demoController.Stop();

                if (_singletonMutex != null)
                {
                    _singletonMutex.ReleaseMutex();
                }

                FileOperationResult result = CloseProject(); // Editor Dispose will be called here.
                if (result == FileOperationResult.Abort)
                {
                    e.Cancel = true;
                    WindowState = oldWindowState;
                }
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        Settings.Default.IsWindowMaximized = true;
                        Settings.Default.RelHeight = 0.9;
                        Settings.Default.RelWidth = 0.9;
                    }
                    else
                    {
                        Settings.Default.IsWindowMaximized = false;
                        Settings.Default.RelHeight = Height / System.Windows.SystemParameters.PrimaryScreenHeight;
                        Settings.Default.RelWidth = Width / System.Windows.SystemParameters.PrimaryScreenWidth;
                    }
                    Settings.Default.logWindowAutoHide = dockWindowLogMessages.IsAutoHide;

                    SaveSettingsSavely();

                    // TODO workaround, find/introduce a new event should be the way we want this to work
                    if (P2PManager.IsP2PSupported)
                        P2PManager.HandleDisconnectOnShutdown();

                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();

                    SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                    SystemEvents.SessionEnding -= SystemEvents_SessionEnding;

                    if (restart)
                        OnUpdate();

                    Application.Current.Shutdown();
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void SaveComponentConnectionStatistics()
        {
            ComponentConnectionStatistics.SaveCurrentStatistics(Path.Combine(DirectoryHelper.DirectoryLocal, "ccs.xml"));
        }

        private int RunningWorkspaces()
        {
            var count = 0;
            foreach (var editor in editorToFileMap.Keys)
            {
                if (editor.CanStop)
                {
                    count++;
                }
            }

            return count;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveEditor == lastEditor)
                return;

            if (lastEditor != null)
            {
                //lastEditor.OnGuiLogNotificationOccured -= OnGuiLogNotificationOccured;
                lastEditor.OnSelectedPluginChanged -= SelectedPluginChanged;
                lastEditor.OnOpenProjectFile -= OpenProjectFileEvent;

                //save tab state of the old editor.. but not maximized:
                var prop = editorTypePanelManager.GetEditorTypePanelProperties(lastEditor.GetType());
                if (prop.ShowMaximized)     //currently maximized
                {
                    prop.ShowMaximized = false;
                    editorTypePanelManager.SetEditorTypePanelProperties(lastEditor.GetType(), prop);
                }
                else
                {
                    editorTypePanelManager.SetEditorTypePanelProperties(lastEditor.GetType(), new EditorTypePanelManager.EditorTypePanelProperties()
                    {
                        ShowComponentPanel = PluginBTN.IsChecked,
                        ShowLogPanel = LogBTN.IsChecked,
                        ShowSettingsPanel = SettingBTN.IsChecked,
                        ShowMaximized = false
                    });
                }
            }

            if (ActiveEditor != null && ActivePlugin == ActiveEditor)
            {
                //ActiveEditor.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                ActiveEditor.OnSelectedPluginChanged += SelectedPluginChanged;
                ActiveEditor.OnOpenProjectFile += OpenProjectFileEvent;
                ShowEditorSpecificPanels(ActiveEditor);
            }

            if (ActivePlugin != null)
            {
                if (contentToTabMap.ContainsKey(ActivePlugin))
                    ProjectTitleChanged((string)contentToTabMap[ActivePlugin].Header);
            }
            else
            {
                ProjectTitleChanged();
            }

            lastEditor = ActiveEditor;
            RecentFileListChanged();
        }

        private void ShowEditorSpecificPanels(IEditor editor)
        {
            try
            {
                var panelProperties = editorTypePanelManager.GetEditorTypePanelProperties(editor.GetType());

                if (!panelProperties.ShowMaximized)
                {
                    LogBTN.IsChecked = panelProperties.ShowLogPanel;
                    LogBTN_Checked(LogBTN, null);
                    PluginBTN.IsChecked = panelProperties.ShowComponentPanel;
                    PluginBTN_Checked(PluginBTN, null);
                    SettingBTN.IsChecked = panelProperties.ShowSettingsPanel;
                    SettingBTN_Checked(SettingBTN, null);
                }
                else
                {
                    MaximizeTab();
                }
            }
            catch (Exception)
            {
                //When editor has no specific settings (or editor parameter is null), just show all panels:
                MinimizeTab();
            }
        }

        private void RecentFileListChanged(List<string> recentFiles)
        {
            buttonDropDownOpen.Items.Clear();

            for (int c = recentFiles.Count - 1; c >= 0; c--)
            {
                string file = recentFiles[c];
                ButtonDropDown btn = new ButtonDropDown();
                btn.Header = file;
                btn.ToolTip = Properties.Resources.Load_this_file_;
                btn.IsChecked = (this.ProjectFileName == file);
                btn.Click += delegate(Object sender, RoutedEventArgs e)
                {
                    OpenProject(file, null);
                };

                buttonDropDownOpen.Items.Add(btn);
            }
        }

        private void RecentFileListChanged()
        {
            RecentFileListChanged(recentFileList.GetRecentFiles());
        }

        private void PluginSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            if (PluginSearchTextBox.Text == "")
            {
                if (navPaneItemSearch.IsSelected)
                    navPaneItemClassic.IsSelected = true;
                navPaneItemSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                navPaneItemSearch.Visibility = Visibility.Visible;
                navPaneItemSearch.IsSelected = true;

                foreach (ListBoxItem items in navListBoxSearch.Items)
                {
                    var panel = (System.Windows.Controls.Panel)items.Content;
                    TextBlock textBlock = (TextBlock)panel.Children[1];
                    string text = (string) textBlock.Tag;
                    string engText = null;
                    if (panel.Tag != null)
                    {
                        engText = (string) panel.Tag;
                    }

                    bool hit = text.ToLower().Contains(PluginSearchTextBox.Text.ToLower());
                    
                    if (!hit && (engText != null))
                    {
                        bool engHit = (engText.ToLower().Contains(PluginSearchTextBox.Text.ToLower()));
                        if (engHit)
                        {
                            hit = true;
                            text = text + " (" + engText + ")";
                        }
                    }

                    Visibility visibility = hit ? Visibility.Visible : Visibility.Collapsed;
                    items.Visibility = visibility;

                    if (hit)
                    {
                        textBlock.Inlines.Clear();
                        int begin = 0;
                        int end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        while (end != -1)
                        {
                            textBlock.Inlines.Add(text.Substring(begin, end - begin));
                            textBlock.Inlines.Add(new Bold(new Italic(new Run(text.Substring(end, PluginSearchTextBox.Text.Length)))));
                            begin = end + PluginSearchTextBox.Text.Length;
                            end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        }
                        textBlock.Inlines.Add(text.Substring(begin, text.Length - begin));
                    }
                }
            }
        }

        private void PluginSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                navPaneItemSearch.Visibility = Visibility.Collapsed;
                PluginSearchTextBox.Text = "";
            }
        }

        private void buttonSysInfo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            OpenTab(systemInfos, new TabInfo() { Title = Properties.Resources.System_Infos }, null);
        }

        private void buttonContactUs_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ContactDevelopersDialog.ShowModalDialog();
        }

        private void buttonReportBug_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Process.Start("https://www.cryptool.org/trac/CrypTool2/newticket");
        }

        private void buttonWebsite_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Process.Start("http://www.cryptool2.vs.uni-due.de");
        }

        private void buttonLicenses_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            OpenTab(licenses, new TabInfo() { Title = Properties.Resources.Licenses }, null);
            
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex < 0)
                return;

            MainTab.SelectedItem = box.Items[box.SelectedIndex];
        }

        private void SettingBTN_Checked(object sender, RoutedEventArgs e)
        {
            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.SettingVisibility = v.ToString();
            SaveSettingsSavely();
            if (v == Visibility.Visible)
                dockWindowAlgorithmSettings.Open();
            else
                dockWindowAlgorithmSettings.Close();
        }

        private void LogBTN_Checked(object sender, RoutedEventArgs e)
        {
            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.LogVisibility = v.ToString();
            SaveSettingsSavely();
            if (v == Visibility.Visible)
                dockWindowLogMessages.Open();
            else
                dockWindowLogMessages.Close();
        }

        private void PluginBTN_Checked(object sender, RoutedEventArgs e)
        {

            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.PluginVisibility = v.ToString();
            SaveSettingsSavely();
            if (v == Visibility.Visible)
                dockWindowNaviPaneAlgorithms.Open();
            else
                dockWindowNaviPaneAlgorithms.Close();
        }

        private void statusBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LogBTN.IsChecked)
                LogBTN.IsChecked = false;
            else
                LogBTN.IsChecked = true;

            LogBTN_Checked(LogBTN, null);
        }


        void doHandleMaxTab() 
        {
            if (LogBTN.IsChecked || SettingBTN.IsChecked || PluginBTN.IsChecked)
            {
                MaximizeTab();
            }
            else
            {
                //Normalize tab:
                if (ActiveEditor != null)
                {
                    var prop = editorTypePanelManager.GetEditorTypePanelProperties(ActiveEditor.GetType());
                    prop.ShowMaximized = false;
                    editorTypePanelManager.SetEditorTypePanelProperties(ActiveEditor.GetType(), prop);
                    ShowEditorSpecificPanels(ActiveEditor);
                }
                else
                {
                    LogBTN.IsChecked = true;
                    SettingBTN.IsChecked = true;
                    PluginBTN.IsChecked = true;

                    LogBTN_Checked(LogBTN, null);
                    SettingBTN_Checked(SettingBTN, null);
                    PluginBTN_Checked(PluginBTN, null);
                }
            }
        }

        void handleMaximizeTab(object sender, EventArgs e)
        {
            doHandleMaxTab();
        }

        private void MaximizeTab()
        {
            if (ActiveEditor != null)
            {
                //save status before maximizing, so it can be restored later:
                editorTypePanelManager.SetEditorTypePanelProperties(ActiveEditor.GetType(), new EditorTypePanelManager.EditorTypePanelProperties()
                                                                                                {
                                                                                                    ShowComponentPanel = PluginBTN.IsChecked,
                                                                                                    ShowLogPanel = LogBTN.IsChecked,
                                                                                                    ShowSettingsPanel = SettingBTN.IsChecked,
                                                                                                    ShowMaximized = true
                                                                                                });
            }

            LogBTN.IsChecked = false;
            SettingBTN.IsChecked = false;
            PluginBTN.IsChecked = false;

            LogBTN_Checked(LogBTN, null);
            SettingBTN_Checked(SettingBTN, null);
            PluginBTN_Checked(PluginBTN, null);
        }

        private void MinimizeTab()
        {
            LogBTN.IsChecked = true;
            SettingBTN.IsChecked = true;
            PluginBTN.IsChecked = true;

            LogBTN_Checked(LogBTN, null);
            SettingBTN_Checked(SettingBTN, null);
            PluginBTN_Checked(PluginBTN, null);
        }

        private void dockWindowAlgorithmSettings_AutoHideChanged(object sender, RoutedEventArgs e)
        {

            //if (activePlugin == null)
            //    return;

            //this.taskpaneCtrl.OnGuiLogNotificationOccured -= OnGuiLogNotificationOccured;
            //this.taskpaneCtrl.OnShowPluginDescription -= OnShowPluginDescription;
            //this.taskpaneCtrl = new TaskPaneCtrl(this);
            //this.taskpaneCtrl.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
            //this.taskpaneCtrl.OnShowPluginDescription += OnShowPluginDescription;
            //taskpaneCtrl.DisplayPluginSettings(activePlugin, activePluginTitle, activePluginMode);
            ////if (!listPluginsAlreadyInitialized.Contains(activePlugin))
            ////{
            ////    listPluginsAlreadyInitialized.Add(activePlugin);
            ////    activePlugin.Initialize();
            ////}
        }

        //private void ButtonDropDown2_Click(object sender, RoutedEventArgs e)
        //{
        //    ButtonDropDown btn = (ButtonDropDown)sender;
        //    switch (btn.Name)
        //    {
        //        case "LogBTN":
        //            if (btn.IsChecked)
        //            {
        //                btn.IsChecked = false;
        //                splitPanelLogMessages.Visibility = System.Windows.Visibility.Visible;
        //            }
        //            else
        //            {
        //                btn.IsChecked = true;
        //                splitPanelLogMessages.Visibility = System.Windows.Visibility.Collapsed;
        //            }
        //            break;
        //        case "SettingBTN":
        //            if (btn.IsChecked)
        //            {
        //                btn.IsChecked = false;
        //                splitPanelAlgorithmSettings.Visibility = System.Windows.Visibility.Visible;
        //            }
        //            else
        //            {
        //                btn.IsChecked = true;
        //                splitPanelAlgorithmSettings.Visibility = System.Windows.Visibility.Collapsed;
        //            }
        //            break;
        //        case "PluginBTN":
        //            if (btn.IsChecked)
        //            {
        //                btn.IsChecked = false;
        //                splitPanelNaviPaneAlgorithms.Visibility = System.Windows.Visibility.Visible;
        //            }
        //            else
        //            {
        //                btn.IsChecked = true;
        //                splitPanelNaviPaneAlgorithms.Visibility = System.Windows.Visibility.Collapsed;
        //            }
        //            break;
        //    }
        //}

        private void ShowHelpPage(object docEntity)
        {
            OnlineHelpTab onlineHelpTab = OnlineHelpTab.GetSingleton(this);
            onlineHelpTab.OnOpenEditor += OpenEditor;
            onlineHelpTab.OnOpenTab += OpenTab;

            //Find out which page to show:
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (docEntity is Type)
            {
                if (!ShowPluginHelpPage((Type) docEntity, onlineHelpTab, lang))
                    return;
            }
            else if (docEntity is OnlineHelp.TemplateType)
            {
                var rel = ((OnlineHelp.TemplateType) docEntity).RelativeTemplateFilePath;
                try
                {
                    onlineHelpTab.ShowHTMLFile(OnlineHelp.GetTemplateDocFilename(rel, lang));
                }
                catch (Exception ex)
                {
                    //Try opening index page in english:
                    try
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetTemplateDocFilename(rel, "en"));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
            else
            {
                ShowPluginHelpPage(null, onlineHelpTab, lang);
            }

            //show tab:
            TabItem tab = OpenTab(onlineHelpTab, new TabInfo() { Title = Properties.Resources.Online_Help }, null);
            if (tab != null)
                tab.IsSelected = true;
        }

        private bool ShowPluginHelpPage(Type docType, OnlineHelpTab onlineHelpTab, string lang)
        {
            try
            {
                if ((docType == typeof (MainWindow)) || (docType == null)) //The doc page of MainWindow is the index page.
                {
                    try
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetComponentIndexFilename(lang));
                    }
                    catch (Exception ex)
                    {
                        //Try opening index page in english:
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetComponentIndexFilename("en"));
                    }
                }
                else if (docType.GetPluginInfoAttribute() != null)
                {
                    var pdp = OnlineDocumentationGenerator.DocGenerator.CreatePluginDocumentationPage(docType);
                    if (pdp.AvailableLanguages.Contains(lang))
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetPluginDocFilename(docType, lang));
                    }
                    else
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetPluginDocFilename(docType, "en"));
                    }
                }
                else
                    throw new FileNotFoundException();
            }
            catch (FileNotFoundException)
            {
                //if file was not found, simply try to open the index page:
                GuiLogMessage(string.Format(Properties.Resources.MainWindow_ShowHelpPage_No_special_help_file_found_for__0__, docType),
                    NotificationLevel.Warning);
                if (docType != typeof (MainWindow))
                {
                    ShowHelpPage(typeof (MainWindow));
                }
                return false;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resource.MainWindow_ShowHelpPage_Error_trying_to_open_documentation___0__, ex.Message),
                    NotificationLevel.Error);
                return false;
            }
            return true;
        }

        private void addimg_Click(object sender, RoutedEventArgs e)
        {
            ActiveEditor.AddImage();
        }

        private void addtxt_Click(object sender, RoutedEventArgs e)
        {
            ActiveEditor.AddText();
        }

        private void dockWindowLogMessages_Closed(object sender, RoutedEventArgs e)
        {
            LogBTN.IsChecked = false;
        }

        private void dockWindowAlgorithmSettings_Closed(object sender, RoutedEventArgs e)
        {
            SettingBTN.IsChecked = false;
        }

        private void dockWindowNaviPaneAlgorithms_Closed(object sender, RoutedEventArgs e)
        {
            PluginBTN.IsChecked = false;
        }

        private void navListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            e2.RoutedEvent = UIElement.MouseWheelEvent;
            ((ListBox)sender).RaiseEvent(e2);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var item in listViewLogList.SelectedItems)
            {
                sb.AppendLine(item.ToString());
            }
            Clipboard.SetText(sb.ToString());
        }

        private void clearManagement()
        {
            ManagementRootEdit.Children.Clear();
            ManagementRootTutorials.Children.Clear();
            ManagementRootMain.Children.Clear();
        }

        private void AppRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ribbonTabHome.IsSelected)
            {
                clearManagement();
                ManagementRootMain.Children.Add(ribbonManagement);
                return;
            }

            if (ribbonTabEdit.IsSelected)
            {
                clearManagement();
                ManagementRootEdit.Children.Add(ribbonManagement);
                return;
            }

            if (ribbonTabView.IsSelected)
            {
                clearManagement();
                ManagementRootTutorials.Children.Add(ribbonManagement);
                return;
            }
        }
    }

    # region helper class

    public class VisibilityToMarginHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            if (vis == Visibility.Collapsed)
                return new Thickness(0, 2, 0, 0);
            else
                return new Thickness(15, 2, 15, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Helper class with GUI elements containing the plugins after init.
    /// </summary>
    /// 
    public class GUIContainerElementsForPlugins
    {
        # region shared
        public readonly Type Plugin;
        public readonly PluginInfoAttribute PluginInfo;
        # endregion shared

        # region naviPane
        public readonly PaneItem PaneItem;
        public readonly ListBox ListBox;
        # endregion naviPane

        # region ribbon
        public readonly string GroupName;
        # endregion ribbon

        public GUIContainerElementsForPlugins(Type plugin, PluginInfoAttribute pluginInfo, PaneItem paneItem, ListBox listBox, string groupName)
        {
            this.Plugin = plugin;
            this.PluginInfo = pluginInfo;
            this.PaneItem = paneItem;
            this.ListBox = listBox;
            this.GroupName = groupName;
        }
    }
    # endregion helper class
}
