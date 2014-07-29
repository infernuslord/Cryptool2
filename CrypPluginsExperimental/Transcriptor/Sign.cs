using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Transcriptor
{
    class Sign
    {
        int id, xCordinate, yCordinate;
        Rectangle rectangle;
        char letter;
        BitmapSource image;

        public Sign()
        {
        }

        public Sign(int SignId, char signLetter, BitmapSource signImage)
        {
            id = SignId;
            letter = signLetter;
            image = signImage;
        }

        public int Id
        {
            get { return id; }
            set { this.id = value ; }
        }

        public int X
        {
            get { return xCordinate; }
            set { this.xCordinate = value; }
        }

        public int Y
        {
            get { return yCordinate; }
            set { this.yCordinate = value; }
        }

        public Rectangle Rectangle
        {
            get { return rectangle; }
            set { this.rectangle = value; }
        }

        public char Letter
        {
            get { return letter; }
            set { this.letter = value; }
        }

        public BitmapSource Image
        {
            get { return image; }
            set { this.image = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Image, Letter);
        }

    }
}
