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
using WorkspaceManager.View.Container;
using WorkspaceManager.View.Converter;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Cryptool.PluginBase.Miscellaneous;
using WorkspaceManager.View.VisualComponents;
using System.Windows.Media.Imaging;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Markup;

//Disable warnings for unused or unassigned fields and events:
#pragma warning disable 0169, 0414, 0067

namespace WorkspaceManager
{
    /// <summary>
    /// Workspace Manager - PluginEditor based on MVC Pattern
    /// </summary>
    [EditorInfo("cwm")]
    [Author("Viktor Matkovic,Nils Kopal", "nils.kopal@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("WorkspaceManager.Resources.Attributes", false, "Workspace Manager", "Graphical plugin editor for the CrypTool workspace", null,
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
            Settings = new WorkspaceManagerSettings(this);
            WorkspaceModel = new WorkspaceModel();
            WorkspaceModel.WorkspaceManagerEditor = this;
            WorkspaceSpaceEditorView = new WorkSpaceEditorView(WorkspaceModel);
            HasChanges = false;                                
        }

        #region private Members

        private WorkspaceModel WorkspaceModel = null;
        private WorkSpaceEditorView WorkspaceSpaceEditorView = null;
        private ExecutionEngine ExecutionEngine = null;
        private volatile bool executing = false;
        private volatile bool stopping = false;

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
        /// Current filename
        /// </summary>
        public string CurrentFilename { private set; get; }

        /// <summary>
        /// Called by clicking on the new button of CrypTool
        /// Creates a new Model
        /// </summary>
        public void New()
        {
            foreach (PluginModel pluginModel in new List<PluginModel>(WorkspaceModel.AllPluginModels))
            {
                WorkspaceModel.deletePluginModel(pluginModel);
            }
            this.HasChanges = false;
            CurrentFilename = "unnamed project";
            this.OnProjectTitleChanged.BeginInvoke(this, "unnamed project", null, null);
        }

        /// <summary>
        /// Called by clicking on the open button of CrypTool
        /// loads a serialized model
        /// </summary>
        /// <param name="fileName"></param>
        public void Open(string fileName)
        {
            try
            {
                New();
                GuiLogMessage("Loading Model: " + fileName, NotificationLevel.Info);                
                WorkspaceModel = ModelPersistance.loadModel(fileName,this);                
                WorkspaceSpaceEditorView.Load(WorkspaceModel);
                HasChanges = false;
                this.OnProjectTitleChanged.BeginInvoke(this, System.IO.Path.GetFileName(fileName), null, null);
                CurrentFilename = fileName;
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not load Model:" + ex.ToString(), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called by clicking on the save button of CrypTool
        /// serializes the current model
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            try
            {
                GuiLogMessage("Saving Model: " + fileName, NotificationLevel.Info);
                ModelPersistance.saveModel(this.WorkspaceModel, fileName);
                HasChanges = false;
                this.OnProjectTitleChanged.BeginInvoke(this, System.IO.Path.GetFileName(fileName), null, null);
                CurrentFilename = fileName;
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
            /*if (!executing)
            {
                PluginModel newPluginModel = WorkspaceModel.newPluginModel(new Point(10, 10), 100, 100, type);
                GuiLogMessage("Added by double click: " + newPluginModel.Name, NotificationLevel.Info);
                HasChanges = true;
            }*/
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

        public void Print()
        {
            try
            {
                Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
                double dx = m.M11 * 96;
                double dy = m.M22 * 96;
                this.GuiLogMessage("dx=" + dx + " dy=" + dy, NotificationLevel.Debug);
                const int factor = 4;
                ModifiedCanvas control = (ModifiedCanvas)((WorkSpaceEditorView)this.Presentation).ViewBox.Content;
                PrintDialog dialog = new PrintDialog();
                dialog.PageRangeSelection = PageRangeSelection.AllPages;
                dialog.UserPageRangeEnabled = true;

                Nullable<Boolean> print = dialog.ShowDialog();
                if (print == true)
                {
                    this.GuiLogMessage("Printing document \"" + this.CurrentFilename + "\" now", NotificationLevel.Info);
                    
                    PrintCapabilities capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
                    System.Windows.Size pageSize = new System.Windows.Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                    System.Windows.Size visibleSize = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);

                    FixedDocument fixedDoc = new FixedDocument();
                    control.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    control.Arrange(new Rect(new System.Windows.Point(0, 0), control.DesiredSize));
                    System.Windows.Size size = control.DesiredSize;

                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width * factor, (int)size.Height * factor, dx * factor, dy * factor, PixelFormats.Pbgra32);
                    bmp.Render(control);


                    double xOffset = 0;
                    double yOffset = 0;
                    while (xOffset < size.Width)
                    {
                        yOffset = 0;
                        while (yOffset < size.Height)
                        {
                            PageContent pageContent = new PageContent();
                            FixedPage page = new FixedPage();
                            ((IAddChild)pageContent).AddChild(page);
                            fixedDoc.Pages.Add(pageContent);
                            page.Width = pageSize.Width;
                            page.Height = pageSize.Height;
                            int width = (xOffset + visibleSize.Width) > size.Width ? (int)(size.Width - xOffset) : (int)visibleSize.Width;
                            int height = (yOffset + visibleSize.Height) > size.Height ? (int)(size.Height - yOffset) : (int)visibleSize.Height;
                            System.Windows.Controls.Image croppedImage = new System.Windows.Controls.Image();                            
                            CroppedBitmap cb = new CroppedBitmap(bmp, new Int32Rect((int)xOffset * factor, (int)yOffset *factor, width * factor, height * factor));
                            croppedImage.Source = cb;
                            croppedImage.Width = width;
                            croppedImage.Height = height;
                            page.Children.Add(croppedImage);
                            yOffset += visibleSize.Height;
                        }
                        xOffset += visibleSize.Width;
                    }
                    dialog.PrintDocument(fixedDoc.DocumentPaginator, "WorkspaceManager_" + this.CurrentFilename);
                    this.GuiLogMessage("Printed \"" + fixedDoc.DocumentPaginator.PageCount + "\" pages of document \"" + this.CurrentFilename + "\"", NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.GuiLogMessage("Exception while printing: " + ex.Message, NotificationLevel.Error);
            }
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

        public bool CanPrint
        {
            get { return true; }
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

        public bool ReadOnly { get; set; }

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
        /// The Presentation of this editor
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get {return WorkspaceSpaceEditorView;}
            set { WorkspaceSpaceEditorView = (WorkSpaceEditorView)value; }
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
            EventsHelper.AsynchronousPropertyChanged = false;
            try
            {
                GuiLogMessage("Execute Model now!", NotificationLevel.Info);
                executing = true;

                if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
                {
                    EventsHelper.AsynchronousProgressChanged = false;
                    EventsHelper.AsynchronousGuiLogMessage = false;
                    EventsHelper.AsynchronousStatusChanged = false;
                }

                //Get the gui Thread
                this.WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.WorkspaceSpaceEditorView.ResetConnections();
                    this.WorkspaceSpaceEditorView.State = EditorState.BUSY;                   
                }
                , null);

                this.ExecutionEngine = new ExecutionEngine(this);

                try
                {
                    ExecutionEngine.GuiUpdateInterval = int.Parse(((WorkspaceManagerSettings)this.Settings).GuiUpdateInterval);
                    if (ExecutionEngine.GuiUpdateInterval <= 0)
                    {
                        GuiLogMessage("GuiUpdateInterval can not be <=0; Use GuiUpdateInterval = 1", NotificationLevel.Warning);
                        ExecutionEngine.GuiUpdateInterval = 1;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Could not set GuiUpdateInterval: " + ex.Message, NotificationLevel.Warning);
                    ExecutionEngine.GuiUpdateInterval = 100;
                }

                try
                {
                    ExecutionEngine.SleepTime = int.Parse(((WorkspaceManagerSettings)this.Settings).SleepTime);
                    if (ExecutionEngine.SleepTime < 0)
                    {
                        GuiLogMessage("SleepTime can not be <=0; Use GuiUpdateInterval = 0", NotificationLevel.Warning);
                        ExecutionEngine.SleepTime = 0;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Could not set SleepTime: " + ex.Message, NotificationLevel.Warning);
                    ExecutionEngine.GuiUpdateInterval = 0;
                }

                int schedulers=0;
                try
                {
                   schedulers = int.Parse(((WorkspaceManagerSettings)this.Settings).Threads);
                    if (ExecutionEngine.SleepTime < 0)
                    {
                        GuiLogMessage("Schedulers can not be <=0; Use Schedulers = 1", NotificationLevel.Warning);
                        schedulers = 1;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Could not set Schedulers: " + ex.Message, NotificationLevel.Warning);
                    schedulers = 1;
                }

                ExecutionEngine.BenchmarkPlugins = ((WorkspaceManagerSettings)this.Settings).BenchmarkPlugins;
                ExecutionEngine.ThreadPriority = ((WorkspaceManagerSettings)this.Settings).ThreadPriority;

                ExecutionEngine.Execute(WorkspaceModel, schedulers);               
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during the execution: " + ex.Message, NotificationLevel.Error);
                executing = false;
                if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
                {
                    EventsHelper.AsynchronousProgressChanged = true;
                    EventsHelper.AsynchronousGuiLogMessage = true;
                    EventsHelper.AsynchronousStatusChanged = true;
                }
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
            //to be implemented
        }

        /// <summary>
        /// Stop the ExecutionEngine
        /// </summary>
        public void Stop()
        {
            if (!executing || stopping)
            {
                return;
            }

            stopping = true;

            Thread stopThread = new Thread(new ThreadStart(waitingStop));
            stopThread.Start(); 

            EventsHelper.AsynchronousPropertyChanged = true;

            if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
            {
                EventsHelper.AsynchronousProgressChanged = true;
                EventsHelper.AsynchronousGuiLogMessage = true;
                EventsHelper.AsynchronousStatusChanged = true;
            }

                       
        }

        /// <summary>
        /// Stops the execution engine and blocks until this work is done
        /// </summary>
        private void waitingStop()
        {
            try
            {
                GuiLogMessage("Executing stopped by User!", NotificationLevel.Info);
                ExecutionEngine.Stop();
                //Get the gui Thread
                this.WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.WorkspaceSpaceEditorView.ResetConnections();
                    this.WorkspaceSpaceEditorView.State = EditorState.READY;
                }
                , null);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during the stopping of the execution: " + ex.Message, NotificationLevel.Error);
            }
            executing = false;
            this.ExecutionEngine = null;
            GC.Collect();
            stopping = false;
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
            if (ExecutionEngine != null && ExecutionEngine.IsRunning)
            {
                ExecutionEngine.Stop();
            }
            EventsHelper.AsynchronousPropertyChanged = true;
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
        /// GuiLogNotificationOccured
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void GuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            //Check if the logging event is Warning or Error and set the State of the PluginModel to
            //the corresponding PluginModelState
            if (args.NotificationLevel == NotificationLevel.Warning)
            {                
                foreach (PluginModel pluginModel in this.WorkspaceModel.AllPluginModels)
                {
                    if (pluginModel.Plugin == sender)
                    {
                        pluginModel.State = PluginModelState.Warning;
                        pluginModel.GuiNeedsUpdate = true;
                    }
                }
            }

            if (args.NotificationLevel == NotificationLevel.Error)
            {               
                foreach (PluginModel pluginModel in this.WorkspaceModel.AllPluginModels)
                {
                    if (pluginModel.Plugin == sender)
                    {
                        pluginModel.State = PluginModelState.Error;
                        pluginModel.GuiNeedsUpdate = true;
                    }
                }
            }

            if (OnGuiLogNotificationOccured != null)
            {
                switch (((WorkspaceManagerSettings)this.Settings).LogLevel)
                {
                    case 3://Error
                        if (args.NotificationLevel == NotificationLevel.Debug ||
                            args.NotificationLevel == NotificationLevel.Info ||
                            args.NotificationLevel == NotificationLevel.Warning)
                        {
                            return;
                        }
                        break;

                    case 2://Warning
                        if (args.NotificationLevel == NotificationLevel.Debug ||
                            args.NotificationLevel == NotificationLevel.Info)
                        {
                            return;
                        }
                        break;

                    case 1://Info
                        if (args.NotificationLevel == NotificationLevel.Debug)
                        {
                            return;
                        }
                        break;
                }
                OnGuiLogNotificationOccured(sender, args);
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

        /// <summary>
        /// Selected Plugin changed by View
        /// </summary>
        /// <param name="args"></param>
        public void onSelectedPluginChanged(PluginChangedEventArgs args)
        {
            this.OnSelectedPluginChanged(this, args);
        }

        #region IEditor Members


        public void Active()
        {            
        }

        #endregion

        #region IEditor Members


        public event OpenTabHandler OnOpenTab;

        #endregion
    }
}

//Restore warnings for unused or unassigned fields and events:
#pragma warning restore 0169, 0414, 0067