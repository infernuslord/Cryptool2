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

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    public partial class AssignmentPresentation : UserControl
    {
        // Delegates
        private UpdateOutputCiphertext updateOutput;
        private RestartSearch restartSearch;

        public AssignmentPresentation()
        {
            InitializeComponent();
        }

        public void RefreshUI(List<LetterPair> listOfPairs)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.ItemsSource = listOfPairs;
            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            updateOutput();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            restartSearch();
        }

        public void SetUpdateOutputCiphertext(UpdateOutputCiphertext method)
        {
            this.updateOutput = method;
        }

        public void SetRestartSearch(RestartSearch method)
        {
            this.restartSearch = method;
        }
    }
}
