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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Cryptool.Core;
using Cryptool.CrypWin.Properties;
using Cryptool.CrypWin.Resources;
using Cryptool.PluginBase;
using CrypWin.Helper;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Cryptool.CrypWin
{
    public partial class MainWindow
    {
        #region PluginManager events
        void pluginManager_OnExceptionOccured(object sender, PluginManagerEventArgs args)
        {
            OnGuiLogNotificationOccured(Resource.pluginManager, new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Error));

            //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //  new GuiLogNotificationDelegate(OnGuiLogNotificationOccuredTS), Resource.pluginManager, new object[] { new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Error) });
        }

        void pluginManager_OnDebugMessageOccured(object sender, PluginManagerEventArgs args)
        {
            OnGuiLogNotificationOccured(Resource.pluginManager, new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Debug));

            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            //  new GuiLogNotificationDelegate(OnGuiLogNotificationOccuredTS), Resource.pluginManager, new object[] { new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Debug) });
        }

        void pluginManager_OnPluginLoaded(object sender, PluginLoadedEventArgs args)
        {
            splashWindow.ShowStatus(string.Format(Properties.Resources.Assembly___0___loaded_, args.AssemblyName),
              (((double)args.CurrentPluginNumber / (double)args.NumberPluginsFound * 100) / 2));
        }
        #endregion PluginManager events

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, null, logLevel));
        }

        int statusBarTextCounter, errorCounter, warningCounter, infoCounter, debugCounter, balloonCounter;

        public void OnGuiLogNotificationOccured(object sender, GuiLogEventArgs arg)
        {
            if (!silent)
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                        new GuiLogNotificationDelegate(OnGuiLogNotificationOccuredTS), sender, new object[] { arg });
                }
                else
                {
                    OnGuiLogNotificationOccuredTS(sender, arg);
                }
            }
        }

        void OnShowPluginDescription(object sender)
        {
            try
            {
                if (ActiveEditor != null)
                    ActiveEditor.ShowSelectedEntityHelp();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// The method shows a new log entry in the GUI. It must be called only from the GUI thread! For 
        /// log messages from other threads use GuiLogMessage()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg">Information about the log message</param>
        public void OnGuiLogNotificationOccuredTS(object sender, GuiLogEventArgs arg)
        {
           try
            {
                statusBarTextCounter++;
                LogMessage logMessage = new LogMessage();

                // Attention: The string value of enum will be used to find appropritate imgage. 
                logMessage.LogLevel = arg.NotificationLevel;
                logMessage.Nr = statusBarTextCounter;

                if (arg.Plugin != null)
                {
                    logMessage.Plugin = arg.Plugin.GetPluginInfoAttribute().Caption;
                    logMessage.Title = arg.Title;
                }
                else if (sender is string)
                {
                    logMessage.Plugin = sender as string;
                    logMessage.Title = "-";
                }
                else
                {
                    logMessage.Plugin = Resource.crypTool;
                    logMessage.Title = "-";
                }

                logMessage.Time =
                  arg.DateTime.Hour.ToString("00") + ":" + arg.DateTime.Minute.ToString("00") + ":" +
                  arg.DateTime.Second.ToString("00") + ":" + arg.DateTime.Millisecond.ToString("000");

                logMessage.Message = arg.Message;

                if (listFilter.Contains(arg.NotificationLevel))
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = logMessage.LogLevel + ": " + logMessage.Time + ": " + arg.Message;
                    statusBarItem.Content = textBlock;
                    if (arg.Message.Length >= 64)
                        notifyIcon.Text = arg.Message.Substring(0, 63);
                    else
                        notifyIcon.Text = arg.Message;
                }

                collectionLogMessages.Add(logMessage);

                //Not more than 1000 messages allowed:
                if (collectionLogMessages.Count > 1000)
                {
                    LogMessage firstLogMessage = collectionLogMessages[0];
                    switch (firstLogMessage.LogLevel)
                    {
                        case NotificationLevel.Debug:
                            debugCounter--;
                            textBlockDebugsCount.Text = string.Format("{0:0,0}", debugCounter);
                            break;
                        case NotificationLevel.Info:
                            infoCounter--;
                            textBlockInfosCount.Text = string.Format("{0:0,0}", infoCounter);
                            break;
                        case NotificationLevel.Warning:
                            warningCounter--;
                            textBlockWarningsCount.Text = string.Format("{0:0,0}", warningCounter);
                            break;
                        case NotificationLevel.Error:
                            errorCounter--;
                            textBlockErrosCount.Text = string.Format("{0:0,0}", errorCounter);
                            break;
                        case NotificationLevel.Balloon:
                            balloonCounter--;
                            textBlockBalloonsCount.Text = string.Format("{0:0,0}", balloonCounter);
                            break;
                    }
                    collectionLogMessages.Remove(firstLogMessage);
                }

                SetMessageCount();

                switch (arg.NotificationLevel)
                {
                    case NotificationLevel.Debug:
                        debugCounter++;
                        textBlockDebugsCount.Text = string.Format("{0:0,0}", debugCounter);
                        break;
                    case NotificationLevel.Info:
                        infoCounter++;
                        textBlockInfosCount.Text = string.Format("{0:0,0}", infoCounter);
                        break;
                    case NotificationLevel.Warning:
                        warningCounter++;
                        textBlockWarningsCount.Text = string.Format("{0:0,0}", warningCounter);
                        break;
                    case NotificationLevel.Error:
                        errorCounter++;
                        textBlockErrosCount.Text = string.Format("{0:0,0}", errorCounter);
                        break;
                    case NotificationLevel.Balloon:
                        balloonCounter++;
                        textBlockBalloonsCount.Text = string.Format("{0:0,0}", balloonCounter);
                        break;
                }

                ScrollToLast();

                //Balloon:
                if (WindowState == WindowState.Minimized)
                {
                    int ms = Settings.Default.BallonVisibility_ms;
                    if (arg.NotificationLevel == NotificationLevel.Balloon && Settings.Default.ShowBalloonLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Balloon_Message, arg.Message, ToolTipIcon.Info);
                    }
                    else if (arg.NotificationLevel == NotificationLevel.Error && Settings.Default.ShowErrorLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Error_Message, arg.Message, ToolTipIcon.Error);
                    }
                    else if (arg.NotificationLevel == NotificationLevel.Info && Settings.Default.ShowInfoLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Information_Message, arg.Message, ToolTipIcon.Info);
                    }
                    else if (arg.NotificationLevel == NotificationLevel.Warning && Settings.Default.ShowWarningLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Warning_Message, arg.Message, ToolTipIcon.Warning);
                    }
                    else if (arg.NotificationLevel == NotificationLevel.Debug && Settings.Default.ShowDebugLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Debug_Message, arg.Message, ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception)
            {
                //OnGuiLogNotificationOccuredTS(this, new GuiLogEventArgs(exception.Message, null, NotificationLevel.Error)); <--- causes recursion (StackOverflowException)
            }
        }

        private void SetMessageCount()
        {
            dockWindowLogMessages.Header = string.Format("{0:0,0}", listViewLogList.Items.Count) + string.Format(Properties.Resources._Messages___0__filtered_, string.Format("{0:0,0}", collectionLogMessages.Count - listViewLogList.Items.Count));
        }

        private void ScrollToLast()
        {
            if (listViewLogList.Items.Count > 0)
                listViewLogList.ScrollIntoView(listViewLogList.Items[listViewLogList.Items.Count - 1]);
        }

        private void ButtonDeleteMessages_Click(object sender, RoutedEventArgs e)
        {
            deleteAllMessages();
        }

        private void deleteAllMessages()
        {
            collectionLogMessages.Clear();
            statusBarTextCounter = 0;
            dockWindowLogMessages.Header = statusBarTextCounter + Properties.Resources._Messages;
            errorCounter = 0;
            textBlockErrosCount.Text = errorCounter.ToString();
            warningCounter = 0;
            textBlockWarningsCount.Text = warningCounter.ToString();
            infoCounter = 0;
            textBlockInfosCount.Text = infoCounter.ToString();
            debugCounter = 0;
            textBlockDebugsCount.Text = debugCounter.ToString();
            balloonCounter = 0;
            textBlockBalloonsCount.Text = balloonCounter.ToString();
        }

        public void DeleteAllMessagesInGuiThread()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                deleteAllMessages();
            }, null);
        }

        public IList<string> GetAllMessagesFromGuiThread(params NotificationLevel[] levels)
        {
            IList<string> logList = new List<string>();

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                foreach (LogMessage msg in collectionLogMessages)
                {
                    if (levels.Contains(msg.LogLevel))
                    {
                        logList.Add(msg.ToString());
                    }
                }
            }, null);

            return logList;
        }

        private void buttonError_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Error)) listFilter.Remove(NotificationLevel.Error);
            else listFilter.Add(NotificationLevel.Error);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = Properties.Resources.Hide_Errors;
                else tb.ToolTip = Properties.Resources.Show_Errors;
            }
            SetMessageCount();
        }

        private void buttonWarning_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Warning)) listFilter.Remove(NotificationLevel.Warning);
            else listFilter.Add(NotificationLevel.Warning);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = Properties.Resources.Hide_Warnings;
                else tb.ToolTip = Properties.Resources.Show_Warnings;
            }
            SetMessageCount();
        }

        private void buttonInfo_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Info)) listFilter.Remove(NotificationLevel.Info);
            else listFilter.Add(NotificationLevel.Info);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = Properties.Resources.Hide_Infos;
                else tb.ToolTip = Properties.Resources.Show_Infos;
            }
            SetMessageCount();
        }

        private void buttonDebug_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Debug)) listFilter.Remove(NotificationLevel.Debug);
            else listFilter.Add(NotificationLevel.Debug);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = Properties.Resources.Hide_Debugs;
                else tb.ToolTip = Properties.Resources.Show_Debugs;
            }
            SetMessageCount();
        }

        private void buttonBalloon_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Balloon)) listFilter.Remove(NotificationLevel.Balloon);
            else listFilter.Add(NotificationLevel.Balloon);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = Properties.Resources.Hide_Balloons;
                else tb.ToolTip = Properties.Resources.Show_Balloons;
            }
            SetMessageCount();
        }

        private void ButtonExportToHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.InitialDirectory = Settings.Default.LastPath;
                dlg.Filter = Resource.HTML_fileFilter;
                dlg.FileName = Resource.fileName_LogMessages;
                if (dlg.ShowDialog() == true)
                {
                    StringBuilder sbList = new StringBuilder();
                    int count = 0;
                    foreach (LogMessage logMessage in collectionLogMessages)
                    {
                        if (listFilter.Contains(logMessage.LogLevel))
                        {
                            sbList.Append(string.Format(Resource.row_template,
                              new object[] 
                  { 
                    LogMessage.Color(logMessage.LogLevel), 
                    logMessage.Nr.ToString(), 
                    logMessage.LogLevel.ToString(), 
                    logMessage.Time, 
                    logMessage.Plugin, 
                    logMessage.Title, 
                    logMessage.Message
                  }));
                            count++;
                        }
                    };

                    FileStream stream = File.Open(dlg.FileName, FileMode.Create);
                    StreamWriter sWriter = new StreamWriter(stream);
                    string temp1 = Resource.table_temlate.Replace("{0}", count.ToString());
                    string temp2 = temp1.Replace("{1}", sbList.ToString());
                    sWriter.Write(temp2);
                    sWriter.Close();
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Filters the LogMessages.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private bool FilterCallback(object item)
        {
            return listFilter.Contains(((LogMessage)item).LogLevel);
        }
    }
}
