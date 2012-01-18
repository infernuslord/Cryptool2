/*
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
using CrypWin.Helper;
using DevComponents.WpfRibbon;
using Microsoft.Win32;
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
        private Dictionary<TabItem, object> tabToContentMap = new Dictionary<TabItem, object>();
        private Dictionary<object, TabItem> contentToTabMap = new Dictionary<object, TabItem>();
        private Dictionary<object, IEditor> contentToParentMap = new Dictionary<object, IEditor>();
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool closingCausedMinimization = false;
        private WindowState oldWindowState;
        private bool restart = false;
        private bool shutdown = false;
        private string personalDir;
        private IEditor lastEditor = null;
        private SystemInfos systemInfos = new SystemInfos();
        private System.Windows.Forms.MenuItem playStopMenuItem;
        private EditorTypePanelManager editorTypePanelManager = new EditorTypePanelManager();
        private System.Windows.Forms.Timer hasChangesCheckTimer;

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

        private TaskPaneCtrl taskpaneCtrl;
        private bool dragStarted;
        private Splash splashWindow;
        private bool startUpRunning = true;
        private string defaultSamplesDirectory = "";
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
                TabItem selectedTab = (TabItem)(MainTab.SelectedItem);
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
                TabItem selectedTab = (TabItem)(MainTab.SelectedItem);
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
        }

        #region Init

        public MainWindow()
        {
            SetLanguage();
            LoadResources();

            // will exit application after doc has been generated
            if (IsCommandParameterGiven("-GenerateDoc"))
            {
                var docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                docGenerator.Generate(DirectoryHelper.BaseDirectory);
                Application.Current.Shutdown();
                return;
            }

            // check whether update is available to be installed
            if (IsUpdaterEnabled
                && CheckCommandProjectFileGiven() == null // NO project file given as command line argument
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

            defaultSamplesDirectory = Path.Combine(DirectoryHelper.BaseDirectory, Settings.Default.SamplesDir);
            if (!Directory.Exists(defaultSamplesDirectory))
            {
                GuiLogMessage("Directory with project templates not found", NotificationLevel.Debug);
                defaultSamplesDirectory = personalDir;
            }

            if (string.IsNullOrEmpty(Settings.Default.LastPath) || !Settings.Default.useLastPath || !Directory.Exists(Settings.Default.LastPath))
            {
                Settings.Default.LastPath = personalDir;
            }

            SaveSettingsSavely();

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
            this.taskpaneCtrl = new TaskPaneCtrl(this);
            this.taskpaneCtrl.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
            this.taskpaneCtrl.OnShowPluginDescription += OnShowPluginDescription;
            this.dockWindowAlgorithmSettings.AutoHideOpen = false;
            this.dockWindowAlgorithmSettings.Content = taskpaneCtrl;

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

            OpenTab(content, type.GetPluginInfoAttribute().Caption, null);
            content.Presentation.ToolTip = type.GetPluginInfoAttribute().ToolTip;
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

                    if (ActiveEditor != null)
                    {
                        taskpaneCtrl.DisplayPluginSettings(ActiveEditor, ActiveEditor.GetPluginInfoAttribute().Caption, DisplayPluginMode.Normal);
                    }
                    this.IsEnabled = true;
                    AppRibbon.Items.Refresh();
                    splashWindow.Close();
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    #endregion Gui-Stuff

                    InitDebug();

                    // open projects at startup if necessary, return whether any project has been opened
                    bool hasOpenedProject = CheckCommandOpenProject();

                    if (Properties.Settings.Default.ShowStartcenter && CheckCommandProjectFileGiven() == null)
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
        private string CheckCommandProjectFileGiven()
        {
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
                    return currentParameter;
                }
            }

            return null;
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
                string filePath = CheckCommandProjectFileGiven();
                if (filePath != null)
                {
                    GuiLogMessage(string.Format(Resource.workspace_loading, filePath), NotificationLevel.Info);
                    OpenProject(filePath);
                    hasOpenedProject = true;
                }
                else
                {
                    if (Settings.Default.ReopenLastFiles && Settings.Default.LastOpenedFiles != null)
                    {
                        foreach (var file in Settings.Default.LastOpenedFiles)
                        {
                            if (File.Exists(file))
                            {
                                this.OpenProject(file);
                                hasOpenedProject = true;
                            }
                        }
                    }
                }

                // Switch to "Play"-state, if parameter is given
                if (IsCommandParameterGiven("-autostart"))
                {
                    PlayProject();
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
                editor.Presentation.Tag = type.GetImage(0).Source;   
            }
            editor.SamplesDir = defaultSamplesDirectory;

            if (editor is StartCenter.StartcenterEditor)
            {
                ((StartCenter.StartcenterEditor) editor).ShowOnStartup = Properties.Settings.Default.ShowStartcenter;
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
            
            OpenTab(editor, editor.GetType().Name, null);

            editor.Initialize();

            editor.New();
            editor.Presentation.Focusable = true;
            editor.Presentation.Focus();
        }

        private IEditor OpenEditor(Type editorType, string title, string filename)
        {
            var editor = AddEditorDispatched(editorType);
            if (filename != null)
                this.ProjectFileName = filename;
            if (title != null)
                OpenTab(editor, title, null);
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
        TabItem OpenTab(object content, string title, IEditor parent)
        {
            if (contentToTabMap.ContainsKey(content))
            {
                contentToTabMap[content].Header = title.Replace("_", "__");
                return contentToTabMap[content];
            }

            TabControl tabs = (TabControl)(MainSplitPanel.Children[0]);
            CTTabItem tabitem = new CTTabItem();
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
            TextBlock tablabel = new TextBlock();
            tablabel.Text = title.Replace("_", "__");
            tablabel.Name = "Text";
            //tabheader.Children.Add(tablabel);

            Binding bind = new Binding();
            bind.Source = tabitem.Content;
            bind.Path = new PropertyPath("Tag");
            tabitem.SetBinding(CTTabItem.IconProperty, bind);
            tabitem.Header = tablabel.Text;

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
            Binding tooltipBinding = new Binding("ToolTip");
            tooltipBinding.Source = tabitem.Content;
            tooltipBinding.Mode = BindingMode.OneWay;
            var headerTooltip = new ToolTip();
            tabitem.Tag = headerTooltip;
            headerTooltip.SetBinding(ContentProperty, tooltipBinding);
            tabs.SelectedItem = tabitem;

            SaveSession();

            return tabitem;
        }

        private void CloseTab(object content, TabControl tabs, TabItem tabitem)
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
        }

        private void SaveSession()
        {
            var session = new StringCollection();
            foreach (var c in tabToContentMap.Values)
            {
                if (c is IEditor)
                {
                    session.Add(((IEditor)c).CurrentFile);
                }
            }
            Properties.Settings.Default.LastOpenedFiles = session;
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

        private void setEditorRibbonElementsState(bool state)
        {
            if (ribbonBarEditor != null && ribbonBarEditor.Items.Count == 1 && ribbonBarEditor.Items[0] is StackPanel)
            {
                foreach (StackPanel stackPanel in (ribbonBarEditor.Items[0] as StackPanel).Children)
                {
                    foreach (FrameworkElement fwElement in stackPanel.Children)
                    {
                        if (fwElement.Tag is BindingInfoRibbon)
                            fwElement.IsEnabled = (((BindingInfoRibbon)fwElement.Tag).RibbonBarAttribute.ChangeableWhileExecuting || state);
                    }
                }
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
            taskpaneCtrl.DisplayPluginSettings(pce.SelectedPlugin, pce.Title, pce.DisplayPluginMode);
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
            if (WorkspacesRunning() && ShowInTaskbar && !closedByMenu && !restart && !shutdown && Settings.Default.RunInBackground)
            {
                oldWindowState = WindowState;
                closingCausedMinimization = true;
                WindowState = System.Windows.WindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                if (WorkspacesRunning() && !restart && !shutdown)
                {
                    MessageBoxButton b = MessageBoxButton.OKCancel;
                    string c = Properties.Resources.Warning;
                    MessageBoxResult res = MessageBox.Show(Properties.Resources.There_are_still_running_tasks__do_you_really_want_to_exit_CrypTool_2_0_, c, b);
                    if (res == MessageBoxResult.OK)
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
                if (demoController != null)
                    demoController.Stop();

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

        private bool WorkspacesRunning()
        {
            foreach (var editor in editorToFileMap.Keys)
            {
                if (editor.CanStop)
                    return true;
            }

            return false;
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

                var attr = Attribute.GetCustomAttribute(ActiveEditor.GetType(), typeof(EditingInfoAttribute));
                if (attr != null)
                {
                    var b = ((EditingInfoAttribute)attr).CanEdit;
                    if (b)
                    {
                        addimg.IsEnabled = addtxt.IsEnabled = true;
                    }
                    else
                    {
                        addimg.IsEnabled = addtxt.IsEnabled = false;
                    }
                }
                else
                {
                    addimg.IsEnabled = addtxt.IsEnabled = false;
                }
                ShowEditorSpecificPanels(ActiveEditor);
            }

            if (ActivePlugin != null)
            {
                if (contentToTabMap.ContainsKey(ActivePlugin))
                    ProjectTitleChanged((string)contentToTabMap[ActivePlugin].Header);

                taskpaneCtrl.DisplayPluginSettings(ActivePlugin, ActivePlugin.GetPluginInfoAttribute().Caption, DisplayPluginMode.Normal);
            }
            else
            {
                taskpaneCtrl.DisplayPluginSettings(null, null, DisplayPluginMode.Normal);
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
                    OpenProject(file);
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
            OpenTab(systemInfos, Properties.Resources.System_Infos, null);
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

        void handleMaximizeTab(object sender, EventArgs e)
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
        
        private void ShowHelpPage(Type type)
        {
            OnlineHelpTab onlineHelpTab = OnlineHelpTab.GetSingleton(this);

            onlineHelpTab.OnOpenEditor += OpenEditor;
            onlineHelpTab.OnOpenTab += OpenTab;

            //Find out which page to show:
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            try
            {
                if ((type == typeof(MainWindow)) || (type == null))     //The doc page of MainWindow is the index page.
                {
                    try
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetIndexFilename(lang));
                    }
                    catch (Exception ex)
                    {
                        //Try opening index page in english:
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetIndexFilename("en"));
                    }
                }
                else if (type.GetPluginInfoAttribute() != null)
                {
                    var pdp = OnlineDocumentationGenerator.DocGenerator.CreateDocumentationPage(type);
                    if (pdp.AvailableLanguages.Contains(lang))
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetDocFilename(type, lang));
                    }
                    else
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetDocFilename(type, "en"));
                    }
                }
                else
                    throw new FileNotFoundException();
            }
            catch (FileNotFoundException)
            {
                //if file was not found, simply try to open the index page:
                GuiLogMessage(string.Format(Properties.Resources.MainWindow_ShowHelpPage_No_special_help_file_found_for__0__, type), NotificationLevel.Warning);
                if (type != typeof(MainWindow))
                {
                    ShowHelpPage(typeof(MainWindow));
                }
                return;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resource.MainWindow_ShowHelpPage_Error_trying_to_open_documentation___0__, ex.Message), NotificationLevel.Error);
                return;
            }

            //show tab:
            TabItem tab = OpenTab(onlineHelpTab, Properties.Resources.Online_Help, null);
            if (tab != null)
                tab.IsSelected = true;
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
