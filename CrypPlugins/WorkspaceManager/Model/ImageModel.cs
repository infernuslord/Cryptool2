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

        /// <summary>
        /// Get the Image stored by this ImageModel
        /// </summary>
        /// <returns></returns>
        public Image getImage()
        {
            Image image = new Image();            
            if (data == null)
            {
                return image;
            }

            MemoryStream stream = new MemoryStream(this.data);
            JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            BitmapFrame frame = decoder.Frames.First();
            image.Source = frame;            
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
        /// Loads the image from the imgUri and converts it into a jpeg
        /// Afterwards the data are stored in an internal byte array
        /// </summary>
        /// <param name="imageSource"></param>
        public ImageModel(Uri imgUri)
        {
            if (imgUri == null)
            {
                return;
            }
            Width = 0;
            Height = 0;
            BitmapImage bmpImage = new BitmapImage(imgUri) ;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpImage));
            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            this.data = stream.ToArray();
            stream.Close();
        }

        /// <summary>
        /// is the image enabled ?
        /// </summary>
        private bool isEnabled = true;
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }
    }
}
