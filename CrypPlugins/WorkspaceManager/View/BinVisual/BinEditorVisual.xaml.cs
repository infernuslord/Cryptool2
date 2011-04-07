using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
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
using Cryptool.PluginBase;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using Cryptool.Core;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinEditorVisual.xaml
    /// </summary>
    public partial class BinEditorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SampleLoaded;
        #endregion

        #region Fields
        private bool isLinkStarted;
        private BinConnectorVisual linkedConnector, from, to;
        private CryptoLineView draggedLink = new CryptoLineView();
        private Point? startDragPoint;
        #endregion

        #region Properties
        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get { return model; }
            set
            {
                model = value;
                model.DeletedChildElement += DeleteChild;
                model.NewChildElement += NewChild;
                model.ChildPositionChanged += ChildPositionChanged;
                model.ChildSizeChanged += ChildSizeChanged;
                model.ChildNameChanged += ChildNameChanged;
            }
        }

        public WorkspaceManager MyEditor { get; private set; }

        private ObservableCollection<UIElement> visualCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> VisualCollection { get { return visualCollection; } private set { visualCollection = value; } }

        private ObservableCollection<BinComponentVisual> componentCollection = new ObservableCollection<BinComponentVisual>();
        public ObservableCollection<BinComponentVisual> ComponentCollection { get { return componentCollection; } private set { componentCollection = value; } }

        private ObservableCollection<CryptoLineView> pathCollection = new ObservableCollection<CryptoLineView>();
        public ObservableCollection<CryptoLineView> PathCollection { get { return pathCollection; } private set { pathCollection = value; } }
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinEditorState), typeof(BinEditorVisual), new FrameworkPropertyMetadata(BinEditorState.READY, null));

        public BinEditorState State
        {
            get
            {
                return (BinEditorState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
            }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, null));

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

        public static readonly DependencyProperty IsFullscreenOpenProperty = DependencyProperty.Register("IsFullscreenOpen",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, null));


        public bool IsFullscreenOpen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenOpenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenOpenProperty, value);
            }
        }

        #endregion

        #region Constructors
        public BinEditorVisual(WorkspaceModel model)
        {
            Model = model;
            MyEditor = (WorkspaceManager)Model.MyEditor;
            VisualCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            InitializeComponent();
        }

        #endregion

        #region Public
        public void Load(WorkspaceModel model)
        {
            Model = model;
            Thread t = new Thread(new ParameterizedThreadStart(internalLoad));
            t.Start(model);
        }

        public void ResetConnections()
        {
            foreach (CryptoLineView line in PathCollection)
	        {             
	            line.Reset();
	        }
        }

        public void FitToScreen()
        {
            if (ScrollViewer.ScrollableWidth > 0 || ScrollViewer.ScrollableHeight > 0)
            {
                while (Properties.Settings.Default.EditScale 
                    > Properties.Settings.Default.MinScale 
                    && (ScrollViewer.ScrollableHeight > 0 
                    || ScrollViewer.ScrollableWidth > 0))
                {
                    Properties.Settings.Default.EditScale -= 0.02;
                    ScrollViewer.UpdateLayout();
                }
            }
            else
            {
                while (Properties.Settings.Default.EditScale
                    < Properties.Settings.Default.MaxScale 
                    && ScrollViewer.ScrollableHeight == 0 
                    && ScrollViewer.ScrollableWidth == 0)
                {
                    Properties.Settings.Default.EditScale += 0.02;
                    ScrollViewer.UpdateLayout();
                }
                if (ScrollViewer.ScrollableHeight > 0 
                    || ScrollViewer.ScrollableWidth > 0)
                    Properties.Settings.Default.EditScale -= 0.02;
            }
        }

        public void ResetPlugins()
        {
	        foreach (BinComponentVisual b in ComponentCollection)
	        {
                b.Progress = 0;
                //TODO: Error State Reset
	        }
        }
        #endregion

        #region Private

        private void internalLoad(object model)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                IsLoading = true;
            }
            , null);

            WorkspaceModel m = (WorkspaceModel)model;
            foreach (PluginModel pluginModel in m.GetAllPluginModels())
            {
                bool skip = false;
                foreach (ConnectorModel connModel in pluginModel.GetInputConnectors())
                {
                    if (connModel.IControl && connModel.GetInputConnections().Count > 0)
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                    Dispatcher.Invoke(DispatcherPriority.ContextIdle, (SendOrPostCallback)delegate
                    {
                        addBinComponentVisual(pluginModel);
                    }
                    , null);
            }

            foreach (ConnectionModel connModel in m.GetAllConnectionModels())
            {
                if (connModel.To.IControl)
                    continue;

                foreach (UIElement element in VisualCollection)
                {
                    BinComponentVisual bin = element as BinComponentVisual;
                    if (bin != null)
                    {
                        foreach (BinConnectorVisual connector in bin.ConnectorCollection)
                        {
                            Dispatcher.Invoke(DispatcherPriority.ContextIdle, (SendOrPostCallback)delegate
                            {
                                if (connModel.From == connector.Model)
                                    from = connector;
                                else if (connModel.To == connector.Model)
                                    to = connector;
                            }
                            , null);
                        }
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.ContextIdle, (SendOrPostCallback)delegate
                {
                    addConnection(from, to, connModel);
                }
                , null);
            }

            //this.UserContentWrapper = new UserContentWrapper(Model, bottomBox);
            //this.UserControlWrapperParent.Children.Clear();
            //this.UserControlWrapperParent.Children.Add(UserContentWrapper);
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsLoading = false;
            }
            , null);

            if (SampleLoaded != null)
                SampleLoaded.BeginInvoke(this,null,null, null);
        }

        private void addConnection(BinConnectorVisual source, BinConnectorVisual target, ConnectionModel model)
        {
            if (this.State != BinEditorState.READY || source == null || target == null)
                return;

            CryptoLineView link = new CryptoLineView(model, source, target);
            link.SetBinding(CryptoLineView.StartPointProperty, Util.CreateConnectorBinding(source, link));
            link.SetBinding(CryptoLineView.EndPointProperty, Util.CreateConnectorBinding(target, link));
            link.SetBinding(CryptoLineView.IsDraggedProperty, Util.CreateIsDraggingBinding(
                new BinComponentVisual[] { target.WindowParent, source.WindowParent }));
            VisualCollection.Add(link);
        }

        private void addBinComponentVisual(PluginModel pluginModel)
        {
            if (this.State != BinEditorState.READY)
                return;

            BinComponentVisual bin = new BinComponentVisual(pluginModel);
            VisualCollection.Add(bin);
        }

        private void resetLinking()
        {
            VisualCollection.Remove(draggedLink);
            linkedConnector = null;
            isLinkStarted = false;
            Mouse.OverrideCursor = null;
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

        #region Model Handler

        /// <summary>
        /// A child is deleted on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DeleteChild(Object sender, ModelArgs args)
        {
            if (args.EffectedModelElement is ConnectionModel)
            {
                if (((ConnectionModel)args.EffectedModelElement).UpdateableView != null)
                {
                    UIElement uielement = (UIElement)((ConnectionModel)args.EffectedModelElement).UpdateableView;
                    if (VisualCollection.Contains(uielement))
                    {
                        VisualCollection.Remove(uielement);
                    }
                }
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    UIElement uielement = (UIElement)((PluginModel)args.EffectedModelElement).UpdateableView;
                    if (VisualCollection.Contains(uielement))
                    {
                        VisualCollection.Remove(uielement);
                    }
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    UIElement uielement = (UIElement)((ImageModel)args.EffectedModelElement).UpdateableView;
                    if (VisualCollection.Contains(uielement))
                    {
                        VisualCollection.Remove(uielement);
                    }
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    UIElement uielement = (UIElement)((TextModel)args.EffectedModelElement).UpdateableView;
                    if (VisualCollection.Contains(uielement))
                    {
                        VisualCollection.Remove(uielement);
                    }
                }
            }
        }

        /// <summary>
        /// A child is created on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void NewChild(Object sender, ModelArgs args)
        {
            if (args.EffectedModelElement is ConnectionModel)
            {
                if (((ConnectionModel)args.EffectedModelElement).UpdateableView != null)
                {
                    CryptoLineView conn = (CryptoLineView)((ConnectionModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(conn))
                    {
                        VisualCollection.Add(conn);
                    }
                }
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinComponentVisual plugin = (BinComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(plugin))
                        VisualCollection.Add(plugin);
                }
            }
            //else if (args.EffectedModelElement is ImageModel)
            //{
            //    if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        ImageWrapper imgWrapper = (ImageWrapper)((ImageModel)args.EffectedModelElement).UpdateableView;
            //        //if (!VisualCollection.Contains(plugin))
            //            //VisualCollection.Add(plugin);
            //    }
            //}
            //else if (args.EffectedModelElement is TextModel)
            //{
            //    if (((TextModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        TextInputWrapper txtInputWrapper = (TextInputWrapper)((TextModel)args.EffectedModelElement).UpdateableView;
            //        //if (!VisualCollection.Contains(plugin))
            //            //VisualCollection.Add(plugin);
            //    }
            //}
        }

        /// <summary>
        /// The position of a child has changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ChildPositionChanged(Object sender, PositionArgs args)
        {
            if (args.OldPosition.Equals(args.NewPosition))
            {
                return;
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinComponentVisual pluginContainerView = (BinComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    pluginContainerView.Position = args.NewPosition;
                }
            }
            //else if (args.EffectedModelElement is ImageModel)
            //{
            //    if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        ImageWrapper imgWrapper = (ImageWrapper)((ImageModel)args.EffectedModelElement).UpdateableView;
            //        imgWrapper.Position = args.NewPosition;
            //        imgWrapper.RenderTransform = new TranslateTransform(imgWrapper.Position.X, imgWrapper.Position.Y);
            //    }
            //}
            //else if (args.EffectedModelElement is TextModel)
            //{
            //    if (((TextModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        TextInputWrapper txtWrapper = (TextInputWrapper)((TextModel)args.EffectedModelElement).UpdateableView;
            //        txtWrapper.Position = args.NewPosition;
            //        txtWrapper.RenderTransform = new TranslateTransform(txtWrapper.Position.X, txtWrapper.Position.Y);
            //    }
            //}
        }

        /// <summary>
        /// The size of a child changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ChildSizeChanged(Object sender, SizeArgs args)
        {
            if (args.NewHeight.Equals(args.OldHeight) &&
               args.NewWidth.Equals(args.OldWidth))
            {
                return;
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinComponentVisual pluginContainerView = (BinComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    pluginContainerView.WindowWidth = args.NewWidth;
                    pluginContainerView.WindowHeight = args.NewHeight;
                }
            }
            //else if (args.EffectedModelElement is ImageModel)
            //{
            //    if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        ImageWrapper imgWrapper = (ImageWrapper)((ImageModel)args.EffectedModelElement).UpdateableView;
            //        imgWrapper.Width = args.NewWidth;
            //        imgWrapper.Height = args.NewHeight;
            //    }
            //}
            //else if (args.EffectedModelElement is TextModel)
            //{
            //    if (((TextModel)args.EffectedModelElement).UpdateableView != null)
            //    {
            //        TextInputWrapper txtWrapper = (TextInputWrapper)((TextModel)args.EffectedModelElement).UpdateableView;
            //        txtWrapper.Width = args.NewWidth;
            //        txtWrapper.Height = args.NewHeight;
            //    }
            //}
        }

        /// <summary>
        /// The size of a child changed on model side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChildNameChanged(Object sender, NameArgs args)
        {
            if (args.NewName == null || args.NewName.Equals(args.Oldname))
            {
                return;
            }

            if (args.EffectedModelElement is PluginModel)
            {
                BinComponentVisual bin = (BinComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                bin.CustomName = args.NewName;
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

        private void FitToScreenHandler(object sender, FitToScreenEventArgs e)
        {
            FitToScreen();
        }

        private void OverviewHandler(object sender, EventArgs e)
        {
            IsFullscreenOpen = !IsFullscreenOpen;
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        return;
                    if (e.NewItems[0] is BinComponentVisual)
                        ComponentCollection.Add(e.NewItems[0] as BinComponentVisual);

                    if (e.NewItems[0] is CryptoLineView)
                        PathCollection.Add(e.NewItems[0] as CryptoLineView);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        return;
                    if (e.OldItems[0] is BinComponentVisual)
                        ComponentCollection.Remove(e.OldItems[0] as BinComponentVisual);

                    if (e.OldItems[0] is CryptoLineView)
                        PathCollection.Remove(e.OldItems[0] as CryptoLineView);
                    break;
            }
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            resetLinking();
            startDragPoint = null;
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Properties.Settings.Default.EditScale + 0.05 < Properties.Settings.Default.MaxScale &&
                    e.Delta >= 0)
                    Properties.Settings.Default.EditScale += 0.05;

                if (Properties.Settings.Default.EditScale - 0.05 > Properties.Settings.Default.MinScale &&
                    e.Delta <= 0)
                    Properties.Settings.Default.EditScale += -0.05;

                e.Handled = true;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (isLinkStarted)
            {
                draggedLink.EndPoint = e.GetPosition(sender as FrameworkElement);
                e.Handled = true;
            }

            if (startDragPoint != null)
            {
	            Point currentPoint = e.GetPosition(sender as FrameworkElement);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + delta.X);
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + delta.Y);
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual)
                {
                    DataObject data = new DataObject("BinConnector", element);
                    DragDrop.AddQueryContinueDragHandler(this, QueryContinueDragHandler);
                    c.IsConnectorDragStarted = true;
                    DragDrop.DoDragDrop(c, data, DragDropEffects.Move);
                    c.IsConnectorDragStarted = false;
                    e.Handled = true;
                }
            }

            if (e.Source is CryptoLineView)
            {
                CryptoLineView l = (CryptoLineView)e.Source;
                Model.ModifyModel(new DeleteConnectionModelOperation(l.Model));
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is BinComponentVisual))
            {
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.Hand;
                e.Handled = true;
            }

            switch(e.ClickCount)
            {
                case 1:

                    if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
                    {
                        BinComponentVisual c = (BinComponentVisual)e.Source;
                        FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                        if (element is BinConnectorVisual && !isLinkStarted)
                        {
                            BinConnectorVisual b = element as BinConnectorVisual;
                            linkedConnector = b;
                            draggedLink.SetBinding(CryptoLineView.StartPointProperty, Util.CreateConnectorBinding(b, draggedLink));
                            draggedLink.EndPoint = e.GetPosition(sender as FrameworkElement);
                            VisualCollection.Add(draggedLink);
                            Mouse.OverrideCursor = Cursors.Cross;
                            e.Handled = isLinkStarted = true;
                        }
                        PluginChangedEventArgs componentArgs = new PluginChangedEventArgs(c.Model.Plugin, c.FunctionName, DisplayPluginMode.Normal);
                        MyEditor.onSelectedPluginChanged(componentArgs);
                        return;
                    }
                    PluginChangedEventArgs editorArgs = new PluginChangedEventArgs(Model.MyEditor, Model.MyEditor.GetPluginInfoAttribute().Caption, DisplayPluginMode.Normal);
                    MyEditor.onSelectedPluginChanged(editorArgs);
                    break;

                case 2:
                    if (e.Source is BinComponentVisual)
                    {
                        BinComponentVisual c = (BinComponentVisual)e.Source;
                        IsFullscreenOpen = true;
                        ((BinFullscreenVisual)FullScreen.Content).ActiveComponent = c;
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)f.TemplatedParent;
                if (element is BinConnectorVisual)
                {
                    BinConnectorVisual b = (BinConnectorVisual)element;
                    if (isLinkStarted && linkedConnector != null)
                    {
                        if (WorkspaceModel.compatibleConnectors(linkedConnector.Model, b.Model))
                        {
                            ConnectionModel connectionModel = (ConnectionModel)Model.ModifyModel(new NewConnectionModelOperation(
                                linkedConnector.Model,
                                b.Model,
                                linkedConnector.Model.ConnectorType));
                            addConnection(linkedConnector, b, connectionModel);
                            e.Handled = true;
                        }
                    }
                }
            }
            startDragPoint = null;
            resetLinking();
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
            if (this.State != BinEditorState.READY)
                return;

            if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject") && !(e.Source is BinComponentVisual))
            {
                try
                {
                    DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = (PluginModel)Model.ModifyModel(new NewPluginModelOperation(Util.MouseUtilities.CorrectGetPosition(sender as FrameworkElement), 0, 0, DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
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

    #region HelperClass
    class DragDropDataObjectToPluginConverter
    {
        public static PluginManager PluginManager { get; set; }
        private static Type type;
        public static Type CreatePluginInstance(string assemblyQualifiedName, string typeVar)
        {
            if (PluginManager != null && assemblyQualifiedName != null && typeVar != null)
            {
                AssemblyName assName = new AssemblyName(assemblyQualifiedName);
                type = PluginManager.LoadType(assName.Name, typeVar);

                if (type != null)
                    return type;
            }
            return null;
        }
    } 
    #endregion
}
