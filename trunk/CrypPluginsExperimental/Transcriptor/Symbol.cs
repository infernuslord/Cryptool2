/*
   Copyright 2014 Olga Groh

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Transcriptor
{
    class Symbol
    {
        int id;
        double xCordinate, yCordinate;
        double probability = 100;
        Rectangle rectangle;
        char letter;
        BitmapSource image;

        # region Constructor
        public Symbol(int symbolId, char symbolLetter, BitmapSource symbolImage)
        {
            id = symbolId;
            letter = symbolLetter;
            image = symbolImage;
        }
        #endregion

        # region Get/Set
        public int Id
        {
            get { return id; }
            set { this.id = value ; }
        }

        public double Probability
        {
            get { return probability; }
            set { this.probability = value; }
        }

        public double X
        {
            get { return xCordinate; }
            set { this.xCordinate = value; }
        }

        public double Y
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
        #endregion

        public override string ToString()
        {
            return string.Format("{0}", Letter);
        }
    }
}
