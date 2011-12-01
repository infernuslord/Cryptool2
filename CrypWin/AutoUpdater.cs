using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Microsoft.Win32;

namespace Cryptool.CrypWin
{
    class AutoUpdater
    {
        #region Fields and properties
        public delegate void UpdaterStateChangedHandler(State newStatus);
        public delegate void UpdateDownloadProgressChangedHandler(int progress);
        public event UpdaterStateChangedHandler OnUpdaterStateChanged;
        public event UpdateDownloadProgressChangedHandler OnUpdateDownloadProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public enum State { Idle, Checking, UpdateAvailable, Downloading, UpdateReady };
            
        private static AutoUpdater autoUpdater = null;
        private const string XmlPath = "https://www.cryptool.org/cryptool2/downloads/Builds/CT2_Versions.xml";
        private readonly string TempPath = DirectoryHelper.DirectoryLocalTemp;


        private XElement onlineUpdateVersions;
        private Version onlineUpdateVersion = new Version();
        private Version currentlyRunningVersion = AssemblyHelper.Version;
        private bool serverAvailable = true;
        private string serverNotAvailableMessage;
        private System.Net.WebClient wc;
        private string changelogTemplate = "https://www.cryptool.org/trac/CrypTool2/log/trunk?format=rss&action=stop_on_copy&mode=stop_on_copy&rev=§&stop_rev=$"; //§ new $ current build
        private string changelog;
        private string updateName;
        private System.Timers.Timer checkTimer = new System.Timers.Timer(1000 * 60 * Settings.Default.CheckInterval);
        private System.Timers.Timer progressTimer;

        private X509Certificate serverTlsCertificate;
        private int downloadRetry = 0;

        private State currentState = State.Idle;
        public State CurrentState
        {
            get { return currentState; }
            private set 
            {
                currentState = value;
                OnUpdaterStateChanged(currentState);
                AutoUpdater_OnUpdaterStatusChanged(currentState);
            }
        }

        public string FilePath
        {
            get
            {
                switch (AssemblyHelper.InstallationType)
                {
                    case Ct2InstallationType.MSI:
                        return Path.Combine(TempPath, "CT2Update.msi");
                    case Ct2InstallationType.NSIS:
                        return Path.Combine(TempPath, "CT2Update.exe");
                    case Ct2InstallationType.ZIP:
                        return Path.Combine(TempPath, "CT2Update.zip");
                    default:
                        return null;
                }
            }
        }

        public string FilePathTemporary
        {
            get
            {
                return Path.Combine(TempPath, "CT2Update.part");
            }
        }

        public bool IsUpdateReady
        {
            get
            {
                return File.Exists(FilePath);
            }
        }

        #endregion

        #region Implementation

        public static AutoUpdater GetSingleton()
        {
            if (autoUpdater == null)
                autoUpdater = new AutoUpdater();

            return autoUpdater;
        }

        private AutoUpdater()
        {
            serverTlsCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate(global::Cryptool.CrypWin.Properties.Resources.www_cryptool_org);

            ServicePointManager.ServerCertificateValidationCallback = UpdateServerCertificateValidationCallback;

            changelogTemplate = changelogTemplate.Replace("$", (currentlyRunningVersion.Build + 1).ToString()); // show only changes newer than current version

            // listen for system suspend/resume
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private string GetBuildTypeXmlString()
        {
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Nightly:
                    return "nightly";
                case Ct2BuildType.Beta:
                    return "beta";
                case Ct2BuildType.Stable:
                    return "stable";
                default:
                    return null;
            }
        }

        private bool UpdateServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                GuiLogMessage("AutoUpdate: Could not validate certificate, as it is null", NotificationLevel.Error);
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                GuiLogMessage("AutoUpdate: Could not validate TLS certificate, as the server did not provide one", NotificationLevel.Error);
                return false;
            }

