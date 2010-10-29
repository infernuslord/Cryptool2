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

namespace CrypUpdater
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        internal bool restartFailed = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            button1.IsEnabled = false;

            if (!restartFailed)
            {
                if (App.CryptoolExePath != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(App.CryptoolExePath);
                        Application.Current.Shutdown();
                    }
                    catch (Exception)
                    {
                        restartFailed = true;
                        textBlock1.Text = "Error occurred while restarting CT2! Try again later.";
                        button1.IsEnabled = true;
                    }
                }
                
            }
            else
            {
                Application.Current.Shutdown();
            }

        }

    }
}
