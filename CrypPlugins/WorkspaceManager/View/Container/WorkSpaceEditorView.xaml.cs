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
using WorkspaceManager.View.Converter;
using WorkspaceManager.View.Interface;
using WorkspaceManager.View.VisualComponents;
using WorkspaceManager.Model;
using Cryptool.PluginBase.Editor;
using Cryptool.Core;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace WorkspaceManager.View.Container
{
    public enum EditorState
    {
        READY,
        BUSY
    };
    /// <summary>
    /// Interaction logic for WorkSpaceEditorView.xaml
    /// </summary>
    /// 
    public partial class WorkSpaceEditorView : UserControl
    {

        private Point? lastCenterPositionOnTarget;
        private Point? lastMousePositionOnTarget;
        private Point? lastDragPoint;
        private Point? startDrag;
        private Point previousDragPoint = new Point();
        private ConnectorView selectedConnector;
        private PluginContainerView selectedPluginContainer;
        private CryptoLineView dummyLine = new CryptoLineView();
        private PluginContainerView currentFullViewContainer;
        private Panel root { get { return (this.scrollViewer.Content as Panel); } }
        private BottomBox bottomBox { get { return (BottomBoxParent.Child as BottomBox); } }
        public bool IsCtrlToggled { get; set; }

        public UserContentWrapper UserContentWrapper { get; set; }
        public EditorState State;
        public EditorState ConnectorState;
        public List<CryptoLineView> ConnectionList = new List<CryptoLineView>();
        public List<PluginContainerView> PluginContainerList = new List<PluginContainerView>();
        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get { return model; }
            set { model = value; }
        }

        public WorkSpaceEditorView()
        {
            InitializeComponent();
        }

        public WorkSpaceEditorView(WorkspaceModel WorkspaceModel)
        {
            setBaseControl(WorkspaceModel);                        
            InitializeComponent();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.DataContext = root;

            this.bottomBox.FitToScreen += new EventHandler<FitToScreenEventArgs>(bottomBox_FitToScreen);
            this.UserContentWrapper = new UserContentWrapper(WorkspaceModel, bottomBox);
            this.UserControlWrapperParent.Children.Clear();
            this.UserControlWrapperParent.Children.Add(UserContentWrapper);       
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (IsCtrlToggled)
                return;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.HorizontalChange);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.VerticalChange);
            this.Cursor = Cursors.SizeAll;
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(scrollViewer);

            if (e.Delta > 0)
            {
                Properties.Settings.Default.EditScale += 0.2;
            }
            if (e.Delta < 0)
            {
                Properties.Settings.Default.EditScale -= 0.2;
            }

            e.Handled = true;
        }

        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlToggled)
                return;

            rectangle.Visibility = Visibility.Visible;
            startDrag = e.GetPosition(root);
            this.Cursor = Cursors.Cross;
        }

        private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rectangle.Visibility = Visibility.Collapsed;
            this.Cursor = Cursors.Arrow;
            startDrag = null;
            lastDragPoint = null;
        }

        void WorkSpaceEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsCtrlToggled && startDrag.HasValue)
            {
                Point currentPoint = e.GetPosition(root);
                Point hasValue = (Point)startDrag;

                //Calculate the top left corner of the rectangle 
                //regardless of drag direction
                double x = hasValue.X < currentPoint.X ? hasValue.X : currentPoint.X;
                double y = hasValue.Y < currentPoint.Y ? hasValue.Y : currentPoint.Y;

                //Move the rectangle to proper place
                rectangle.RenderTransform = new TranslateTransform(x, y);
                //Set its size
                rectangle.Width = Math.Abs(e.GetPosition(root).X - hasValue.X);
                rectangle.Height = Math.Abs(e.GetPosition(root).Y - hasValue.Y);
            }
            dummyLine.EndPoint = Mouse.GetPosition(root);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsCtrlToggled)
                return;

            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, scrollViewer);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(scrollViewer);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / scrollViewer.Width;
                    double multiplicatorY = e.ExtentHeight / scrollViewer.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0)
            {
                IsCtrlToggled = true;
            }
            else
                IsCtrlToggled = false;

            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & Keyboard.GetKeyStates(Key.C) & KeyStates.Down) > 0)
            {
                Copy();
            }

            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & Keyboard.GetKeyStates(Key.V) & KeyStates.Down) > 0)
            {
                Paste();
            }
        }

        public void Copy()
        {
            if (IsMouseOver)
            {
                List<PluginModel> list = new List<PluginModel>();
                foreach (PluginContainerView plugin in Model.SelectedPluginsList)
                {
                    list.Add(plugin.Model);
                }

                PluginCopyWrapper w = new PluginCopyWrapper(list.ToArray());
                Clipboard.SetData("PluginCopy", w);
                var v = Clipboard.GetData("PluginCopy");
            }
        }

        public void Paste()
        {
            if (IsMouseOver)
            {
                PluginCopyWrapper v = (PluginCopyWrapper)Clipboard.GetData("PluginCopy");

                if (!(v is PluginCopyWrapper))
                    return;

                foreach (PluginModel model in v.Model)
                {
                    //this.loadPluginContainerView(model);
                    model.WorkspaceModel = this.Model;
                    model.generateConnectors();
                    this.AddPluginContainerView(model.Position, model);
                }

                //foreach (ConnectionModel connModel in Model.AllConnectionModels)
                //{
                //    CryptoLineView conn = new CryptoLineView(connModel);
                //    connModel.UpdateableView = conn;
                //    connModel.OnDelete += DeleteConnection;

                //    foreach (UIElement element in root.Children)
                //    {
                //        PluginContainerView container = element as PluginContainerView;
                //        if (container != null)
                //        {
                //            foreach (ConnectorView connector in container.ConnectorViewList)
                //            {
                //                if (connModel.From == connector.Model)
                //                {
                //                    conn.StartPointSource = connector;
                //                    conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(connector));
                //                }
                //                else if (connModel.To == connector.Model)
                //                {
                //                    conn.EndPointSource = connector;
                //                    conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(connector));
                //                }
                //            }
                //        }
                //    }

                //    root.Children.Add(conn);
                //    ConnectionList.Add(conn);
                //    Canvas.SetZIndex(conn, 0);
                //}
            }
        }

        

        void bottomBox_FitToScreen(object sender, FitToScreenEventArgs e)
        {

            if (scrollViewer.ScrollableWidth > 0 || scrollViewer.ScrollableHeight > 0)
            {
                while (Properties.Settings.Default.EditScale > Properties.Settings.Default.MinScale && (scrollViewer.ScrollableHeight > 0 || scrollViewer.ScrollableWidth > 0))
                {
                    Properties.Settings.Default.EditScale -= 0.02;
                    scrollViewer.UpdateLayout();
                }
            }
            else
            {
                while (Properties.Settings.Default.EditScale < Properties.Settings.Default.MaxScale && scrollViewer.ScrollableHeight == 0 && scrollViewer.ScrollableWidth == 0)
                {
                    Properties.Settings.Default.EditScale += 0.02;
                    scrollViewer.UpdateLayout();
                }
                if (scrollViewer.ScrollableHeight > 0 || scrollViewer.ScrollableWidth > 0)
                    Properties.Settings.Default.EditScale -= 0.02;
            }
        }

        private void setBaseControl(WorkspaceModel WorkspaceModel)
        {
            this.MouseMove +=new MouseEventHandler(WorkSpaceEditorView_MouseMove);
            this.MouseLeave += new MouseEventHandler(WorkSpaceEditorView_MouseLeave);
            this.Loaded += new RoutedEventHandler(WorkSpaceEditorView_Loaded);
            this.DragEnter += new DragEventHandler(WorkSpaceEditorView_DragEnter);
            this.Drop += new DragEventHandler(WorkSpaceEditorView_Drop);
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(WorkSpaceEditorView_PreviewMouseRightButtonDown);
            this.Model = WorkspaceModel;
            this.State = EditorState.READY;
        }

        void PluginDelete(object sender, PluginContainerViewDeleteViewEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                Model.deletePluginModel(e.container.Model);
                root.Children.Remove(e.container);
                Model.WorkspaceManagerEditor.HasChanges = true;
            }
        }

        void WorkSpaceEditorView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void AddPluginContainerView(Point position, PluginModel model)
        {
            if (this.State == EditorState.READY)
            {
                PluginContainerView newPluginContainerView = new PluginContainerView(model);
                newPluginContainerView.Delete += new EventHandler<PluginContainerViewDeleteViewEventArgs>(PluginDelete);
                newPluginContainerView.FullScreen += new EventHandler<PluginContainerViewFullScreenViewEventArgs>(shape_FullScreen);
                newPluginContainerView.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
                newPluginContainerView.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
                newPluginContainerView.ConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
                newPluginContainerView.SetPosition(new Point((Math.Round((position.X) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((position.Y) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));

                this.root.Children.Add(newPluginContainerView);
                Canvas.SetZIndex(newPluginContainerView, 100);
                Model.WorkspaceManagerEditor.HasChanges = true;
            }
        }


        void shape_FullScreen(object sender, PluginContainerViewFullScreenViewEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Visible;
            e.container.PrepareFullscreen();
            this.PercentageTextPanel.Children.Add(e.container.ProgressPercentage);
            this.PrstPanel.DataContext = e.container;
            this.CtrPanel.DataContext = e.container;
            this.CtrPanel.Children.Add(e.container.OptionPanel);
            this.PrstPanel.Children.Add(e.container.ViewPanel);
            this.ProgressbarPanel.Children.Add(e.container.ProgressbarParent);
            this.currentFullViewContainer = e.container;
        }

        public void AddConnection(ConnectorView source, ConnectorView target)
        {
            if (this.State == EditorState.READY)
            {
                ConnectionModel connectionModel = this.Model.newConnectionModel(((ConnectorView)source).Model, ((ConnectorView)target).Model, ((ConnectorView)source).Model.ConnectorType);
                CryptoLineView conn = new CryptoLineView(connectionModel,source,target);
                conn.StartPointSource = source;
                conn.EndPointSource = target;
                connectionModel.UpdateableView = conn;
                connectionModel.OnDelete += DeleteConnection;
                conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(source));
                conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(target));
                root.Children.Add(conn);
                ConnectionList.Add(conn);
                Canvas.SetZIndex(conn, 0);
            }
        }

        public void DeleteConnection(Object sender, EventArgs args)
        {
            if (sender is ConnectionModel)
            {
                if(((ConnectionModel)sender).UpdateableView != null){
                    UIElement uielement = (UIElement)((ConnectionModel)sender).UpdateableView;
                    root.Children.Remove(uielement);
                }
            }
        }

        private void AddConnectionSource(ConnectorView source, CryptoLineView conn)
        {
            if (this.State == EditorState.READY)
            {
                Color color = ColorHelper.GetColor((source as ConnectorView).Model.ConnectorType);
                conn.Stroke = new SolidColorBrush(color);
                conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(source));
                conn.EndPoint = Mouse.GetPosition(root);
            }
        }

        private MultiBinding CreateConnectorBinding(ConnectorView connectable)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new ConnectorBindingConverter();
            multiBinding.ConverterParameter = connectable;

            Binding binding = new Binding();
            binding.Source = connectable.Parent;
            binding.Path = new PropertyPath(PluginContainerView.X);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.Parent;
            binding.Path = new PropertyPath(PluginContainerView.Y);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.Parent.West;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.Parent.East;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.Parent.North;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.Parent.South;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            return multiBinding;
        }

        #region Controls

        //void WorkSpaceEditorView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    UserContentWrapper.SelectedItem = null;
        //    PluginChangedEventArgs args = new PluginChangedEventArgs(this.model.WorkspaceManagerEditor, "WorkspaceManager", DisplayPluginMode.Normal);
        //    this.Model.WorkspaceManagerEditor.onSelectedPluginChanged(args);
        //    e.Handled = false;
        //}

        void shape_OnConnectorMouseLeftButtonDown(object sender, ConnectorViewEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                if (selectedConnector != null && WorkspaceModel.compatibleConnectors(selectedConnector.Model, e.connector.Model))
                {
                    this.root.Children.Remove(dummyLine);
                    this.AddConnection(selectedConnector, e.connector);
                    this.selectedConnector = null;
                    return;
                }

                if (selectedConnector == null && e.connector.Model.Outgoing)
                {
                    this.root.Children.Add(dummyLine);
                    this.selectedConnector = e.connector;
                    this.AddConnectionSource(e.connector, dummyLine);
                    return;
                }
            }
            
        }

        void WorkSpaceEditorView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserContentWrapper.SelectedItem = null;

            if (this.State == EditorState.READY)
            {
                this.selectedConnector = null;
                this.root.Children.Remove(dummyLine);
            }
        }

        void WorkSpaceEditorView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                this.selectedPluginContainer = null;
            }
        }      



        void shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(sender as PluginContainerView, 100);
            this.selectedPluginContainer = null;
        }

        void shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.selectedPluginContainer = sender as PluginContainerView;
            this.previousDragPoint = Mouse.GetPosition(selectedPluginContainer); 
            Canvas.SetZIndex(selectedPluginContainer, 101);

            PluginChangedEventArgs args = new PluginChangedEventArgs(this.selectedPluginContainer.Model.Plugin, this.selectedPluginContainer.Model.Name, DisplayPluginMode.Normal);
            this.Model.WorkspaceManagerEditor.onSelectedPluginChanged(args);

            e.Handled = true;
        }

        void WorkSpaceEditorView_Drop(object sender, DragEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject"))
                {
                    try
                    {
                        DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                        PluginModel pluginModel = Model.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName));
                        if (obj != null)
                            this.AddPluginContainerView(e.GetPosition(root), pluginModel);
                        Model.WorkspaceManagerEditor.HasChanges = true;
                    }
                    catch (Exception ex)
                    {
                        this.Model.WorkspaceManagerEditor.GuiLogMessage("Could not add Plugin to Workspace:" + ex.Message, NotificationLevel.Error);
                        this.Model.WorkspaceManagerEditor.GuiLogMessage(ex.StackTrace, NotificationLevel.Error);
                        return;
                    }
                }
                else
                    return;
            }
        }

        void WorkSpaceEditorView_DragEnter(object sender, DragEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (this.State == EditorState.BUSY)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Collapsed;
            this.CtrPanel.Children.Clear();
            this.PrstPanel.Children.Clear();
            this.ProgressbarPanel.Children.Clear();
            this.PercentageTextPanel.Children.Clear();
            this.currentFullViewContainer.Reset();
            this.currentFullViewContainer = null;
            this.CtrPanel.DataContext = null;
            this.PrstPanel.DataContext = null;
        }

        internal void Load(WorkspaceModel WorkspaceModel)
        {
            this.Model = WorkspaceModel;

            foreach (PluginModel model in this.Model.AllPluginModels)
            {
                bool skip = false;
                foreach (ConnectorModel connModel in model.InputConnectors)
                {
                    if (connModel.IControl && connModel.InputConnections.Count > 0)
                    {
                        skip = true;
                        break;
                    }
                }
                if(!skip)
                    this.loadPluginContainerView(model);
            }
            foreach (ConnectionModel connModel in WorkspaceModel.AllConnectionModels)
            {
                if (connModel.To.IControl)
                    continue;

                CryptoLineView conn = new CryptoLineView(connModel,null,null);
                connModel.UpdateableView = conn;
                connModel.OnDelete += DeleteConnection;

                foreach (UIElement element in root.Children)
                {
                    PluginContainerView container = element as PluginContainerView;
                    if (container != null)
                    {
                        foreach (ConnectorView connector in container.ConnectorViewList)
                        {
                            if (connModel.From == connector.Model)
                            {
                                conn.StartPointSource = connector;
                                conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(connector));
                            }
                            else if (connModel.To == connector.Model)
                            {
                                conn.EndPointSource = connector;
                                conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(connector));
                            }
                        }
                    }
                }

                root.Children.Add(conn);
                ConnectionList.Add(conn);
                Canvas.SetZIndex(conn, 0);
            }
        }

        private void loadPluginContainerView(PluginModel model)
        {
            PluginContainerView newPluginContainerView = new PluginContainerView(model);

            newPluginContainerView.Delete += new EventHandler<PluginContainerViewDeleteViewEventArgs>(PluginDelete);
            newPluginContainerView.FullScreen += new EventHandler<PluginContainerViewFullScreenViewEventArgs>(shape_FullScreen);
            newPluginContainerView.ConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
            newPluginContainerView.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
            newPluginContainerView.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
            newPluginContainerView.SetPosition(model.Position);
            this.root.Children.Add(newPluginContainerView);
            Canvas.SetZIndex(newPluginContainerView, 100);
        }

        internal void ResetConnections()
        {
            foreach (CryptoLineView line in ConnectionList)
            {
                line.Reset();
            }
        }

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                PluginChangedEventArgs args = new PluginChangedEventArgs(this.model.WorkspaceManagerEditor, "WorkspaceManager", DisplayPluginMode.Normal);
                this.Model.WorkspaceManagerEditor.onSelectedPluginChanged(args);
            }
        }

        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
        }
    }

    [Serializable]
    public class PluginCopyWrapper
    {
        public PluginModel[] Model { get; private set; }

        public PluginCopyWrapper(PluginModel[] model)
        {
            this.Model = model;
        }
    }
}
