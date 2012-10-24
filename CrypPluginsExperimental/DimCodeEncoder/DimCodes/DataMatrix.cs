using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DataMatrix.net;
using DimCodeEncoder.model;


namespace Cryptool.Plugins.DimCodeEncoder.DimCodes
{
    class DataMatrix : DimCode
    {
      
        #region legend Strings

        private readonly LegendItem icvLegend = new LegendItem
        {
            ColorValue = Color.Blue,
            LableValue = "EAN8_ICV_LABLE",
            DiscValue = "EAN8_ICV_DISC"
            
        };

        private readonly LegendItem fixedLegend = new LegendItem
        {
            ColorValue = Color.Green,
            LableValue = "EAN8_FIXED_LABLE",
            DiscValue = "EAN8_FIXED_DISC"
        };

        #endregion

        public DataMatrix(DimCodeEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            DmtxImageEncoder encoder = new DmtxImageEncoder();
            var payload = Encoding.ASCII.GetString(input);
            return encoder.EncodeImage(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {

            var inp = new List<byte>(); // we do not know the exact size now
            foreach (byte t in input)
            {
                if (t != 0)
                    inp.Add(t);
                else //if it is 0, the cryptStream buffer was bigger than the user input, so we ignore the rest
                    return inp.ToArray();
            }
            return inp.ToArray();
        }

        protected override bool VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
            return true; //TODO
        }

        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            var legend = new List<LegendItem>();
            return legend; //TODO
        }

        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
            var bitmap = new Bitmap(input); //TODO
            return bitmap;
        }

        #region helper
        private Bitmap fillBarOnX(int x, Bitmap bitmap, Color color)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).R == Color.Black.R)
                    bitmap.SetPixel(x, y, color);
                else
                    y = bitmap.Height;
            }
            return bitmap;
        }

        #endregion
    }
}
