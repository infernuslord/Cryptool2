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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.Core;
using Cryptool.P2P;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.GUI;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;

namespace Cryptool.P2PEditor
{
    [EditorInfo("p2p")]
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.P2PEditor.Resources.Attributes", false, "P2P Interface",
        "Control interface for the integrated P2P network.", null,
        "P2PEditor/images/icon.png")]
    public class P2PEditor : IEditor
    {
        private readonly JobListManager jobListManager;

        private bool initialNewEventHandled;

        public P2PEditor()
        {
            jobListManager = new JobListManager(this);
            initialNewEventHandled = false;

            Presentation = new P2PEditorPresentation(this, jobListManager);
            Settings = new P2PEditorSettings(this);
        }

        #region IEditor Members

        public event ChangeDisplayLevelHandler OnChangeDisplayLevel;

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;

        public void New()
        {
            if (OnSelectedPluginChanged != null)
                OnSelectedPluginChanged(this, new PluginChangedEventArgs(this, "P2P Configuration", DisplayPluginMode.Normal));
            
            if (!P2PManager.IsConnected)
            {
                GuiLogMessage("Cannot display new job form, no connection to p2p network.", NotificationLevel.Warning);
                return;
            }

            // Avoid switching to the add view, but allow using the new button later
            if (!initialNewEventHandled)
                initialNewEventHandled = true;
            else
                ((P2PEditorPresentation) Presentation).ShowJobCreation();
        }

        public void Open(string fileName)
        {
            GuiLogMessage("P2PEditor: Open(" + fileName + ")", NotificationLevel.Debug);
        }

        public void Save(string fileName)
        {
            GuiLogMessage("P2PEditor: Save(" + fileName + ")", NotificationLevel.Debug);
        }

        public void Add(Type type)
        {
            GuiLogMessage("P2PEditor: Add(" + type + ")", NotificationLevel.Debug);
        }

        public void AddEditorSpecific(EditorSpecificPluginInfo espi)
        {
            GuiLogMessage("P2PEditor: AddEditorSpecific()", NotificationLevel.Debug);
        }

        public void DeleteEditorSpecific(EditorSpecificPluginInfo espi)
        {
            GuiLogMessage("P2PEditor: DeleteEditorSpecific()", NotificationLevel.Debug);
        }

        public void Undo()
        {
            GuiLogMessage("P2PEditor: Undo()", NotificationLevel.Debug);
        }

        public void Redo()
        {
            GuiLogMessage("P2PEditor: Redo()", NotificationLevel.Debug);
        }

        public void ShowHelp()
        {
            GuiLogMessage("P2PEditor: ShowHelp()", NotificationLevel.Debug);
        }

        public void ShowSelectedPluginDescription()
        {
            GuiLogMessage("P2PEditor: ShowSelectedPluginDescription()", NotificationLevel.Debug);
        }

        public bool CanUndo
        {
            get { return false; }
        }

        public bool CanRedo
        {
            get { return false; }
        }

        public bool CanExecute
        {
            get { return false; }
        }

        public bool CanStop
        {
            get { return false; }
        }

        public bool HasChanges
        {
            get { return false; }
        }

        public DisplayLevel DisplayLevel { get; set; }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get { return new List<EditorSpecificPluginInfo>(); }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings { get; private set; }

        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation
        {
            get { throw new NotImplementedException(); }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PluginManager PluginManager { get; set; }

        #endregion

        public void GuiLogMessage(string message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured == null) return;

            var args = new GuiLogEventArgs(message, this, notificationLevel) {Title = "-"};
            OnGuiLogNotificationOccured(this, args);
        }

        public void SendOpenProjectFileEvent(string filename)
        {
            if (OnOpenProjectFile != null) OnOpenProjectFile(this, filename);
        }
    }
}