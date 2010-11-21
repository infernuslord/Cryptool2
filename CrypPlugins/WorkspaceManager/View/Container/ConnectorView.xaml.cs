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
using WorkspaceManager.View.Interface;
using System.ComponentModel;
using WorkspaceManager.Model;
using System.Windows.Controls.Primitives;

namespace WorkspaceManager.View.Container
{
    public enum ConnectorOrientation
    {
        North,
        South,
        West,
        East,
        Unset
    };

    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class ConnectorView : UserControl, IConnectable, IUpdateableView
    {

        public event EventHandler<ConnectorViewEventArgs> OnConnectorMouseLeftButtonDown;
        public ConnectorModel model;
        public ConnectorModel Model
        {
            get { return model; }
            private set { model = value; }
        }

        private ConnectorOrientation orientation;
        public ConnectorOrientation Orientation 
        {
            get 
            {
                return orientation;
            }
            set
            {
                orientation = value;
                switch (value)
                {
                    case ConnectorOrientation.West:
                        if (model.Outgoing)
                            Rotation.Angle = 90;
                        else
                            Rotation.Angle = -90;
                        break;
                    case ConnectorOrientation.East:
                        if (model.Outgoing)
                            Rotation.Angle = -90;
                        else
                            Rotation.Angle = 90;
                        break;
                    case ConnectorOrientation.North:
                        if (model.Outgoing)
                            Rotation.Angle = 180;
                        else
                            Rotation.Angle = 0;
                        break;
                    case ConnectorOrientation.South:
                        if (model.Outgoing)
                            Rotation.Angle = 0;
                        else
                            Rotation.Angle = 180;
                        break;
                }

                this.Model.Orientation = value;
            }
        }

        public PluginContainerView Parent { get; set; }

        public ConnectorView()
        {
            InitializeComponent();
        }

        public ConnectorView(ConnectorModel Model, PluginContainerView Parent)
        {
            InitializeComponent();
            setBaseControl(Model);
            this.Parent = Parent;

            if (Model.IsMandatory)
            {
                ConnectorRep.Stroke = Brushes.White;
                Scale.ScaleX = 0.8;
                Scale.ScaleY = 0.7;
            }

            if (Model.Orientation == ConnectorOrientation.Unset)
            {
                if (model.Outgoing)
                    this.Orientation = ConnectorOrientation.East;
                else
                    this.Orientation = ConnectorOrientation.West;
            }
            else
                this.Orientation = Model.Orientation;

            Color color = ColorHelper.GetLineColor(Model.ConnectorType);
            this.ConnectorRep.Fill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            this.ConnectorRep.ToolTip = Model.ToolTip;
        }

        public Point GetPositionOnWorkspace()
        {
            try
            {
                GeneralTransform gTransform, gTransformSec;
                Point point, relativePoint;
                StackPanel currentSp = null;

                if (Parent.West.Children.Contains(this))
                    currentSp = Parent.West;
                if (Parent.East.Children.Contains(this))
                    currentSp = Parent.East;
                if (Parent.North.Children.Contains(this))
                    currentSp = Parent.North;
                if (Parent.South.Children.Contains(this))
                    currentSp = Parent.South;

                gTransform = currentSp.TransformToVisual(Parent);
                gTransformSec = this.TransformToVisual(currentSp);

                point = gTransform.Transform(new Point(0, 0));
                relativePoint = gTransformSec.Transform(new Point(0, 0));
                Point result = new Point(Parent.GetPosition().X + point.X + relativePoint.X, Parent.GetPosition().Y + point.Y + relativePoint.Y);
                return result;
            }
            catch (Exception)
            {
                return new Point(0, 0);
            }
        }

        void ConnectorView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (this.OnConnectorMouseLeftButtonDown != null)
            //{
            //    this.OnConnectorMouseLeftButtonDown.Invoke(this, new ConnectorViewEventArgs { connector = this });
            //}
        }

        private void setBaseControl(ConnectorModel Model)
        {
            //this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            //this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(ConnectorView_MouseRightButtonDown);
            //this.PreviewMouseRightButtonUp += new MouseButtonEventHandler(ConnectorView_MouseRightButtonUp);
            //this.MouseLeave += new MouseEventHandler(ConnectorView_MouseLeave);
            this.Model = Model;
            this.DataContext = Model;
            this.Model.UpdateableView = this;
        }

        //void ConnectorView_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    BubblePopup.StaysOpen = false;
        //}

        //void ConnectorView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    BubblePopup.StaysOpen = false;
        //}

        //void ConnectorView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.BubblePopup.IsOpen = true;
        //    BubblePopup.StaysOpen = true;
        //}

        //void ConnectorView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (this.OnConnectorMouseLeftButtonDown != null)
        //    {
        //        this.OnConnectorMouseLeftButtonDown.Invoke(this, new ConnectorViewEventArgs { connector = this });
        //    }
        //}

        //public void ResetPopUp()
        //{
        //    Random random = new Random();
        //    BubblePopup.PlacementRectangle = new Rect(new Point(random.NextDouble() / 1000, 0), new Size(0, 0));
        //}

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }

        public void update()
        {
            if (model.HasData)
            {
                ToolTip = model.Data;
            }

        }

    }

    public class ConnectorViewEventArgs : EventArgs
    {
        public ConnectorView connector;
    }
}
