using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
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
using System.Windows.Threading;
using System.Threading;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using Cryptool.Core;
using System.Windows.Data;
using WorkspaceManager.Base.Sort;
using System.Windows.Controls.Primitives;
using WorkspaceManager.View.VisualComponents.CryptoLineView;
using WorkspaceManagerModel.Model.Tools;

namespace WorkspaceManager.View.Visuals
{
    public class VisualsHelper : INotifyPropertyChanged
    {
        private System.Drawing.RectangleF rect = new System.Drawing.RectangleF((float)-2000, (float)-2000, (float)6000, (float)6000);
        private Point? startDragPoint;
        private Point? draggedFrom, draggedTo;
        private FromTo draggedFT;
        private Popup pop;

        internal Line part = new Line();
        LinkedList<FromTo> linkedPointList = new LinkedList<FromTo>();
        public ObservableCollection<UIElement> Visuals { get; set; }
        public QuadTreeLib.QuadTree<FakeNode> PluginTree { get; set; }
        public QuadTreeLib.QuadTree<FakeNode> FromToTree { get; set; }
        public CryptoLineView CurrentLine { get; set; }
        public CryptoLineView LastLine { get; set; }  
        public  int LineCount { get; set; }

        //private uint _computationDone = 0;
        //public uint ComputationDone
        //{
        //    get
        //    {
        //        return _computationDone;
        //    }
        //    set 
        //    {
        //        _computationDone = value;
        //        if (_computationDone >= LineCount && !editor.IsExecuting && editor.IsVisible)
        //        {
        //            foreach (var element in Visuals.OfType<CryptoLineView>())
        //            {
        //                element.Line.ClearIntersect();
        //            }
 
        //            foreach (var element in Visuals.OfType<CryptoLineView>())
        //            {
        //                element.Line.DrawDecoration();
        //            }

        //            foreach (var element in Visuals.OfType<CryptoLineView>())
        //            {
        //                element.Line.InvalidateOnce();
        //            }

        //            _computationDone = 0;
        //        }
        //    }
        //}

        private FromTo selectedPart;
        public FromTo SelectedPart
        {
            get { return selectedPart; }
            set
            {
                selectedPart = value;
                OnPropertyChanged("SelectedPart");
            }
        }

        private WorkspaceModel model;
        public WorkspaceModel Model
        {
            get { return model; }
            set
            {
                model = value;
            }
        }

        private EditorVisual editor;
        public EditorVisual Editor
        {
            get { return editor; }
            private set
            {
                editor = value;
            }
        }

        private bool callback  = true;
        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
        public VisualsHelper(WorkspaceModel model, EditorVisual editor)
        {
            this.Model = model;
            this.Editor = editor;
            PluginTree = new QuadTreeLib.QuadTree<FakeNode>(rect);
            FromToTree = new QuadTreeLib.QuadTree<FakeNode>(rect);
            timer.Tick += delegate(object o, EventArgs args)
            {
                callback = false;
                
                var op = editor.Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                    {
                        var filter = Visuals.OfType<CryptoLineView>();

                        foreach (var element in filter)
                            element.Line.ClearIntersect();

                        foreach (var element in filter)
                            element.Line.DrawDecoration();

                        foreach (var element in filter)
                            element.Line.InvalidateVisual();

                    },null);

                op.Completed += delegate
                    {
                        callback = true;
                    };

                timer.Stop();
            };

            Visuals = Editor.VisualCollection;
            Visuals.CollectionChanged += new NotifyCollectionChangedEventHandler(VisualsCollectionChanged);
            Visuals.Add(part);
        }


        internal void DrawDecoration()
        {
            if(!timer.IsEnabled && callback)
                timer.Start();
        }

