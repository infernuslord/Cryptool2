﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using Ionic.Zip;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CrypUpdater
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private MainWindow m = new CrypUpdater.MainWindow();
        private bool mayRestart = false;
        internal static string cryptoolExePath;
        private string filePath;
        private string cryptoolFolderPath;
        private string tempPath = Environment.SpecialFolder.LocalApplicationData + "\\CrypTool2\\Temp";
        private string logfilePath = Environment.SpecialFolder.LocalApplicationData + "\\CrypTool2\\Temp" + "\\install.txt";
        private int cryptoolProcessID;
        private Process p;
        private List<Process> unwantedProcesses = new List<Process>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // flomar, 04/08/2011: we don't want users to use the auto updater as stand-alone 
                // application; therefore we exit with an error message if the application was 
                // started without any additional arguments (such as CT2 folder path...)
                if (e.Args.Length <= 1)
                {
                    MessageBox.Show("You shouldn't run this program.\n\nIt is part of CrypTool 2, and it is invoked automatically during updates.");
                    return;
                }

                filePath = e.Args[0];
                cryptoolFolderPath = e.Args[1];
                cryptoolExePath = e.Args[2];
                cryptoolProcessID = Convert.ToInt32(e.Args[3]);
                mayRestart = Convert.ToBoolean(e.Args[4]);
                p = Process.GetProcessById(cryptoolProcessID);

                if (p.WaitForExit(1000 * 30))
                    StartUpdateProcess();
                else
                {
                    MessageBoxButton b = MessageBoxButton.OKCancel;
                    string caption = "Timeout error";
                    MessageBoxResult result;
                    result = MessageBox.Show("CrypTool 2.0 failed to shut down. Kill the process to proceed?", caption, b);
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit();
                            StartUpdateProcess();
                        }
                        catch (Exception)
                        {
                            StartUpdateProcess();
                        }
                    }
                    else
                        MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.");
                }

            }
            catch (IndexOutOfRangeException) // parameter not set
            {
                if (cryptoolExePath != null)
                    MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.", "Error");
                else
                    UpdateFailure();
            }
            catch (FormatException) // no id or mayrestart was parsable 
            {
                UpdateFailure();
            }
            catch (ArgumentException) // the invoking process has already exited (no such process with this id exists)
            {
                StartUpdateProcess();
            }

            if (mayRestart)
            {
                File.Delete(filePath);
                RestartCryptool();
            }
            else
                App.Current.Shutdown();

        }

        private void StartUpdateProcess()
        {
            // make sure we have a valid temp path
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            unwantedProcesses = FindCrypToolProcesses();
            if (unwantedProcesses.Count == 0)
            {
                if(filePath.EndsWith("msi"))
                    StartMSI();
                else if(filePath.EndsWith("exe"))
                    StartNSIS();
                else UnpackZip(filePath, cryptoolFolderPath);
            }
            else
                AskForLicenseToKill();
        }

        private void StartMSI()
        {
            // flomar, 04/01/2011: from now on, whenever someone wants to upgrade to an MSI installation 
            // we warn the user that he should switch to an NSIS installation (TODO: this could be i18n-ed)
            MessageBox.Show("You are about to install an MSI-based installation of CrypTool 2. MSI-based installations will soon no longer be supported. We suggest you uninstall your existing MSI-based installation and upgrade to the new NSIS-based installation instead (visit the CrypTool 2 download page).", "Warning");

            try
            {
                DirectorySecurity ds = Directory.GetAccessControl(cryptoolFolderPath);

                Process p = new Process();
                p.StartInfo.FileName = "msiexec.exe";
                p.StartInfo.Arguments = "/i \"" + filePath + "\" /qb /l* " + logfilePath + " INSTALLDIR=\"" + cryptoolFolderPath + "\"";
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2.0 will be restarted.", "Error");
            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "msiexec.exe";
                    p.StartInfo.Arguments = "/i \"" + filePath + "\" /qb /l* " + logfilePath + " INSTALLDIR=\"" + cryptoolFolderPath + "\"";
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                        MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2.0 will be restarted.", "Error");
                }
                else
                    MessageBox.Show("MSI update failed: CrypTool 2.0 will be restarted.", "Error");
            }
            catch (Exception e)
            {
                MessageBox.Show("MSI update failed: " + e.Message + ". CrypTool 2.0 will be restarted.", "Error");
            }
        }

        private void StartNSIS()
        {
            try
            {
                DirectorySecurity ds = Directory.GetAccessControl(cryptoolFolderPath);

                Process p = new Process();
                p.StartInfo.FileName = filePath;
                p.StartInfo.Arguments = "/S >" + logfilePath;
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2.0 will be restarted.", "Error");
            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = filePath;
                    p.StartInfo.Arguments = "/S >" + logfilePath;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                        MessageBox.Show("The exit code is not equal to zero. See log file for more information. CrypTool 2.0 will be restarted.", "Error");
                }
                else
                    MessageBox.Show("NSIS update failed: CrypTool 2.0 will be restarted.", "Error");
            }
            catch (Exception e)
            {
                MessageBox.Show("NSIS update failed: " + e.Message + ". CrypTool 2.0 will be restarted.", "Error");
            }
        }

        private void AskForLicenseToKill()
        {
            MessageBoxButton mbb = MessageBoxButton.YesNo;
            string caption = "Error";
            string messagePart1;
            string messagePart2;
            string messagePart3;
            if (unwantedProcesses.Count > 1)
            {
                messagePart1 = "Several instances";
                messagePart2 = "are";
                messagePart3 = "these processes";
            }
            else
            {
                messagePart1 = "Another instance";
                messagePart2 = "is";
                messagePart3 = "this process";
            }
            MessageBoxResult result;
            result = MessageBox.Show(messagePart1 + " of CrypTool 2.0 using the same resources " + messagePart2 + " still running. Kill " + messagePart3 + " to proceed?", caption, mbb);
            if (result == MessageBoxResult.Yes)
            {
                KillOtherProcesses();
                StartUpdateProcess();
            }
            else
            {
                MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.");
            }
        }

        private void KillOtherProcesses()
        {
            try
            {
                foreach (Process pr in unwantedProcesses)
                {
                    if (!pr.HasExited)
                    {
                        pr.Kill();
                        pr.WaitForExit();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Update failed. Not able to remove remaining CrypTool 2.0 instances.", "Error");
            }
        }

        private void UpdateFailure()
        {
            MessageBox.Show("Update failed, wrong parameters!", "Error");
            Application.Current.Shutdown();
        }

        private void RestartCryptool()
        {
            try
            {
                Process.Start(cryptoolExePath);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                MessageBoxButton bu = MessageBoxButton.OK;
                string caption2 = "Error";
                MessageBoxResult res = MessageBox.Show("CrypTool 2.0 could not be restarted! Try again later.", caption2, bu);
                if (res == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }
        }


        private void UnpackZip(string ZipFilePath, string CryptoolFolderPath)
        {

            try
            {
                DirectorySecurity ds = Directory.GetAccessControl(CryptoolFolderPath);

                using (ZipFile zip = ZipFile.Read(ZipFilePath))
                {
                    m.Show();

                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(CryptoolFolderPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

            }
            catch (UnauthorizedAccessException)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

                if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ProcessStartInfo psi = new ProcessStartInfo("CrypUpdater.exe", "\"" + ZipFilePath + "\" " + "\"" + CryptoolFolderPath + "\" " + "\"" + cryptoolExePath + "\" " + "\"" + cryptoolProcessID + "\" \"" + Boolean.FalseString + "\"");
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                    psi.WorkingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    Process cu = Process.Start(psi);
                    cu.WaitForExit();
                }
                else
                    MessageBox.Show("Extraction failed: CrypTool 2.0 will be restarted.", "Error");
            }
            catch (Exception e)
            {
                MessageBox.Show("Extraction failed: "+e.Message+". CrypTool 2.0 will be restarted.", "Error");
            }

        }


        private List<Process> FindCrypToolProcesses()
        {
            List<Process> processList = new List<Process>();

            try
            {
                Process[] p2 = Process.GetProcessesByName("CrypWin");
                foreach (Process p in p2)
                {
                    if (Path.GetDirectoryName(p.MainModule.FileName) == cryptoolFolderPath)
                        processList.Add(p);
                }
            }
            catch (Exception)
            {
                //32 bit updater cannot check for 64 bit processes
            }

            return processList;
        }

    }
}
