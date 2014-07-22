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
        BitmapSource signImage;

        public Sign()
        {
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

        public BitmapSource SignImage
        {
            get { return signImage; }
            set { this.signImage = value; }
        }

    }
}
