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
using Cryptool.MD5.Algorithm;

namespace Cryptool.MD5.Presentation
{
    /// <summary>
    /// Interaktionslogik für PresentationContainer.xaml
    /// </summary>
    public partial class PresentationContainer : UserControl
    {
        private PresentableMD5 md5;

        public PresentationContainer(PresentableMD5 presentableMd5)
        {
            md5 = presentableMd5;
            DataContext = presentableMd5;

            InitializeComponent();
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStep();
        }

        private void previousStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.PreviousStep();
        }

        private void endOfRoundButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilRoundEnd();
        }

        private void endOfCompressionButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilBlockEnd();
        }
    }
}
