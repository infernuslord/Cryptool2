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
  
   For invisible Watermarks, an existing Project has been used:
   Original Project can be found at https://code.google.com/p/dct-watermark/
   Ported to C# to be used within CrypTool 2 by Nils Rehwald
   Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
   Thanks to Nils Kopal for Support and Bugfixing 
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
using WatermarkCreator.Properties;

namespace Cryptool.Plugins.WatermarkCreator
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("WatermarkCreator.Properties.Resources", "PluginCaption", "PluginCaptionTooltip", "WatermarkCreator/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.Steganography)]
    public class WatermarkCreator : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly WatermarkCreatorSettings settings = new WatermarkCreatorSettings();
        //Default values for invisible watermarking
        private const int BoxSize = 10;
        private const int ErrorCorrection = 0;
        private const double Opacity = 1.0;
        private const long Seed1 = 19;
        private const long Seed2 = 24;

        enum Commands { EmbVisText, EmbInvisText, ExtInvisText };

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "ImageCaption", "ImageTooltip")]
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

        [PropertyInfo(Direction.InputData, "Watermark Image", "Image to be used as Watermark")]
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

        [PropertyInfo(Direction.OutputData, "EmbeddedText", "Text that was embedded in the Picture")]
        public string EmbeddedText
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

            switch (settings.ModificationType)
            {
                case (int)Commands.EmbVisText: //Visible Text
                    if (Watermark == null)
                    {
                        GuiLogMessage("Please provide a text", NotificationLevel.Error);
                        return;
                    }

                    WmVisibleText();

                    OnPropertyChanged("OutputPicture");
                    ProgressChanged(1, 1);
                    break;

                case (int)Commands.EmbInvisText: //Invisible Text
                    if (Watermark == null)
                    {
                        GuiLogMessage("Please provide a text", NotificationLevel.Error);
                        return;
                    }

                    CreateInvisibleWatermark();

                    OnPropertyChanged("OutputPicture");
                    ProgressChanged(1, 1);

                    break;

                case (int)Commands.ExtInvisText: //Detect Invisible Text

                    EmbeddedText = DetectInvisibleWatermark();

                    OnPropertyChanged("EmbeddedText");
                    ProgressChanged(1, 1);

                    break;

                default:
                    GuiLogMessage("This error should actually never happen. WTF?", NotificationLevel.Error);
                    break;
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

        private void WmVisibleText()
        {
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    Bitmap image = PaletteToRGB(bitmap);
                    System.Drawing.Image photo = image;
                    int width = photo.Width; 
                    int height = photo.Height;

                    Bitmap bitmapmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    bitmapmPhoto.SetResolution(72, 72);
                    Graphics graphicPhoto = Graphics.FromImage(bitmapmPhoto);

                    graphicPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicPhoto.DrawImage(photo, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                    int[] sizes = new int[] { 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2 }; //possible text sizes
                    Font font = null;
                    SizeF size = new SizeF();
                    for (int i = 0; i < sizes.Length; i++) //Test which is the largest possible fontsize to be used
                    {
                        font = new Font("arial", sizes[i], FontStyle.Bold);
                        size = graphicPhoto.MeasureString(Watermark, font);
                        if ((ushort)size.Width < (ushort)width)
                            break;
                    }
                    int yPixlesFromBottom = (int)(height * .05);
                    float yPosFromBottom = ((height - yPixlesFromBottom) - (size.Height / 2));
                    float xCenterOfImg = ((float)width / 2f);

                    StringFormat strFormat = new StringFormat();
                    strFormat.Alignment = StringAlignment.Center;

                    SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));

                    graphicPhoto.DrawString(Watermark, font, semiTransBrush2, new PointF(xCenterOfImg + 1, yPosFromBottom + 1), strFormat);

                    SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

                    graphicPhoto.DrawString(Watermark, font, semiTransBrush, new PointF(xCenterOfImg, yPosFromBottom), strFormat);

                    CreateOutputStream(bitmapmPhoto);
                }
            }
        }

        private void CreateInvisibleWatermark()
        {
            net.watermark.Watermark water = new net.watermark.Watermark(BoxSize, ErrorCorrection, Opacity, Seed1, Seed2);
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    water.embed(bitmap, Watermark);
                    CreateOutputStream(bitmap);
                }
            }
        }

        private string DetectInvisibleWatermark()
        {
            net.watermark.Watermark water = new net.watermark.Watermark(BoxSize, ErrorCorrection, Opacity, Seed1, Seed2);
            using (CStreamReader reader = InputPicture.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    return water.extractText(bitmap);   
                }
            }
        }

        private void CreateOutputStream(Bitmap bitmap)
        {
            ImageFormat format = ImageFormat.Bmp;

            Bitmap saveableBitmap = CopyBitmap(bitmap, format);

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
