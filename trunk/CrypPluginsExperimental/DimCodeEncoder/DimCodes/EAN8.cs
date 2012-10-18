using System.Collections.Generic;
using System.Text;
using Cryptool.Plugins.DimCodeEncoder;
using DimCodeEncoder.model;
using ZXing;
using ZXing.Common;

namespace DimCodeEncoder.DimCodes
{
    class EAN8 : DimCode
    {

        protected override byte[] GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.EAN_8,
                Options = new EncodingOptions
                {
                    Height = 30,
                    Width = 300
                }
            };
            var bitmap = barcodeWriter.Write(Encoding.ASCII.GetString(input));

            return imageToByteArray(bitmap);
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
            return null;// todo ;)
        }


        protected override byte[] GeneratePresentationBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            return null;// todo ;)
        }

    }
}