            // Check equality of remote and local certificate
            if (!certificate.Equals(this.serverTlsCertificate))
            {
                GuiLogMessage("AutoUpdate: Received TLS certificate is not a valid certificate: Equality check failed", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        private string GetUserAgentString(char checkingReference)
        {
            string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            return "CrypTool/" + AssemblyHelper.Version + " (" + checkingReference + "; " + CultureInfo.CurrentUICulture.Name + "; "
                                    + AssemblyHelper.BuildType + "; " + Environment.OSVersion.ToString() + "; "
                                    + Environment.OSVersion.Platform + "; " + arch + "; .NET/" + Environment.Version + ")";
        }

        private void AutoUpdater_OnUpdaterStatusChanged(AutoUpdater.State newStatus)
        {
            UpdaterPresentation.GetSingleton().Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                        {
                                                            UpdaterStateChanged(newStatus);
                                                        }, null);
            if (newStatus == State.UpdateAvailable && Settings.Default.AutoDownload)
            {
                if (downloadRetry < 3)
                {
                    Download();
                    downloadRetry++;
                }
                else
                    GuiLogMessage("AutoUpdate: Auto download failed, try again later.", NotificationLevel.Warning);
            }
        }

        private void UpdaterStateChanged(State newStatus)
        {
            try
            {
                switch (newStatus)
                {
                    case State.Idle:
                        UpdaterPresentation.GetSingleton().button1.IsEnabled = true;
                        UpdaterPresentation.GetSingleton().button1.Content = Properties.Resources.Check_for_updates_now;
                        if (serverAvailable)
                            UpdaterPresentation.GetSingleton().label1.Content = Properties.Resources.You_have_currently_the_latest_version_installed_;
                        else
                            UpdaterPresentation.GetSingleton().label1.Content = string.Format(Properties.Resources.Checking_failed__cannot_contact_server__0, serverNotAvailableMessage);
                        break;
                    case State.Checking:
                        UpdaterPresentation.GetSingleton().button1.IsEnabled = false;
                        UpdaterPresentation.GetSingleton().label1.Content = Properties.Resources.Checking_for_updates___;
                        break;
                    case State.UpdateAvailable:
                        UpdaterPresentation.GetSingleton().button1.IsEnabled = true;
                        UpdaterPresentation.GetSingleton().button1.Content = Properties.Resources.Download_update_now;
                        UpdaterPresentation.GetSingleton().label1.Content = string.Format(Properties.Resources.Update_available___0, updateName);
                        UpdaterPresentation.GetSingleton().button1.Visibility = Visibility.Visible;
                        UpdaterPresentation.GetSingleton().progressBar1.Visibility = Visibility.Collapsed;
                        UpdaterPresentation.GetSingleton().image1.Source = (ImageSource)UpdaterPresentation.GetSingleton().FindResource("Update");
                        UpdaterPresentation.GetSingleton().ChangelogBorder.Visibility = Visibility.Visible;
                        if (changelog != null)
                        {
                            UpdaterPresentation.GetSingleton().ReadAndFillRSSChangelog(changelog);
                        }
                        break;
                    case State.Downloading:
                        UpdaterPresentation.GetSingleton().button1.IsEnabled = false;
                        UpdaterPresentation.GetSingleton().button1.Visibility = Visibility.Collapsed;
                        UpdaterPresentation.GetSingleton().progressBar1.Visibility = Visibility.Visible;
                        UpdaterPresentation.GetSingleton().label1.Content = string.Format(Properties.Resources.Downloading_update___0_____, updateName);
                        UpdaterPresentation.GetSingleton().image1.Source = (ImageSource)UpdaterPresentation.GetSingleton().FindResource("Update");
                        UpdaterPresentation.GetSingleton().ChangelogBorder.Visibility = Visibility.Visible;
                        if (changelog != null)
                        {
                            UpdaterPresentation.GetSingleton().ReadAndFillRSSChangelog(changelog);
                        }
                        break;
                    case State.UpdateReady:
                        UpdaterPresentation.GetSingleton().button1.IsEnabled = true;
                        UpdaterPresentation.GetSingleton().button1.Content = Properties.Resources.Restart_and_install_now;
                        UpdaterPresentation.GetSingleton().button1.Visibility = Visibility.Visible;
                        UpdaterPresentation.GetSingleton().progressBar1.Visibility = Visibility.Collapsed;
                        UpdaterPresentation.GetSingleton().label1.Content = string.Format(Properties.Resources.Update___0___ready_to_install_, updateName);
                        UpdaterPresentation.GetSingleton().image1.Source = (ImageSource)UpdaterPresentation.GetSingleton().FindResource("UpdateReady");
                        UpdaterPresentation.GetSingleton().ChangelogBorder.Visibility = Visibility.Visible;
                        if (changelog != null)
                        {
                            UpdaterPresentation.GetSingleton().ReadAndFillRSSChangelog(changelog);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                GuiLogMessage("AutoUpdate: Error occured while trying to get update information.", NotificationLevel.Warning);
            }
        }

        private void progressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!wc.IsBusy)
                {
                    // TODO: how to get the error cause here?
                    GuiLogMessage("AutoUpdate: Download failed.", NotificationLevel.Warning);
                    wc.CancelAsync();
                    progressTimer.Stop();
                    CurrentState = State.UpdateAvailable;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error during download: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void StartCheckTimer()
        {
            try
            {
                checkTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
                                                                                {
                                                                                    downloadRetry = 0;
                                                                                    CheckForUpdates('P');
                                                                                });
                checkTimer.Start();
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error starting the checking timer: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void BeginCheckingForUpdates(char reference, int waitSecs = 0)
        {
            new Thread(delegate()
                           {
                               if (waitSecs > 0)
                                   Thread.Sleep(waitSecs*1000);

                               CheckForUpdates(reference);
                           } ).Start();
        }

        private void CheckForUpdates(char reference)
        {
            try
            {
                CurrentState = State.Checking;
                ReadXml(reference); // sets onlineUpdateVersion

                Version downloadedVersion = ReadDownloadedUpdateVersion();

                if (IsOnlineUpdateAvailable(downloadedVersion))
                {
                    changelog = changelogTemplate.Replace("§", onlineUpdateVersion.Build.ToString());
                    updateName = onlineUpdateVersions.Element(GetBuildTypeXmlString()).Element("name").Value;

                    if (File.Exists(FilePath))
                        File.Delete(FilePath);

                    CurrentState = State.UpdateAvailable;
                }
                else if (IsUpdateReady) // downloaded update ready
                {
                    if (CurrentState != State.UpdateReady)
                    {
                        if (downloadedVersion > new Version()) // always true with NSIS update
                        {
                            changelog = changelogTemplate.Replace("§", downloadedVersion.Build.ToString());
                            updateName = downloadedVersion.ToString();
                        }
                        else
                            // may happen with ZIP update, but is unusual -- ZIP updates are either installed at startup or redownloaded again
                        {
                            changelog = null;
                            updateName = "unknown version";
                        }

                        CurrentState = State.UpdateReady;
                    }
                }
                else
                {
                    CurrentState = State.Idle;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error occured while checking: "+ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Reads available update version from temporary download location (if anything found).
        /// </summary>
        /// <returns>empty version if nothing found</returns>
        public Version ReadDownloadedUpdateVersion()
        {
            if (!File.Exists(FilePath))
                return new Version();

            var versionInfo = FileVersionInfo.GetVersionInfo(FilePath);
            return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
        }

        private bool IsOnlineUpdateAvailable(Version downloadedVersion)
        {
            return onlineUpdateVersion > currentlyRunningVersion    // newer than running
                && onlineUpdateVersion > downloadedVersion          // newer than downloaded update (always true for ZIP)
                && serverAvailable;                                 // online check succeeded
        }

        private void ReadXml(char userAgentRef)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers["User-Agent"] = GetUserAgentString(userAgentRef);

                Stream s = client.OpenRead(XmlPath);
                XElement xml = XElement.Load(s);

                onlineUpdateVersions = xml.Element("x86");

                Version.TryParse(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("version").Value, out onlineUpdateVersion);
                
                if (!serverAvailable)
                {
                    serverAvailable = true;
                    serverNotAvailableMessage = null;
                    GuiLogMessage("AutoUpdate: Checking for updates successful, connection to server available.", NotificationLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                if (serverAvailable)
                {
                    serverAvailable = false;
                    serverNotAvailableMessage = ex.Message;
                    GuiLogMessage("AutoUpdate: Cannot check for updates, no connection to server.", NotificationLevel.Warning);
                }
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
        }

        public void Download()
        {
            try
            {
                progressTimer = new System.Timers.Timer(1000 * 10);

                progressTimer.Elapsed += new System.Timers.ElapsedEventHandler(progressTimer_Elapsed);

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                wc = new System.Net.WebClient();

                wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);

                Uri downloadUri = null;
                switch(AssemblyHelper.InstallationType)
                {
                    case Ct2InstallationType.MSI:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("msidownload").Value);
                        break;
                    case Ct2InstallationType.NSIS:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("nsisdownload").Value);
                        break;
                    case Ct2InstallationType.ZIP:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("zipdownload").Value);
                        break;
                    default:
                        return;
                }

                wc.DownloadFileAsync(downloadUri, FilePathTemporary);

                CurrentState = State.Downloading;

                progressTimer.Start();

                GuiLogMessage("AutoUpdate: Downloading update...", NotificationLevel.Info);

                if (!serverAvailable)
                {
                    serverAvailable = true;
                    serverNotAvailableMessage = null;
                    GuiLogMessage("AutoUpdate: Downloading update... (Retry)", NotificationLevel.Info);
                }
                     
            }
            catch (Exception e)
            {
                if (serverAvailable)
                {
                    serverAvailable = false;
                    serverNotAvailableMessage = e.Message;
                    GuiLogMessage("AutoUpdate: Download failed (" + GetBuildTypeXmlString() + "). " + e.Message, NotificationLevel.Warning);
                }
                CurrentState = State.UpdateAvailable;
            }
        }

        private void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
                PrepareUpdate();
        }

        private void wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            try
            {
                progressTimer.Stop();
                OnUpdateDownloadProgressChanged(e.ProgressPercentage);
                UpdaterPresentation.GetSingleton().Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    UpdaterPresentation.GetSingleton().progressBar1.Value = e.ProgressPercentage;
                }, e.ProgressPercentage);
                if (wc.IsBusy)
                    progressTimer.Start();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Error during download: "+ex.Message, NotificationLevel.Error);
            }
        }

