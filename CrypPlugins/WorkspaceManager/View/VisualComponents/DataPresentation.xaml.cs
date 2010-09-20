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
using WorkspaceManager.View.Container;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for DataPresentation.xaml
    /// </summary>
    public partial class DataPresentation : UserControl
    {
        public ConnectorView Connector { get; set; }

        public DataPresentation()
        {
            InitializeComponent();
        }

        public DataPresentation(ConnectorView connector)
        {
            setBaseControl(connector);
            InitializeComponent();
        }

        private void setBaseControl(ConnectorView connector)
        {
            this.Connector = connector;
            this.DataContext = connector;
        }

        public void update()
        {
            if(Connector.Model.HasData)
                Data.Text = Connector.Model.Data.ToString();

            return;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataPanel.Visibility == Visibility.Collapsed)
            {
                DataPanel.Visibility = Visibility.Visible;
                return;
            }

            if (DataPanel.Visibility == Visibility.Visible)
            {
                DataPanel.Visibility = Visibility.Collapsed;
                return;
            }

        }
    }
}
