/*
   Copyright 2008 Martin Saternus, Arno Wacker, Thomas Schmid, Sebastian Przybylski

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
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase;
using System.Windows;
using Cryptool.CrypWin.Resources;
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase.Editor;
using System.Windows.Controls;
using StartCenter;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Cryptool.CrypWin.Helper;

namespace Cryptool.CrypWin
{
    public partial class MainWindow
    {
        #region Private variables
        private bool closedByMenu = false;
        private Window aboutWindow;

        private DemoController demoController;
        #endregion

        #region New, Open, Save, SaveAs, Close
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            if (Settings.Default.useDefaultEditor)
                NewProject(GetDefaultEditor());
            else
                NewProject(GetEditor(Settings.Default.preferredEditor));
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            OpenProject();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanSave;
            e.Handled = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            SaveProject();
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanSave;
            e.Handled = true;
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            SaveProjectAs();
        }

        private void Print_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (MainTab == null)
                return;

            var tab = (CTTabItem) (MainTab.SelectedItem);
            if (tab == null)
                return;

            if (tabToContentMap.ContainsKey(tab))
            {
                var o = tabToContentMap[tab];
                if (o is OnlineHelpTab)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = ActiveEditor != null && ActiveEditor.CanPrint;
                }
            }
            e.Handled = true;
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MainTab == null)
                return;

            var tab = (CTTabItem)(MainTab.SelectedItem);
            if (tab == null)
                return;

            if (tabToContentMap.ContainsKey(tab))
            {
                var o = tabToContentMap[tab];
                if (o is OnlineHelpTab)
                {
                    ((OnlineHelpTab) o).Print();
                }
                else
                {
                    if (ActiveEditor != null)
                        ActiveEditor.Print();
                }
            }
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            closedByMenu = true;
            this.Close();
        }
        #endregion New, Open, Save, SaveAs, Close

        # region ContextHelp, Play, Stop, Pause, Undo, Redo, Maximize, Fullscreen
        private void ContextHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ContextHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
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

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanExecute;
            if (e.CanExecute)
            {
                playStopMenuItem.Text = "Play";
                playStopMenuItem.Tag = true;
            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            PlayProject();
        }

        public void PlayProjectInGuiThread()
        {
            if (this.Dispatcher.CheckAccess())
            {
                PlayProject();
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    PlayProject();
                }, null);
            }
        }

        private void PlayProject()
        {
            PlayProject(ActiveEditor);
        }

        private void PlayProject(IEditor editor)
        {
            try
            {
                if (editor != null)
                {
                    var editorThread = new Thread(editor.Execute);
                    editorThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                    editorThread.Start();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanStop;
            if (e.CanExecute)
            {
                playStopMenuItem.Text = "Stop";
                playStopMenuItem.Tag = false;
            }
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            StopProjectExecution();
        }

        public void StopProjectInGuiThread()
        {
            if (this.Dispatcher.CheckAccess())
            {
                StopProjectExecution();
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    StopProjectExecution();
                }, null);
            }
        }

        private void StopProjectExecution(IEditor editor)
        {
            if (editor != null)
            {
                editor.Stop();
            }
        }

        private void StopProjectExecution()
        {
            StopProjectExecution(ActiveEditor);
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanUndo;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Undo();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanRedo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Redo();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null;
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Cut();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanCopy;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Copy();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanPaste;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Paste();
        }

        private void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanRemove;
        }

        private void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null) ActiveEditor.Remove();
        }

        private void AddImage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.GetEditorInfoAttribute().CanEdit;
        }

        private void AddText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.GetEditorInfoAttribute().CanEdit;
        }

        private void Maximize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Fullscreen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Fullscreen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AppRibbon.IsMinimized = !AppRibbon.IsMinimized; // toggle
        }

        private void PlayDemo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = demoController != null && !demoController.IsRunning;
        }

        private void PlayDemo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            string[] samples = OpenMultipleProjectsDialog();
            string saveFile = null;

            if (IsCommandParameterGiven("-test"))
            {
                SaveFileDialog saveSelector = new SaveFileDialog();
                saveSelector.Filter = "Log file (*.txt)|*.txt";
                if (saveSelector.ShowDialog() == true)
                {
                    saveFile = saveSelector.FileName;
                }
                else
                {
                    return;
                }
            }

            if (samples.Length > 0)
            {
                SetRibbonControlEnabled(false);
                demoController.Start(samples, saveFile); // saveFile may be null, it's okay
            }
        }

        private void StopDemo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = demoController != null && demoController.IsRunning;
        }

        private void StopDemo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
                return;
            demoController.Stop();
        }
        # endregion Play, Stop, Pause, Undo, Redo, Maximize, Fullscreen, Demo

        # region P2P

        private void P2P_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void P2P_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO synchronize instance with editor selection in settings tab
            AddEditorDispatched(typeof(P2PEditor.P2PEditor));
        }

        # endregion P2P

        # region Startcenter

        private void Startcenter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Startcenter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddEditorDispatched(typeof(StartcenterEditor));
        }

        # endregion Startcenter

        # region AutoUpdater
        private void AutoUpdater_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AutoUpdater_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdaterPresentation updaterPresentation = UpdaterPresentation.GetSingleton();
            OpenTab(updaterPresentation, new TabInfo() { Title = Properties.Resources.CrypTool_2_0_Update }, null).IsSelected = true;
        }

        # endregion P2P

        #region Settings
        private void Settings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsPresentation settingsPresentation = SettingsPresentation.GetSingleton();
            OpenTab(settingsPresentation, new TabInfo() { Title = Properties.Resources.Settings }, null).IsSelected = true;
        }

        #endregion Settings

        # region help
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowHelpPage(typeof(MainWindow));
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                aboutWindow = new Splash(true);    
                // does not activate window, though this may hide the modal dialog in background in certain (uncommon) situations
                aboutWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        # endregion help
    }

}
