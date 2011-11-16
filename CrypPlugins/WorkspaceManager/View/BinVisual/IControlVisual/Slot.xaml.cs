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
using System.Windows.Controls.Primitives;
using Cryptool.PluginBase;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManager.Model;
using System.Collections.ObjectModel;

namespace WorkspaceManager.View.BinVisual.IControlVisual
{
    /// <summary>
    /// Interaction logic for Slot.xaml
    /// </summary>
    public partial class Slot : UserControl
    {
        public WorkspaceManagerClass MyEditor { get; set; }

        private IControlMasterElement element;
        private bool loading;

        public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register("SelectedType",
            typeof(SlaveType), typeof(Slot), new FrameworkPropertyMetadata(null, OnSelectedTypeChanged));

        public SlaveType SelectedType
        {
            get
            {
                return (SlaveType)base.GetValue(SelectedTypeProperty);
            }
            set
            {
                base.SetValue(SelectedTypeProperty, value);
            }
        }

        public static readonly DependencyProperty ActiveModelProperty = DependencyProperty.Register("ActiveModel",
         typeof(PluginModel), typeof(Slot), new FrameworkPropertyMetadata(null, OnActiveModelChanged));

        public PluginModel ActiveModel
        {
            get
            {
                return (PluginModel)base.GetValue(ActiveModelProperty);
            }
            set
            {
                base.SetValue(ActiveModelProperty, value);
            }
        }

        public static readonly DependencyProperty TypesProperty = DependencyProperty.Register("Types",
            typeof(ObservableCollection<SlaveType>), typeof(Slot), new FrameworkPropertyMetadata(null));

        public ObservableCollection<SlaveType> Types
        {
            get
            {
                return (ObservableCollection<SlaveType>)base.GetValue(TypesProperty);
            }
            set
            {
                base.SetValue(TypesProperty, value);
            }
        }

        public Slot()
        {
            InitializeComponent();
        }

        public Slot(IControlMasterElement element)
        {
            loading = true;
            InitializeComponent();
            this.element = element;
            Types = new ObservableCollection<SlaveType>();
            MyEditor = (WorkspaceManagerClass)element.ConnectorModel.WorkspaceModel.MyEditor;
            if (ComponentInformations.PluginsWithSpecificController.ContainsKey(element.ConnectorModel.ConnectorType))
            {
                var list = ComponentInformations.PluginsWithSpecificController[element.ConnectorModel.ConnectorType];

                foreach (var e in list)
                    Types.Add(new SlaveType(e));

                var collection = element.ConnectorModel.GetOutputConnections();
                if (collection.Count != 0)
                {
                    ActiveModel = element.ConnectorModel.GetOutputConnections()[0].To.PluginModel;
                    SelectedType = Types.Single(a => a.Type.IsAssignableFrom(ActiveModel.PluginType));
                }
            }
            loading = false;
        }

        private static void OnActiveModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slot slot = (Slot)d;
            slot.element.PluginModel = slot.ActiveModel;
        }

        private static void OnSelectedTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slot slot = (Slot)d;
            SlaveType type;
            PluginChangedEventArgs args;

            ConnectorModel model = slot.element.ConnectorModel;
            if (slot.loading)
                return;
            
            if (e.OldValue != null)
            {
                type = e.NewValue as SlaveType;
                model.WorkspaceModel.ModifyModel(new DeletePluginModelOperation(slot.ActiveModel));
                args= new PluginChangedEventArgs(slot.ActiveModel.Plugin, slot.ActiveModel.GetName(), DisplayPluginMode.Normal);
                slot.MyEditor.onSelectedPluginChanged(args);
                slot.ActiveModel = null;
            }

            if (e.OldValue != null && e.NewValue != null)
            {
                if (((SlaveType)e.OldValue).Type.IsAssignableFrom(((SlaveType)e.NewValue).Type))
                    return;
            }

            if (e.NewValue == null) 
            {
                slot.ActiveModel = null;
                return;
            }
            

            type = e.NewValue as SlaveType;
            var v = (PluginModel)model.WorkspaceModel.ModifyModel(new NewPluginModelOperation(new Point(0, 0), (double)0, (double)0, type.Type));
            slot.ActiveModel = v;
            var f = v.GetInputConnectors().Single(a => a.ConnectorType.IsAssignableFrom(slot.element.ConnectorModel.ConnectorType));
            var m = (ConnectionModel)model.WorkspaceModel.ModifyModel(new NewConnectionModelOperation(model, f, f.ConnectorType));

            args = new PluginChangedEventArgs(slot.ActiveModel.Plugin, slot.ActiveModel.GetName(), DisplayPluginMode.Normal);
            slot.MyEditor.onSelectedPluginChanged(args);
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (ActiveModel != null)
            {
                PluginChangedEventArgs args = new PluginChangedEventArgs(ActiveModel.Plugin, ActiveModel.GetName(), DisplayPluginMode.Normal);
                MyEditor.onSelectedPluginChanged(args);
                e.Handled = true;
            }
            return;
        }

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            SelectedType = null;
        }


    }

    public class SlaveType
    {
        public Image Icon { get; set; }
        public Type Type { get; set; }


        public SlaveType(Type type)
        {
            Icon = type.GetImage(0);
            Type = type;
        }
    }

    public class CustomPopUp : Popup
    {
        private static double offset = -3.8;

        public static readonly DependencyProperty ItemPanelProperty = DependencyProperty.Register("ItemPanel",
            typeof(Panel), typeof(CustomPopUp), new FrameworkPropertyMetadata(null));

        public Panel ItemPanel
        {
            get
            {
                return (Panel)base.GetValue(ItemPanelProperty);
            }
            set
            {
                base.SetValue(ItemPanelProperty, value);
            }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index",
            typeof(int), typeof(CustomPopUp), new FrameworkPropertyMetadata(0));

        public int Index
        {
            get
            {
                return (int)base.GetValue(IndexProperty);
            }
            set
            {
                base.SetValue(IndexProperty, value);
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            int i = (int)Index;

            if (Index > ItemPanel.Children.Count - 1)
                throw new Exception("Index blub");

            Point point;
            if (Index <= -1)
                point = new Point(52, 52);
            else
                point = ItemPanel.TransformToVisual(ItemPanel.Children[i]).Transform(new Point(0, 0));
            HorizontalOffset = point.X + offset;
        }
    }
}
