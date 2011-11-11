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
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;

namespace Cryptool.P2PEditor
{
    [TabColor("orange")]
    [EditorInfo("p2p", false, true, false, false, true)]
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.P2PEditor.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", 
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

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public void New()
        {
            if (OnSelectedPluginChanged != null)
                OnSelectedPluginChanged(this, new PluginChangedEventArgs(this, Properties.Resources.P2P_Configuration, DisplayPluginMode.Normal));
            
            if (!P2PManager.IsConnected)
            {
                //GuiLogMessage("Cannot display new job form, no connection to p2p network.", NotificationLevel.Warning);
                return;
            }

            // Avoid switching to the add view, but allow using the new button later
            if (!initialNewEventHandled)
                initialNewEventHandled = true;
            else
                ((P2PEditorPresentation) Presentation).ShowJobCreationView();
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

        public void Undo()
        {
            GuiLogMessage("P2PEditor: Undo()", NotificationLevel.Debug);
        }

        public void Redo()
        {
            GuiLogMessage("P2PEditor: Redo()", NotificationLevel.Debug);
        }

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Copy()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void Print()
        {
            throw new NotImplementedException();
        }

        public void ShowSelectedPluginDescription()
        {
        }

        public bool CanUndo
        {
            get { return false; }
        }

        public bool CanRedo
        {
            get { return false; }
        }

        public bool CanCut
        {
            get { return false; }
        }

        public bool CanCopy
        {
            get { return false; }
        }

        public bool CanPaste
        {
            get { return false; }
        }

        public bool CanRemove
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

        public bool CanPrint
        {
            get { return false; }
        }

        public bool CanSave
        {
            get { return false; }
        }

        public string CurrentFile
        {
            get { return null; }
        }

        public string SamplesDir
        {
            set {  }
        }

        public bool ReadOnly { get; set; }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings { get; private set; }

        public UserControl Presentation { get; private set; }

        public void PreExecution()
        {
        }

        public void Execute()
        {
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            ((P2PEditorSettings)Settings).UpdateSettings();
            OnProjectTitleChanged(this, Properties.Resources.P2PEditor_Tab_Caption);
            Presentation.ToolTip = Properties.Resources.P2PEditor_Tab_Caption;
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

        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;


        public double GetZoom()
        {
            return double.NaN;
        }

        public void Zoom(double value)
        {
            
        }

        public void FitToScreen()
        {
            
        }


        public event EventHandler<PluginBase.Miscellaneous.ZoomChanged> OnZoomChanged;


        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }
    }
}
