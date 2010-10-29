using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using Ionic.Zip;


namespace CrypUpdater
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

        private MainWindow m;

        internal static string CryptoolExePath;

        private string ZipFilePath;

        private string CryptoolFolderPath;

        private Process p;


        private void Application_Startup(object sender, StartupEventArgs e)
        {

            m = new CrypUpdater.MainWindow();
            m.Show();
            int CryptoolProcessID;

            try
            {
                ZipFilePath = e.Args[0];
                CryptoolFolderPath = e.Args[1];
                CryptoolExePath = e.Args[2];
                CryptoolProcessID = Convert.ToInt32(e.Args[3]);
                p = Process.GetProcessById(CryptoolProcessID);

                if (p.WaitForExit(1000 * 20))
                {
                    UnpackZip(ZipFilePath, CryptoolFolderPath);
                }
                else
                {
                    m.restartFailed = true;
                    m.textBlock1.Text = "Timeout: CT2 failed to shut down, click OK to close.";
                    m.button1.IsEnabled = true;
                }

            }
            catch (IndexOutOfRangeException) // parameter not set
            {
                if (CryptoolExePath != null)
                {
                    m.textBlock1.Text = "Wrong parameters, click OK to restart CT2!";               
                }
                else
                {
                    m.restartFailed = true;
                    m.textBlock1.Text = "Wrong parameters, click OK to close.";
                }
                
                m.button1.IsEnabled = true;
            }
            catch (FormatException) // no id was parsable 
            {
                m.restartFailed = true;
                m.textBlock1.Text = "Wrong parameters, click OK to close.";
                m.button1.IsEnabled = true;
            }
            catch (ArgumentException) // the invoking process has already exited (no such process with this id exists)
            {
                UnpackZip(ZipFilePath, CryptoolFolderPath);
            }

        }


        private void UnpackZip(string ZipFilePath, string CryptoolFolderPath)
        {

            try
            {
                using (ZipFile zip = ZipFile.Read(ZipFilePath))
                {
                    int count = zip.Entries.Count;
                    int i = 0;
                    int progress = 0;

                    m.textBlock1.Text = "Processing update...";

                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(CryptoolFolderPath, ExtractExistingFileAction.OverwriteSilently);
                        i++;
                        progress = i * 100 / count;
                        UpdateProgress(progress);
                    }

                    m.textBlock1.Text = "Update successful, click OK to restart CT2!";
                    m.button1.IsEnabled = true;

                }
            }
            catch (Exception)
            {
                m.textBlock1.Text = "Update failed: File path not accessible, click OK to restart CT2!";
                m.button1.IsEnabled = true;
            }

        }

        private void UpdateProgress(double progress)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                m.progressBar1.Value = progress;
            }, null);
        }


    }
}
