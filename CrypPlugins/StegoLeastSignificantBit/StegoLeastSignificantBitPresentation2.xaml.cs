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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class StegoLeastSignificantBitPresentation : UserControl
    {
        private StegoLeastSignificantBitDataContext dataContext = new StegoLeastSignificantBitDataContext();
        private StegoLeastSignificantBitDataContext resultDataContext = new StegoLeastSignificantBitDataContext();

        private Bitmap picture;

        public StegoLeastSignificantBitDataContext DataContext
        {
            get
            {
                return this.dataContext;
            }
            set
            {
                this.dataContext = value;
            }
        }

        public void UpdatePicture(Bitmap img)
        {
            picture = img;
            ShowPicture();
        }

        private void ShowPicture()
        {
            MemoryStream stream = new MemoryStream();
            BitmapImage bmpimg = new BitmapImage();
            picture.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;

            bmpimg.BeginInit();
            bmpimg.StreamSource = stream;
            bmpimg.EndInit();

            image1.Source = bmpimg;
        }

        public void AddPixel(System.Drawing.Point p)
        {
            this.dataContext.Pixels.Add(p);

            Graphics graphics = Graphics.FromImage(picture);

            foreach(System.Drawing.Point pixel in this.dataContext.Pixels)
            {
                graphics.FillEllipse(
                    new SolidBrush(System.Drawing.Color.Red), 
                    pixel.X-1, pixel.Y-1,
                    pixel.X+1, pixel.Y+1);
            }

            ShowPicture();            
        }

        public StegoLeastSignificantBitPresentation()
        {
            InitializeComponent();

            /*Binding pixelsBinding = new Binding();
            pixelsBinding.Source = this.dataContext.Pixels;
            pixelsBinding.Path = new PropertyPath("Text");
            pixelsBinding.Mode = BindingMode.OneWay;
            pixelsBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            */
            //txtResultList.DataContext = this.ResultList;
            //txtResultList.SetBinding(TextBox.TextProperty, resTextBinding);
        }
    }

    public class StegoLeastSignificantBitDataContext : INotifyPropertyChanged
    {
        public StegoLeastSignificantBitDataContext()
        {
            this.pixels = new Collection<System.Drawing.Point>();
        }

        private Bitmap image;
        public Bitmap Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
            }
        }

        private Collection<System.Drawing.Point> pixels;
        public Collection<System.Drawing.Point> Pixels
        {
            get
            {
                return pixels;
            }
            set
            {
                this.pixels.Clear();
                foreach (System.Drawing.Point p in value)
                {
                    this.pixels.Add(p);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        /*public string Text
        {
            get
            {
                if((list != null)&&(list.Count > 0))
                {
                    string text = ListItemToChar(0);
                    for (int n = 1; n < list.Count; n++)
                    {
                        text += "," + ListItemToChar(n);
                    }
                    return text;
                }
                else if (number > -1)
                {
                    return number.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }*/

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
