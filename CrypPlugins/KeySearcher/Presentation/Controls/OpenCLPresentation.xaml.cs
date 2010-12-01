using System;
using System.Windows.Media;
using System.Collections.ObjectModel;
using KeySearcher;

namespace KeySearcherPresentation.Controls
{    
    public partial class OpenCLPresentation
    {
        public OpenCLPresentation()
        {
            InitializeComponent();
        }

        private int amountOfDevices;
        public int AmountOfDevices
        {
            get { return amountOfDevices; }
            set
            {
                amountOfDevices = value;
                devices.Content = amountOfDevices;
            }
        }
    }
}
