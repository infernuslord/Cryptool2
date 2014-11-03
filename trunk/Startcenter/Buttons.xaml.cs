 
using System.Windows;
using System.Windows.Controls; 
using Cryptool.CrypTutorials;  
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

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

                DistCompButton.Visibility = Visibility.Collapsed;
                DistCompLabel.Visibility = Visibility.Collapsed; 
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(Wizard.Wizard), null);
        }

        private void CrypTutorialsButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(CrypTutorials), null);
        }
            
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            OnlineHelp.InvokeShowDocPage(null);
        }

        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            OnOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass), null);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        { 
        }

        private void WebpageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cryptool2.vs.uni-due.de");
        }

        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.facebook.de/cryptool20");
        }
    }
}
