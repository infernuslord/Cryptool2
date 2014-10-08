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
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Emgu.CV;
using Emgu.CV.Structure;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using System.Collections;
using System.Runtime.CompilerServices;

namespace Cryptool.Plugins.ImageHash
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "", "")]
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("ImageHash", "Calculate the robust hash of an image", "ImageHash/userdoc.xml", new[] { "ImageHash/icon.png"/*"CrypWin/images/default.png"*/ })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class ImageHash : ICrypComponent
    {

        #region Private Variables

        private readonly ImageHashSettings settings;
        private ICryptoolStream inputImage;
        private Image<Bgr, Byte> orgImg;
        private Image<Gray, double> step1Img;
        private Image<Gray, double> step2Img;
        private Image<Gray, double> step4Img;
        private Bitmap step6Bmp;
        private Bitmap step2Bmp;
        private byte[] outputHash;

        #endregion

        #region Data Properties

        /// <summary>
        /// Input image ICryptoolStream, handles "inputImage".
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImage", "This is the standard image used for the hashing.")]
        public ICryptoolStream InputImage
        {
            get
            {
                return inputImage;
            }
            set
            {
                if (value != inputImage)
                {
                    inputImage = value;
                }
            }
        }

        /// <summary>
        /// Output hash byte[] as ICryptoolStream.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputHashStreamCaption", "OutputHashStreamTooltip", true)]
        public ICryptoolStream OutputHashStream
        {
            get
            {
                if (outputHash != null)
                {
                    return new CStreamWriter(outputHash);
                }
                return null;
            }
            set { } // read-only
        }

        /// <summary>
        /// Output hash byte[].
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputHashCaption", "OutputHashTooltip", true)]
        public byte[] OutputHash
        {
            get { return this.outputHash; }
            set
            {
                if (outputHash != value)
                {
                    this.outputHash = value;
                    OnPropertyChanged("OutputHash");
                    OnPropertyChanged("OutputHashStream");
                }
            }
        }

        /// <summary>
        /// Output original unprocessed image as ICryptoolStream.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputOriginalImage", "This is the original unprocessed Image.")]
        public ICryptoolStream OutputOriginalImage
        {
            get;
            set;
        }

        /// <summary>
        /// Output processed image as ICryptoolStream.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputImage", "This is the OutputImage after processing.")]
        public ICryptoolStream OutputImage
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members and Execution

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Constructor. Called once when class is called.
        /// </summary>
        public ImageHash()
        {
            settings = new ImageHashSettings();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            int progress = 1;
            const int STEPS = 11;
            // An imagesize under 4x4 does not make any sense
            if (settings.Size < 4)
            {
                settings.Size = 4;
                GuiLogMessage("Changed size to 4x4.", NotificationLevel.Info);
            }
            OnPropertyChanged("size");
            ProgressChanged(0, 1);

            if (InputImage == null)
            {
                GuiLogMessage("Please select an image.", NotificationLevel.Error);
                return;
            }

            using (CStreamReader reader = inputImage.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    // Original Image:
                    orgImg = new Image<Bgr, Byte>(bitmap);
                    CreateOutputStream(bitmap, 1);
                    OnPropertyChanged("OutputOriginalImage");

                    // Step 1: Gray scale
                    step1Img = orgImg.Convert<Gray, byte>().Convert<Gray, double>();
                    ProgressChanged(progress++, STEPS);
                    CreateOutputStream(step1Img.ToBitmap());
                    OnPropertyChanged("OutputImage");
                    ProgressChanged(progress++, STEPS);
                    
                    // Step 2: Resize to sizexsize (16x16)
                    int size = settings.Size;   // usually 16
                    int halfSize = size / 2;    // usually 8
                    step2Img = step1Img.Resize(size, size, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
                    step2Bmp = ResizeBitmap(step1Img.ToBitmap(), size, size);
                    ProgressChanged(progress++, STEPS);
                    CreateOutputStream(step2Img.ToBitmap());
                    OnPropertyChanged("OutputImage");
                    ProgressChanged(progress++, STEPS);

                    // Step 3: Find brightest quarter
                    float[] subImage = new float[4];
                    for (int i = 0; i < step2Img.Width; i++)
                    {
                        for (int j = 0; j < step2Img.Height; j++)
                        {
                            if ((i < halfSize) && (j < halfSize))
                            {
                                subImage[0] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i >= halfSize) && (j < halfSize))
                            {
                                subImage[1] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i < halfSize) && (j >= halfSize))
                            {
                                subImage[2] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                            else if ((i >= halfSize) && (j >= halfSize))
                            {
                                subImage[3] += step2Img.ToBitmap().GetPixel(i, j).GetBrightness();
                            }
                        }
                    }

                    float maxValue = subImage.Max();
                    int flip = subImage.ToList().IndexOf(maxValue);
                    ProgressChanged(progress++, STEPS);

                    // Step 4: Flip brightest quarter to left upper corner
                    step4Img = step2Img;
                    switch (flip)
                    {
                        case 1:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
                            subImage = swap(subImage, 1);
                            break;
                        case 2:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                            subImage = swap(subImage, 2);
                            break;
                        case 3:
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
                            step4Img = step4Img.Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                            subImage = swap(subImage, 3);
                            break;
                    }

                    ProgressChanged(progress++, STEPS);
                    CreateOutputStream(step4Img.ToBitmap());
                    OnPropertyChanged("OutputImage");
                    ProgressChanged(progress++, STEPS);

                    // Step 5: Find median
                    float[] median = new float[4];
                    for (int i = 0; i < median.Length; i++)
                    {
                        median[i] = subImage[i] / ((size*size)/median.Length);
                    }
                    ProgressChanged(progress++, STEPS);

                    // Step 6: Set Brightness to 0 or 1, if above or under median
                    Bitmap step4Bmp = step4Img.ToBitmap();
                    step6Bmp = new Bitmap(step4Img.Width, step4Img.Height);
                    GuiLogMessage("step6 size: " + step6Bmp.Width + "x" + step6Bmp.Height, NotificationLevel.Debug);
                    Boolean[][] b = new Boolean[4][];
                    for (int i=0; i<b.Length; i++)
                        b[i] = new Boolean[(size * size) / median.Length];
                    GuiLogMessage("b[0].length: " + b[0].Length, NotificationLevel.Debug);
                    for (int i = 0; i < step4Bmp.Width; i++)
                    {
                        for (int j = 0; j < step4Bmp.Height; j++)
                        {
                            int index;
                            if ((i < halfSize) && (j < halfSize))
                            {
                                index = i * halfSize + j;
                                GuiLogMessage("b[0][" + index + "]", NotificationLevel.Debug);
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[0])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[0][i * halfSize + j] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[0][i * halfSize + j] = true;
                                }
                            }
                            else if ((i >= halfSize) && (j < halfSize))
                            {
                                index = (i - halfSize) * halfSize + j;
                                //GuiLogMessage("b[1][" + index + "]", NotificationLevel.Debug);
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[1])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[1][(i - halfSize) * halfSize + j] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[1][(i - halfSize) * halfSize + j] = true;
                                }
                            }
                            else if ((i < halfSize) && (j >= halfSize))
                            {
                                index = i * halfSize + (j - halfSize);
                                //GuiLogMessage("b[2][" + index + "]", NotificationLevel.Debug);
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[2])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[2][i * halfSize + (j - halfSize)] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[2][i * halfSize + (j - halfSize)] = true;
                                }
                            }
                            else if ((i >= halfSize) && (j >= halfSize))
                            {
                                index = (i - halfSize) * halfSize + (j - halfSize);
                                //GuiLogMessage("b[3][" + index + "]", NotificationLevel.Debug);
                                float brightness = step4Bmp.GetPixel(i, j).GetBrightness();
                                if (brightness >= median[3])
                                {
                                    step6Bmp.SetPixel(i, j, Color.White);
                                    b[3][(i - halfSize) * halfSize + (j - halfSize)] = false;
                                }
                                else
                                {
                                    step6Bmp.SetPixel(i, j, Color.Black);
                                    b[3][(i - halfSize) * halfSize + (j - halfSize)] = true;
                                }
                            }
                        }
                    }

                    ProgressChanged(progress++, STEPS);
                    CreateOutputStream(step6Bmp);
                    OnPropertyChanged("OutputImage");
                    ProgressChanged(progress++, STEPS);

                    // Step 7: Calculate the hash
                    bool[] bools = new bool[b.Length * b[0].Length];
                    for (int i = 0; i < b.Length; i++)
                    {
                        for (int j = 0; j < b[i].Length; j++)
                        {
                            bools[i*b[i].Length+j] = b[i][j];
                        }
                    }

                    byte[] byteArray = new byte[bools.Length / 8];
                    int bitIndex = 0, byteIndex = 0;
                    for (int i = 0; i < bools.Length; i++)
                    {
                        if (bools[i])
                        {
                            byteArray[byteIndex] |= (byte)(((byte)1) << bitIndex);
                        }
                        bitIndex++;
                        if (bitIndex == 8)
                        {
                            bitIndex = 0;
                            byteIndex++;
                        }
                    }
                    OutputHash = byteArray;
                    OnPropertyChanged("OutputHash");
                }
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Listener that checks which property changed and acts accordingly.
        /// </summary>
        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PresentationStep":
                    switch (settings.PresentationStep)
                    {
                        case 1:
                            CreateOutputStream(orgImg.ToBitmap());
                            break;
                        case 2:
                            CreateOutputStream(step1Img.ToBitmap());
                            break;
                        case 3:
                            CreateOutputStream(step2Img.ToBitmap());
                            //CreateOutputStream(step2Bmp);
                            break;
                        case 4:
                            CreateOutputStream(step4Img.ToBitmap());
                            break;
                        case 5:
                            CreateOutputStream(step6Bmp);
                            break;
                    }
                            OnPropertyChanged("OutputImage");
                    break;
            }
        }

        /// <summary>
        /// Swaps indices of subImage horizontally, vertically or both.
        /// </summary>
        /// <param name="subImage">The float array to swap.</param>
        /// <param name="i">Integer how to swap (1=horizontal) (2=vertical) (3=both).</param>
        private float[] swap(float[] subImage, int i)
        {
            switch (i)
            {
                case 1:
                    subImage = swap(subImage, 0, 1);
                    subImage = swap(subImage, 2, 3);
                    break;
                case 2:
                    subImage = swap(subImage, 0, 2);
                    subImage = swap(subImage, 1, 3);
                    break;
                case 3:
                    subImage = swap(subImage, 1);
                    subImage = swap(subImage, 2);
                    break;
            }

            return subImage;
        }

        /// <summary>
        /// Swaps indices i and j in a float array.
        /// </summary>
        /// <param name="subImage">The float array to swap.</param>
        /// <param name="i">First index.</param>
        /// <param name="j">Second index.</param>
        private float[] swap(float[] subImage, int i, int j)
        {
            float f = subImage[i];
            subImage[i] = subImage[j];
            subImage[j] = f;
            return subImage;
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        
        #region HelpFunctions

        /// <summary>
        /// Resizes bitmap b to width nWidth and height nHeight.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <param name="nWidth">The new width.</param>
        /// <param name="nHeight">The new height.</param>
        private Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            }
            return result;
        }

        private void CreateOutputStream(Bitmap bitmap)
        {
            CreateOutputStream(bitmap, 2);
        }

        /// <summary>Create output stream to display.</summary>
        /// <param name="bitmap">The bitmap to display.</param>
        private void CreateOutputStream(Bitmap bitmap, int i)
        {
            Bitmap newBitmap;
            if (bitmap.HorizontalResolution < 100)
            {
                newBitmap = ResizeBitmap(bitmap, 320, 320);
            }
            else
            {
                newBitmap = bitmap;
            }
            ImageFormat format = ImageFormat.Bmp;
            if (settings.OutputFileFormat == 1)
            {
                format = ImageFormat.Png;
            }
            else if (settings.OutputFileFormat == 2)
            {
                format = ImageFormat.Tiff;
            }

            //avoid "generic error in GDI+"
            Bitmap saveableBitmap = CopyBitmap(newBitmap, format);

            //save bitmap
            MemoryStream outputStream = new MemoryStream();
            saveableBitmap.Save(outputStream, format);
            saveableBitmap.Dispose();
            newBitmap.Dispose();

            switch (i)
            {
                case 1:
                    OutputOriginalImage = new CStreamWriter(outputStream.GetBuffer());
                    break;
                case 2:
                    OutputImage = new CStreamWriter(outputStream.GetBuffer());
                    break;
            }
            
        }

        /// <summary>Makes sure that a bitmap is not a useless "MemoryBitmap".</summary>
        /// <param name="bitmap">Any image.</param>
        /// <param name="format">Image format.</param>
        /// <returns>Definitely not broken bitmap.</returns>
        private Bitmap CopyBitmap(Bitmap bitmap, ImageFormat format)
        {
            MemoryStream buffer = new MemoryStream();
            bitmap.Save(buffer, format);
            Bitmap saveableBitmap = (Bitmap)System.Drawing.Image.FromStream(buffer);
            return saveableBitmap;
        }

        #endregion


        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
