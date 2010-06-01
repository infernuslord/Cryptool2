/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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
using Cryptool.Core;
using Cryptool.PluginBase.Editor;
using Cryptool.UiPluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;

using WorkspaceManager.Model;
using WorkspaceManager.View;
using WorkspaceManager.Execution;
using Cryptool.Plugins.CostFunction;
using Cryptool.TextInput;
using WorkspaceManager.View.Container;
using WorkspaceManager.View.Converter;

//Disable warnings for unused or unassigned fields and events:
#pragma warning disable 0169, 0414, 0067

namespace WorkspaceManager
{
    /// <summary>
    /// Workspace Manager - PluginEditor based on MVC Pattern
    /// </summary>
    [EditorInfo("cwm")]
    [Author("Viktor Matkovic,Nils Kopal", "nils.kopal@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("WorkspaceManager.Resources.Attributes", false, "Workspace Manager", "Graphical plugin editor for the CrypTool workspace", "WorkspaceManager/DetailedDescription/Description.xaml",
      "AnotherEditor/icon.png",
      "AnotherEditor/Images/addWorkspace.png",
      "AnotherEditor/Images/deleteWorkspace.png",
      "AnotherEditor/Images/importSubWorkspace.png")]
    public class WorkspaceManager : IEditor
    {

        /// <summary>
        /// Create a new Instance of the Editor
        /// </summary>
        public WorkspaceManager()
        {
            New();            
            
            PluginModel cost = WorkspaceModel.newPluginModel(1, 1, 1, 1, typeof(CostFunction));
            PluginModel text = WorkspaceModel.newPluginModel(1, 1, 1, 1, typeof(TextInput));
            
            ConnectorModel connCost = cost.InputConnectors[0];
            ConnectorModel connText = text.OutputConnectors[0];

            WorkspaceModel.newConnectionModel(connText, connCost, connCost.ConnectorType);

            ((TextInput)text.Plugin).TextOutput = "Test";
            
        }

        #region private Members

        private WorkspaceModel WorkspaceModel = null;
        private WorkSpaceEditorView WorkspaceManagerEditorView = null;
        private ExecutionEngine ExecutionEngine = null;
        private bool executing = false;

        #endregion

        /// <summary>
        /// Is this Editor executing?
        /// </summary>
        public bool isExecuting(){
            return executing;
        }

        #region IEditor Members

        /// <summary>
        /// 
        /// </summary>
        public event ChangeDisplayLevelHandler OnChangeDisplayLevel;

        /// <summary>
        /// 
        /// </summary>
        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        /// <summary>
        /// 
        /// </summary>
        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        /// <summary>
        /// 
        /// </summary>
        public event OpenProjectFileHandler OnOpenProjectFile;

        /// <summary>
        /// 
        /// </summary>
        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;

        /// <summary>
        /// Called by clicking on the new button of CrypTool
        /// Creates a new Model
        /// </summary>
        public void New()
        {
            WorkspaceModel = new WorkspaceModel();
            WorkspaceModel.WorkspaceManagerEditor = this;
            WorkspaceManagerEditorView = new WorkSpaceEditorView(WorkspaceModel);
            ExecutionEngine = new ExecutionEngine(this);
            HasChanges = false;
        }

