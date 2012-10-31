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
using Cryptool.Plugins.DimCodeEncoder.Model;
using DimCodeEncoder.Properties;
using ZXing;
using ZXing.Common;

namespace Cryptool.Plugins.DimCodeEncoder.DimCodes
{
    class Code128 : DimCode
    {
        #region legend Strings

        private readonly LegendItem startEndLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.C128_STARTEND_LABLE,
            DiscValue = Resources.C128_STARTEND_DISC

        };

        private readonly LegendItem ivcLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.C128_ICV_LABLE,
            DiscValue = Resources.C128_ICV_DISC
        };

        #endregion

        public Code128(DimCodeEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };
            var payload = Encoding.ASCII.GetString(input);
            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            var inp = new List<byte>(); // we do not know the exact size now
            for (int i = 0; i < 80; i++) // but 80 is the uppersize
            {
                if (input.Length > i)
                {
                    if (input[i] != 0)
                    {
                        inp.Add(input[i]);
                    }
                    else
                    {   //if it is 0, the cryptStream buffer was bigger than the user input, so we ignore the rest
                        return inp.ToArray();
                    }
                }
            }
            return inp.ToArray();
        }

        protected override bool VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
          
            return true;
        }


        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            return new List<LegendItem> {startEndLegend, ivcLegend};
        }


        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);
            var barSpaceCount = 0;
            var isOnBlackBar = false;
            var barHight = 0;

            #region left 6 bars
            for (int x = 0; barSpaceCount <= 6; x++)
            { 
                if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = true;
                    }
                   
                    if(barHight == 0)
                    {
                        barHight = CalcBarHight(bitmap, x);
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = false;
                    }
                   
                }

                if(barSpaceCount > 0)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, startEndLegend.ColorBlack, startEndLegend.ColorWhite); 
                }
            }
            #endregion
            barSpaceCount = 0;
            isOnBlackBar = false;
            #region right bars
            for (int x = bitmap.Width-1; barSpaceCount <= 13; x--)
            {
                if (bitmap.GetPixel(x, bitmap.Height / 2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = true;
                    }
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barSpaceCount++;
                        isOnBlackBar = false;
                    }

                }

                if (barSpaceCount > 0 && barSpaceCount <=7)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, startEndLegend.ColorBlack, startEndLegend.ColorWhite);
                }
                else if (barSpaceCount > 7)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, ivcLegend.ColorBlack, ivcLegend.ColorWhite);
                }
            #endregion
            }
            return bitmap;
        }
      
    }
}
