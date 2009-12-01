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
using Microsoft.Win32;

namespace SimpleEditor
{
    /// <summary>
    /// Interaction logic for UserCtrlInput.xaml
    /// </summary>
    public partial class UserCtrlInput : UserControl
    {
        public UserCtrlInput(string header, string toolTip)
        {
            InitializeComponent();
            mainGroupBox.Header = header;
            mainGroupBox.ToolTip = toolTip;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            textBoxString.IsEnabled = true;
            textBoxFile.IsEnabled = buttonFile.IsEnabled = false;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            textBoxFile.IsEnabled = buttonFile.IsEnabled = true;
            textBoxString.IsEnabled = false;
        }

        private void buttonFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == true)
            {
                textBoxFile.Text = openFile.FileName;
            }

            // TODO: output block
            //SaveFileDialog saveFile = new SaveFileDialog();
            //if (saveFile.ShowDialog() == true)
            //{
            //    textBoxFile.Text = saveFile.FileName;
            //}
        }
    }
}
