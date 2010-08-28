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

namespace Cryptool.Plugins.QuadraticSieve
{
    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    public partial class QuadraticSievePresentation : UserControl
    {
        private ProgressRelationPackages progressRelationPackages;
        public ProgressRelationPackages ProgressRelationPackages
        {
            get { return progressRelationPackages; }
        }

        public QuadraticSievePresentation()
        {
            InitializeComponent();

            progressRelationPackages = new ProgressRelationPackages(peer2peerScrollViewer);
            peer2peerScrollViewer.Content = progressRelationPackages;
            progressRelationPackages.MaxWidth = 620 - 30;
        }

        public void SelectFirstComposite()
        {
            foreach (String item in factorList.Items)
            {
                if (item.StartsWith("Composite"))
                {
                    factorList.SelectedItem = item;
                    factorList.ScrollIntoView(item);
                    return;
                }
            }
        }
    }
}
