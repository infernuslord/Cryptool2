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

namespace WebCamCap
{
    /// <summary>
    /// Interaktionslogik für WebCamCapPresentation.xaml
    /// </summary>
    public partial class WebCamCapPresentation : UserControl
    {
        public static readonly DependencyProperty SelectedWebcamMonikerStringProperty = DependencyProperty.Register("SelectedWebcamMonikerString", typeof (object), typeof (WebCamCapPresentation), new PropertyMetadata(default(object)));

        public WebCamCapPresentation()
        {
            InitializeComponent();
        }

        public object SelectedWebcamMonikerString
        {
            get { return (object) GetValue(SelectedWebcamMonikerStringProperty); }
            set { SetValue(SelectedWebcamMonikerStringProperty, value); }
        }
    }
}
