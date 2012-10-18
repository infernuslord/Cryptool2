using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DimCodeEncoder.model;

namespace DimCodeEncoder
{
    /// <summary>
    /// Interaktionslogik für DimCodeEncoderPresentation.xaml
    /// </summary>
    public partial class DimCodeEncoderPresentation : UserControl
    {
        public DimCodeEncoderPresentation()
        {
            InitializeComponent();
        }

        public void SetImage(byte[] data) //TODO change data format to do less converting
        {
            var c = new ImageSourceConverter();
            var image_s = (ImageSource)c.ConvertFrom(data);
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state => image.Source = image_s), image_s);
        }

        public void SetList(List<ColoredString> legend)
        {
            //TODO
        }

    }
}
