﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexBox
{
    public class StretchText : Control
    {

        public int[] mark = { 0, 0 };

        public Boolean removemarks;
        
        private double charwidth;

        public double CharWidth
        {
            get { return charwidth; }


        }

        private double textWidth;

        public double TextWidth
        {
            get { return textWidth; }


        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            String tempString = "";
            for (int i = 0; i < ByteContent.Count(); i++)
                if (ByteContent[i] < 128 && ByteContent[i] > 34)
                    tempString += (char) ByteContent[i];
                else
                    tempString += ".";

            //tempString = tempString.Replace(' ', '.');

            int f = ByteContent.Count() ;

            if (f > 16)
            {
                f = 16;
            }

            if (!removemarks)
            {

                if (mark[0] < mark[1])
                {

                    double y = (int)(mark[0] / 16) * 20;
                    double x = mark[0] % 16 * charwidth;
                    double z = mark[1] % 16 * charwidth - x + 2 * charwidth;

                    double z2 = 16 * charwidth - x - charwidth;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = (int)(mark[1] / 16) * 20;
                    double x1 = 0;
                    double z1 = mark[1] % 16 * charwidth + 2 * charwidth;

                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }

                    if (mark[0] % 16 > mark[1] % 16 || mark[1] - mark[0] > 16)
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z2, 20));
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x1, y1, z1, 20));
                        int v = (int)mark[1] / 16 - (int)mark[0] / 16;

                        for (int ix = 1; ix < v; ix++)
                        {
                            double y3 = y + ix * 20;
                            drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                         new Rect(0, y3, 16 * charwidth, 20));
                        }

                    }
                    else
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z, 20));
                    }

                }

                else
                {
                    double y = (int)(mark[0] / 16) * 20;
                    double x = mark[1] % 16 * charwidth;
                    double z = mark[0] % 16 * charwidth - x + 2 * charwidth;

                    double z2 = mark[0] % 16 * charwidth + 2 * charwidth;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = (int)(mark[1] / 16) * 20;
                    double x1 = 0;
                    double z1 = mark[1] % 16 * charwidth + 2 * charwidth;

                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }




                    if (mark[0] % 32 < mark[1] % 16 || mark[0] - mark[1] > 16)
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(0, y, z2, 20));
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x1, y1, z1, 20));
                        int v = (int)mark[0] / 16 - (int)mark[1] / 16;

                        for (int ix = 1; ix < v; ix++)
                        {
                            double y3 = y1 + ix * 20;
                            drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                         new Rect(0, y3, 47 * charwidth, 20));
                        }

                    }
                    else
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z, 20));
                    }
                }
            }

            for (int i = 0; i <= tempString.Length / 16; i++)
            {
                int max = 16;

                if (tempString.Length - i * 16 < 16)
                {
                    max = tempString.Length - i * 16;
                }

                FormattedText formattedText = new FormattedText(
                tempString.Substring(i * 16, max),
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                13,
                Brushes.Black, new NumberSubstitution(), TextFormattingMode.Ideal);

                formattedText.MaxTextWidth = 150;

                formattedText.Trimming = TextTrimming.None;

                if (textWidth<formattedText.WidthIncludingTrailingWhitespace)
                {
                    textWidth = formattedText.WidthIncludingTrailingWhitespace ;
                    charwidth = formattedText.WidthIncludingTrailingWhitespace/f;
                }

                Point p = new Point();

                int yi = i * 20;

                p = new Point(0, yi);

                drawingContext.DrawText(formattedText, p);

                


            }

        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public byte[] ByteContent
        {
            get { return (byte[]) GetValue(ByteProperty); }
            set { SetValue(ByteProperty, value); }
        }

        private static byte[] b = {};

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("ByteContent",
                                        typeof (byte[]),
                                        typeof (StretchText),
                                        new FrameworkPropertyMetadata(b, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof (string),
                                        typeof (StretchText),
                                        new FrameworkPropertyMetadata(string.Empty,
                                                                      FrameworkPropertyMetadataOptions.AffectsRender));
    }
}