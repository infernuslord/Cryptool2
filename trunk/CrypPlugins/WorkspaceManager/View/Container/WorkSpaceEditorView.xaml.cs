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
        private Point previousDragPoint = new Point();
        private ConnectorView selectedConnector;
        private PluginContainerView selectedPluginContainer;
        private CryptoLineView dummyLine = new CryptoLineView();
        private Point point;

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
        }

        private void setBaseControl(WorkspaceModel WorkspaceModel)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(WorkSpaceEditorView_OnMouseLeftButtonDown);
            this.MouseLeave += new MouseEventHandler(WorkSpaceEditorView_MouseLeave);
            this.Loaded += new RoutedEventHandler(WorkSpaceEditorView_Loaded);
            this.DragEnter += new DragEventHandler(WorkSpaceEditorView_DragEnter);
            this.Drop += new DragEventHandler(WorkSpaceEditorView_Drop);
            this.MouseMove += new MouseEventHandler(WorkSpaceEditorView_MouseMove);
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
                newPluginContainerView.ShowSettings += new EventHandler<PluginContainerViewSettingsViewEventArgs>(shape_ShowSettings);
                newPluginContainerView.ConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
                newPluginContainerView.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
                newPluginContainerView.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
                newPluginContainerView.SetPosition(new Point((Math.Round((position.X) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((position.Y) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));

                this.root.Children.Add(newPluginContainerView);
                Canvas.SetZIndex(newPluginContainerView, 100);
                Model.WorkspaceManagerEditor.HasChanges = true;
            }
        }

        void shape_ShowSettings(object sender, PluginContainerViewSettingsViewEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Visible;
        }

        public void AddConnection(ConnectorView source, ConnectorView target)
        {
            if (this.State == EditorState.READY)
            {
                ConnectionModel connectionModel = this.Model.newConnectionModel(((ConnectorView)source).Model, ((ConnectorView)target).Model, ((ConnectorView)source).Model.ConnectorType);
                CryptoLineView conn = new CryptoLineView(connectionModel);
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

        #region Controls

        void WorkSpaceEditorView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                PluginChangedEventArgs args = new PluginChangedEventArgs(this.model.WorkspaceManagerEditor, "WorkspaceManager", DisplayPluginMode.Normal);
                this.Model.WorkspaceManagerEditor.onSelectedPluginChanged(args);
            }
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

        void WorkSpaceEditorView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
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

        void WorkSpaceEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectedPluginContainer != null)
            {
                point = selectedPluginContainer.GetPosition();
                this.selectedPluginContainer.SetPosition(new Point((Math.Round((Mouse.GetPosition(root).X - previousDragPoint.X) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((Mouse.GetPosition(root).Y - previousDragPoint.Y) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));
                Model.WorkspaceManagerEditor.HasChanges = true;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SliderEditorSize.Value += 0.3;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SliderEditorSize.Value -= 0.3;
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
                    DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = Model.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName));
                    if (obj != null)
                        this.AddPluginContainerView(e.GetPosition(root), pluginModel);
                    Model.WorkspaceManagerEditor.HasChanges = true;
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

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Hidden;
        }

        internal void Load(WorkspaceModel WorkspaceModel)
        {
            this.Model = WorkspaceModel;

            foreach (PluginModel model in this.Model.AllPluginModels)
            {
                this.loadPluginContainerView(model);
            }
            foreach (ConnectionModel connModel in WorkspaceModel.AllConnectionModels)
            {
                CryptoLineView conn = new CryptoLineView(connModel);
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
                                conn.SetBinding(CryptoLineView.StartPointProperty, CreateConnectorBinding(connector));
                            else if (connModel.To == connector.Model)
                                conn.SetBinding(CryptoLineView.EndPointProperty, CreateConnectorBinding(connector));
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
            newPluginContainerView.ShowSettings += new EventHandler<PluginContainerViewSettingsViewEventArgs>(shape_ShowSettings);
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
    }
}
