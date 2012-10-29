/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DimCodeEncoder.model;
using ZXing;
using ZXing.Common;

namespace Cryptool.Plugins.DimCodeEncoder.DimCodes
{
    class QRCode : DimCode
    {
        #region legend Strings

        private readonly LegendItem present_ICV = new LegendItem
        {
            ColorBlack = Color.Blue,
            LableValue = "ICV_Lable",
            DiscValue = "ICV_Disc"
            
        };

        private readonly LegendItem present_opoints = new LegendItem
        {
            ColorBlack = Color.Green,
            LableValue = "fix_points_Lable",
            DiscValue = "fix_points_Disc"
        };

        #endregion

        public QRCode(DimCodeEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 300,
                    Width = 300
                }
            };

            var payload = Encoding.ASCII.GetString(input);

       /*     if (settings.AppendICV)
                payload = payload.Substring(0, 12); // cut of last byte to let the lib calculate the ICV
            */
            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            /*
            var inp = new byte[13];
            for (int i = 0; i < 13; i++)
            {
                if (input.Length > i )
                {
                    inp[i] = (input[i] != 0) ? input[i] : Encoding.ASCII.GetBytes("0")[0];
                }
                else
                {
                    inp[i] = Encoding.ASCII.GetBytes("0")[0];
                }
            }
            */
            return input;
        }

        protected override bool VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
           /* foreach (var b in input)
            {
                if (b < Encoding.ASCII.GetBytes("0")[0] || b > Encoding.ASCII.GetBytes("9")[0])
                {
                    caller.GuiLogMessage("OnlyDigits", NotificationLevel.Error);
                    return false;
                }
            }*/
            return true;
        }


        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            var legend = new List<LegendItem> {present_opoints};

            if (settings.AppendICV)
                legend.Add(present_ICV);

           return legend;
        }


        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
           var bitmap = new Bitmap(input);
           /*
          var barcount = 0;
          bool isOnBlackBar = false;
          for (int x = 0; x < bitmap.Width; x++)
          {
              if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
              {
                 if (!isOnBlackBar)
                      barcount++;
                  isOnBlackBar = true;

                  if (barcount <= 2 || barcount == 15 || barcount == 16  || barcount >= 29) 
                  {
                      bitmap = fillBarOnX(x, bitmap, present_opoints.ColorValue);
                  }
                  else if ((barcount == 27 || barcount == 28) && settings.AppendICV)
                  {
                      bitmap = fillBarOnX(x, bitmap, present_ICV.ColorValue);
                  }
              }
              else
              {
                  isOnBlackBar = false;
              }
          }
*/            return bitmap;
        }
    }
}
