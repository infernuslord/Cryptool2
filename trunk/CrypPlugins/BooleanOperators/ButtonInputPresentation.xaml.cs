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
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase;

namespace BooleanOperators
{
    /// <summary>
    /// Interaktionslogik für Button.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("BooleanOperators.Properties.Resources")]
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

        private void setButton()
        {
            if (Value)
            {
                this.myButton.Background = Brushes.LawnGreen;
                this.myButton.Content = Properties.Resources.True;
            }
            else
            {
                this.myButton.Background = Brushes.Tomato;
                this.myButton.Content = Properties.Resources.False;
            }
        }

        public void update() 
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setButton();
            }, null);
        }

        public void ExecuteThisMethodWhenButtonIsClicked(object sender, EventArgs e)
        {
            Value = !Value;
            setButton();

            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }
    }
}