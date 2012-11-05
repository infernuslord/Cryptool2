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
    class QRCode : DimCode
    {
        #region legend Strings

        private readonly LegendItem alignmentLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            LableValue = Resources.QR_ALIG_LABLE,
            DiscValue = Resources.QR_ALIG_DISC,
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
                    Margin = 1,
                    Height = 300,
                    Width = 300
                }
            };
            var payload = Encoding.ASCII.GetString(input);
            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            return input;
        }

        protected override bool VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
            return true;
        }


        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            var legend = new List<LegendItem> { alignmentLegend };

           
           return legend;
        }


        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
           var bitmap = new Bitmap(input);
           return bitmap;
        }
    }
}