        internal void panelPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            reset();
        }

        internal void panelMouseLeave(object sender, MouseEventArgs e)
        {
            reset();
        }

        internal void panelPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedPart != null)
                draggedFT = SelectedPart;

            startDragPoint = Util.MouseUtilities.CorrectGetPosition((UIElement)sender);
        }

        void reset()
        {
            if (CurrentLine != null)
            {
                CurrentLine.Line.IsEditingPoint = false;
                LastLine = CurrentLine;
                DrawDecoration();
            }

            CurrentLine = null;
            SelectedPart = null;
            startDragPoint = null;
            draggedFrom = null;
            draggedTo = null;
            draggedFT = null;
        }

        internal void panelPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is CryptoLineView && draggedFT == null)
            {
                CryptoLineView l = (CryptoLineView)e.Source;
                if (l.IsSelected)
                {
                    Point p = Mouse.GetPosition(sender as FrameworkElement);
                    var list = FromToTree.Query(new System.Drawing.RectangleF(
                        (float)p.X - (float)1.5, (float)p.Y - (float)1.5, (float)3, (float)3));

                    if (list.Count != 0)
                    {
                        if (CurrentLine == null)
                            CurrentLine = l;

                        foreach (var x in list)
                        {
                            if (x.LogicalParent == CurrentLine.Line)
                                SelectedPart = x.FromTo;
                        }      
                    }
                }
            }
            else
            {
                if (draggedFT == null && !(e.Source is Line))
                {
                    SelectedPart = null;
                }
            }

            if (startDragPoint == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed && draggedFT != null && CurrentLine != null)
            {
                if (e.Source is CryptoLineView)
                    if (e.Source as CryptoLineView != CurrentLine)
                        return;

                Point currentPoint = Util.MouseUtilities.CorrectGetPosition(Editor.panel);
                Vector delta = Point.Subtract((Point)startDragPoint, currentPoint);
                delta.Negate();

                var data = draggedFT;
                foreach (var p in CurrentLine.Line.PointList)
                {
                    linkedPointList.AddLast(p);
                }

                var curData = linkedPointList.Find(data);
                if (curData == null)
                    return;

                var prevData = linkedPointList.Find(data).Previous;
                if (prevData == null)
                    return;

                var nextData = linkedPointList.Find(data).Next;
                if (nextData == null)
                    return;

                CurrentLine.Line.IsEditingPoint = true;

                if (draggedFrom == null || draggedTo == null)
                {
                    draggedFrom = new Point(data.From.X, data.From.Y);
                    draggedTo = new Point(data.To.X, data.To.Y);
                }

                CurrentLine.Line.ClearIntersect();

                switch (data.DirSort)
                {
                    case DirSort.X_ASC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X, ((Point)draggedFrom).Y + delta.Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X, ((Point)draggedTo).Y + delta.Y);
                        break;
                    case DirSort.X_DESC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X, ((Point)draggedFrom).Y + delta.Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X, ((Point)draggedTo).Y + delta.Y);
                        break;
                    case DirSort.Y_ASC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X + delta.X, ((Point)draggedFrom).Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X + delta.X, ((Point)draggedTo).Y);
                        break;
                    case DirSort.Y_DESC:
                        data.From = prevData.Value.To = new Point(((Point)draggedFrom).X + delta.X, ((Point)draggedFrom).Y);
                        data.To = nextData.Value.From = new Point(((Point)draggedTo).X + delta.X, ((Point)draggedTo).Y);
                        break;
                }
                CurrentLine.Line.InvalidateVisual();

                foreach (var fromTo in CurrentLine.Line.PointList)
                {
                    fromTo.Update();
                }

                CurrentLine.Line.Save();
            }

        }

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

        void VisualsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        return;
                    if (e.NewItems[0] is ComponentVisual)
                    {
                        (e.NewItems[0] as ComponentVisual).IsDraggingChanged += new EventHandler<IsDraggingChangedArgs>(VisualsHelperIsDraggingChanged);
                        (e.NewItems[0] as ComponentVisual).Loaded += new RoutedEventHandler(VisualsHelper_Loaded);
                    }

                    if (e.NewItems[0] is CryptoLineView)
                    {
                        (e.NewItems[0] as CryptoLineView).Line.ComputationDone += new EventHandler<ComputationDoneEventArgs>(Line_ComputationDone);
                        //(e.NewItems[0] as CryptoLineView).IsSelectedChanged += new EventHandler(VisualsHelper_IsSelectedChanged);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        return;
                    if (e.OldItems[0] is ComponentVisual)
                    {
                        (e.OldItems[0] as ComponentVisual).IsDraggingChanged -= new EventHandler<IsDraggingChangedArgs>(VisualsHelperIsDraggingChanged);
                        (e.OldItems[0] as ComponentVisual).Loaded -= new RoutedEventHandler(VisualsHelper_Loaded);
                    }


                    if (e.OldItems[0] is CryptoLineView)
                    {
                        (e.OldItems[0] as CryptoLineView).Line.ComputationDone -= new EventHandler<ComputationDoneEventArgs>(Line_ComputationDone);
                        //(e.OldItems[0] as CryptoLineView).IsSelectedChanged -= new EventHandler(VisualsHelper_IsSelectedChanged);
                    }

                    break;
            }

            //LineCount = Visuals.OfType<CryptoLineView>().Count();
        }

        //void VisualsHelper_IsSelectedChanged(object sender, EventArgs e)
        //{
        //    var line = (CryptoLineView)sender;
        //}

        void Line_ComputationDone(object sender, ComputationDoneEventArgs e)
        {
            if(e.IsPathComputationDone)
                renewFromToTree();
        }

        void VisualsHelper_Loaded(object sender, RoutedEventArgs e)
        {
            renewPluginTree();
        }

        void VisualsHelperIsDraggingChanged(object sender, IsDraggingChangedArgs e)
        {
            if (!e.IsDragging)
            {
                renewPluginTree();
            }
        }

        private void renewPluginTree()
        {
            PluginTree = new QuadTreeLib.QuadTree<FakeNode>(rect);
            foreach (var element in Visuals.OfType<ComponentVisual>())
            {
                PluginTree.Insert(new FakeNode()
                {
                    Rectangle = new System.Drawing.RectangleF((float)element.Position.X,
                                                               (float)element.Position.Y,
                                                               (float)element.ObjectSize.X,
                                                               (float)element.ObjectSize.Y)
                });
            }
        }

        private void renewFromToTree()
        {
            FromToTree = new QuadTreeLib.QuadTree<FakeNode>(rect);
            var temp = Visuals.OfType<CryptoLineView>();
            foreach (var element in temp)
            {
                foreach (var fromTo in element.Line.PointList)
                {
                    if (fromTo.MetaData == FromToMeta.HasEndpoint || fromTo.MetaData == FromToMeta.HasStartPoint)
                        continue;

                    float x = 0, y = 0, sizeY = 0, sizeX = 0;
                    double stroke = 8;
                    switch (fromTo.DirSort)
                    {
                        case DirSort.X_ASC:
                            x = (float)(fromTo.From.X);
                            y = (float)(fromTo.From.Y - (stroke / 2));
                            sizeX = (float)(fromTo.To.X);
                            sizeY = (float)((stroke * 2));
                            break;
                        case DirSort.X_DESC:
                            x = (float)(fromTo.To.X);
                            y = (float)(fromTo.To.Y - (stroke / 2));
                            sizeX = (float)(fromTo.From.X);
                            sizeY = (float)((stroke * 2));
                            break;
                        case DirSort.Y_ASC:
                            y = (float)(fromTo.From.Y);
                            x = (float)(fromTo.From.X - (stroke / 2));
                            sizeY = (float)(fromTo.To.Y);
                            sizeX = (float)((stroke * 2));
                            break;
                        case DirSort.Y_DESC:
                            y = (float)(fromTo.To.Y);
                            x = (float)(fromTo.To.X - (stroke / 2));
                            sizeY = (float)(fromTo.From.Y);
                            sizeX = (float)((stroke * 2));
                            break;
                    }
                    FromToTree.Insert(new FakeNode()
                    {
                        Rectangle = new System.Drawing.RectangleF(x,
                                                                   y,
                                                                   sizeX,
                                                                   sizeY),
                        FromTo = fromTo,
                        LogicalParent = element.Line
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Interaction logic for BinEditorVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class EditorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectedTextChanged;
        public event EventHandler SelectedConnectorChanged;
        public event EventHandler<SelectedItemsEventArgs> ItemsSelected;
        #endregion

        #region Fields
        internal ModifiedCanvas panel;
        private Window window;
        private ArevaloRectanglePacker packer;
        private ConnectorVisual from, to;
        private RectangleGeometry selectRectGeometry = new RectangleGeometry();
        private bool startedSelection;
        private CryptoLineView draggedLink;
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

        private VisualsHelper visualsHelper;
        public VisualsHelper VisualsHelper
        {
            get { return visualsHelper; }
            set
            {
                visualsHelper = value;
                OnPropertyChanged("VisualsHelper");
            }
        }

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
                if (model.Zoom != 0)
                    ZoomLevel = model.Zoom;
            }
        }

        public WorkspaceManagerClass MyEditor { get; private set; }

        public FullscreenVisual FullscreenVisual { get { return (FullscreenVisual)FullScreen.Content; } }

        private ObservableCollection<UIElement> selectedItemsObservable = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> SelectedItemsObservable { get { return selectedItemsObservable; } private set { selectedItemsObservable = value; } }

        private ObservableCollection<UIElement> visualCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> VisualCollection { get { return visualCollection; } private set { visualCollection = value; } }

        private ObservableCollection<ComponentVisual> componentCollection = new ObservableCollection<ComponentVisual>();
        public ObservableCollection<ComponentVisual> ComponentCollection { get { return componentCollection; } private set { componentCollection = value; } }

        private ObservableCollection<CryptoLineView> pathCollection = new ObservableCollection<CryptoLineView>();
        public ObservableCollection<CryptoLineView> PathCollection { get { return pathCollection; } private set { pathCollection = value; } }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSettingsOpenProperty = DependencyProperty.Register("IsSettingsOpen",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(true));

        public bool IsSettingsOpen
        {
            get { return (bool)base.GetValue(IsSettingsOpenProperty); }
            set
            {
                base.SetValue(IsSettingsOpenProperty, value);
            }
        }

        public static readonly DependencyProperty IsLinkingProperty = DependencyProperty.Register("IsLinking",
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, OnIsLinkingChanged));

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

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register("ZoomLevel",
    typeof(double), typeof(EditorVisual), new FrameworkPropertyMetadata((double)1, OnZoomLevelChanged));

        public double ZoomLevel
        {
            get
            {
                return (double)base.GetValue(ZoomLevelProperty);
            }
            set
            {
                base.SetValue(ZoomLevelProperty, value);
            }
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinEditorState), typeof(EditorVisual), new FrameworkPropertyMetadata(BinEditorState.READY, null));

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
            typeof(ConnectorVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedConnectorChanged));

        public ConnectorVisual SelectedConnector
        {
            get
            {
                return (ConnectorVisual)base.GetValue(SelectedConnectorProperty);
            }
            private set
            {
                base.SetValue(SelectedConnectorProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText",
            typeof(TextVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedTextChanged));

        public TextVisual SelectedText
        {
            get
            {
                return (TextVisual)base.GetValue(SelectedTextProperty);
            }
            set
            {
                base.SetValue(SelectedTextProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage",
            typeof(ImageVisual), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedImageChanged));

        public ImageVisual SelectedImage
        {
            get
            {
                return (ImageVisual)base.GetValue(SelectedImageProperty);
            }
            set
            {
                base.SetValue(SelectedImageProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems",
            typeof(UIElement[]), typeof(EditorVisual), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

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
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, OnIsLoadingChanged));

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
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));


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
            typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));


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
    typeof(bool), typeof(EditorVisual), new FrameworkPropertyMetadata(false, null));

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
    typeof(string), typeof(EditorVisual), new FrameworkPropertyMetadata(string.Empty, null));

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
        public EditorVisual(WorkspaceModel model)
        {
            Model = model;
            VisualsHelper = new VisualsHelper(Model, this);
            MyEditor = (WorkspaceManagerClass)Model.MyEditor;
            MyEditor.executeEvent += new EventHandler(ExecuteEvent);
            VisualCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            VisualCollection.Add(selectionPath);
            draggedLink = new CryptoLineView(VisualCollection);
            MyEditor.LoadingErrorOccurred += new EventHandler<LoadingErrorEventArgs>(LoadingErrorOccurred);
            MyEditor.PasteOccured += new EventHandler(PasteOccured);
            InitializeComponent();
            _usagePopup = new UsageStatisticPopup(this);
            //_usagePopup.Closed += new EventHandler(_usagePopup_Closed);
            //_usagePopup.Opened += new EventHandler(_usagePopup_Closed);
        }


        //void _usagePopup_Closed(object sender, EventArgs e)
        //{
        //    if (_usagePopup.IsOpen)
        //    {
        //        reset();
        //    }
        //    else
        //    {
        //        reset();
        //    }
        //}

        #endregion

        #region Public

        void PasteOccured(object sender, EventArgs e)
        {
            var concat = new UIElement[0];

            foreach (var bin in ComponentCollection)
            {
                if (MyEditor.CurrentCopies.Contains(bin.Model))
                {
                    concat = concat.Concat(new UIElement[] { bin }).ToArray();
                }
            }
            SelectedItems = concat;
        }

        public void AddComponentVisual(PluginModel pluginModel, int mode = 0)
        {
            if (this.State != BinEditorState.READY)
                return;

            ComponentVisual bin = new ComponentVisual(pluginModel);
            Binding bind = new Binding();
            bind.Path = new PropertyPath(EditorVisual.SelectedItemsProperty);
            bind.Source = this;
            bind.ConverterParameter = bin;
            bind.Converter = new SelectionChangedConverter();
            bin.SetBinding(ComponentVisual.IsSelectedProperty, bind);
            bin.PositionDeltaChanged += new EventHandler<PositionDeltaChangedArgs>(ComponentPositionDeltaChanged);

            if (mode == 0)
            {

            }

            if (mode == 1)
            {
                GeneralTransform g = new ScaleTransform(Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale, Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale, 0, 0);
                Point p = g.Transform(new Point(randomNumber(0, (int)(ActualWidth - bin.ActualWidth)), randomNumber(0, (int)(ActualHeight - bin.ActualHeight))));
                bin.Position = p;
            }

            VisualCollection.Add(bin);
        }

        public DispatcherOperation Load(WorkspaceModel model, bool isPaste = false)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (!isPaste)
            {
                Model = model;
                var op = internalLoad(model);
                op.Completed += delegate
                {
                    IsLoading = false;
                };
                return op;
            }
            else
            {
                internalPasteLoad(model);
            }
            return null;
        }

        private void internalPasteLoad(WorkspaceModel model)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (SendOrPostCallback)delegate
            {
                WorkspaceModel m = (WorkspaceModel)model;

                foreach (PluginModel pluginModel in m.GetAllPluginModels())
                {
                    this.model.addPluginModel(pluginModel);
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
                        AddComponentVisual(pluginModel, 0);
                }

                foreach (ConnectionModel connModel in m.GetAllConnectionModels())
                {
                    if (connModel.To.IControl)
                        continue;

                    foreach (UIElement element in VisualCollection)
                    {
                        ComponentVisual bin = element as ComponentVisual;
                        if (bin != null)
                        {
                            foreach (ConnectorVisual connector in bin.ConnectorCollection)
                            {
                                if (connModel.From == connector.Model)
                                    from = connector;
                                else if (connModel.To == connector.Model)
                                    to = connector;
                            }
                        }
                    }

                    AddConnectionVisual(from, to, connModel);
                }
                PartialCopyHelper.CurrentSelection = null;
            }
            , null);
        }

        public void ResetConnections()
        {
            foreach (CryptoLineView view in PathCollection)
            {
                view.Line.reset();
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
                while (ZoomLevel
                    > Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale
                    && (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0))
                {
                    ZoomLevel -= 0.02;
                    ScrollViewer.UpdateLayout();
                }
            }
            else
            {
                while (ZoomLevel
                    < Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale
                    && ScrollViewer.ScrollableHeight == 0
                    && ScrollViewer.ScrollableWidth == 0)
                {
                    ZoomLevel += 0.02;
                    ScrollViewer.UpdateLayout();
                }
                if (ScrollViewer.ScrollableHeight > 0
                    || ScrollViewer.ScrollableWidth > 0)
                    ZoomLevel -= 0.02;
            }
        }

        public void ResetPlugins(int value)
        {
            if (value == 0)
            {
                foreach (ComponentVisual b in ComponentCollection)
                    b.Progress = 0;
            }

            if (value == 1)
            {
                foreach (ComponentVisual b in ComponentCollection)
                    b.LogMessages.Clear();
            }
        }

        public void AddText()
        {
            var bin = new TextVisual((TextModel)Model.ModifyModel(new NewTextModelOperation()));
            VisualCollection.Add(bin);
            SelectedText = bin;
        }

        public void AddImage(Uri uri)
        {
            try
            {
                ImageVisual bin = new ImageVisual((ImageModel)Model.ModifyModel(new NewImageModelOperation(uri)));
                VisualCollection.Add(bin);
            }
            catch (Exception e)
            {
                MyEditor.GuiLogMessage(string.Format("Could not add image to workspace: {0}", e.Message), NotificationLevel.Error);
            }
        }
        #endregion

        #region Private

        private DispatcherOperation internalLoad(object model)
        {
            IsLoading = true;
            return Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (SendOrPostCallback)delegate
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
                        AddComponentVisual(pluginModel, 0);
                }

                foreach (ConnectionModel connModel in m.GetAllConnectionModels())
                {
                    if (connModel.To.IControl)
                        continue;

                    foreach (UIElement element in VisualCollection)
                    {
                        ComponentVisual bin = element as ComponentVisual;
                        if (bin != null)
                        {
                            foreach (ConnectorVisual connector in bin.ConnectorCollection)
                            {
                                if (connModel.From == connector.Model)
                                    from = connector;
                                else if (connModel.To == connector.Model)
                                    to = connector;
                            }
                        }
                    }

                    AddConnectionVisual(from, to, connModel);
                }

                foreach (var img in m.GetAllImageModels())
                {
                    this.VisualCollection.Add(new ImageVisual(img));
                }

                foreach (var txt in m.GetAllTextModels())
                {

                    try
                    {
                        this.VisualCollection.Add(new TextVisual(txt));
                    }
                    catch (Exception e)
                    {
                        MyEditor.GuiLogMessage(string.Format("Could not load Text to Workspace: {0}", e.Message), NotificationLevel.Error);
                    }
                }
            }
            , null);
        }

        public void AddConnectionVisual(ConnectorVisual source, ConnectorVisual target, ConnectionModel model)
        {
            if (this.State != BinEditorState.READY || source == null || target == null)
                return;

            CryptoLineView link = new CryptoLineView(model, source, target);
            Binding bind = new Binding();
            bind.Path = new PropertyPath(EditorVisual.SelectedItemsProperty);
            bind.Source = this;
            bind.ConverterParameter = link;
            bind.Converter = new SelectionChangedConverter();
            link.SetBinding(CryptoLineView.IsSelectedProperty, bind);
            VisualCollection.Add(link);
        }

        private void reset()
        {
            VisualCollection.Remove(draggedLink);
            SelectedConnector = null;
            IsLinking = false;
            Mouse.OverrideCursor = null;
        }

        private static Random random = new Random();
        private UsageStatisticPopup _usagePopup;

        private double randomNumber(int min, int max)
        {
            return (double)random.Next(min, max);
        }

        internal void SetFullscreen(ComponentVisual bin, BinComponentState state)
        {
            FullscreenVisual.ActiveComponent = bin;
            bin.FullScreenState = state;
            IsFullscreenOpen = true;
        }

        private void dragReset()
        {
            selectionPath.Data = null;
            startDragPoint = null;

            if (!startedSelection)
                SelectedItems = null;

            startedSelection = false;
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
                }else
                {
                    var model = (ConnectionModel) args.EffectedModelElement;
                    if (!model.To.IControl)
                        AddConnectionVisual((ConnectorVisual)model.From.UpdateableView, (ConnectorVisual)model.To.UpdateableView, model);
                }
            }
            else if (args.EffectedModelElement is PluginModel)
            {
                if (((PluginModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ComponentVisual plugin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(plugin))
                        VisualCollection.Add(plugin);
                }else
                {
                    var pluginModel = (PluginModel)args.EffectedModelElement;
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
                        AddComponentVisual(pluginModel);
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {

                    ImageVisual img = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    if (!VisualCollection.Contains(img))
                        VisualCollection.Add(img);
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txt = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
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
                    ComponentVisual bin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    bin.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ImageVisual img = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    img.Position = args.NewPosition;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txt = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
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
                    ComponentVisual pluginContainerView = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                    pluginContainerView.WindowWidth = args.NewWidth;
                    pluginContainerView.WindowHeight = args.NewHeight;
                }
            }
            else if (args.EffectedModelElement is ImageModel)
            {
                if (((ImageModel)args.EffectedModelElement).UpdateableView != null)
                {
                    ImageVisual imgWrapper = (ImageVisual)((ImageModel)args.EffectedModelElement).UpdateableView;
                    imgWrapper.WindowWidth = args.NewWidth;
                    imgWrapper.WindowHeight = args.NewHeight;
                }
            }
            else if (args.EffectedModelElement is TextModel)
            {
                if (((TextModel)args.EffectedModelElement).UpdateableView != null)
                {
                    TextVisual txtWrapper = (TextVisual)((TextModel)args.EffectedModelElement).UpdateableView;
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
                ComponentVisual bin = (ComponentVisual)((PluginModel)args.EffectedModelElement).UpdateableView;
                bin.CustomName = args.NewName;
            }
        }
        #endregion

        #region Event Handler

        private void ComponentPositionDeltaChanged(object sender, PositionDeltaChangedArgs e)
        {
            if (MyEditor.isExecuting())
                return;
            var b = (ComponentVisual)sender;
            if (SelectedItems != null)
            {
                var list = new List<Operation>();
                foreach (var element in SelectedItems.OfType<ComponentVisual>())
                {
                    var bin = (ComponentVisual)element;
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
            try
            {
                Clipboard.SetText(LoadingErrorText);
            }
            catch (Exception)
            {
                //1ms
                DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(10000) };
                timer.Start();
                timer.Tick += new EventHandler(delegate(object timerSender, EventArgs ee)
                {
                    DispatcherTimer t = (DispatcherTimer)timerSender;
                    t.Stop();
                    Clipboard.SetText(LoadingErrorText);
                });
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            UIElement[] newItem = e.NewValue as UIElement[];
            UIElement[] oldItem = e.OldValue as UIElement[];

            if (b.ItemsSelected != null)
                b.ItemsSelected.Invoke(b, new SelectedItemsEventArgs() { Items = b.SelectedItems });
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            bool newItem = (bool)e.NewValue;
            bool oldItem = (bool)e.OldValue;
            if (newItem)
                Mouse.OverrideCursor = Cursors.Wait;
            else
                Mouse.OverrideCursor = Cursors.Arrow;
        }

        private static void OnSelectedImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            ImageVisual newItem = e.NewValue as ImageVisual;
            ImageVisual oldItem = e.OldValue as ImageVisual;

            if (newItem != null)
                newItem.IsSelected = true;
            if (oldItem != null)
                oldItem.IsSelected = false;
        }

        private static void OnIsLinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            if (b.IsLinking)
            {

            }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            b.model.Zoom = b.ZoomLevel;
        }

        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            TextVisual newItem = e.NewValue as TextVisual;
            TextVisual oldItem = e.OldValue as TextVisual;

            if (newItem != null)
                newItem.IsSelected = true;
            if (oldItem != null)
                oldItem.IsSelected = false;

            b.SelectedTextChanged.Invoke(b, null);
        }

        private static void OnSelectedConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorVisual b = (EditorVisual)d;
            if (b.SelectedConnectorChanged != null) b.SelectedConnectorChanged.Invoke(b, null);
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
                packer = new ArevaloRectanglePacker(Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortWidth, Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortHeight);
                foreach (var element in ComponentCollection)
                {
                    Point point;
                    if (packer.TryPack(element.ActualWidth + Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding, element.ActualHeight + Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding, out point))
                    {
                        point.X += Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding;
                        point.Y += Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SortPadding;
                        element.Position = point;
                    }
                }
            }
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        return;
                    if (e.NewItems[0] is ComponentVisual && ComponentCollection != null)
                        ComponentCollection.Add(e.NewItems[0] as ComponentVisual);

                    if (e.NewItems[0] is CryptoLineView && PathCollection != null)
                        PathCollection.Add(e.NewItems[0] as CryptoLineView);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        return;
                    if (e.OldItems[0] is ComponentVisual && ComponentCollection != null)
                        ComponentCollection.Remove(e.OldItems[0] as ComponentVisual);

                    if (e.OldItems[0] is TextVisual)
                        SelectedText = null;

                    if (e.OldItems[0] is CryptoLineView && PathCollection != null)
                        PathCollection.Remove(e.OldItems[0] as CryptoLineView);

                    if (SelectedItems != null && SelectedItems.Length > 0)
                    {
                        var x = SelectedItems.ToList();
                        foreach (var uiElement in e.OldItems)
                        {
                            x.Remove(uiElement as UIElement);
                        }
                        SelectedItems = x.ToArray();
                    }
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
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (ZoomLevel + 0.05 < Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale &&
                    e.Delta >= 0)
                    ZoomLevel += 0.05;

                if (ZoomLevel - 0.05 > Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale &&
                    e.Delta <= 0)
                    ZoomLevel += -0.05;

                e.Handled = true;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (IsLinking)
            {
                draggedLink.Line.EndPoint = e.GetPosition(sender as FrameworkElement);
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

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            if (VisualsHelper.LastLine != null)
            {
                VisualsHelper.LastLine.Model.WorkspaceModel.ModifyModel(new DeleteConnectionModelOperation(VisualsHelper.LastLine.Model));
            }
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is ComponentVisual) && !(e.Source is ImageVisual) &&
                !(e.Source is TextVisual))
            {
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.ScrollAll;
                e.Handled = true;
            }

            if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
            {
                ComponentVisual c = (ComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource,
                    element = (FrameworkElement)Util.TryFindParent<ConnectorVisual>(f);
                if (element is ConnectorVisual)
                {
                    ConnectorVisual con = (ConnectorVisual)element;
                    DataObject data = new DataObject("BinConnector", element);
                    DragDrop.AddQueryContinueDragHandler(this, QueryContinueDragHandler);
                    con.IsDragged = true;
                    DragDrop.DoDragDrop(c, data, DragDropEffects.Move);
                    con.IsDragged = false;
                    e.Handled = true;
                }
            }

            //if (e.Source is CryptoLineView)
            //{
            //    CryptoLineView l = (CryptoLineView)e.Source;
            //    Model.ModifyModel(new DeleteConnectionModelOperation(l.Line.Model));
            //}
        }

        private void returnWindowFocus()
        {
            var win = Util.TryFindParent<Window>(this);
            if (win != null)
            {
                Keyboard.Focus(win as IInputElement);
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is ComponentVisual) && !(e.Source is ImageVisual) && !(e.Source is TextVisual)
                && !(e.Source is CryptoLineView) && !(e.Source is Line))
            {
                window = Window.GetWindow(this);
                setDragWindowHandle();
                startDragPoint = Mouse.GetPosition(sender as FrameworkElement);
                Mouse.OverrideCursor = Cursors.Arrow;
                this.Focus();
                Keyboard.Focus(this);
                e.Handled = true;
            }
     
            
            switch (e.ClickCount)
            {
                case 1:
                    var result = Util.TryFindParent<IControlVisual>(e.OriginalSource as UIElement);
                    if (result != null)
                        return;

                    if (e.Source is ImageVisual || e.Source is TextVisual)
                    {
                        if (e.Source is ImageVisual)
                        {
                            ImageVisual c = (ImageVisual)e.Source;
                            if (SelectedImage != c)
                                SelectedImage = c;
                        }
                        else
                            SelectedImage = null;

                        if (e.Source is TextVisual)
                        {
                            TextVisual c = (TextVisual)e.Source;
                            if (SelectedText != c)
                                SelectedText = c;
                        }
                        else
                            SelectedText = null;

                        return;
                    }
                    else
                    { SelectedText = null; SelectedImage = null; }

                    if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
                    {
                        ComponentVisual c = (ComponentVisual)e.Source;
                        FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)Util.TryFindParent<ConnectorVisual>(f);
                        if ((element is ConnectorVisual && !IsLinking && State == BinEditorState.READY))
                        {
                            ConnectorVisual b = element as ConnectorVisual;
                            SelectedConnector = b;
                            //draggedLink.SetBinding(CryptoLineView.IsLinkingProperty, new Binding() { Source = this, Path = new PropertyPath(BinEditorVisual.IsLinkingProperty) });
                            draggedLink.Line.SetBinding(InternalCryptoLineView.StartPointProperty, Util.CreateConnectorBinding(b, draggedLink));
                            draggedLink.Line.EndPoint = e.GetPosition(sender as FrameworkElement);
                            VisualCollection.Add(draggedLink);
                            Mouse.OverrideCursor = Cursors.Cross;
                            e.Handled = IsLinking = true;
                        }
                        PluginChangedEventArgs componentArgs = new PluginChangedEventArgs(c.Model.Plugin, c.FunctionName, DisplayPluginMode.Normal);
                        MyEditor.onSelectedPluginChanged(componentArgs);
                        if (SelectedItems == null || !SelectedItems.Contains(c))
                            SelectedItems = new UIElement[] { c };
                        startedSelection = true;
                        //var res = Util.TryFindParent<Thumb>(e.OriginalSource as UIElement);

                        //Dispatcher.BeginInvoke((ThreadStart)delegate
                        //{
                        //    c.Focus();
                        //    Keyboard.Focus(c);
                        //});
                        return;
                    }

                    if (e.Source is CryptoLineView)
                    {
                        CryptoLineView line = e.Source as CryptoLineView;
                        if (SelectedItems == null || !SelectedItems.Contains(line))
                            SelectedItems = new UIElement[] { line };
                        startedSelection = true;
                        return;
                    }
                    break;

                case 2:
                    if (e.Source is ComponentVisual)
                    {
                        ComponentVisual c = (ComponentVisual)e.Source;
                        if (c.IsICPopUpOpen || Util.TryFindParent<TextBox>(e.OriginalSource as UIElement) != null ||
                            Util.TryFindParent<Thumb>(e.OriginalSource as UIElement) == null)
                        {
                            startedSelection = true;
                            break;
                        }

                        SetFullscreen(c, c.State != BinComponentState.Min ? c.State : c.FullScreenState);
                        e.Handled = true;
                        startedSelection = true;
                    }
                    break;
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

        private void LoadingErrorOccurred(object sender, LoadingErrorEventArgs e)
        {
            HasLoadingError = true;
            LoadingErrorText = e.Message;
        }

        private void PanelLoaded(object sender, RoutedEventArgs e)
        {
            panel = (ModifiedCanvas)sender;
            panel.PreviewMouseMove += new MouseEventHandler(VisualsHelper.panelPreviewMouseMove);
            panel.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(VisualsHelper.panelPreviewMouseLeftButtonUp);
            panel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(VisualsHelper.panelPreviewMouseLeftButtonDown);
            panel.MouseLeave += new MouseEventHandler(VisualsHelper.panelMouseLeave);
            VisualsHelper.part.Style = (Style)FindResource("FromToLine");
        }

        void WindowPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_usagePopup.IsOpen)
            {
                if (startDragPoint != null && e.LeftButton == MouseButtonState.Pressed)
                {
                    startedSelection = true;
                    Point currentPoint = Util.MouseUtilities.CorrectGetPosition(panel);
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
                    foreach (var line in PathCollection)
                    {
                        foreach (var ft in line.Line.PointList)
                        {
                            Rect elementRect = new Rect(ft.From, ft.To);
                            if (selectRectGeometry.Rect.IntersectsWith(elementRect))
                            {
                                items.Add(line);
                                break;
                            }
                            else
                                items.Remove(line);
                        }

                    }
                    SelectedItems = items.ToArray();
                    return;
                }
            }
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ComponentVisual && e.OriginalSource is FrameworkElement)
            {
                ComponentVisual c = (ComponentVisual)e.Source;
                FrameworkElement f = (FrameworkElement)e.OriginalSource, element = (FrameworkElement)Util.TryFindParent<ConnectorVisual>(f);
                if (element is ConnectorVisual)
                {
                    ConnectorVisual b = (ConnectorVisual)element;
                    if (IsLinking && SelectedConnector != null)
                    {
                        if (SelectedConnector.Model != null || b.Model != null)
                        {
                            if (SelectedConnector.Model.ConnectorType != null || b.Model.ConnectorType != null)
                            {
                                ConnectorModel input, output;
                                input = SelectedConnector.Model.Outgoing == true ? b.Model : SelectedConnector.Model;
                                output = SelectedConnector.Model.Outgoing == false ? b.Model : SelectedConnector.Model;
                                ConversionLevel lvl = WorkspaceModel.compatibleConnectors(output, input);
                                if (lvl != ConversionLevel.Red && lvl != ConversionLevel.NA)
                                {
                                    ConnectionModel connectionModel = (ConnectionModel)Model.ModifyModel(new NewConnectionModelOperation(
                                        output,
                                        input,
                                        output.ConnectorType));
                                    AddConnectionVisual(SelectedConnector, b, connectionModel);
                                    e.Handled = true;
                                }
                                reset();
                                startedSelection = false;
                                return;
                            }
                        }
                    }
                }
            }

            _usagePopup.Open();

            reset();
            startedSelection = false;
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

            if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject") && !(e.Source is ComponentVisual))
            {
                try
                {
                    DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                    PluginModel pluginModel = (PluginModel)Model.ModifyModel(new NewPluginModelOperation(Util.MouseUtilities.CorrectGetPosition(sender as FrameworkElement), 0, 0, DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName)));
                    AddComponentVisual(pluginModel, 0);
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

        #endregion
    }

    #region HelperClass

    public class SelectedItemsEventArgs : EventArgs
    {
        public UIElement[] Items { get; set; }
    }

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
