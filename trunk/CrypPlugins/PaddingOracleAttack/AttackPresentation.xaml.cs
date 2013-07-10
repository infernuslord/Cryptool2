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
using System.ComponentModel;
using PaddingOracleAttack.Properties;

namespace Cryptool.Plugins.PaddingOracleAttack
{
    /// <summary>
    /// Interaction logic for AttackPresentation.xaml
    /// </summary>

    [Cryptool.PluginBase.Attributes.Localization("PaddingOracleAttack.Properties.Resources")]
    public partial class AttackPresentation : UserControl
    {
        public AttackPresentation()
        {
            InitializeComponent();
            this.Width = 582;
            this.Height = 406;

            imgPhase[0] = phase1;
            imgPhase[1] = phase2;
            imgPhase[2] = phase3;

            this.viewByteScroller.Value = 1;
            //this.viewByteScroller.Minimum = 0.4;
        }

        public void padInput(bool valid)
        {
            //if valid, then show the "valid" image. if not, show the "invalid" image
            if (valid)
            {
                this.inPadValid.Visibility = System.Windows.Visibility.Visible;
                this.inPadInvalid.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.inPadValid.Visibility = System.Windows.Visibility.Hidden;
                this.inPadInvalid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void setBytePointer(int position, bool visible)
        {
            System.Windows.Visibility vis = System.Windows.Visibility.Hidden;
            if (visible) vis = System.Windows.Visibility.Visible;

            int pos;

            if (position < 0)
            {
                this.bytePointer.Width = 3;
                pos = 0;
            }
            else if (position > 7)
            {
                this.bytePointer.Width = 3;
                pos = 8;
            }
            else
            {
                this.bytePointer.Width = 23;
                pos = position;
            }



            System.Windows.Thickness thick = new System.Windows.Thickness(78 + 29 * pos, 9, 0, 0);

            

            this.bytePointer.Visibility = vis;
            this.bytePointer.Margin = thick;
        }

        Image[] imgPhase = new Image[3];
        public void setPhase(int phaseNum)
        {
            for (int imgCounter = 0; imgCounter < 3; imgCounter++)
            {
                imgPhase[imgCounter].Visibility = Visibility.Hidden;
            }

            imgPhase[phaseNum-1].Visibility = Visibility.Visible;
        }

        public void changeBorderColor(bool endOfPhase)
        {
            if (endOfPhase)
            {
                this.descBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                this.descBorder.BorderBrush = Brushes.Gray;
            }
        }
        /*
        private void Test_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            this.descShownBytes.Text = e.ToString() + ", " + sender.ToString();
            double value = this.viewByteScroller.Value;
            this.descShownBytes.Text = String.Format("{0:0.00}", value);
        }
        */
 

    }
}
