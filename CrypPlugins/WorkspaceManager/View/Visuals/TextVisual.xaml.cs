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
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinTextVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class TextVisual : UserControl, IUpdateableView, IRouting
    {
        #region Properties
        private TextModel model;
        public TextModel Model { get { return model; } private set { model = value; Model.UpdateableView = this; } }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(TextVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedChanged)));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(TextVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowHeight
        {
            get
            {
                return (double)base.GetValue(WindowHeightProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowHeightProperty, value);
            }
        }

        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth",
            typeof(double), typeof(TextVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowWidth
        {
            get
            {
                return (double)base.GetValue(WindowWidthProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowWidthProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(TextVisual), new FrameworkPropertyMetadata(new Point(0, 0), new PropertyChangedCallback(OnPositionValueChanged)));

        public Point Position
        {
            get
            {
                return (Point)base.GetValue(PositionProperty);
            }
            set
            {
                base.SetValue(PositionProperty, value);
            }
        } 

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked",
            typeof(bool), typeof(TextVisual), new FrameworkPropertyMetadata(false, null));

        public bool IsLocked
        {
            get
            {
                return (bool)base.GetValue(IsLockedProperty);
            }
            set
            {
                base.SetValue(IsLockedProperty, value);
            }
        }

        public static readonly DependencyProperty WindowNameProperty = DependencyProperty.Register("WindowName",
            typeof(string), typeof(TextVisual), new FrameworkPropertyMetadata( Properties.Resources.Enter_Name, null));

        public string WindowName
        {
            get
            {
                return (string)base.GetValue(WindowNameProperty);
            }
            set
            {
                base.SetValue(WindowNameProperty, value);
            }
        }

        #endregion

        #region Constructors

        public TextVisual(TextModel model)
        {
            InitializeComponent();
            WindowWidth = model.GetWidth();
            WindowHeight = model.GetHeight();
            Position = model.GetPosition();
            Model = model;
            Model.loadRTB(mainRTB);
            Model.UpdateableView = this;
        } 
        #endregion

        #region Event Handler
        private static void OnPositionValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextVisual bin = (TextVisual)d;
            if (bin.PositionDeltaChanged != null)
                bin.PositionDeltaChanged.Invoke(bin, new PositionDeltaChangedArgs() { });

        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextVisual bin = (TextVisual)d;
            bin.mainRTB.Focusable = bin.IsSelected;
            if (bin.IsSelected)
                bin.mainRTB.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
        }

        virtual protected void CloseClick(object sender, RoutedEventArgs e) 
        {
            Model.WorkspaceModel.ModifyModel(new DeleteTextModelOperation(Model));
        }


        private void LockHandler(object sender, RoutedEventArgs e)
        {
            IsLocked = !IsLocked;
            e.Handled = true;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            WindowHeight += e.VerticalChange;
            WindowWidth += e.HorizontalChange;
            Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, WindowWidth, WindowHeight));
            e.Handled = true;
        }

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Position = new Point(Position.X + e.HorizontalChange, Position.Y + e.VerticalChange);
            Model.WorkspaceModel.ModifyModel(new MoveModelElementOperation(Model, Position));
        }

        public void update()
        {

        }

        private void TextChangedHandler(object sender, TextChangedEventArgs e)
        {
            Model.saveRTB((RichTextBox)sender);
        }
        #endregion

        public ObjectSize ObjectSize
        {
            get { throw new NotImplementedException(); }
        }

        public Point[] RoutingPoints
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;


        public Point GetRoutingPoint(int routPoint)
        {
            throw new NotImplementedException();
        }
    }
}