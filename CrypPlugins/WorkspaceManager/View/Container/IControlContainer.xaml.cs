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
using WorkspaceManager.Model;
using WorkspaceManager.View.VisualComponents;
using WorkspaceManager.View.Converter;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for IControlContainer.xaml
    /// </summary>
    public partial class IControlContainer : UserControl
    {
        public PluginModel Model { get; set; }
        public EditorState State { get; set; }
        public List<CryptoLineView> IControlConnList { get; set; }

        private ConnectorView selectedConnector;
        private CryptoLineView dummyLine;
        private PluginContainerView selectedPluginContainer;
        private Point previousDragPoint;
        private Point dummyPoint;

        public IControlContainer()
        {
            InitializeComponent();
        }

        public IControlContainer(PluginModel Model)
        {
            InitializeComponent();
            this.dummyLine = new CryptoLineView();
            this.Model = Model;
            setBaseControl(Model.WorkspaceModel);
        }

        private void setBaseControl(WorkspaceModel WorkspaceModel)
        {
            //this.MouseLeftButtonDown += new MouseButtonEventHandler(WorkSpaceEditorView_OnMouseLeftButtonDown);
            this.MouseLeave += new MouseEventHandler(WorkSpaceEditorView_MouseLeave);
            this.Drop += new DragEventHandler(WorkSpaceEditorView_Drop);
            this.MouseMove += new MouseEventHandler(WorkSpaceEditorView_MouseMove);
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(WorkSpaceEditorView_PreviewMouseRightButtonDown);
            this.State = EditorState.READY;
        }

        public void AddConnection(ConnectorView source, ConnectorView target)
        {
            if (this.State == EditorState.READY)
            {
                ConnectionModel connectionModel = this.Model.WorkspaceModel.newConnectionModel(((ConnectorView)source).Model, ((ConnectorView)target).Model, ((ConnectorView)source).Model.ConnectorType);
                CryptoLineView conn = new CryptoLineView(connectionModel);
                connectionModel.OnDelete += DeleteConnection;
                conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(source));
                conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(target));
                root.Children.Add(conn);
                IControlConnList.Add(conn);
                Canvas.SetZIndex(conn, 0);
            }
        }

        public void DeleteConnection(Object sender, EventArgs args)
        {
            if (sender is ConnectionModel)
            {
                if (((ConnectionModel)sender).UpdateableView != null)
                {
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
                conn.EndPoint = Mouse.GetPosition(this);
            }
        }

        private MultiBinding CreateConnectorBinding(ConnectorView connectable)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new ConnectorBindingConverter();
            multiBinding.ConverterParameter = connectable;

            Binding binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(ConnectorView.X);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(ConnectorView.Y);
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

        void PluginDelete(object sender, PluginContainerViewDeleteViewEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                Model.WorkspaceModel.deletePluginModel(e.container.Model);
                root.Children.Remove(e.container);
                Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
            }
        }

        public void AddPluginContainerView(Point position, PluginModel model)
        {
            if (this.State == EditorState.READY)
            {
                PluginContainerView newPluginContainerView = new PluginContainerView(model);
                newPluginContainerView.Delete += new EventHandler<PluginContainerViewDeleteViewEventArgs>(PluginDelete);
                newPluginContainerView.ConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
                newPluginContainerView.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
                newPluginContainerView.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
                newPluginContainerView.SetPosition(new Point((Math.Round((position.X) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((position.Y) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));

                this.root.Children.Add(newPluginContainerView);
                Canvas.SetZIndex(newPluginContainerView, 100);
                Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
            }
        }

        void WorkSpaceEditorView_Drop(object sender, DragEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject"))
                {
                    DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = Model.WorkspaceModel.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName));
                    if (obj != null)
                        this.AddPluginContainerView(e.GetPosition(root), pluginModel);
                    Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
                }
                else
                    return;
            }
        }

        void shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.selectedPluginContainer = sender as PluginContainerView;
            this.previousDragPoint = Mouse.GetPosition(selectedPluginContainer);
            Canvas.SetZIndex(selectedPluginContainer, 101);

            PluginChangedEventArgs args = new PluginChangedEventArgs(this.selectedPluginContainer.Model.Plugin, this.selectedPluginContainer.Model.Name, DisplayPluginMode.Normal);
            this.Model.WorkspaceModel.WorkspaceManagerEditor.onSelectedPluginChanged(args);

            e.Handled = true;
        }

        void WorkSpaceEditorView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                this.selectedPluginContainer = null;
            }
        }

        void WorkSpaceEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectedPluginContainer != null)
            {
                dummyPoint = selectedPluginContainer.GetPosition();
                this.selectedPluginContainer.SetPosition(new Point((Math.Round((Mouse.GetPosition(root).X - previousDragPoint.X) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((Mouse.GetPosition(root).Y - previousDragPoint.Y) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));
                Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
                //this.selectedPluginContainer.SetPosition(new Point((Math.Round(( Mouse.GetPosition(root).X )/Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                //                                            (Math.Round(( Mouse.GetPosition(root).Y ) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));
            }



            if (selectedConnector != null && root.Children.Contains(dummyLine))
            {
                this.dummyLine.EndPoint = Mouse.GetPosition(root);
            }
        }

        void shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetZIndex(sender as PluginContainerView, 100);
            this.selectedPluginContainer = null;
        }


        void WorkSpaceEditorView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.State == EditorState.READY)
            {
                this.selectedConnector = null;
                this.root.Children.Remove(dummyLine);
            }
        }
    }
}
