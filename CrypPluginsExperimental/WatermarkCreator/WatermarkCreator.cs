﻿/*
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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Cryptool.Plugins.WatermarkCreator
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Watermark Creator", "Let's you add a watermark to an image", "WatermarkCreator/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.Steganography)]
    public class WatermarkCreator : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly WatermarkCreatorSettings settings = new WatermarkCreatorSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Image", "Image to add Watermark to")]
        public ICryptoolStream InputPicture 
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Watermark", "Watermark Text to be added to the image")]
        public string Watermark
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Image", "Image to be used as Watermark")]
        public ICryptoolStream WImage
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>

        [PropertyInfo(Direction.OutputData, "WatermarkImage", "Image with watermark text added")]
        public ICryptoolStream OutputPicture
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
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            if (InputPicture == null)
            {
                GuiLogMessage("Please provide a picture", NotificationLevel.Error);
                return;
            }

            switch (settings.ModificationType) //ENUM
            {
                case 0:
                  break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }
            if (Watermark == null)
            {
                GuiLogMessage("Please provide a watermark", NotificationLevel.Error);
                return;
            }

            if(settings.ModificationType == null)

            wmVisibleText();

            OnPropertyChanged("OutputPicture");
            ProgressChanged(1, 1);
        }

        private void wmVisibleText()
        {
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    Bitmap image = PaletteToRGB(bitmap);
                    System.Drawing.Image imgPhoto = image;
                    int phWidth = imgPhoto.Width; int phHeight = imgPhoto.Height;

                    Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);
                    bmPhoto.SetResolution(72, 72);
                    Graphics grPhoto = Graphics.FromImage(bmPhoto);

                    grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                    grPhoto.DrawImage(
                        imgPhoto,
                        new Rectangle(0, 0, phWidth, phHeight),
                        0,
                        0,
                        phWidth,
                        phHeight,
                        GraphicsUnit.Pixel);

                    int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };
                    Font crFont = null;
                    SizeF crSize = new SizeF();
                    for (int i = 0; i < 7; i++)
                    {
                        crFont = new Font("arial", sizes[i],
                                                     FontStyle.Bold);
                        crSize = grPhoto.MeasureString(Watermark,
                                                             crFont);

                        if ((ushort)crSize.Width < (ushort)phWidth)
                            break;
                    }

                    int yPixlesFromBottom = (int)(phHeight * .05);
                    float yPosFromBottom = ((phHeight -
                               yPixlesFromBottom) - (crSize.Height / 2));
                    float xCenterOfImg = (phWidth / 2);

                    StringFormat StrFormat = new StringFormat();
                    StrFormat.Alignment = StringAlignment.Center;

                    SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));

                    grPhoto.DrawString(Watermark,
                        crFont,
                        semiTransBrush2,
                        new PointF(xCenterOfImg + 1, yPosFromBottom + 1),
                        StrFormat);

                    SolidBrush semiTransBrush = new SolidBrush(
                                 Color.FromArgb(153, 255, 255, 255));

                    grPhoto.DrawString(Watermark,
                        crFont,
                        semiTransBrush,
                        new PointF(xCenterOfImg, yPosFromBottom),
                        StrFormat);

                    //image = new Bitmap(image.Width, image.Height, grPhoto);

                    CreateOutputStream(bmPhoto);
                }
            }
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

        #region Helpers
        private void CreateOutputStream(Bitmap bitmap)
        {
            ImageFormat format = ImageFormat.Bmp;
            //if (settings.OutputFileFormat == 1)
            //{
            //    format = ImageFormat.Png;
            //}
            //else if (settings.OutputFileFormat == 2)
            //{
            //    format = ImageFormat.Tiff;
            //}

            //avoid "generic error in GDI+"
            Bitmap saveableBitmap = CopyBitmap(bitmap, format);

            //save bitmap
            MemoryStream outputStream = new MemoryStream();
            saveableBitmap.Save(outputStream, format);
            saveableBitmap.Dispose();
            bitmap.Dispose();

            OutputPicture = new CStreamWriter(outputStream.GetBuffer());
        }

        private Bitmap CopyBitmap(Bitmap bitmap, ImageFormat format)
        {
            MemoryStream buffer = new MemoryStream();
            bitmap.Save(buffer, format);
            Bitmap saveableBitmap = (Bitmap)System.Drawing.Image.FromStream(buffer);
            return saveableBitmap;
        }

        private Bitmap PaletteToRGB(Bitmap original)
        {
            original = CopyBitmap(original, ImageFormat.Bmp);
            Bitmap image = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawImage(original, 0, 0, original.Width, original.Height);
            graphics.Dispose();
            original.Dispose();
            return image;
        }

        #endregion

    }
}