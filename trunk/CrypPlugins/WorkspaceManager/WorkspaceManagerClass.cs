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
using Cryptool.Core;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase;

using WorkspaceManager.Model;
using WorkspaceManager.Execution;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Media.Imaging;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using WorkspaceManager.View.BinVisual;
using WorkspaceManager.View.Base;
using WorkspaceManagerModel.Model.Operations;

//Disable warnings for unused or unassigned fields and events:
#pragma warning disable 0169, 0414, 0067

namespace WorkspaceManager
{
    /// <summary>
    /// Workspace Manager - PluginEditor based on MVC Pattern
    /// </summary>
    [EditingInfoAttribute(true)]
    [TabColor("LightSlateGray")]
    [EditorInfo("cwm")]
    [Author("Viktor Matkovic,Nils Kopal", "nils.kopal@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("WorkspaceManager.Properties.Resources", "PluginCaption", "PluginTooltip", "WorkspaceManager/Documentation/doc.xml", "WorkspaceManager/View/Image/WorkspaceManagerIcon.ico")]
    public class WorkspaceManagerClass : IEditor
    {
        public event EventHandler SampleLoaded;
        public event EventHandler<LoadingErrorEventArgs> LoadingErrorOccurred;

        /// <summary>
        /// Create a new Instance of the Editor
        /// </summary>
        public WorkspaceManagerClass()
        {
            this.SelectedPluginsList = new ObservableCollection<BinComponentVisual>();
            Settings = new WorkspaceManagerSettings(this);            
            WorkspaceModel = new WorkspaceModel();
            WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
            WorkspaceModel.MyEditor = this;
            WorkspaceSpaceEditorView = new BinEditorVisual(WorkspaceModel);
            WorkspaceSpaceEditorView.SampleLoaded += new EventHandler(WorkspaceSpaceEditorView_SampleLoaded);
        }

        void WorkspaceSpaceEditorView_SampleLoaded(object sender, EventArgs e)
        {
            if (SampleLoaded != null)
                SampleLoaded.Invoke(this,null);
        }

        #region private Members

        private WorkspaceModel WorkspaceModel = null;
        private BinEditorVisual WorkspaceSpaceEditorView = null;
        public ExecutionEngine ExecutionEngine = null;
        private volatile bool executing = false;

        #endregion

        /// <summary>
        /// Is this Editor executing?
        /// </summary>
        public bool isExecuting(){
            return executing;
        }

        public event EventHandler executeEvent;     //Event for BinSettingsVisual to notice when executing, to disable settings that may not be changed during execution       
        


        #region IEditor Members

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
        /// Current filename
        /// </summary>
        public string CurrentFile { private set; get; }

        /// <summary>
        /// Called by clicking on the new button of CrypTool
        /// Creates a new Model
        /// </summary>
        public void New()
        {
            //foreach (PluginModel pluginModel in new List<PluginModel>(WorkspaceModel.GetAllPluginModels()))
            //{
            //    WorkspaceModel.deletePluginModel(pluginModel);
            //}
            CurrentFile = typeof(WorkspaceManagerClass).GetPluginStringResource("unnamed_project");
            if (this.OnProjectTitleChanged != null)
            {
                this.OnProjectTitleChanged.Invoke(this, CurrentFile);
            }
            WorkspaceModel.UndoRedoManager.ClearStacks();
            WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
            WorkspaceModel.MyEditor = this;
            this.SelectedPluginsList.Clear();
        }

        /// <summary>
        /// Open the given model in the editor
        /// </summary>
        /// <param name="fileName"></param>
        public void Open(WorkspaceModel model)
        {
            try
            {
                New();
                WorkspaceModel = model;
                WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
                WorkspaceSpaceEditorView.Load(WorkspaceModel);
                WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
                WorkspaceModel.MyEditor = this;
                WorkspaceModel.UndoRedoManager.ClearStacks();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not open Model:" + ex.ToString(), NotificationLevel.Error);
            }
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
                var persistance = new ModelPersistance();
                persistance.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                WorkspaceModel = persistance.loadModel(fileName);
                WorkspaceModel.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;
                WorkspaceSpaceEditorView.Load(WorkspaceModel);
                WorkspaceModel.UpdateableView = this.WorkspaceSpaceEditorView;
                this.OnProjectTitleChanged.Invoke(this, System.IO.Path.GetFileName(fileName));
                CurrentFile = fileName;
                WorkspaceModel.MyEditor = this;
                WorkspaceModel.UndoRedoManager.ClearStacks();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                GuiLogMessage( "Could not load Model:" +s, NotificationLevel.Error);
                if(LoadingErrorOccurred != null)
                    LoadingErrorOccurred.Invoke(this, new LoadingErrorEventArgs() { Message = s });
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
                var persistance = new ModelPersistance();
                persistance.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                persistance.saveModel(this.WorkspaceModel, fileName);
                this.OnProjectTitleChanged.Invoke(this, System.IO.Path.GetFileName(fileName));
                CurrentFile = fileName;
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
                PluginModel pluginModel = (PluginModel)WorkspaceSpaceEditorView.Model.ModifyModel(new NewPluginModelOperation(new Point(0,0), 0, 0, type));
                WorkspaceSpaceEditorView.AddBinComponentVisual(pluginModel, 1);
            }
        }

        /// <summary>
        /// Undo changes
        /// </summary>
        public void Undo()
        {
            if (WorkspaceModel.UndoRedoManager != null)
            {
                try
                {
                    WorkspaceModel.UndoRedoManager.Undo();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Can not undo:" + ex.Message,NotificationLevel.Error);
                }
            }
        }

        /// <summary>
        /// Redo changes
        /// </summary>
        public void Redo()
        {
            if (WorkspaceModel.UndoRedoManager != null)
            {
                try
                {
                    WorkspaceModel.UndoRedoManager.Redo();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Can not redo:" + ex.Message, NotificationLevel.Error);
                }
            }
        }

        public void Cut()
        {
           
        }

        public void Copy()
        {
            
        }

        public void Paste()
        {
            
        }

        public void Remove()
        {
            
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
                UIElement control = (UIElement)((BinEditorVisual)this.Presentation).ScrollViewer.Content;
                PrintDialog dialog = new PrintDialog();
                dialog.PageRangeSelection = PageRangeSelection.AllPages;
                dialog.UserPageRangeEnabled = true;

                Nullable<Boolean> print = dialog.ShowDialog();
                if (print == true)
                {
                    this.GuiLogMessage("Printing document \"" + this.CurrentFile + "\" now", NotificationLevel.Info);
                    
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
                    dialog.PrintDocument(fixedDoc.DocumentPaginator, "WorkspaceManager_" + this.CurrentFile);
                    this.GuiLogMessage("Printed \"" + fixedDoc.DocumentPaginator.PageCount + "\" pages of document \"" + this.CurrentFile + "\"", NotificationLevel.Info);
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
            if (((BinEditorVisual)Presentation).SelectedItems != null && ((BinEditorVisual)Presentation).SelectedItems[0] is BinComponentVisual)
            {
                var element = (BinComponentVisual)((BinEditorVisual)Presentation).SelectedItems[0];
                OnlineHelp.InvokeShowPluginDocPage(element.Model.PluginType);
            }
            else
            {
                OnlineHelp.InvokeShowPluginDocPage(typeof (WorkspaceManagerClass));
            }
        }

        /// <summary>
        /// Show the Description of the selected plugin
        /// </summary>
        public void ShowSelectedPluginDescription()
        {
            if (selectedPluginsList.Count > 0)      //This doesn't work!
            {
                var plugin = selectedPluginsList[0];
                //OnlineHelp.InvokeShowPluginDocPage(plugin.cont);
            }
            else
            {
                ShowHelp();
            }
        }

        /// <summary>
        /// Is Undo possible
        /// </summary>
        public bool CanUndo
        {
            get
            {
                if (WorkspaceModel.UndoRedoManager != null)
                {
                    return !this.isExecuting() && WorkspaceModel.UndoRedoManager.CanUndo();
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Is Redo possible?
        /// </summary>
        public bool CanRedo
        {
            get
            {
                if (WorkspaceModel.UndoRedoManager != null)
                {
                    return !this.isExecuting() && WorkspaceModel.UndoRedoManager.CanRedo();
                }
                else
                {
                    return false;
                }
            }
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

        /// <summary>
        /// Can the ExecutionEngine be started?
        /// </summary>
        public bool CanExecute
        {
            get{
                return ((BinEditorVisual)Presentation).IsLoading == true || executing ? false : true;
            }
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
            get
            {
                if (this.WorkspaceModel != null)
                {
                    return this.WorkspaceModel.HasChanges;
                }
                return false;
            }
        }

        public bool CanPrint
        {
            get { return true; }
        }

        public bool CanSave
        {
            get { return true; }
        }
        
        public string SamplesDir
        {
            set {  }
        }

        public bool ReadOnly { get; set; }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// 
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

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
            set { WorkspaceSpaceEditorView = (BinEditorVisual)value; }
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
                executeEvent(this, EventArgs.Empty);
                
                
                
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
                    this.WorkspaceSpaceEditorView.ResetPlugins(1);
                    this.WorkspaceSpaceEditorView.State = BinEditorState.BUSY;                   
                }
                , null);

                this.ExecutionEngine = new ExecutionEngine(this);
                this.ExecutionEngine.OnGuiLogNotificationOccured += this.GuiLogNotificationOccured;

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
                        GuiLogMessage("SleepTime can not be <=0; Use SleepTime = 0", NotificationLevel.Warning);
                        ExecutionEngine.SleepTime = 0;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Could not set SleepTime: " + ex.Message, NotificationLevel.Warning);
                    ExecutionEngine.GuiUpdateInterval = 0;
                }                

                ExecutionEngine.BenchmarkPlugins = ((WorkspaceManagerSettings)this.Settings).BenchmarkPlugins;
                
                //we only start gui update thread if we are visible (for example, during the execution of the wizard
                //we are not visible, so we need no update of gui elements)
                var updateGuiElements = Presentation.IsVisible;

                ExecutionEngine.Execute(WorkspaceModel, updateGuiElements);               
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during the execution: " + ex.Message, NotificationLevel.Error);
                executing = false;
                executeEvent(this, EventArgs.Empty);
                if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
                {
                    EventsHelper.AsynchronousProgressChanged = true;
                    EventsHelper.AsynchronousGuiLogMessage = true;
                    EventsHelper.AsynchronousStatusChanged = true;
                }
            }
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
       
            var stopThread = new Thread(new ThreadStart(waitingStop));
            stopThread.Start(); 

            EventsHelper.AsynchronousPropertyChanged = true;

            if (((WorkspaceManagerSettings)this.Settings).SynchronousEvents)
            {
                EventsHelper.AsynchronousProgressChanged = true;
                EventsHelper.AsynchronousGuiLogMessage = true;
                EventsHelper.AsynchronousStatusChanged = true;
            }

            this.WorkspaceSpaceEditorView.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.WorkspaceSpaceEditorView.ResetConnections();
                this.WorkspaceSpaceEditorView.ResetPlugins(0);
                this.WorkspaceSpaceEditorView.State = BinEditorState.READY;
            }
            , null);
                       
        }

