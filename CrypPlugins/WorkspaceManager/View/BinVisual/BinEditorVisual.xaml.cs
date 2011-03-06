using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManager.Model;
using WorkspaceManager.View.Container;
using WorkspaceManager.View.Base;
using System.ComponentModel;
using WorkspaceManager.View.VisualComponents;
using Cryptool.PluginBase.Editor;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManager.View.Converter;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinEditorVisual.xaml
    /// </summary>
    public partial class BinEditorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public WorkspaceModel Model { get; private set; }
        public WorkspaceManager MyEditor { get; private set; }

        private ObservableCollection<BinComponentVisual> componentCollection = new ObservableCollection<BinComponentVisual>();
        public ObservableCollection<BinComponentVisual> ComponentCollection { get { return componentCollection; } private set { componentCollection = value; } }
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(EditorState), typeof(BinEditorVisual), new FrameworkPropertyMetadata(EditorState.READY, null));

        public EditorState State
        {
            get
            {
                return (EditorState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
            }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(true, null));

        public bool IsLoading
        {
            get
            {
                return (bool)base.GetValue(IsLoadingProperty);
            }
            set
            {
                base.SetValue(IsLoadingProperty, value);
            }
        }

        public static readonly DependencyProperty IsFullscreenProperty = DependencyProperty.Register("IsFullscreen",
            typeof(BinComponentVisual), typeof(BinEditorVisual), new FrameworkPropertyMetadata(null, null));

        public bool IsFullscreen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenProperty, value);
            }
        }
        #endregion

        #region Constructors
        public BinEditorVisual(WorkspaceModel model)
        {
            Model = model;
            MyEditor = (WorkspaceManager)Model.MyEditor;
            InitializeComponent();
        }
        #endregion

        #region Public
        public void Load(WorkspaceModel modelodel)
        {
            throw new NotImplementedException();
        }

        public void ResetConnections()
        {
            throw new NotImplementedException();
        }

        public void FitToScreen()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private
        #endregion

        #region Protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Event Handler
        public void update()
        {

        }

        private void AddTextHandler(object sender, AddTextEventArgs e)
        {

        }

        private void AddImageHandler(object sender, ImageSelectedEventArgs e)
        {

        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ItemsControl)
            {
                ItemsControl items = (ItemsControl)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual)
                {
                    DataObject data = new DataObject("BinConnector", element);
                    DragDrop.AddQueryContinueDragHandler(this, QueryContinueDragHandler);
                    DragDrop.DoDragDrop(items, data, DragDropEffects.Move);
                }
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {

        }

        #region DragDropHandler

        private void QueryContinueDragHandler(Object source, QueryContinueDragEventArgs e)
        {
            e.Handled = true;

            // Check if we need to bail
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                return;
            }

            // Now, default to actually having dropped
            e.Action = DragAction.Drop;

            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.None)
            {
                // Still dragging with Left Mouse Button
                e.Action = DragAction.Continue;
            }
            else if ((e.KeyStates & DragDropKeyStates.RightMouseButton) != DragDropKeyStates.None)
            {
                // Still dragging with Right Mouse Button
                e.Action = DragAction.Continue;
            }
        }


        private void PreviewDragEnterHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDragLeaveHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (this.State != EditorState.READY)
                return;

            if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject"))
            {
                try
                {
                    DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = (PluginModel)Model.ModifyModel(new NewPluginModelOperation(new Point(300,300), 0, 0, DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
                    ComponentCollection.Add(new BinComponentVisual(pluginModel));
                    //(ScrollViewer2.Content as Panel).Children.Add(new BinComponentVisual(pluginModel));
                    MyEditor.HasChanges = true;
                }
                catch (Exception ex)
                {
                    MyEditor.GuiLogMessage(string.Format("Could not add Plugin to Workspace: {0}", ex.Message), NotificationLevel.Error);
                    MyEditor.GuiLogMessage(ex.StackTrace, NotificationLevel.Error);
                }
            }

        }
        #endregion

        #endregion
    }
}
