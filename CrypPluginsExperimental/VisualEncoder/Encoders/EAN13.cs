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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.Plugins.VisualEncoder.Model;
using VisualEncoder.Properties;
using ZXing;
using ZXing.Common;

namespace Cryptool.Plugins.VisualEncoder.Encoders
{
    class EAN13 : DimCodeEncoder
    {
        #region legend Strings
        

        private readonly LegendItem icvLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue,
            LableValue = Resources.EAN13_ICV_LABLE,
            DiscValue = Resources.EAN13_ICV_DISC

        };

        private readonly LegendItem fixedLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.EAN13_FIXED_LABLE,
            DiscValue = Resources.EAN13_FIXED_DISC
        };


        #endregion

        public EAN13(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, VisualEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.EAN_13,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };

            var payload = Encoding.ASCII.GetString(input);

            if (settings.AppendICV)
                payload = payload.Substring(0, 12); // cut of last byte to let the lib calculate the ICV

            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, VisualEncoderSettings settings)
        {
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

            return inp;
        }

        protected override bool VerifyInput(byte[] input, VisualEncoderSettings settings)
        {
            if (input.Any(b => b < Encoding.ASCII.GetBytes("0")[0] || b > Encoding.ASCII.GetBytes("9")[0]))
            {
                caller.GuiLogMessage(Resources.EAN_INVALIDE_INPUT, NotificationLevel.Error);
                return false;
            }
            return true;
        }

        protected override List<LegendItem> GetLegend(byte[] input, VisualEncoderSettings settings)
        {
            var legend = new List<LegendItem> { fixedLegend };

            if (settings.AppendICV)
                legend.Add(icvLegend);

            return legend;
        }

        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);
            var barcount = 0;
            var isOnBlackBar = false;
            var barHight = 0;

            for (int x = 0; x < bitmap.Width; x++)
            { 
                if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = true;
                    }
                   
                    if(barHight == 0)
                        barHight = CalcBarHight(bitmap, x);
                }
                else
                {
                    if (isOnBlackBar)
                    {
                        barcount++;
                        isOnBlackBar = false;
                    }
                   
                }

                if ((barcount >= 1 && barcount <= 3) || (barcount >= 29 && barcount <= 31) || (barcount >= 57 && barcount <= 59)) 
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, fixedLegend.ColorBlack, fixedLegend.ColorWhite); 
                }
                else if ((barcount >= 53 && barcount <= 56) && settings.AppendICV)
                {
                    bitmap = FillBitmapOnX(x, 0, barHight, bitmap, icvLegend.ColorBlack, icvLegend.ColorWhite); 
                }
            }
            return bitmap;
        }
    }
}
