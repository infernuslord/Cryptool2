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

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for WorkSpaceEditorView.xaml
    /// </summary>
    public partial class WorkSpaceEditorView : UserControl
    {
        public enum EditorState
        {
            READY,
            BUSY
        };

        private Point previousDragPoint = new Point();
        private ConnectorView selectedConnector;
        private PluginContainerView selectedPluginContainer;
        private CryptoLineView dummyLine = new CryptoLineView();
        private Point p;

        public EditorState State;
        public EditorState ConnectorState;
        public List<CryptoLineView> LineList = new List<CryptoLineView>();
        public List<PluginContainerView> PluginContainerList = new List<PluginContainerView>();
        private WorkspaceModel WorkspaceModel;

        public WorkSpaceEditorView()
        {
            InitializeComponent();
        }

        public WorkSpaceEditorView(WorkspaceModel WorkspaceModel)
        {
            this.Loaded += new RoutedEventHandler(WorkSpaceEditorView_Loaded);
            this.DragEnter += new DragEventHandler(WorkSpaceEditorView_DragEnter);
            this.Drop += new DragEventHandler(WorkSpaceEditorView_Drop);
            this.MouseMove += new MouseEventHandler(WorkSpaceEditorView_MouseMove);
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(WorkSpaceEditorView_PreviewMouseRightButtonDown);
            this.WorkspaceModel = WorkspaceModel;
            this.State = EditorState.READY;
            InitializeComponent();
        }

        void WorkSpaceEditorView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void AddPluginContainerView(Point position, PluginModel model)
        {
            PluginContainerView shape = new PluginContainerView(model);
            shape.MouseLeave += new MouseEventHandler(shape_MouseLeave);
            shape.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
            shape.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
            shape.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
            shape.SetPosition(position);
            this.root.Children.Add(shape);
            Canvas.SetZIndex(shape, 100);
        }

        public void AddConnection(IConnectable source, IConnectable target)
        {
            ConnectionModel connectionModel = this.WorkspaceModel.newConnectionModel(((ConnectorView)source).cModel, ((ConnectorView)target).cModel, ((ConnectorView)source).cModel.ConnectorType);
            CryptoLineView conn = new CryptoLineView(connectionModel);
            conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(source));
            conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(target));
            root.Children.Add(conn);
            Canvas.SetZIndex(conn, 0);            
        }

        private void AddConnectionSource(IConnectable source, CryptoLineView conn)
        {
            conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(source));
            conn.EndPoint = Mouse.GetPosition(this);
        }

        private MultiBinding CreateConnectorBinding(IConnectable connectable)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new ConnectorBindingConverter();

            Binding binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(ConnectorView.PositionOnWorkSpaceXProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(ConnectorView.PositionOnWorkSpaceYProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            return multiBinding;
        }

        #region Controls

        void shape_OnConnectorMouseLeftButtonDown(object sender, ConnectorViewEventArgs e)
        {
            if (selectedConnector != null)
            {
                this.root.Children.Remove(dummyLine);
                this.AddConnection(selectedConnector, e.connector);
                this.selectedConnector = null;
                this.State = EditorState.READY;
                return;
            }

            if (selectedConnector == null)
            {
                this.root.Children.Add(dummyLine);
                this.selectedConnector = e.connector;
                this.AddConnectionSource(e.connector, dummyLine);
                this.State = EditorState.BUSY;
                return;
            }
            
        }

        void WorkSpaceEditorView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.selectedConnector = null;
            this.root.Children.Remove(dummyLine);
        }

        void WorkSpaceEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectedPluginContainer != null)
            {
                p = selectedPluginContainer.GetPosition();
                this.selectedPluginContainer.SetPosition(new Point(p.X + (Mouse.GetPosition(sender as WorkSpaceEditorView).X - previousDragPoint.X), p.Y + (Mouse.GetPosition(sender as WorkSpaceEditorView).Y - previousDragPoint.Y)));
            }

            if (selectedConnector != null && root.Children.Contains(dummyLine))
            {
                this.dummyLine.EndPoint = Mouse.GetPosition(sender as WorkSpaceEditorView);
            }
            this.previousDragPoint = Mouse.GetPosition(sender as WorkSpaceEditorView);
        }

        void shape_MouseLeave(object sender, MouseEventArgs e)
        {
            this.selectedPluginContainer = null;
            this.State = EditorState.READY;
        }

        void shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(sender as PluginContainerView, 100);
            this.selectedPluginContainer = null;
            this.State = EditorState.BUSY;
        }

        void shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(sender as PluginContainerView, 101);
            this.selectedPluginContainer = sender as PluginContainerView;
            this.State = EditorState.BUSY;
        }

        void WorkSpaceEditorView_Drop(object sender, DragEventArgs e)
        {
            DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
            if(obj != null)
                this.AddPluginContainerView(e.GetPosition(this), WorkspaceModel.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
        }

        void WorkSpaceEditorView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        #endregion
    }
}
