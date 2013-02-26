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

namespace Cryptool.Plugins.PaddingOracle
{
    /// <summary>
    /// Interaction logic for OraclePresentation.xaml
    /// </summary>
    public partial class OraclePresentation : UserControl
    {
        public OraclePresentation()
        {
            InitializeComponent();
            this.Height=130;
            this.Width=293;
            this.padPointer.Visibility = Visibility.Hidden;

            this.viewByteScroller.Value = 1;
        }

        public void showPaddingImg(bool valid) 
        {
            if (valid)
            {
                this.padValid.Visibility = Visibility.Visible;
                this.padInvalid.Visibility = Visibility.Hidden;
            }
            else
            {
                this.padValid.Visibility = Visibility.Hidden;
                this.padInvalid.Visibility = Visibility.Visible;
            }
            
        }

        public void setPadPointer(int padLen, int viewMode)
        {
            int showLen = padLen;

            this.padPointer.Visibility = Visibility.Visible;

            System.Windows.Thickness marginThick = new System.Windows.Thickness(268 - 29 * showLen, 9, 0, 0);
            this.padPointer.Margin = marginThick;

            System.Windows.Thickness borderThick;

            if (viewMode == 0) //0 = all bytes
            {
                borderThick = new System.Windows.Thickness(2);
                this.padPointer.BorderThickness = borderThick;

                this.padPointer.Width = -1 + 29 * showLen;
            }
            else //1 = no bytes, 2 = mix
            {
                borderThick = new System.Windows.Thickness(2, 2, 1, 2);
                this.padPointer.BorderThickness = borderThick;

                this.padPointer.Width = 2 + 29 * showLen;
            }
        }
    }
}
