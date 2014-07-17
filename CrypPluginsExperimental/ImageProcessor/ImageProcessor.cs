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
    [PluginInfo("ImageProcessor", "Process and save an image", "ImageProcessor/userdoc.xml", new[] { "ImageProcessor/icon.png"/*"CrypWin/images/default.png"*/ })]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class ImageProcessor : ICrypComponent
    {
        #region Private Variables and Constructor

        private readonly ImageProcessorSettings settings = new ImageProcessorSettings();
        
        public ImageProcessor()
        {
            this.settings = new ImageProcessorSettings();
            this.settings.UpdateTaskPaneVisibility();
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Description
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip")]
        public ICryptoolStream InputData
        {
            get;
            set;
        }

        /// <summary>
        /// Description
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip")]
        public ICryptoolStream OutputData
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

            if (InputData == null)
            {
                GuiLogMessage("Please select an image.", NotificationLevel.Error);
                return;
            }
            
            using (CStreamReader reader = InputData.CreateReader())
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
                                Image<Gray, double> grayImg = img.Convert<Gray, byte>().Convert<Gray, double>();
                                CreateOutputStream(grayImg.ToBitmap());
                                break;
                            case ActionType.smooth: // Smoothing
                                img._SmoothGaussian(settings.Smooth);
                                CreateOutputStream(img.ToBitmap());
                                break;
                            case ActionType.resize: // Resizeing
                                Image<Bgr, byte> img2 = img.Resize(settings.SizeX, settings.SizeY, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                                CreateOutputStream(img2.ToBitmap());
                                break;
                            case ActionType.rotate: // Rotating
                                Image<Bgr, byte> img3 = img.Rotate(settings.Degrees, new Bgr(Color.White));
                                CreateOutputStream(img3.ToBitmap());
                                break;
                            case ActionType.invert: // Inverting
                                Image<Bgr, byte> img4 = img.Not();
                                CreateOutputStream(img4.ToBitmap());
                                break;
                            case ActionType.create: // Create Image
                                Image<Gray, Single> img5 = new Image<Gray, Single>(settings.SizeX, settings.SizeY);
                                CreateOutputStream(img5.ToBitmap());
                                break;

                                //TODO: and, or, rauschen
                        }

                        OnPropertyChanged("OutputData");
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

            OutputData = new CStreamWriter(outputStream.GetBuffer());
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
