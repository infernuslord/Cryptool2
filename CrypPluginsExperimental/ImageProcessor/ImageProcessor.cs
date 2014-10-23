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
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Emgu.CV;
using Emgu.CV.Structure;


namespace Cryptool.Plugins.ImageProcessor
{
    [Author("Heuser", "bhe@student.uni-kassel.de", "", "")]
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("ImageProcessor", "Process and save an image", "ImageProcessor/userdoc.xml", new[] { "ImageProcessor/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ImageProcessor : ICrypComponent
    {
        #region Private Variables and Constructor

        private readonly ImageProcessorSettings settings = new ImageProcessorSettings();
        
        public ImageProcessor()
        {
            this.settings = new ImageProcessorSettings();
            this.settings.UpdateTaskPaneVisibility();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Description
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImage1", "This is the standard image used for the processing.")]
        public ICryptoolStream InputImage1
        {
            get;
            set;
        }

        /// <summary>
        /// Description
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputImage1", "This is the second image only used for the and- and or-functions.")]
        public ICryptoolStream InputImage2
        {
            get;
            set;
        }

        /// <summary>
        /// Description
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputImage", "This is the processed image.")]
        public ICryptoolStream OutputImage
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

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
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (InputImage1 == null)
            {
                if (InputImage2 != null)
                {
                    InputImage1 = InputImage2;
                }
                else
                {
                    GuiLogMessage("Please select an image.", NotificationLevel.Error);
                    return;
                }
            }

            using (CStreamReader reader = InputImage1.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    using (Image<Bgr, Byte> img = new Image<Bgr, Byte>(bitmap))
                    {
                        switch (settings.Action)
                        {
                            case ActionType.flip: // Flip Image
                                switch (settings.FlipType)
                                {
                                    case 0: // Horizontal
                                        img._Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
                                        CreateOutputStream(img.ToBitmap());
                                        break;
                                    case 1: // Vertical
                                        img._Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
                                        CreateOutputStream(img.ToBitmap());
                                        break;
                                }
                                break;
                            case ActionType.gray: // Gray Scale
                                using (Image<Gray, double> grayImg = img.Convert<Gray, byte>().Convert<Gray, double>())
                                {
                                    CreateOutputStream(grayImg.ToBitmap());
                                }
                                break;
                            case ActionType.smooth: // Smoothing
                                img._SmoothGaussian(settings.Smooth);
                                CreateOutputStream(img.ToBitmap());
                                break;
                            case ActionType.resize: // Resizeing
                                using (Image<Bgr, byte> newImg = img.Resize(settings.SizeX, settings.SizeY, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.crop: // cropping
                                this.CropImage();
                                break;
                            case ActionType.rotate: // Rotating
                                using (Image<Bgr, byte> newImg = img.Rotate(settings.Degrees, new Bgr(Color.White)))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.invert: // Inverting
                                using (Image<Bgr, byte> newImg = img.Not())
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.create: // Create Image
                                using (Image<Gray, Single> newImg = new Image<Gray, Single>(settings.SizeX, settings.SizeY))
                                {
                                    CreateOutputStream(newImg.ToBitmap());
                                }
                                break;
                            case ActionType.and:    // and-connect Images
                                using (Image<Bgr, Byte> secondImg = new Image<Bgr, Byte>(new Bitmap(InputImage2.CreateReader())))
                                {
                                    using (Image<Bgr, byte> newImg = img.And(secondImg))
                                    {
                                        CreateOutputStream(newImg.ToBitmap());
                                    }
                                }
                                break;
                            case ActionType.or:    // and-connect Images
                                using (Image<Bgr, Byte> secondImg = new Image<Bgr, Byte>(new Bitmap(InputImage2.CreateReader())))
                                {
                                    using (Image<Bgr, byte> newImg = img.Or(secondImg))
                                    {
                                        CreateOutputStream(newImg.ToBitmap());
                                    }
                                }
                                break;
                            case ActionType.xor:    // xor-connect Images
                                using (Image<Bgr, Byte> secondImg = new Image<Bgr, Byte>(new Bitmap(InputImage2.CreateReader())))
                                {
                                    using (Image<Bgr, byte> newImg = img.Xor(secondImg))
                                    {
                                        CreateOutputStream(newImg.ToBitmap());
                                    }
                                }
                                break;
                            case ActionType.xorgray:    // xor- Imagegrayscales
                                using (Image<Gray, byte> grayImg2 = new Image<Bgr, Byte>(new Bitmap(InputImage2.CreateReader())).Convert<Gray, byte>())
                                {
                                    using (Image<Gray, byte> grayImg1 = img.Convert<Gray, byte>())
                                    {
                                        using (Image<Gray, byte> newImg = grayImg1.Xor(grayImg2))
                                        {
                                            CreateOutputStream(newImg.ToBitmap());
                                        }
                                    }
                                }
                                break;
                        }

                        OnPropertyChanged("OutputImage");
                    }
                }
            }
           
            ProgressChanged(1, 1);
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

        /// <summary>Save an image to a file</summary>
        /// <param name="bitmap">The image to save</param>
        private void CreateOutputStream(Bitmap bitmap)
        {
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
            Bitmap saveableBitmap = CopyBitmap(bitmap, format);

            //save bitmap
            MemoryStream outputStream = new MemoryStream();
            saveableBitmap.Save(outputStream, format);
            saveableBitmap.Dispose();
            bitmap.Dispose();

            OutputImage = new CStreamWriter(outputStream.GetBuffer());
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

        private void CropImage()
        {
            if (InputImage1 == null)
                return;
            using (CStreamReader reader = InputImage1.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    int x1 = settings.SliderX1*bitmap.Width/10000;
                    int x2 = bitmap.Width - settings.SliderX2*bitmap.Width/10000 - x1;
                    int y1 = settings.SliderY1*bitmap.Height/10000;
                    int y2 = bitmap.Height - settings.SliderY2*bitmap.Height/10000 - y1;
                    Rectangle cropRect = new Rectangle(x1, y1, x2, y2);
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                        CreateOutputStream(target);
                    }
                }
            }
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
        
        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SliderX1":
                case "SliderX2":
                case "SliderY1":
                case "SliderY2":
                    this.CropImage();
                    OnPropertyChanged("OutputImage");
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

}
