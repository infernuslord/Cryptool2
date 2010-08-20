using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;


namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a image which can be put onto the workspace
    /// </summary>
    [Serializable]
    public class ImageModel : VisualElementModel
    {
        private byte[] data = null;
        private int height;
        private int width;
        private int stride;

        /// <summary>
        /// Get the BitmapImage represented by this ImageModel
        /// </summary>
        /// <returns></returns>
        public Image getImage()
        {
            Image image = new Image();            
            if (data == null)
            {
                return image;
            }

            image.Source = BitmapImage.Create(width,
                          height,
                          96,
                          96,
                          System.Windows.Media.PixelFormats.Bgr32,
                          null,
                          data,
                          stride);
            return image;
        }

        /// <summary>
        /// Instantiate a new ImageModel
        /// </summary>
        public ImageModel()           
        {

        }
        /// <summary>
        /// Instantiate a new ImageModel
        /// </summary>
        /// <param name="imageSource"></param>
        public ImageModel(Uri imgUri)
        {
            if (imgUri == null)
            {
                return;
            }

            BitmapImage bmpImage = new BitmapImage(imgUri) ;
            height = bmpImage.PixelHeight;
            width = bmpImage.PixelWidth;
            PixelFormat format = bmpImage.Format;
            stride = width * ((format.BitsPerPixel + 7) / 8);            
            byte[] byteImage = new byte[height * stride];
            bmpImage.CopyPixels(byteImage, stride, 0);

            this.data = byteImage;
        }
    }
}
