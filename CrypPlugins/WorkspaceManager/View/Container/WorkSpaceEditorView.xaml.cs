﻿using System;
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
        private Point point;

        public EditorState State;
        public EditorState ConnectorState;
        public List<CryptoLineView> ConnectionList = new List<CryptoLineView>();
        public List<PluginContainerView> PluginContainerList = new List<PluginContainerView>();
        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get { return model; }
            private set { model = value; }
        }

        public WorkSpaceEditorView()
        {
            InitializeComponent();
        }

        public WorkSpaceEditorView(WorkspaceModel WorkspaceModel)
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
            InitializeComponent();
        }

        void PluginDelete(object sender, PluginContainerViewDeleteViewEventArgs e)
        {            
            Model.deletePluginModel(e.container.Model);
            root.Children.Remove(e.container);
        }

        void WorkSpaceEditorView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void AddPluginContainerView(Point position, PluginModel model)
        {
            PluginContainerView newPluginContainerView = new PluginContainerView(model);
            newPluginContainerView.Delete += new EventHandler<PluginContainerViewDeleteViewEventArgs>(PluginDelete);
            newPluginContainerView.ShowSettings += new EventHandler<PluginContainerViewSettingsViewEventArgs>(shape_ShowSettings);
            newPluginContainerView.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(shape_OnConnectorMouseLeftButtonDown);
            newPluginContainerView.MouseLeftButtonDown += new MouseButtonEventHandler(shape_MouseLeftButtonDown);
            newPluginContainerView.MouseLeftButtonUp += new MouseButtonEventHandler(shape_MouseLeftButtonUp);
            newPluginContainerView.SetPosition(position);
            this.root.Children.Add(newPluginContainerView);
            Canvas.SetZIndex(newPluginContainerView, 100);
        }

        void shape_ShowSettings(object sender, PluginContainerViewSettingsViewEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Visible;
        }

        public void AddConnection(IConnectable source, IConnectable target)
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

        private void AddConnectionSource(IConnectable source, CryptoLineView conn)
        {
            Color color = ColorHelper.GetColor((source as ConnectorView).Model.ConnectorType);
            conn.Stroke = new SolidColorBrush(color);
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
            if (selectedConnector != null && WorkspaceModel.compatibleConnectors(selectedConnector.Model, e.connector.Model))
            {
                this.root.Children.Remove(dummyLine);
                this.AddConnection(selectedConnector, e.connector);
                this.selectedConnector = null;
                this.State = EditorState.READY;
                return;
            }

            if (selectedConnector == null && e.connector.Model.Outgoing)
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

        void WorkSpaceEditorView_MouseLeave(object sender, MouseEventArgs e)
        {
            this.selectedPluginContainer = null;
            this.State = EditorState.READY;
        }      

        void WorkSpaceEditorView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectedPluginContainer != null)
            {
                point = selectedPluginContainer.GetPosition();
                this.selectedPluginContainer.SetPosition(new Point(point.X + Mouse.GetPosition(root).X - previousDragPoint.X, point.Y + Mouse.GetPosition(root).Y - previousDragPoint.Y));
            }

            if (selectedConnector != null && root.Children.Contains(dummyLine))
            {
                this.dummyLine.EndPoint = Mouse.GetPosition(root);
            }
            this.previousDragPoint = Mouse.GetPosition(root);
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
            PluginChangedEventArgs args = new PluginChangedEventArgs(this.selectedPluginContainer.Model.Plugin, this.selectedPluginContainer.Model.Name, DisplayPluginMode.Normal);
            this.Model.WorkspaceManagerEditor.onSelectedPluginChanged(args);
            e.Handled = true;
        }

        void WorkSpaceEditorView_Drop(object sender, DragEventArgs e)
        {
            DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
            PluginModel pluginModel = Model.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName));
            if(obj != null)
                this.AddPluginContainerView(e.GetPosition(root), pluginModel);
        }

        void WorkSpaceEditorView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        #endregion

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.InformationPanel.Visibility = Visibility.Hidden;
        }
    }
}