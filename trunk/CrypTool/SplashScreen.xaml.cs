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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrypTool
{
    public partial class SplashScreen : Window
    {
        private int value = 0;
        
        public int Value
        {
            get { return this.value; }
            set
            {
                this.value = value; this.ProgressBar.Width = this.Width / 100 * this.value;
            }
        }
        public string Text
        {
            get { return this.Description.Text; }
            set
            {
                this.Description.Text = value;
            }
        }

        public SplashScreen()
        {
            InitializeComponent();
        }
    }
}
