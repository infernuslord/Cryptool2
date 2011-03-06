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
using WorkspaceManager.View.Interface;
using System.ComponentModel;
using WorkspaceManager.Model;
using System.Windows.Controls.Primitives;
using WorkspaceManagerModel.Model.Interfaces;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class BinConnectorVisual : Thumb, IConnectable, IUpdateableView
    {
        #region Dependency Properties

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model",
            typeof(ConnectorModel), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMyValueChanged)));

        public ConnectorModel Model
        {
            get
            {
                return (ConnectorModel)base.GetValue(ModelProperty);
            }
            set
            {
                base.SetValue(ModelProperty, value);
                Model.UpdateableView = this;
            }
        }

        public static readonly DependencyProperty IsMandatoryProperty = DependencyProperty.Register("IsMandatory",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsMandatory
        {
            get
            {
                return (bool)base.GetValue(IsMandatoryProperty);
            }
            set
            {
                base.SetValue(IsMandatoryProperty, value);
            }
        }

        public static readonly DependencyProperty IsOutgoingProperty = DependencyProperty.Register("IsOutgoing",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsOutgoing
        {
            get
            {
                return (bool)base.GetValue(IsOutgoingProperty);
            }
            set
            {
                base.SetValue(IsOutgoingProperty, value);
            }
        }

        public static readonly DependencyProperty FunctionColorProperty = DependencyProperty.Register("FunctionColor",
            typeof(Color), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(Colors.Black));

        public Color FunctionColor
        {
            get
            {
                return (Color)base.GetValue(FunctionColorProperty);
            }
            set
            {
                base.SetValue(FunctionColorProperty, value);
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(ConnectorOrientation), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(ConnectorOrientation.Unset));

        public ConnectorOrientation Orientation
        {
            get
            {
                return (ConnectorOrientation)base.GetValue(OrientationProperty);
            }
            set
            {
                base.SetValue(OrientationProperty, value);
            }
        }

        public static readonly DependencyProperty WindowParentProperty = DependencyProperty.Register("WindowParent",
            typeof(BinComponentVisual), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(null));

        public BinComponentVisual WindowParent
        {
            get
            {
                return (BinComponentVisual)base.GetValue(WindowParentProperty);
            }
            set
            {
                base.SetValue(WindowParentProperty, value);
            }
        }
        #endregion

        public BinConnectorVisual(ConnectorModel model, BinComponentVisual component)
        {
            // TODO: Complete member initialization
            this.Model = model;
            this.WindowParent = component;
            InitializeComponent();
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;
            bin.FunctionColor = ColorHelper.GetColor(bin.Model.ConnectorType);
            bin.IsMandatory = bin.Model.IsMandatory;
            bin.IsOutgoing = bin.Model.Outgoing;
            bin.Model.Orientation = bin.Orientation;
        }

        public Point GetPosition()
        {
            try
            {
                if (!(Parent is ItemsControl))
                    throw new Exception("Parent is not ItemsControl");

                GeneralTransform gTransform, gTransformSec;
                Point point, relativePoint;
                ItemsControl ic = (ItemsControl)Parent;

                gTransform = ic.TransformToVisual(WindowParent);
                gTransformSec = this.TransformToVisual(ic);

                point = gTransform.Transform(new Point(0, 0));
                relativePoint = gTransformSec.Transform(new Point(0, 0));
                Point result = new Point(WindowParent.Position.X + point.X + relativePoint.X, WindowParent.Position.Y + point.Y + relativePoint.Y);
                return result;
            }
            catch (Exception)
            {
                return new Point(0, 0);
            }
        }

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }

        public void update()
        {
            throw new NotImplementedException();
        }
    }
}