        /// <summary>
        /// Stops the execution engine and blocks until this work is done
        /// </summary>
        private void waitingStop()
        {
            lock (this)
            {
                if (!executing)
                {
                    return;
                }
                try
                {
                    GuiLogMessage("Stopping execution.", NotificationLevel.Info);
                    ExecutionEngine.Stop();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Exception during the stopping of the execution: " + ex.Message, NotificationLevel.Error);
                }
                this.ExecutionEngine = null;
                GC.Collect();
                executing = false;
                executeEvent(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to initialize the editor
        /// </summary>
        public void Initialize()
        {
            //nothing
        }

        /// <summary>
        /// Called when the editor is disposed
        /// </summary>
        public void Dispose()
        {
            if (ExecutionEngine != null && ExecutionEngine.IsRunning())
            {
                try
                {
                    ExecutionEngine.Stop();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during stopping of the ExecutionEngine: {0}", ex), NotificationLevel.Error);
                }
            }

            EventsHelper.AsynchronousPropertyChanged = true;

            try
            {
                if (WorkspaceModel != null)
                {
                    WorkspaceModel.Dispose();
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Exception during disposing of the Model: {0}",ex),NotificationLevel.Error);
            }
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

        #region GuiLogMessage

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
                foreach (PluginModel pluginModel in this.WorkspaceModel.GetAllPluginModels())
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
                foreach (PluginModel pluginModel in this.WorkspaceModel.GetAllPluginModels())
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

        #endregion GuiLogMessage

        /// <summary>
        /// Selected Plugin changed by View
        /// </summary>
        /// <param name="args"></param>
        public void onSelectedPluginChanged(PluginChangedEventArgs args)
        {
            this.OnSelectedPluginChanged(this, args);
        }

        #region IEditor Members

        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        #endregion

        private ObservableCollection<BinComponentVisual> selectedPluginsList;

        /// <summary>
        /// Selected Collection of Plugin's
        /// </summary> 
        public ObservableCollection<BinComponentVisual> SelectedPluginsList
        {
            get
            {
                return selectedPluginsList;
            }
            set
            {
                selectedPluginsList = value;
            }
        }

        public bool IsCtrlToggled = false;
        public BinEditorState State { get; set; }


        public void AddText()
        {
            ((BinEditorVisual)Presentation).AddText();
        }

        public void AddImage()
        {
            System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri uriLocal = new Uri(diag.FileName);
                ((BinEditorVisual)Presentation).AddImage(uriLocal);
            }
        }
    }

    public class LoadingErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}

//Restore warnings for unused or unassigned fields and events:
#pragma warning restore 0169, 0414, 0067