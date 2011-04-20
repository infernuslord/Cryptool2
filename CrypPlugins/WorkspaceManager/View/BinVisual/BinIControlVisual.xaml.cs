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
using Cryptool.PluginBase.Editor;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base;
using Cryptool.PluginBase;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinIControlVisual.xaml
    /// </summary>
    public partial class BinIControlVisual : Popup
    {
        #region Properties

        //public WorkspaceManager MyEditor { get { return (WorkspaceManager)this.Model.WorkspaceModel.MyEditor; } }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty IControlListProperty = DependencyProperty.Register("IControlList",
            typeof(IEnumerable<ConnectorModelWrapper>), typeof(BinIControlVisual), new FrameworkPropertyMetadata(null, null));

        public IEnumerable<ConnectorModelWrapper> IControlList
        {
            get
            {
                return (IEnumerable<ConnectorModelWrapper>)base.GetValue(IControlListProperty);
            }
            set
            {
                base.SetValue(IControlListProperty, value);
            }
        }
        #endregion

        #region Constructors
        public BinIControlVisual()
        {
            InitializeComponent();
        } 
        #endregion

        #region public

        #endregion

        #region Event Handler

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            //if (Model == null)
            //    return;

            //PluginChangedEventArgs args = new PluginChangedEventArgs(Model.Plugin, Model.GetName(), DisplayPluginMode.Normal);
            //MyEditor.onSelectedPluginChanged(args);
            //e.Handled = true;
        }

        private void DropHandler(object sender, DragEventArgs e)
        {

        } 
        #endregion
    }

    #region Converter

    public class IControlListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ConnectorModel m = (ConnectorModel)value;

            var l = Util.GetICSlaves(m.ConnectorType.FullName);
            List<IPluginWrapper> returnValue = new List<IPluginWrapper>();

            foreach (var e in l)
            {
                returnValue.Add(new IPluginWrapper(e));
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion
}
