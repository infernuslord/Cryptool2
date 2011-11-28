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
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Cryptool.Core;
using Cryptool.CrypWin.Helper;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Microsoft.Win32;

namespace Cryptool.CrypWin
{
    public enum FileOperationResult
    {
        Abort,
        Continue
    }

    public partial class MainWindow
    {
        private RecentFileList recentFileList = RecentFileList.GetSingleton();

        private void NewProject(Type editor)
        {
            AddEditorDispatched(editor);
            ActiveEditor.New();

            //ActiveEditor.Presentation.ToolTip = Properties.Resources.Unsaved_workspace;

            if (ActiveEditor != null)
                taskpaneCtrl.DisplayPluginSettings(ActiveEditor, ActiveEditor.GetPluginInfoAttribute().Caption, DisplayPluginMode.Normal);
            else
                taskpaneCtrl.DisplayPluginSettings(null, null, DisplayPluginMode.Normal);

            SetCurrentEditorAsDefaultEditor();
        }

        internal void OpenProjectInGuiThread(string fileName)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                OpenProject(fileName);
            }, null);
        }

        /// <summary>
        /// Warning: does not check for unsaved changes. Use OpenProject() if you need to.
        /// </summary>
        /// <param name="fileName"></param>
        private void OpenProject(string fileName)
        {
            if (File.Exists(fileName))
            {
                this.listPluginsAlreadyInitialized.Clear();
                taskpaneCtrl.ClearCache();

                var ext = new FileInfo(fileName).Extension.Remove(0, 1);
                if (ComponentInformations.EditorExtension.ContainsKey(ext))
                {
                    Type editorType = ComponentInformations.EditorExtension[ext];

                    AddEditorDispatched(editorType);
                    this.ActiveEditor.Open(fileName);
                    ActiveEditor.Presentation.ToolTip = fileName;

                    SetCurrentEditorAsDefaultEditor();
                    this.ProjectFileName = fileName;

                    if (ActiveEditor != null)
                        taskpaneCtrl.DisplayPluginSettings(ActiveEditor, ActiveEditor.GetPluginInfoAttribute().Caption,
                                                           DisplayPluginMode.Normal);
                    else
                        taskpaneCtrl.DisplayPluginSettings(null, null, DisplayPluginMode.Normal);

                    recentFileList.AddRecentFile(fileName);
                }
            }
            else
            {
                MessageBox.Show(string.Format(Properties.Resources.File__0__doesn_t_exist_, fileName), Properties.Resources.Error_loading_file);
                recentFileList.RemoveFile(fileName);
            }
        }

        private void OpenProjectFileEvent(IEditor editor, string fileName)
        {
            OpenProject(fileName);
        }

        private void OpenProject()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = CreateOpenProjectFilter();
            dlg.InitialDirectory = Settings.Default.LastPath;
            
            if (dlg.ShowDialog() == true)            
            {
                this.OpenProject(dlg.FileName);

                if (Settings.Default.useLastPath)
                    Settings.Default.LastPath = Directory.GetParent(dlg.FileName).FullName;
            }
        }

        private string[] OpenMultipleProjectsDialog()
        {
            if (SaveChangesIfNecessary() == FileOperationResult.Abort)
                return new string[0];

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = CreateOpenProjectFilter();

#if (!DEBUG)
            if (Settings.Default.LastPath != null) dlg.InitialDirectory = Settings.Default.LastPath;
#endif

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileNames;
            }

            return new string[0];
        }

        private FileOperationResult CloseProject()
        {
            FileOperationResult result = SaveChangesIfNecessary();

            if (result == FileOperationResult.Continue && !restart) // in case restart was clicked there is no project to close (updater tab is open)
                CloseIfOpen();

            return result;
        }

        private void CloseIfOpen()
        {
            this.ProjectFileName = null;

            if (ActiveEditor != null)
            {
                if (ActiveEditor.CanStop)
                {
                    StopProjectExecution();
                }
            }
        }

        private FileOperationResult SaveChangesIfNecessary(IEditor editor)
        {
            if (editor == null || !editor.HasChanges || shutdown)
                return FileOperationResult.Continue;
                        
            string file = null;
            if (editorToFileMap.ContainsKey(editor))
                file = editorToFileMap[editor];

            switch (MessageBoxHelper.SaveChanges(file))
            {
                case MessageBoxResult.Yes:
                    var before = this.ActiveEditor;
                    this.ActiveEditor = editor;
                    return SaveProject();
                    this.ActiveEditor = before;
                case MessageBoxResult.No:
                    return FileOperationResult.Continue;
                case MessageBoxResult.Cancel:
                default:
                    return FileOperationResult.Abort;
            }
        }

        private FileOperationResult SaveChangesIfNecessary()
        {
            return SaveChangesIfNecessary(this.ActiveEditor);
        }

        private FileOperationResult SaveProject()
        {
            if (ProjectFileName == null || this.ProjectFileName == String.Empty || Path.GetFullPath(this.ProjectFileName).StartsWith(defaultSamplesDirectory,true,null))
            {
                return SaveProjectAs();
            }

            this.ActiveEditor.Save(this.ProjectFileName);
            ActiveEditor.Presentation.ToolTip = this.ProjectFileName;
            recentFileList.AddRecentFile(this.ProjectFileName);
            return FileOperationResult.Continue;
        }

        private FileOperationResult SaveProjectAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (this.ProjectFileName != null)
            {
                dlg.FileName = new FileInfo(this.ProjectFileName).Name; // propose current file name as new name    
            }
            dlg.InitialDirectory = Settings.Default.LastPath;
            dlg.Filter = CreateSaveProjectFilter();

            bool isInvalidPath = true;
            do
            {
                try
                {
                    if (dlg.ShowDialog() != true) // nullable bool? may be null or false
                        return FileOperationResult.Abort;
                }
                catch (Exception) // if dialog raises a Win32Exception, we silently retry again once with another InitialDirectory (addresses #362)
                {
                    dlg.InitialDirectory = personalDir;
                    if (dlg.ShowDialog() != true)
                        return FileOperationResult.Abort;
                }

                isInvalidPath = Path.GetFullPath(dlg.FileName).StartsWith(defaultSamplesDirectory);
                if (isInvalidPath)
                {
                    MessageBox.Show("Overwriting the delivered templates is not recommended. You are being redirected to your home directory.",
                        "Please store your changes in a different directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    dlg.InitialDirectory = personalDir;
                    dlg.FileName = new FileInfo(dlg.FileName).Name;
                }
            }
            while (isInvalidPath);

            this.ProjectFileName = dlg.FileName; // dialog successful

            ActiveEditor.Save(this.ProjectFileName);
            ActiveEditor.Presentation.ToolTip = this.ProjectFileName;
            recentFileList.AddRecentFile(this.ProjectFileName);

            if (Settings.Default.useLastPath)
                Settings.Default.LastPath = Directory.GetParent(ProjectFileName).FullName;

            return FileOperationResult.Continue;
        }

        private string CreateOpenProjectFilter()
        {
            string a = string.Empty,b = string.Empty,filter = string.Empty;
            foreach (Type type in ComponentInformations.EditorExtension.Values)
            {
                filter += string.Format("{0} (*.{1}) | *.{1}|", type.GetPluginInfoAttribute().Caption, type.GetEditorInfoAttribute().DefaultExtension);
                b += string.Format("*.{0};", type.GetEditorInfoAttribute().DefaultExtension);
            }
            if (b.Length > 0)
                b.Remove(b.Length - 1, 1);
            a = string.Format("All ({0}) | {1}", b ,b);
            filter = filter.Substring(0, filter.Length - 1);
            a += "|" + filter;
            return a;
        }

        private string CreateSaveProjectFilter()
        {
            return string.Format("{0} (*.{1}) | *.{1}", this.ActiveEditor.GetPluginInfoAttribute().Caption, this.ActiveEditor.GetEditorInfoAttribute().DefaultExtension);
        }
    }

}
