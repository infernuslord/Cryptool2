using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using DimCodeEncoder.model;
using ZXing;
using ZXing.Common;

namespace Cryptool.Plugins.DimCodeEncoder.DimCodes
{
    class Code39 : DimCode
    {
        #region legend Strings

        private readonly LegendItem startEndLegend = new LegendItem
        {
            ColorValue = Color.Green,
            LableValue = "C38_STARTEND_LABLE", // TODO
            DiscValue = "C38_STARTEND_DISC" //TODO
            
        };

        private readonly LegendItem ivcLegend = new LegendItem
        {
            ColorValue = Color.Blue,
            LableValue = "C38_ICV_Lable", //TODO
            DiscValue = "some blahblah 'bout icv's calc" //TODO
        };

        #endregion

        public Code39(DimCodeEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_39,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300// it automaticly gets bigger 
                    
                }
            };

            var payload = Encoding.ASCII.GetString(input);
            if(settings.AppendICV)
            {
                if(payload.Length == 80) // replace last digit with icv
                    payload = payload.Substring(0, 79) + calcICV(payload.Substring(0, 79));

                else // simply append
                    payload += calcICV(payload);
            }

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
            if (Encoding.ASCII.GetChars(input).Any(c => Code39CharToInt(c)  == -1))
            {
                caller.GuiLogMessage("CODE39_INVALIDE_INPUT", NotificationLevel.Error); // TODO
                return false;
            }
            return true;
        }

        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            return new List<LegendItem> { startEndLegend, ivcLegend };
        }

        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);
           
            var barcount = 0;
            bool isOnBlackBar = false;

            #region color startvalue

            for (int x = 0; barcount <= 5; x++)
            {
              if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
              {
                 if (!isOnBlackBar)
                      barcount++;
                  isOnBlackBar = true;

                  bitmap = fillBarOnX(x, bitmap, startEndLegend.ColorValue);
                   
              }
              else
              {
                  isOnBlackBar = false;
              }
            }

            #endregion
           
            #region color endregion and checksum

            barcount = 0;
            isOnBlackBar = false;

            for (int x = bitmap.Width-1 ; barcount <= 10; x--)
            {
                if (bitmap.GetPixel(x, bitmap.Height / 2).R == Color.Black.R)
                {
                    if (!isOnBlackBar)
                        barcount++;

                    isOnBlackBar = true;

                    if (barcount <= 5)
                        bitmap = fillBarOnX(x, bitmap, startEndLegend.ColorValue);

                    else if (settings.AppendICV)
                        bitmap = fillBarOnX(x, bitmap, ivcLegend.ColorValue);
                }
                else
                {
                    isOnBlackBar = false;
                }
            }
            
            #endregion
          
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

        /// <summary>
        /// calculation of icv: add all char with his corresponding Code39 value in module 43
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string calcICV(string p)
        {
            return "" + Code39IntToChar(p.Sum(c => Code39CharToInt(c)) % 43);
        }

   

        /// <summary>
        /// get the code39 encoding value  0 = 0, ... , 9 = 9, A = 10, ... , Z = 35 ...
        /// </summary>
        /// <param name="c"></param>
        /// <returns> the value or -1 if the char has no representation in the CODE38 coding</returns>
        private static int Code39CharToInt(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0'; //char c is 0-9 return integer 0-9
            if (c >= 'A' && c <= 'Z')
                return c - 'A' + 10; //A = 10 ... Z = 35

            switch (c)
            {
                case '-':
                    return 36;
                case '.':
                    return 37;
                case ' ':
                    return 38;
                case '$':
                    return 39;
                case '/':
                    return 40;
                case '+':
                    return 41;
                case '%':
                    return 42;
            }

            return -1;
            //throw new Exception("Code39CharToInt: Invalide char got: '" + c + "'. fix your input validation!");
        }

        /// <summary>
        /// get the code39 encoding value  0 = 0, ... , 9 = 9, 10 = 'A', ... , 35 = 'Z' ...
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static char Code39IntToChar(int i)
        {
            if (i >= 0 && i <= 9)
                return (char)('0' + i);
            if (i >= 10 && i <= 35)
                return (char)('A' + i-10); 

            switch (i)
            {
                case 36:
                    return '-';
                case 37:
                    return '.';
                case 38:
                    return ' ';
                case 39:
                    return '$';
                case 40:
                    return '/';
                case 41:
                    return '+';
                case 42:
                    return '%';
            }

            throw new Exception("code39intToChar: Invalide int got: '" + i + "'. fix your input validation!");
        }
#endregion
    } 
}
