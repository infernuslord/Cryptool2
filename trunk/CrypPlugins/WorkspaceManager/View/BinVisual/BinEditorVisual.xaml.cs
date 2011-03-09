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
using System.Runtime.InteropServices;

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

        #region Fields
        private bool isLinkStarted;
        private CryptoLineView draggedLink;
        #endregion

        #region Properties
        public WorkspaceModel Model { get; private set; }
        public WorkspaceManager MyEditor { get; private set; }

        private ObservableCollection<UIElement> visualCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> VisualCollection { get { return visualCollection; } private set { visualCollection = value; } }

        private ObservableCollection<CryptoLineView> pathCollection = new ObservableCollection<CryptoLineView>();
        public ObservableCollection<CryptoLineView> PathCollection { get { return pathCollection; } private set { pathCollection = value; } }
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

        private void addConnection(BinConnectorVisual source, BinConnectorVisual target, out CryptoLineView line)
        {
            line = null;
            if (this.State != EditorState.READY || source == null)
                return;

            CryptoLineView link = new CryptoLineView(source, target);
            VisualCollection.Add(link);
            line = link;
        }

        private void addBinComponentVisual(PluginModel pluginModel)
        {
            if (this.State != EditorState.READY)
                return;

            VisualCollection.Add(new BinComponentVisual(pluginModel));
        }

        public static MultiBinding CreateConnectorBinding(BinConnectorVisual connectable)
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
            binding.Source = connectable.WindowParent.West;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.East;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.North;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.South;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            return multiBinding;
        }
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

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (isLinkStarted || draggedLink != null)
            {
                draggedLink.EndPoint = e.GetPosition(sender as FrameworkElement);
                e.Handled = true;
            }
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual && isLinkStarted)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual)
                {
                    BinConnectorVisual b = (BinConnectorVisual) element;
                    if (WorkspaceModel.compatibleConnectors(draggedLink.StartPointSource.Model, b.Model))
                    {
                        draggedLink.EndPointSource = element as BinConnectorVisual;
                        ConnectionModel connectionModel = (ConnectionModel)Model.ModifyModel(new NewConnectionModelOperation(
                            draggedLink.StartPointSource.Model,
                            draggedLink.EndPointSource.Model,
                            draggedLink.StartPointSource.Model.ConnectorType));
                        draggedLink.Model = connectionModel;
                        e.Handled = true;
                    }
                    else
                    {
                        VisualCollection.Remove(draggedLink);
                        draggedLink = null;
                    }
                }
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual)
                {
                    DataObject data = new DataObject("BinConnector", element);
                    DragDrop.AddQueryContinueDragHandler(this, QueryContinueDragHandler);
                    c.IsConnectorDragStarted = true;
                    DragDrop.DoDragDrop(c, data, DragDropEffects.Move);
                    e.Handled = true;
                }
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual && !isLinkStarted && draggedLink == null)
                {
                    Point position = e.GetPosition(this);
                    CryptoLineView line;
                    addConnection((BinConnectorVisual) element,null, out line);
                    draggedLink = line;
                    e.Handled = isLinkStarted = true;
                }
            }
        }

        #region DragDropHandler

        private void QueryContinueDragHandler(Object source, QueryContinueDragEventArgs e)
        {
            e.Handled = true;

            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                return;
            }

            e.Action = DragAction.Drop;
            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.None)
            {
                e.Action = DragAction.Continue;
            }
            else if ((e.KeyStates & DragDropKeyStates.RightMouseButton) != DragDropKeyStates.None)
            {
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
                    PluginModel pluginModel = (PluginModel)Model.ModifyModel(new NewPluginModelOperation(MouseUtilities.CorrectGetPosition(sender as FrameworkElement), 0, 0, DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
                    addBinComponentVisual(pluginModel);
                    MyEditor.HasChanges = e.Handled = true;
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

    public static class MouseUtilities
    {
        public static Point CorrectGetPosition(Visual relativeTo)
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
    }
}
