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

namespace BooleanOperators
{
    /// <summary>
    /// Interaktionslogik für Button.xaml
    /// </summary>
    public partial class ButtonInputPresentation : UserControl
    {

        public event EventHandler StatusChanged;



        public ButtonInputPresentation()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Current value of the button
        /// </summary>
        public Boolean Value { get; set; }
        
        public void update() 
        {
            try
            { 
                this.myButton.Content = Value;
                if (Value)
                { this.myButton.Background = Brushes.LawnGreen; }
                else
                { this.myButton.Background = Brushes.Tomato; }
            }
            catch { }
        }
        public void ExecuteThisMethodWhenButtonIsClicked(object sender, EventArgs e)
        {

            if (Value)
            {

                this.myButton.Background = Brushes.Tomato;
                this.myButton.Content = Value;
                Value = false;

            }

            else
            {
                this.myButton.Background = Brushes.LawnGreen;
                this.myButton.Content = Value;
                Value = true;
            }

            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }

        }
    }
}
