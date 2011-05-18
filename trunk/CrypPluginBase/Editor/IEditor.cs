/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

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
using Cryptool.UiPluginBase;
using System.Collections.Generic;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.PluginBase.Editor
{
    public interface IEditor : IPlugin, IApplication
    {
        event SelectedPluginChangedHandler OnSelectedPluginChanged;
        event ProjectTitleChangedHandler OnProjectTitleChanged;
        event OpenProjectFileHandler OnOpenProjectFile;
        event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;
        event OpenTabHandler OnOpenTab;
        event OpenEditorHandler OnOpenEditor;
        event EventHandler<ZoomChanged> OnZoomChanged;

        void New();
        void Open(string fileName);
        void Save(string fileName);

        void Add(Type type);
        void AddEditorSpecific(EditorSpecificPluginInfo espi);
        void DeleteEditorSpecific(EditorSpecificPluginInfo espi);
        void Undo();
        void Redo();
        void Cut();
        void Copy();
        void Paste();
        void Remove();
        void Print();
        void AddText();
        void AddImage();

        double GetZoom(); 
        void Zoom(double value);
        void FitToScreen();
        
        /// <summary>
        /// Temp. extension to show help page if the tutorial-pdf-file is not available and
        /// the "?-Button" in CrypWin is pressed.
        /// </summary>
        void ShowHelp();

        /// <summary>
        /// Used to display a plugin specific description button in settings pane. 
        /// </summary>
        void ShowSelectedPluginDescription();

        void Active();

        bool CanUndo { get; }
        bool CanRedo { get; }
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanRemove { get; }
        bool CanExecute { get; }
        bool CanStop { get; }
        bool HasChanges { get; }
        bool CanPrint { get; }

        string CurrentFile { get; }

        string SamplesDir { set; }

        /// <summary>
        /// Gets the editor specific plugins, e.g. connector plugins to build subworkspace and the
        /// currently available subworkspaces.
        /// </summary>
        /// <value>The editor specific plugins.</value>        
        List<EditorSpecificPluginInfo> EditorSpecificPlugins { get; }

        /// <summary>
        /// Gets or sets the readOnly propability of an editor i.e. if something on the workspace can be changed.
        /// </summary>
        bool ReadOnly { get; set; }
    }
}
