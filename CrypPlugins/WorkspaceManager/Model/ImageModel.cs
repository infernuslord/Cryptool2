using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a image which can be put onto the workspace
    /// </summary>
    public class ImageModel : VisualElementModel
    {
        private byte[] data = null;

        /// <summary>
        /// Get the BitmapImage represented by this ImageModel
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public BitmapImage getImage(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        /// <summary>
        /// Instantiate a new ImageModel
        /// </summary>
        /// <param name="imageSource"></param>
        public ImageModel(Uri imgUri)
        {
            BitmapImage img = new BitmapImage(imgUri);
            Stream stream = img.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            this.data = buffer;
        }
    }
}