        /// <summary>
        /// Called by clicking on the open button of CrypTool
        /// </summary>
        /// <param name="fileName"></param>
        public void Open(string fileName)
        {
            try
            {
                GuiLogMessage("Loading Model: " + fileName, NotificationLevel.Info);
                WorkspaceModel = ModelPersistance.loadModel(fileName);
                HasChanges = false;
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not load Model:" + ex.ToString(), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called by clicking on the save button of CrypTool
        /// Serializes the Model into an xml file
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            try
            {
                GuiLogMessage("Saving Model: " + fileName, NotificationLevel.Info);
                ModelPersistance.saveModel(this.WorkspaceModel, fileName);
                HasChanges = false;
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not save Model:" + ex.ToString(), NotificationLevel.Error);                
            }
        }

        /// <summary>
        /// Called by double clicking on a plugin symbol of CrypTool
        /// Adds a new PluginModel wrapping an instance of the selected plugin
        /// </summary>
        /// <param name="type"></param>
        public void Add(Type type)
        {
            if (!executing)
            {
                PluginModel newPluginModel = WorkspaceModel.newPluginModel(10, 10, 100, 100, type);
                GuiLogMessage("Added by double click: " + newPluginModel.Name, NotificationLevel.Info);
                HasChanges = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="espi"></param>
        public void AddEditorSpecific(EditorSpecificPluginInfo espi)
        {
            //to be implemented
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="espi"></param>
        public void DeleteEditorSpecific(EditorSpecificPluginInfo espi)
        {
            //to be implemented    
        }

        /// <summary>
        /// Undo changes
        /// </summary>
        public void Undo()
        {
            //to be implemented
        }

        /// <summary>
        /// Redo changes
        /// </summary>
        public void Redo()
        {
            //to be implemented
        }

        /// <summary>
        /// Show the Help site
        /// </summary>
        public void ShowHelp()
        {
            //to be implemented
        }

        /// <summary>
        /// Show the Description of the selected plugin
        /// </summary>
        public void ShowSelectedPluginDescription()
        {
            //to be implemented
        }

        /// <summary>
        /// Is Undo possible
        /// </summary>
        public bool CanUndo
        {
            get;
            set;
        }

        /// <summary>
        /// Is Redo possible?
        /// </summary>
        public bool CanRedo
        {
            get;
            set;
        }

        /// <summary>
        /// Can the ExecutionEngine be started?
        /// </summary>
        public bool CanExecute
        {
            get{return !executing;}
        }

        /// <summary>
        /// Can the ExecutionEngine be stopped?
        /// </summary>
        public bool CanStop
        {
            get { return executing; }
        }

        /// <summary>
        /// Does this Editor has changes?
        /// </summary>
        public bool HasChanges
        {
            get;
            set;
        }

        /// <summary>
        /// DisplayLevel
        /// </summary>
        public DisplayLevel DisplayLevel
        {
            get;
            set;
            
        }

        /// <summary>
        /// 
        /// </summary>
        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// 
        /// </summary>
        public event StatusChangedEventHandler OnPluginStatusChanged;

        /// <summary>
        /// 
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// 
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Settings of this editor
        /// </summary>
        public ISettings Settings
        {
            get;
            set;
        }

        /// <summary>
        /// The Presentatio of this editor
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get {return WorkspaceManagerEditorView;}
            set { WorkspaceManagerEditorView = (WorkSpaceEditorView)value; }
        }

        /// <summary>
        /// The QuickWatchPresentation of this editor
        /// </summary>
        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get;
            set;
        }

        /// <summary>
        /// Called before execution
        /// </summary>
        public void PreExecution()
        {
            //to be implemented
        }

        /// <summary>
        /// Starts the ExecutionEngine to execute the model
        /// </summary>
        public void Execute()
        {
            if (executing)
            {
                return;
            }

            //for debug purposes. Later on this will be a setting of the new editor
            ExecutionEngine.DebugMode = true;

            try
            {
                GuiLogMessage("Execute Model now!", NotificationLevel.Info);
                executing = true;                
                ExecutionEngine.Execute(WorkspaceModel);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during the execution: " + ex.Message, NotificationLevel.Error);
                executing = false;
            }
        }

        /// <summary>
        /// Called after the execution
        /// </summary>
        public void PostExecution()
        {
            //to be implemented
        }

        /// <summary>
        /// Pause the execution
        /// </summary>
        public void Pause()
        {
            ExecutionEngine.Pause();
        }

        /// <summary>
        /// Stop the ExecutionEngine
        /// </summary>
        public void Stop()
        {
            if (!executing)
            {
                return;
            }

            try
            {
                GuiLogMessage("Executing stopped by User!", NotificationLevel.Info);
                ExecutionEngine.Stop();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during the stopping of the execution: " + ex.Message, NotificationLevel.Error);
                
            }
            executing = false;
        }

        /// <summary>
        /// Called to initialize the editor
        /// </summary>
        public void Initialize()
        {
            //to be implemented
        }

        /// <summary>
        /// Called when the editor is disposed
        /// </summary>
        public void Dispose()
        {
            //to be implemented
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IApplication Members

        /// <summary>
        /// 
        /// </summary>
        private PluginManager pluginManager;
        public PluginManager PluginManager
        {
            get { return pluginManager; }
            set
            {
                pluginManager = value;
                DragDropDataObjectToPluginConverter.PluginManager = value;
            }          
        }

        #endregion

        #region GuiLogMessage, Progress

        /// <summary>
        /// Loggs a message to the logging mechanism of CrypTool
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="notificationLevel"></param>
        public void GuiLogMessage(string Message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                GuiLogEventArgs args = new GuiLogEventArgs(Message, this, notificationLevel);
                args.Title = "-";
                OnGuiLogNotificationOccured(this, args);
            }
        }

        /// <summary>
        /// Progress of this editor
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="MaxValue"></param>
        private void Progress(int Value, int MaxValue)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(Value, MaxValue));
            }
        }
        #endregion GuiLogMessage, Progress
    }
}

//Restore warnings for unused or unassigned fields and events:
#pragma warning restore 0169, 0414, 0067