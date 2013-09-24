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

namespace Sigaba
{
    /// <summary>
    /// Interaction logic for ORing2.xaml
    /// </summary>
    public partial class ORing2 : UserControl
    {   
        private int[] _oring;

        public ORing2(int[] oring)
        {
            _oring = oring;
            InitializeComponent();
            Line[] lineArray = new Line[10];
            for (int i = 0; i < LineCanvas.Children.Count; i++)
            {
                lineArray[i] = (Line)LineCanvas.Children[i];
            }
            Connections(lineArray);
        }

        private void Connections(Line[] lineArray)
        {

            for (int i = 0; i < lineArray.Length; i++)
            {
                //lineArray[i].Y1 = (SigabaConstants.ControlCipherRotors[Type][i]-65)*33 - i*33 + 16;
                lineArray[i].X1 = (_oring[i]) * 33 - i * 33 + 16 + 87;
            }



        }
    }
}
