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

        #region secure setter

        public void SetImages(byte[] explaindImg, byte[] pureImg  )
        {
            ImageSource[] image_jar = new ImageSource[2];
            var c = new ImageSourceConverter();

            image_jar[0] = (ImageSource)c.ConvertFrom(explaindImg);
            image_jar[1] = (ImageSource)c.ConvertFrom(pureImg);

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>  
            {
                try
                {
                    ExplImage.Source = image_jar[0];
                    PureImage.Source = image_jar[1];
                   UpdateImage();
                }
                catch
                {
                    throw; //TODO REMOVE 
                }
            }), image_jar);

        }

        public void SetList(List<LegendItem> legend)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    legend1.Visibility = System.Windows.Visibility.Hidden;
                    legend2.Visibility = System.Windows.Visibility.Hidden;
                    legend3.Visibility = System.Windows.Visibility.Hidden;
                    legend4.Visibility = System.Windows.Visibility.Hidden;
                    legend5.Visibility = System.Windows.Visibility.Hidden;

                    if (legend.Count >= 1)
                    {
                        legend1.Visibility = System.Windows.Visibility.Visible;
                        lable1.Content = legend[0].LableValue;
                        disc1.Content = legend[0].DiscValue;
                        ellipse1.Fill = contvertColorToBrush(legend[0].ColorValue);
                    }
                   if (legend.Count >= 2) 
                    {
                        legend2.Visibility = System.Windows.Visibility.Visible;
                        lable2.Content = legend[1].LableValue;
                        disc2.Content = legend[1].DiscValue;
                        ellipse2.Fill = contvertColorToBrush(legend[1].ColorValue);
                    }
                   if (legend.Count >= 3)
                   {
                       legend3.Visibility = System.Windows.Visibility.Visible;
                       lable3.Content = legend[2].LableValue;
                       disc3.Content = legend[2].DiscValue;
                       ellipse3.Fill = contvertColorToBrush(legend[2].ColorValue);
                   }
                   if (legend.Count >= 4)
                   {
                       legend4.Visibility = System.Windows.Visibility.Visible;
                       lable4.Content = legend[3].LableValue;
                       disc4.Content = legend[3].DiscValue;
                       ellipse4.Fill = contvertColorToBrush(legend[3].ColorValue);
                   }
                   if (legend.Count >= 5)
                   {
                       legend5.Visibility = System.Windows.Visibility.Visible;
                       lable5.Content = legend[4].LableValue;
                       disc5.Content = legend[4].DiscValue;
                       ellipse5.Fill = contvertColorToBrush(legend[4].ColorValue);
                   }
                 }
                catch
                {
                    throw; //TODO REMOVE 
                }
            }), legend);
        }

        #endregion

        #region helper

        private SolidColorBrush contvertColorToBrush(System.Drawing.Color ColorValue)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(ColorValue.A,
                                                                           ColorValue.R,
                                                                           ColorValue.G,
                                                                           ColorValue.B));
        }

        private void Explain_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            panel.Width = 365;
            UpdateImage();
        }

        private void Explain_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            panel.Width = 565;
            UpdateImage();
        }

        private void UpdateImage()
        {
            Image.Source = !Explain.IsExpanded ? ExplImage.Source : PureImage.Source;
            Image.Width = !Explain.IsExpanded ? ExplImage.Width : PureImage.Width;
            Image.Height = !Explain.IsExpanded ? ExplImage.Height : PureImage.Height;
        }
        #endregion 
    }
}
