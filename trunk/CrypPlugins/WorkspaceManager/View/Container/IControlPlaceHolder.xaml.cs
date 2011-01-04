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
using WorkspaceManager.Model;
using Cryptool.PluginBase.Editor;
using WorkspaceManager.View.Converter;
using Cryptool.PluginBase;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for IControlPlaceHolder.xaml
    /// </summary>
    public partial class IControlPlaceHolder : UserControl
    {
        public PluginModel PluginModel
        {
            get { return (PluginModel)GetValue(PluginModelProperty); }
            set
            {
                SetValue(PluginModelProperty, value);
                bg.DataContext = PluginModel;
            }
        }

        public static readonly DependencyProperty PluginModelProperty =
           DependencyProperty.Register(
           "PluginModel",
           typeof(PluginModel),
           typeof(IControlPlaceHolder),
           null);

        public ConnectorModel Model
        {
            get { return (ConnectorModel)GetValue(ModelProperty); }
            set 
            { 
                SetValue(ModelProperty, value); 
            }
        }

        public static readonly DependencyProperty ModelProperty =
           DependencyProperty.Register(
           "Model",
           typeof(ConnectorModel),
           typeof(IControlPlaceHolder),
           null);

        public IControlPlaceHolder()
        {
            InitializeComponent();
            this.MouseEnter += new MouseEventHandler(IControlPlaceHolder_MouseEnter);
            this.MouseLeave += new MouseEventHandler(IControlPlaceHolder_MouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(IControlPlaceHolder_MouseLeftButtonDown);
            this.Loaded += new RoutedEventHandler(IControlPlaceHolder_Loaded);
            this.Drop +=new DragEventHandler(IControlPlaceHolder_Drop);
            bg.DataContext = PluginModel;
        }

        void IControlPlaceHolder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled && PluginModel != null)
            {
                PluginChangedEventArgs args = new PluginChangedEventArgs(PluginModel.Plugin, PluginModel.Name, DisplayPluginMode.Normal);
                this.Model.WorkspaceModel.WorkspaceManagerEditor.onSelectedPluginChanged(args);
            }
        }

        void IControlPlaceHolder_Loaded(object sender, RoutedEventArgs e)
        {
            bg.DataContext = PluginModel;
            this.ToolTip = Model.Name;
        }

        void IControlPlaceHolder_MouseLeave(object sender, MouseEventArgs e)
        {
            SolidColorBrush color = (SolidColorBrush)this.Resources["Color"];
            color.Color = Colors.Black;
        }

        void IControlPlaceHolder_MouseEnter(object sender, MouseEventArgs e)
        {
            SolidColorBrush color = (SolidColorBrush)this.Resources["Color"];
            color.Color = Colors.White;
        }

        void  IControlPlaceHolder_Drop(object sender, DragEventArgs e)
        {
 	        if (this.Model.WorkspaceModel.WorkspaceEditor.State == EditorState.READY)
            {
                if (e.Data.GetDataPresent("Cryptool.PluginBase.Editor.DragDropDataObject"))
                {
                    try
                    {
                        DragDropDataObject obj = e.Data.GetData("Cryptool.PluginBase.Editor.DragDropDataObject") as DragDropDataObject;
                        PluginModel pluginModel = this.Model.WorkspaceModel.newPluginModel(DragDropDataObjectToPluginConverter.CreatePluginInstance(obj.AssemblyFullName, obj.TypeFullName));
                        if (obj != null)
                        {
                            foreach (ConnectorModel mod in pluginModel.InputConnectors)
                            {
                                if (mod.IControl && mod.ConnectorType.Name == Model.ConnectorType.Name)
                                {
                                    this.PluginModel = pluginModel;
                                    break;
                                }
                            }
                        }

                        this.Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
                    }
                    catch (Exception ex)
                    {
                        this.Model.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("Could not add Plugin to Workspace:" + ex.Message, NotificationLevel.Error);
                        this.Model.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage(ex.StackTrace, NotificationLevel.Error);
                        return;
                    }
                }
                else
                    return;
            }

            e.Handled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Model.WorkspaceModel.deletePluginModel(PluginModel);
            PluginModel = null;
        }
    }
}