        private void PrepareUpdate()
        {
            try
            {
                progressTimer.Stop();
                string exepath = Assembly.GetExecutingAssembly().Location;
                string exedir = Path.GetDirectoryName(exepath);

                File.Copy(Path.Combine(exedir, "Lib\\Ionic.Zip.Reduced.dll"), Path.Combine(TempPath, "Ionic.Zip.Reduced.dll"), true);
                File.Move(FilePathTemporary, FilePath);

                GuiLogMessage("AutoUpdate: Update ready to install (" + GetBuildTypeXmlString() + ").", NotificationLevel.Info);

                CurrentState = State.UpdateReady;

            }
            catch (Exception)
            {
                GuiLogMessage("AutoUpdate: Cannot prepare update procedure (" + GetBuildTypeXmlString() + ").", NotificationLevel.Error);
                CurrentState = State.UpdateAvailable;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch(e.Mode)
            {
                case PowerModes.Suspend:
                    checkTimer.Stop(); // avoid timer being triggered at system resume (before network is up)
                    break;
                case PowerModes.Resume:
                    // with periodic intervals > 5 min, also check 120 sec after resume
                    if (Settings.Default.CheckInterval > 5)
                        BeginCheckingForUpdates('R', 120);

                    checkTimer.Start();
                    break;
            }
        }

        #endregion

    }

}
