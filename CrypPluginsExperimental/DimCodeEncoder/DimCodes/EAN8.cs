using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Cryptool.Plugins.DimCodeEncoder;
using DimCodeEncoder.model;
using ZXing;
using ZXing.Common;

namespace DimCodeEncoder.DimCodes
{
    class EAN8 : DimCode
    {
        #region legend Strings

        private readonly ColoredString present_ICV = new ColoredString
        {
            ColorValue = Color.Blue,
            StringValue = "present_ICV"
        };

        private readonly ColoredString present_opoints = new ColoredString
        {
            ColorValue = Color.Green,
            StringValue = "present_opoints"
        };

        #endregion

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.EAN_8,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };

            var payload = Encoding.ASCII.GetString(input);

            if (settings.AppendICV)
                payload = payload.Substring(0, 7); // cut of last byte to let the lib calculate the ICV

            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            var inp = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                inp[i] = Encoding.ASCII.GetBytes("0")[0];
            }

            if (input.Length <= 8)
            {
                input.CopyTo(inp, 0);
            }            
            
            if (input.Length > 8)
            {
                for (var i = 0; i < 8; i++)
                {
                    inp[i] = input[i];
                }
            }
            return inp;
        
        }

        protected override string VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
           return null;// todo ;)
        }


        protected override List<ColoredString> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            var legend = new List<ColoredString> {present_opoints};

            if (settings.AppendICV)
                legend.Add(present_ICV);

            return legend;
        }


        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);
            var barcount = 0;
            bool isOnBlackBar = false;
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
                {
                   if (!isOnBlackBar)
                        barcount++;
                    isOnBlackBar = true;

                    if (barcount <= 2 || barcount == 11 || barcount == 12  || barcount >= 21) 
                    {
                        bitmap = fillBarOnX(x, bitmap, present_opoints.ColorValue);
                    }
                    else if ((barcount == 19 || barcount == 20) && settings.AppendICV)
                    {
                        bitmap = fillBarOnX(x, bitmap, present_ICV.ColorValue);
                    }
                }
                else
                {
                    isOnBlackBar = false;
                }
            


            }
            return bitmap;
        }


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
    }
}
