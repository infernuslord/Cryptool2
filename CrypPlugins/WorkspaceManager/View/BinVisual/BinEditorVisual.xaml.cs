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
using WorkspaceManager.View.BinVisual.IControlVisual;
using System.Windows.Data;
using WorkspaceManager.Base.Sort;
using System.Windows.Controls.Primitives;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinEditorVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class BinEditorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SampleLoaded;
        #endregion

        #region Fields
        private Window window;
        private ArevaloRectanglePacker packer;
        private BinConnectorVisual from, to;
        private RectangleGeometry selectRectGeometry = new RectangleGeometry();
        private bool startedSelection;
        private CryptoLineView draggedLink = new CryptoLineView();
        private Path selectionPath = new Path() 
        { 
            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3399ff")),
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
            StrokeThickness = 1, 
            Opacity = 0.5
        };
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

        public WorkspaceManagerClass MyEditor { get; private set; }

        public BinFullscreenVisual FullscreenVisual { get { return (BinFullscreenVisual)FullScreen.Content; } }

        private ObservableCollection<UIElement> selectedItemsObservable = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> SelectedItemsObservable { get { return selectedItemsObservable; } private set { selectedItemsObservable = value; } }

        private ObservableCollection<UIElement> visualCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> VisualCollection { get { return visualCollection; } private set { visualCollection = value; } }

        private ObservableCollection<BinComponentVisual> componentCollection = new ObservableCollection<BinComponentVisual>();
        public ObservableCollection<BinComponentVisual> ComponentCollection { get { return componentCollection; } private set { componentCollection = value; } }

        private ObservableCollection<CryptoLineView> pathCollection = new ObservableCollection<CryptoLineView>();
        public ObservableCollection<CryptoLineView> PathCollection { get { return pathCollection; } private set { pathCollection = value; } }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsLinkingProperty = DependencyProperty.Register("IsLinking",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, null));

        public bool IsLinking
        {
            get
            {
                return (bool)base.GetValue(IsLinkingProperty);
            }
            set
            {
                base.SetValue(IsLinkingProperty, value);
            }
        }

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

        public static readonly DependencyProperty SelectedConnectorProperty = DependencyProperty.Register("SelectedConnector",
            typeof(BinConnectorVisual), typeof(BinEditorVisual), new FrameworkPropertyMetadata(null, null));

        public BinConnectorVisual SelectedConnector
        {
            get
            {
                return (BinConnectorVisual)base.GetValue(SelectedConnectorProperty);
            }
            private set
            {
                base.SetValue(SelectedConnectorProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText",
            typeof(BinTextVisual), typeof(BinEditorVisual), new FrameworkPropertyMetadata(null, OnSelectedTextChanged));

        public BinTextVisual SelectedText
        {
            get
            {
                return (BinTextVisual)base.GetValue(SelectedTextProperty);
            }
            set
            {
                base.SetValue(SelectedTextProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage",
            typeof(BinImageVisual), typeof(BinEditorVisual), new FrameworkPropertyMetadata(null, OnSelectedImageChanged));

        public BinImageVisual SelectedImage
        {
            get
            {
                return (BinImageVisual)base.GetValue(SelectedImageProperty);
            }
            set
            {
                base.SetValue(SelectedImageProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems",
            typeof(UIElement[]), typeof(BinEditorVisual), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        public UIElement[] SelectedItems
        {
            get
            {
                return (UIElement[])base.GetValue(SelectedItemsProperty);
            }
            set
            {
                base.SetValue(SelectedItemsProperty, value);
            }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, OnIsLoadingChanged));

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

        public static readonly DependencyProperty IsExecutingProperty = DependencyProperty.Register("IsExecuting",
            typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, null));


        public bool IsExecuting
        {
            get
            {
                return (bool)base.GetValue(IsExecutingProperty);
            }
            set
            {
                base.SetValue(IsExecutingProperty, value);
            }
        }

        public static readonly DependencyProperty HasLoadingErrorProperty = DependencyProperty.Register("HasLoadingError",
    typeof(bool), typeof(BinEditorVisual), new FrameworkPropertyMetadata(false, null));

        public bool HasLoadingError
        {
            get
            {
                return (bool)base.GetValue(HasLoadingErrorProperty);
            }
            set
            {
                base.SetValue(HasLoadingErrorProperty, value);
            }
        }

        public static readonly DependencyProperty LoadingErrorTextProperty = DependencyProperty.Register("LoadingErrorText",
    typeof(string), typeof(BinEditorVisual), new FrameworkPropertyMetadata(string.Empty, null));

        public string LoadingErrorText
        {
            get
            {
                return (string)base.GetValue(LoadingErrorTextProperty);
            }
            set
            {
                base.SetValue(LoadingErrorTextProperty, value);
            }
        }

        #endregion

        #region Constructors
        public BinEditorVisual(WorkspaceModel model)
        {
            Model = model;
            MyEditor = (WorkspaceManagerClass)Model.MyEditor;
            MyEditor.executeEvent += new EventHandler(ExecuteEvent);
            VisualCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            VisualCollection.Add(selectionPath);
            MyEditor.LoadingErrorOccurred += new EventHandler<LoadingErrorEventArgs>(LoadingErrorOccurred);
            InitializeComponent();
        }

        #endregion

        #region Public

        public void AddBinComponentVisual(PluginModel pluginModel, int mode)
        {
            if (this.State != BinEditorState.READY)
                return;

            BinComponentVisual bin = new BinComponentVisual(pluginModel);
            Binding bind = new Binding();
            bind.Path = new PropertyPath(BinEditorVisual.SelectedItemsProperty);
            bind.Source = this;
            bind.ConverterParameter = bin;
            bind.Converter = new SelectionChangedConverter();
            bin.SetBinding(BinComponentVisual.IsSelectedProperty, bind);
            bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);
            VisualCollection.Add(bin);

            if (mode == 0)
                return;

            if (mode == 1)
            {
                GeneralTransform g = new ScaleTransform(Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale, Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale, 0, 0);
                Point p = g.Transform(new Point(randomNumber(0, (int)ActualWidth), randomNumber(0, (int)ActualHeight)));
                bin.Position = p;
                return;
            }
        }

        public void Load(WorkspaceModel model)
        {
            Model = model;
            internalLoad(Model);
        }

        public void ResetConnections()
        {
            foreach (CryptoLineView line in PathCollection)
	        {             
	            line.Reset();
	        }
        }

        /// <summary>
        /// TODO: Optimise this algorithm.
        /// </summary>
        public void FitToScreen()
        {
            if (ComponentCollection.Count == 0)
                return;

            if (ScrollViewer.ScrollableWidth > 0 || ScrollViewer.ScrollableHeight > 0)
            {
                while (Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale
                    > Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale
                    && (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0))
                {
                    Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale -= 0.02;
                    ScrollViewer.UpdateLayout();
                }
            }
            else
            {
                while (Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale
                    < Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale
                    && ScrollViewer.ScrollableHeight == 0
                    && ScrollViewer.ScrollableWidth == 0)
                {
                    Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale += 0.02;
                    ScrollViewer.UpdateLayout();
                }
                if (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0)
                    Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale -= 0.02;
            }
        }

        public void ResetPlugins(int value)
        {
            if (value == 0)
            {
                foreach (BinComponentVisual b in ComponentCollection)
                    b.Progress = 0;
            }

            if (value == 1)
            {
                foreach (BinComponentVisual b in ComponentCollection)
                    b.LogMessages.Clear();
            }
        }

        public void AddText()
        {
            VisualCollection.Add(new BinTextVisual((TextModel)Model.ModifyModel(new NewTextModelOperation())));
        }

        public void AddImage(Uri uri)
        {
            VisualCollection.Add(new BinImageVisual((ImageModel)Model.ModifyModel(new NewImageModelOperation(uri))));
        }
        #endregion

        #region Private

        private void internalLoad(object model)
        {
            IsLoading = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
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
                        AddBinComponentVisual(pluginModel,0);
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
                                if (connModel.From == connector.Model)
                                    from = connector;
                                else if (connModel.To == connector.Model)
                                    to = connector;
                            }
                        }
                    }

                    addConnection(from, to, connModel);
                }
            
                foreach(var img in m.GetAllImageModels())
                {
                    this.VisualCollection.Add(new BinImageVisual(img));
                }

                foreach(var txt in m.GetAllTextModels())
                {
                    this.VisualCollection.Add(new BinTextVisual(txt));
                }

                if (SampleLoaded != null)
                    SampleLoaded.Invoke(this, null);

                IsLoading = false;
            }
            , null);
        }

        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
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

        private void reset()
        {
            VisualCollection.Remove(draggedLink);
            SelectedConnector = null;
            IsLinking = false;
            //selectionPath.Data = null;
            //startDragPoint = null;
            Mouse.OverrideCursor = null;
        }

        private void dragReset()
        {
            selectionPath.Data = null;
            startDragPoint = null;

            if (!startedSelection)
                SelectedItems = null;

            startedSelection = false;
        }

        private static Random random = new Random();
        private double randomNumber(int min, int max)
        {
            return (double)random.Next(min, max);
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
            if (State == BinEditorState.READY)
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
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {

                    BinImageVisual img = (BinImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(img))
                        VisualCollection.Add(img);
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinTextVisual txt = (BinTextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(txt))
                        VisualCollection.Add(txt);
                }
            }
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
                    BinComponentVisual bin = (BinComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    bin.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinImageVisual img = (BinImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    img.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinTextVisual txt = (BinTextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    txt.Position = args.NewPosition;
                }
            }
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
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinImageVisual imgWrapper = (BinImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    imgWrapper.WindowWidth = args.NewWidth;
                    imgWrapper.WindowHeight = args.NewHeight;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    BinTextVisual txtWrapper = (BinTextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
                    txtWrapper.WindowWidth = args.NewWidth;
                    txtWrapper.WindowHeight = args.NewHeight;
                }
            }
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

        private void ComponentPositionDeltaChanged(object sender, PositionDeltaChangedArgs e)
        {
            var b = (BinComponentVisual)sender;
            if (SelectedItems != null)
            {
                var list = new List<Operation>();
                foreach (var element in SelectedItems)
                {
                    var bin = (BinComponentVisual)element;
                    list.Add(new MoveModelElementOperation(bin.Model, bin.Position + e.PosDelta));
                }
                b.Model.WorkspaceModel.ModifyModel(new MultiOperation(list));
            }
        }

        private void ExecuteEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsExecuting = MyEditor.isExecuting();
            }, null);
        }

        private void CopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, LoadingErrorText);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinEditorVisual b = (BinEditorVisual)d;
            UIElement[] newItem = e.NewValue as UIElement[];
            UIElement[] oldItem = e.OldValue as UIElement[];
            if (newItem != null)
            {
                b.SelectedItemsObservable.Concat(newItem);
            }
            else
            {
                b.SelectedItemsObservable.Clear();
            }
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinEditorVisual b = (BinEditorVisual)d;
            bool newItem = (bool)e.NewValue;
            bool oldItem = (bool)e.OldValue;
            if(newItem)
                Mouse.OverrideCursor = Cursors.Wait;
            else
                Mouse.OverrideCursor = Cursors.Arrow;
        }

        private static void OnSelectedImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinEditorVisual b = (BinEditorVisual)d;
            BinImageVisual newItem = e.NewValue as BinImageVisual;
            BinImageVisual oldItem = e.OldValue as BinImageVisual;

            if (newItem != null)
                newItem.IsSelected = true;
            if (oldItem != null)
                oldItem.IsSelected = false;
        }

        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinEditorVisual b = (BinEditorVisual)d;
            BinTextVisual newItem = e.NewValue as BinTextVisual;
            BinTextVisual oldItem = e.OldValue as BinTextVisual;

            if(newItem != null)
                newItem.IsSelected = true;
            if (oldItem != null)
                oldItem.IsSelected = false;
        }

        public void update()
        {

        }

        private void AddTextHandler(object sender, AddTextEventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                AddText();
            }
        }

        private void AddImageHandler(object sender, ImageSelectedEventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                AddImage(e.uri);
            }
        }

        private void FitToScreenHandler(object sender, FitToScreenEventArgs e)
        {
            FitToScreen();
        }

        private void OverviewHandler(object sender, EventArgs e)
        {
            IsFullscreenOpen = !IsFullscreenOpen;
        }

        private void SortHandler(object sender, EventArgs e)
        {
            if (State == BinEditorState.READY)
            {
                packer = new ArevaloRectanglePacker( Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortWidth,  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortHeight);
                foreach (var element in ComponentCollection)
                {
                    Point point;
                    if (packer.TryPack(element.ActualWidth +  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding, element.ActualHeight +  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding, out point))
                    {
                        point.X +=  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding;
                        point.Y +=  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding;
                        element.Position = point;
                    }
                }
            }
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

                    if (e.OldItems[0] is BinTextVisual)
                        this.SelectedText = null;

                    if (e.OldItems[0] is CryptoLineView)
                        PathCollection.Remove(e.OldItems[0] as CryptoLineView);

                    break;
            }
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            reset();
        }

        private void MouseUpButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            reset();

            //if (e.Source is BinComponentVisual)
            //{
            //    BinComponentVisual c = (BinComponentVisual)e.Source;
            //    return;
            //}
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if ( Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale + 0.05 <  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale &&
                    e.Delta >= 0)
                     Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale += 0.05;

                if ( Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale - 0.05 >  Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale &&
                    e.Delta <= 0)
                     Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale += -0.05;

                e.Handled = true;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (IsLinking)
            {
                draggedLink.EndPoint = e.GetPosition(sender as FrameworkElement);
                e.Handled = true;
                return;
            }

            if (startDragPoint != null && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(sender as FrameworkElement);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + delta.X);
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + delta.Y);
                return;
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is BinComponentVisual) && !(e.Source is BinImageVisual) && !(e.Source is BinTextVisual))
            {
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.ScrollAll;
                e.Handled = true;
            }

            if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)Util.TryFindParent<BinConnectorVisual>(f);
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
            if (!(e.Source is BinComponentVisual) && !(e.Source is BinImageVisual) && !(e.Source is BinTextVisual))
            {
                window = Window.GetWindow(this);
                setDragWindowHandle();
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.Arrow;
                e.Handled = true;
            }

            switch(e.ClickCount)
            {
                case 1:
                    var result = Util.TryFindParent<BinIControlVisual>(e.OriginalSource as UIElement);
                    if (result != null)
                        return;

                    if (e.Source is BinImageVisual || e.Source is BinTextVisual)
                    {
                        if (e.Source is BinImageVisual)
                        {
                            BinImageVisual c = (BinImageVisual)e.Source;
                            if (SelectedImage != c)
                                SelectedImage = c;
                        }
                        else
                            SelectedImage = null;

                        if (e.Source is BinTextVisual)
                        {
                            BinTextVisual c = (BinTextVisual)e.Source;
                            if (SelectedText != c)
                                SelectedText = c;
                        }
                        else
                            SelectedText = null;

                        return;
                    }
                    else
                    { SelectedText = null; SelectedImage = null; }

                    if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
                    {
                        BinComponentVisual c = (BinComponentVisual)e.Source;
                        FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)Util.TryFindParent<BinConnectorVisual>(f);
                        if ((element is BinConnectorVisual && !IsLinking && State == BinEditorState.READY))
                        {
                            BinConnectorVisual b = element as BinConnectorVisual;
                            SelectedConnector = b;
                            //draggedLink.SetBinding(CryptoLineView.IsLinkingProperty, new Binding() { Source = this, Path = new PropertyPath(BinEditorVisual.IsLinkingProperty) });
                            draggedLink.SetBinding(CryptoLineView.StartPointProperty, Util.CreateConnectorBinding(b, draggedLink));
                            draggedLink.EndPoint = e.GetPosition(sender as FrameworkElement);
                            VisualCollection.Add(draggedLink);
                            Mouse.OverrideCursor = Cursors.Cross;
                            e.Handled = IsLinking = true;
                        }
                        PluginChangedEventArgs componentArgs = new PluginChangedEventArgs(c.Model.Plugin, c.FunctionName, DisplayPluginMode.Normal);
                        MyEditor.onSelectedPluginChanged(componentArgs);
                        if (SelectedItems == null || !SelectedItems.Contains(c))
                            SelectedItems = new UIElement[] { c };
                        startedSelection = true;
                        return;
                    }
                    break;

                case 2:
                    if (e.Source is BinComponentVisual)
                    {
                        BinComponentVisual c = (BinComponentVisual)e.Source;
                        if (c.IsICPopUpOpen || Util.TryFindParent<TextBox>(e.OriginalSource as UIElement) != null ||
                            Util.TryFindParent<BinSettingsVisual>(e.OriginalSource as UIElement) != null)
                        {
                            startedSelection = true;
                            break;
                        }

                        IsFullscreenOpen = true;
                        FullscreenVisual.ActiveComponent = c;
                        e.Handled = true;
                        startedSelection = true;
                    }
                    break;
            }
        }

        private void removeDragWindowHandle()
        {
            if (window != null)
            {
                window.PreviewMouseMove -= new MouseEventHandler(WindowPreviewMouseMove);
                window.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(WindowPreviewMouseLeftButtonUp);
                window.MouseLeave -= new MouseEventHandler(WindowMouseLeave);
            }
        }

        private void setDragWindowHandle()
        {
            if (window != null)
            {
                window.PreviewMouseMove += new MouseEventHandler(WindowPreviewMouseMove);
                window.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(WindowPreviewMouseLeftButtonUp);
                window.MouseLeave += new MouseEventHandler(WindowMouseLeave);
            }
        }

        void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            removeDragWindowHandle();
            dragReset();
        }

        void WindowPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            removeDragWindowHandle();
            dragReset();
        }

        void WindowPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (startDragPoint != null && e.LeftButton == MouseButtonState.Pressed)
            {
                startedSelection = true;
                Point currentPoint = e.GetPosition(ScrollViewer.Content as UIElement);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                delta.Negate();
                selectRectGeometry.Rect = new Rect((Point)startDragPoint, delta);
                selectionPath.Data = selectRectGeometry;
                List<UIElement> items = new List<UIElement>();
                foreach (var element in ComponentCollection)
                {
                    Rect elementRect = new Rect(element.Position, new Size(element.ActualWidth, element.ActualHeight));
                    if (selectRectGeometry.Rect.IntersectsWith(elementRect))
                        items.Add(element);
                    else
                        items.Remove(element);
                }
                SelectedItems = items.ToArray();
                return;
            }
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is BinComponentVisual && e.OriginalSource is FrameworkElement)
            {
                BinComponentVisual c = (BinComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)Util.TryFindParent<BinConnectorVisual>(f);
                if (element is BinConnectorVisual)
                {
                    BinConnectorVisual b = (BinConnectorVisual)element;
                    if (IsLinking && SelectedConnector != null)
                    {
                        if (SelectedConnector.Model != null || b.Model != null)
                        {
                            if (SelectedConnector.Model.ConnectorType != null || b.Model.ConnectorType != null)
                            {
                                ConversionLevel lvl = WorkspaceModel.compatibleConnectors(SelectedConnector.Model, b.Model);
                                if (lvl != ConversionLevel.Red && lvl != ConversionLevel.NA)
                                {
                                    ConnectorModel input, output;
                                    input = SelectedConnector.Model.Outgoing == true ? b.Model : SelectedConnector.Model;
                                    output = SelectedConnector.Model.Outgoing == false ? b.Model : SelectedConnector.Model;

                                    ConnectionModel connectionModel = (ConnectionModel)Model.ModifyModel(new NewConnectionModelOperation(
                                        output,
                                        input,
                                        output.ConnectorType));
                                    addConnection(SelectedConnector, b, connectionModel);
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }
            }
            reset();
        }

        private void LoadingErrorOccurred(object sender, LoadingErrorEventArgs e)
        {
            HasLoadingError = true;
            LoadingErrorText = e.Message;
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
                    AddBinComponentVisual(pluginModel,0);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MyEditor.GuiLogMessage(string.Format("Could not add Plugin to Workspace: {0}", ex.Message), NotificationLevel.Error);
                    MyEditor.GuiLogMessage(ex.StackTrace, NotificationLevel.Error);
                }
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                foreach (string fileLoc in filePaths)
                {
                    // Code to read the contents of the text file
                    if (System.IO.File.Exists(fileLoc))
                    {
                        MyEditor.Open(fileLoc);
                        break;
                    }
                }
                return;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        internal void SetFullscreen(BinComponentVisual bin, BinComponentState state)
        {
            FullscreenVisual.ActiveComponent = bin;
            bin.State = state;
            IsFullscreenOpen = true;
        }
    }

    #region HelperClass

    class SelectionChangedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            UIElement[] elements = (UIElement[])value;
            if (elements.Contains(parameter))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
