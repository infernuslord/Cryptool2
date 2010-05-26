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
using Cryptool.P2PEditor.GUI;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;

namespace Cryptool.P2PEditor
{
    [EditorInfo("p2p")]
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.P2PEditor.Resources.Attributes", false, "P2P Interface",
        "Control interface for the integrated P2P network.", "P2PEditor/DetailedDescription/Description.xaml",
        "P2PEditor/images/icon.png")]
    public class P2PEditor : IEditor
    {
        public static readonly P2PEditor Instance = new P2PEditor();
        private DisplayLevel _displayLevel;

        public P2PEditor()
        {
            Presentation = new P2PEditorPresentation(this);
        }

        #region IEditor Members

        public event ChangeDisplayLevelHandler OnChangeDisplayLevel;

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;

        public void New()
        {
        }

        public void Open(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Add(Type type)
        {
            throw new NotImplementedException();
        }

        public void AddEditorSpecific(EditorSpecificPluginInfo espi)
        {
            throw new NotImplementedException();
        }

        public void DeleteEditorSpecific(EditorSpecificPluginInfo espi)
        {
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Redo()
        {
            throw new NotImplementedException();
        }

        public void ShowHelp()
        {
            throw new NotImplementedException();
        }

        public void ShowSelectedPluginDescription()
        {
            throw new NotImplementedException();
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

        public DisplayLevel DisplayLevel
        {
            get { return _displayLevel; }
            set
            {
                _displayLevel = value;
                ((P2PEditorPresentation) Presentation).DisplayLevel = value;
            }
        }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get { throw new NotImplementedException(); }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { throw new NotImplementedException(); }
        }

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
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PluginManager PluginManager
        {
            get { return null; }
            set { }
        }

        #endregion

        public void GuiLogMessage(string message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured == null) return;

            var args = new GuiLogEventArgs(message, this, notificationLevel) {Title = "-"};
            OnGuiLogNotificationOccured(this, args);
        }
    }
}