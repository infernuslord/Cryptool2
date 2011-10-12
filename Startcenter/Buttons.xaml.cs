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
using Cryptool.P2P;
using Cryptool.P2PEditor;
using Cryptool.PluginBase;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Buttons.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Buttons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public Buttons()
        {
            InitializeComponent();

            if (!P2PManager.IsP2PSupported)
            {
                DistCompButton.Visibility = Visibility.Collapsed;
                DistCompLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(Wizard.Wizard), null, null);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            OnlineHelp.InvokeShowPluginDocPage(null);
        }

        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(WorkspaceManager.WorkspaceManager), null, null);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(P2PEditor), null, null);
        }

        private void WebpageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cryptool2.vs.uni-due.de");
        }
    }
}
