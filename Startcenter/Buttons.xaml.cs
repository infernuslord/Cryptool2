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
using Cryptool.P2PEditor;
using Cryptool.PluginBase;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Buttons.xaml
    /// </summary>
    public partial class Buttons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public Buttons()
        {
            InitializeComponent();
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(Wizard.Wizard), null);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Soon...");
        }

        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(WorkspaceManager.WorkspaceManager), null);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(P2PEditor), null);
        }

        private void WebpageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cryptool2.vs.uni-due.de");
        }
    }
}
