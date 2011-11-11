/* 
   Copyright 2011 Corinna John

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

// Define this variable and allow unsafe code for this project
// to improve the performance of hide/extract.
// #define Use_Unsafe_Pointers

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    [Author("Corinna John", "coco@steganografie.eu", "", "http://www.steganografie.eu")]
    [PluginInfo("Cryptool.Plugins.StegoLeastSignificantBit.Properties.Resources", "PluginCaption", "PluginTooltip", "StegoLeastSignificantBit/DetailedDescription/Description.xaml", "StegoLeastSignificantBit/Images/StegoLeastSignificantBit.png")]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class StegoLeastSignificantBit : ICrypComponent
    {
        #region Local Types

        /// <summary>Stores the colors of a pixel</summary>
        public struct PixelData
        {
            /// <summary>Blue component.</summary>
            public byte Blue;
            /// <summary>Green </summary>
            public byte Green;
            /// <summary>Red component.</summary>
            public byte Red;
        }

        #endregion

        #region Private Variables

        private readonly StegoLeastSignificantBitSettings settings = new StegoLeastSignificantBitSettings();
        private int noisePercent = 0;
        private int currentColorComponent = 0;
        private ImageInfo imageInfo;
        private const int PixelSize = 3;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip")]
        public ICryptoolStream InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputCarrierCaption", "InputCarrierTooltip")]
        public ICryptoolStream InputCarrier
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip")]
        public ICryptoolStream OutputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCarrierCaption", "OutputCarrierTooltip")]
        public ICryptoolStream OutputCarrier
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "PasswordStreamCaption", "PasswordStreamTooltip")]
        public ICryptoolStream PasswordStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "BitCountCaption", "BitCountTooltip")]
        public byte BitCount
        {
            get;
            set;
        }
        
        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// Reads the carrier image and calls Hide/Extract.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            using (CStreamReader reader = InputCarrier.CreateReader())
            {
                using (Bitmap bitmap = new Bitmap(reader))
                {
                    switch (settings.Action)
                    {
                        case 0:
                            if (settings.CustomizeRegions)
                            {
                                RegionHideForm dialog = new RegionHideForm(bitmap, (int)InputData.Length);
                                dialog.ShowDialog();
                                this.imageInfo = dialog.ImageInfo;
                            }
                            else
                            {
                                CreateDefaultImageInfo(bitmap);
                            }
                            
                            Hide(bitmap, InputData.CreateReader(), PasswordStream.CreateReader());
                            OnPropertyChanged("OutputCarrier");
                            break;
                        case 1:
                            Extract(bitmap, PasswordStream.CreateReader());
                            OnPropertyChanged("OutputString");

                            if (settings.CustomizeRegions)
                            {
                                RegionExtractForm dialog = new RegionExtractForm(this.imageInfo);
                                dialog.ShowDialog();
                            }

                            break;
                        default:
                            break;
                    }
                }
            }

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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

        #region Helper Methods

        /// <summary>
        /// Initialize [imageInfo] with a region covering the whole image.
        /// </summary>
        /// <param name="bitmap">Carrier image.</param>
        private void CreateDefaultImageInfo(Bitmap bitmap)
        {
            // how many lines do we need to hide the region info
            int countLines = (int)Math.Ceiling((double)ImageInfo.EffectiveSize*8 / bitmap.Width);
            // construct a maxium sized region
            Point[] points = new Point[4] { new Point(0, countLines), new Point(bitmap.Width, countLines), new Point(bitmap.Width, bitmap.Height), new Point(0, bitmap.Height) };
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(points);
            Collection<RegionInfo> regions = new Collection<RegionInfo>();
            regions.Add(new RegionInfo(path, points, bitmap.Size));
            regions[0].Capacity = (int)InputData.Length;
            this.imageInfo = new ImageInfo(bitmap, regions);
        }

        /// <summary>Converts an image to a 24-bit bitmap with default resolution.</summary>
        /// <param name="original">Any image.</param>
        /// <returns>Formatted image.</returns>
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

        /// <summary>Save an image to a file</summary>
        /// <param name="bitmap">The iamge to save</param>
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

            OutputCarrier = new CStreamWriter(outputStream.GetBuffer());
        }

#if Use_Unsafe_Pointers
        /// <summary>Hide an Int32 value in pPixel an the following pixels</summary>
        /// <param name="secretValue">The value to hide</param>
        /// <param name="pPixel">The first pixel to use</param>
        private unsafe void HideInt32(Int32 secretValue, ref PixelData* pPixel)
        {
            byte secretByte;

            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                secretByte = (byte)(secretValue >> (8 * byteIndex));
                HideByte(secretByte, ref pPixel);
            }
        }

        /// <summary>Return one component of a color</summary>
        /// <param name="pPixel">Pointer to the pixel</param>
        /// <param name="colorComponent">The component to return (0-R, 1-G, 2-B)</param>
        /// <returns>The requested component</returns>
        private unsafe byte GetColorComponent(PixelData* pPixel, int colorComponent)
        {
            byte returnValue = 0;
            switch (colorComponent)
            {
                case 0:
                    returnValue = pPixel->Red;
                    break;
                case 1:
                    returnValue = pPixel->Green;
                    break;
                case 2:
                    returnValue = pPixel->Blue;
                    break;
            }
            return returnValue;
        }

        /// <summary>Hide a byte in pPixel an the following pixels</summary>
        /// <param name="secretByte">The value to hide</param>
        /// <param name="pPixel">The first pixel to use</param>
        private unsafe void HideByte(byte secretByte, ref PixelData* pPixel)
        {
            byte colorComponent;

            for (int bitIndex = 0; bitIndex < 8; )
            {
                pPixel += 1;

                //rotate color components
                currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                //get value of Red, Green or Blue
                colorComponent = GetColorComponent(pPixel, currentColorComponent);

                CopyBitsToColor(1, secretByte, ref bitIndex, ref colorComponent);
                SetColorComponent(pPixel, currentColorComponent, colorComponent);
            }
        }

        /// <summary>Changes one component of a color</summary>
        /// <param name="pPixel">Pointer to the pixel</param>
        /// <param name="colorComponent">The component to change (0-R, 1-G, 2-B)</param>
        /// <param name="newValue">New value of the component</param>
        private unsafe void SetColorComponent(PixelData* pPixel, int colorComponent, byte newValue)
        {
            switch (colorComponent)
            {
                case 0:
                    pPixel->Red = newValue;
                    break;
                case 1:
                    pPixel->Green = newValue;
                    break;
                case 2:
                    pPixel->Blue = newValue;
                    break;
            }
        }

#else
        /// <summary>Hide an Int32 value in pPixel an the following pixels</summary>
        /// <param name="secretValue">The value to hide</param>
        /// <param name="pPixel">The first pixel to use</param>
        private void HideInt32(Int32 secretValue, ref IntPtr pixelPosition)
        {
            byte secretByte;

            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                secretByte = (byte)(secretValue >> (8 * byteIndex));
                HideByte(secretByte, ref pixelPosition);
            }
        }

        /// <summary>Return one component of a color</summary>
        /// <param name="pPixel">Pointer to the pixel</param>
        /// <param name="colorComponent">The component to return (0-R, 1-G, 2-B)</param>
        /// <returns>The requested component</returns>
        private byte GetColorComponent(PixelData pPixel, int colorComponent)
        {
            byte returnValue = 0;
            switch (colorComponent)
            {
                case 0:
                    returnValue = pPixel.Red;
                    break;
                case 1:
                    returnValue = pPixel.Green;
                    break;
                case 2:
                    returnValue = pPixel.Blue;
                    break;
            }
            return returnValue;
        }

        /// <summary>Hide a byte in pPixel an the following pixels</summary>
        /// <param name="secretByte">The value to hide</param>
        /// <param name="pPixel">The first pixel to use</param>
        private void HideByte(byte secretByte, ref IntPtr pixelPosition)
        {
            byte colorComponent;
            PixelData pPixel;

            for (int bitIndex = 0; bitIndex < 8; )
            {
                pixelPosition += PixelSize;
                ReadPixel(pixelPosition, out pPixel);

                //rotate color components
                currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                //get value of Red, Green or Blue
                colorComponent = GetColorComponent(pPixel, currentColorComponent);

                CopyBitsToColor(1, secretByte, ref bitIndex, ref colorComponent);
                SetColorComponent(ref pPixel, currentColorComponent, colorComponent);

                WritePixel(pixelPosition, pPixel);
            }
        }

        /// <summary>Changes one component of a color</summary>
        /// <param name="pPixel">Pointer to the pixel</param>
        /// <param name="colorComponent">The component to change (0-R, 1-G, 2-B)</param>
        /// <param name="newValue">New value of the component</param>
        private void SetColorComponent(ref PixelData pPixel, int colorComponent, byte newValue)
        {
            switch (colorComponent)
            {
                case 0:
                    pPixel.Red = newValue;
                    break;
                case 1:
                    pPixel.Green = newValue;
                    break;
                case 2:
                    pPixel.Blue = newValue;
                    break;
            }
        }
#endif

        /// <summary>Copy one or more bits from the message into a color value</summary>
        /// <param name="bitsPerUnit">Count of bits to copy</param>
        /// <param name="messageByte">Source byte</param>
        /// <param name="messageBitIndex">Index of the first copied bit</param>
        /// <param name="colorComponent">Destination byte</param>
        private void CopyBitsToColor(int bitsPerUnit, byte messageByte, ref int messageBitIndex, ref byte colorComponent)
        {
            for (int carrierBitIndex = 0; carrierBitIndex < bitsPerUnit; carrierBitIndex++)
            {
                colorComponent = SetBit(messageBitIndex, messageByte, carrierBitIndex, colorComponent);
                messageBitIndex++;
            }
        }

        private void ReadPixel(IntPtr from, out PixelData to)
        {
            byte[] colorValues = new byte[3];
            Marshal.Copy(from, colorValues, 0, PixelSize);

            to.Blue = colorValues[0];
            to.Green = colorValues[1];
            to.Red = colorValues[2];
        }

        private void WritePixel(IntPtr to, PixelData from)
        {
            Marshal.WriteByte(to, from.Blue);
            Marshal.WriteByte(to + 1, from.Green);
            Marshal.WriteByte(to + 2, from.Red);
        }

        /// <summary>Hides the given message stream in the image.</summary>
        /// <param name="message">Message.</param>
        /// <param name="key">A stream with varying seed values for a random number generator.</param>
        public
        #if Use_Unsafe_Pointers
            unsafe
        #endif
        void Hide(Bitmap bitmap, CStreamReader message, CStreamReader key)
        {
            //make sure that the image is in RGB format
            Bitmap image = PaletteToRGB(bitmap);

            int pixelOffset = 0;
            int maxOffset = 0;
            int messageValue;
            byte keyByte, messageByte, colorComponent;
            Random random;

            BitmapData bitmapData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //go to the first pixel

#if Use_Unsafe_Pointers
            PixelData* pPixel = (PixelData*)bitmapData.Scan0.ToPointer();
            PixelData* pFirstPixel;
#else
            PixelData pPixel;
            IntPtr pFirstPixel;
            IntPtr pixelPosition = bitmapData.Scan0;
#endif

            //get the first pixel that belongs to a region
            //and serialise the regions to a map stream
            int firstPixelInRegions = image.Width * image.Height;
            MemoryStream regionData = new MemoryStream();
            BinaryWriter regionDataWriter = new BinaryWriter(regionData);
            foreach (RegionInfo regionInfo in this.imageInfo.RegionInfo)
            {
                regionInfo.PixelIndices.Sort();
                if ((int)regionInfo.PixelIndices[0] < firstPixelInRegions)
                {
                    firstPixelInRegions = (int)regionInfo.PixelIndices[0];
                }

                byte[] regionBytes = PointsToBytes(regionInfo.Points);
                regionDataWriter.Write((Int32)regionBytes.Length);
                regionDataWriter.Write((Int32)regionInfo.Capacity);
                regionDataWriter.Write(regionInfo.CountUsedBitsPerPixel);
                regionDataWriter.Write(regionBytes);
            }
            //go to the beginning of the stream
            regionDataWriter.Flush();
            regionData.Seek(0, SeekOrigin.Begin);

            
#if Use_Unsafe_Pointers
            //hide firstPixelInRegions
            HideInt32(firstPixelInRegions, ref pPixel);

            //hide length of map stream
            HideInt32((Int32)regionData.Length, ref pPixel);

            //hide regions

            pFirstPixel = pPixel; //don't overwrite already written header
#else
            //hide firstPixelInRegions
            HideInt32(firstPixelInRegions, ref pixelPosition);

            //hide length of map stream
            HideInt32((Int32)regionData.Length, ref pixelPosition);

            //hide regions

            pFirstPixel = pixelPosition;
#endif

            int regionByte;
            while ((regionByte = regionData.ReadByte()) >= 0)
            {
                keyByte = GetKeyValue(key);
                random = new Random(keyByte);

                for (int regionBitIndex = 0; regionBitIndex < 8; )
                {
                    int countRemainingPixels = firstPixelInRegions - 1 - pixelOffset;
                    int lengthRemainingStream = (int)(regionData.Length - regionData.Position + 1);
                    int lengthBlock = countRemainingPixels / (lengthRemainingStream * 8);

                    pixelOffset += random.Next(1, lengthBlock);

#if Use_Unsafe_Pointers
                    pPixel = pFirstPixel + pixelOffset;
#else
                    pixelPosition = pFirstPixel + (PixelSize * pixelOffset);
                    ReadPixel(pixelPosition, out pPixel);
#endif

                    //place [regionBit] in one bit of the colour component

                    //rotate color components
                    currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                    //get value of Red, Green or Blue
                    colorComponent = GetColorComponent(pPixel, currentColorComponent);

                    //put the bits into the color component and write it back into the bitmap
                    CopyBitsToColor(1, (byte)regionByte, ref regionBitIndex, ref colorComponent);

#if Use_Unsafe_Pointers
                    SetColorComponent(pPixel, currentColorComponent, colorComponent);
#else
                    SetColorComponent(ref pPixel, currentColorComponent, colorComponent);
                    WritePixel(pixelPosition, pPixel);
#endif
                }
            }

            // ----------------------------------------- Hide the Message

            //begin with the first pixel of the image
#if Use_Unsafe_Pointers
            pFirstPixel = (PixelData*)bitmapData.Scan0.ToPointer();
#else
            pFirstPixel = bitmapData.Scan0;
#endif

            foreach (RegionInfo regionInfo in this.imageInfo.RegionInfo)
            {
                //go to the first pixel of this region
#if Use_Unsafe_Pointers
                pPixel = (PixelData*)bitmapData.Scan0.ToPointer();
                pPixel += (int)regionInfo.PixelIndices[0];
#else
                pixelPosition = bitmapData.Scan0 + (PixelSize * (int)regionInfo.PixelIndices[0]);
#endif
                pixelOffset = 0;

                for (int n = 0; n < regionInfo.Capacity; n++)
                {
                    messageValue = message.ReadByte();
                    if (messageValue < 0) { break; } //end of message
                    messageByte = (byte)messageValue;

                    keyByte = GetKeyValue(key);
                    random = new Random(keyByte);

                    for (int messageBitIndex = 0; messageBitIndex < 8; )
                    {
                        float countRemainingPixels = regionInfo.CountPixels - 1 - pixelOffset;
                        float lengthRemainingStream = regionInfo.Capacity - n;
                        float countMessageBytesPerPixel = (float)regionInfo.CountUsedBitsPerPixel / 8;
                        float lengthBlock = countRemainingPixels * countMessageBytesPerPixel / lengthRemainingStream;

                        maxOffset = (int)Math.Floor(lengthBlock);
                        pixelOffset += random.Next(1, (maxOffset > 0) ? maxOffset : 1);

#if Use_Unsafe_Pointers
                        pPixel = pFirstPixel + (int)regionInfo.PixelIndices[pixelOffset];
#else
                        pixelPosition = pFirstPixel + (PixelSize * (int)regionInfo.PixelIndices[pixelOffset]);
                        ReadPixel(pixelPosition, out pPixel);
#endif

                        //place [messageBit] in one bit of the colour component

                        //rotate color components
                        currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                        //get value of Red, Green or Blue
                        colorComponent = GetColorComponent(pPixel, currentColorComponent);

                        //put the bits into the color component and write it back into the bitmap
                        CopyBitsToColor(regionInfo.CountUsedBitsPerPixel, messageByte, ref messageBitIndex, ref colorComponent);

#if Use_Unsafe_Pointers
                        SetColorComponent(pPixel, currentColorComponent, colorComponent);
#else
                        SetColorComponent(ref pPixel, currentColorComponent, colorComponent);
                        WritePixel(pixelPosition, pPixel);
#endif
                    }
                }
            }

            image.UnlockBits(bitmapData);
            CreateOutputStream(image);
        }

        /// <summary>Convert points (X;Y|X;Y|X;Y) to plain bytes (XYXYXY)</summary>
        private byte[] PointsToBytes(Point[] points)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            for (int pointsIndex = 0; pointsIndex < points.Length; pointsIndex++)
            {
                writer.Write(points[pointsIndex].X);
                writer.Write(points[pointsIndex].Y);
            }

            writer.Flush();
            byte[] result = stream.ToArray();
            return result;
        }

        /// <summary>Convert plain bytes (XYXYXY) to points (X;Y|X;Y|X;Y)</summary>
        private Point[] BytesToPoints(byte[] bytes)
        {
            Point[] result = new Point[bytes.Length / 8];

            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(stream);
            stream.Position = 0;

            int resultIndex = 0;
            while (stream.Position < stream.Length)
            {
                result[resultIndex].X = reader.ReadInt32();
                result[resultIndex].Y = reader.ReadInt32();
                resultIndex++;
            }

            return result;
        }

#if Use_Unsafe_Pointers
        /// <summary>Extract an Int32 value from pPixel and the following pixels</summary>
        /// <param name="pPixel">The first pixel to use</param>
        /// <returns>The extracted value</returns>
        private unsafe Int32 ExtractInt32(ref PixelData* pPixel)
        {
            int returnValue = 0;
            byte readByte;

            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                readByte = ExtractByte(ref pPixel);
                returnValue += readByte << (byteIndex * 8);
            }

            return returnValue;
        }

        /// <summary>Extract a byte value from pPixel and the following pixels</summary>
        /// <param name="pPixel">The first pixel to use</param>
        /// <returns>The extracted value</returns>
        private unsafe byte ExtractByte(ref PixelData* pPixel)
        {
            byte colorComponent;
            byte readByte = 0;

            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                pPixel += 1;
                //rotate color components
                currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                //get value of Red, Green or Blue
                colorComponent = GetColorComponent(pPixel, currentColorComponent);
                AddBit(bitIndex, ref readByte, 0, colorComponent);
            }

            return readByte;
        }
#else
        /// <summary>Extract an Int32 value from pPixel and the following pixels</summary>
        /// <param name="pPixel">The first pixel to use</param>
        /// <returns>The extracted value</returns>
        private Int32 ExtractInt32(ref IntPtr pixelPosition)
        {
            int returnValue = 0;
            byte readByte;

            for (int byteIndex = 0; byteIndex < 4; byteIndex++)
            {
                readByte = ExtractByte(ref pixelPosition);
                returnValue += readByte << (byteIndex * 8);
            }

            return returnValue;
        }

        /// <summary>Extract a byte value from pPixel and the following pixels</summary>
        /// <param name="pPixel">The first pixel to use</param>
        /// <returns>The extracted value</returns>
        private byte ExtractByte(ref IntPtr pixelPosition)
        {
            PixelData pPixel;
            byte colorComponent;
            byte readByte = 0;

            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                pixelPosition += PixelSize;
                ReadPixel(pixelPosition, out pPixel);

                //rotate color components
                currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                //get value of Red, Green or Blue
                colorComponent = GetColorComponent(pPixel, currentColorComponent);
                AddBit(bitIndex, ref readByte, 0, colorComponent);
            }

            return readByte;
        }
#endif

        /// <summary>Copy the lowest bit from [carrierByte] into a specific bit of [messageByte]</summary>
        /// <param name="messageBitIndex">Position of the bit in [messageByte]</param>
        /// <param name="messageByte">a byte to write into the message stream</param>
        /// <param name="carrierBitIndex">Position of the bit in [carrierByte]</param>
        /// <param name="carrierByte">a byte from the carrier file</param>
        private void AddBit(int messageBitIndex, ref byte messageByte, int carrierBitIndex, byte carrierByte)
        {
            int carrierBit = ((carrierByte & (1 << carrierBitIndex)) > 0) ? 1 : 0;
            messageByte += (byte)(carrierBit << messageBitIndex);
        }

        /// <summary>
        /// Read the next byte of the key stream.
        /// Reset the stream if it is too short.
        /// </summary>
        /// <returns>The next key byte</returns>
        protected byte GetKeyValue(CStreamReader keyStream)
        {
            int keyValue = 0;
            if (keyStream.Length > 0)
            {
                if ((keyValue = keyStream.ReadByte()) < 0)
                {
                    keyStream.Seek(0, SeekOrigin.Begin);
                    keyValue = keyStream.ReadByte();
                }
            }
            return (byte)keyValue;
        }

        /// <summary>Extract the header from an image</summary>
        /// <remarks>The header contains information about the regions which carry the message</remarks>
        /// <param name="key">Key stream</param>
        /// <returns>The extracted regions with all meta data that is needed to extract the message</returns>
        public 
        #if Use_Unsafe_Pointers
            unsafe
        #endif    
        Collection<RegionInfo> ExtractRegionData(Bitmap bitmap, CStreamReader key)
        {
            byte keyByte, colorComponent;
            Random random;
            int pixelOffset = 0;

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

#if Use_Unsafe_Pointers
            PixelData* pPixel;
            PixelData* pFirstPixel;

            //go to the first pixel
            pPixel = (PixelData*)bitmapData.Scan0.ToPointer();

            //get firstPixelInRegions
            int firstPixelInRegions = ExtractInt32(ref pPixel);

            //get length of region information
            int regionDataLength = ExtractInt32(ref pPixel);
            
            pFirstPixel = pPixel;
#else
            PixelData pPixel;
            IntPtr pFirstPixel;
            IntPtr pixelPosition = bitmapData.Scan0;
            int firstPixelInRegions = ExtractInt32(ref pixelPosition);
            int regionDataLength = ExtractInt32(ref pixelPosition);
            pFirstPixel = pixelPosition;
#endif

            //get region information

            MemoryStream regionData = new MemoryStream();

            byte regionByte;
            while (regionDataLength > regionData.Length)
            {
                regionByte = 0;
                keyByte = GetKeyValue(key);
                random = new Random(keyByte);

                for (int regionBitIndex = 0; regionBitIndex < 8; regionBitIndex++)
                {
                    //move to the next pixel

                    int countRemainingPixels = firstPixelInRegions - 1 - pixelOffset;
                    int lengthRemainingStream = (int)(regionDataLength - regionData.Length);
                    int lengthBlock = countRemainingPixels / (lengthRemainingStream * 8);
                    pixelOffset += random.Next(1, lengthBlock);

#if Use_Unsafe_Pointers
                    pPixel = pFirstPixel + pixelOffset;
                    //rotate color components
                    currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                    //get value of Red, Green or Blue
                    colorComponent = GetColorComponent(pPixel, currentColorComponent);
#else
                    pixelPosition = pFirstPixel + (PixelSize * pixelOffset);
                    ReadPixel(pixelPosition, out pPixel);
                    //rotate color components
                    currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                    //get value of Red, Green or Blue
                    colorComponent = GetColorComponent(pPixel, currentColorComponent);
#endif
                    //extract one bit and add it to [regionByte]
                    AddBit(regionBitIndex, ref regionByte, 0, colorComponent);
                }

                //write the extracted byte
                regionData.WriteByte(regionByte);
            }

            bitmap.UnlockBits(bitmapData);

            //read regions from [regionData]

            Collection<RegionInfo> regions = new Collection<RegionInfo>();
            BinaryReader regionReader = new BinaryReader(regionData);

            //extract region header

            regionReader.BaseStream.Seek(0, SeekOrigin.Begin);
            do
            {
                //If the program crashes here,
                //the image is damaged,
                //it contains no hidden data,
                //or you tried to use a wrong key.
                try
                {
                    int regionLength = regionReader.ReadInt32();
                    int regionCapacity = regionReader.ReadInt32();
                    byte regionBitsPerPixel = regionReader.ReadByte();
                    byte[] regionContent = regionReader.ReadBytes(regionLength);
                
                    Point[] regionPoints = BytesToPoints(regionContent);
                    GraphicsPath regionPath = new GraphicsPath();
                    regionPath.AddPolygon(regionPoints);
                    Region region = new Region(regionPath);
                    regions.Add(new RegionInfo(region, regionCapacity, regionBitsPerPixel, bitmap.Size));
                }
                catch
                {
                    GuiLogMessage("Wrong key or nothing hidden in the picture", NotificationLevel.Warning);
                    throw;
                }
            } while (regionData.Position < regionData.Length);

            return regions;
        }

        /// <summary>Extracts a stream from the HTML document.</summary>
        /// <param name="key">A stream with varying seed values for a random number generator.</param>
        /// <returns>The extracted stream.</returns>
        public
        #if Use_Unsafe_Pointers
            unsafe
        #endif
        Stream Extract(Bitmap bitmap, CStreamReader key)
        {
            //Bitmap image = bitmap;

            Collection<RegionInfo> regionInfos = ExtractRegionData(bitmap, key);
            this.imageInfo = new ImageInfo(bitmap, regionInfos);

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            MemoryStream messageStream = new MemoryStream();
            byte keyByte;
            byte messageByte, colorComponent;

#if Use_Unsafe_Pointers
            PixelData* pPixel;
            PixelData* pFirstPixel = (PixelData*)bitmapData.Scan0.ToPointer();
#else
            PixelData pPixel;
            IntPtr pixelPosition;
            IntPtr pFirstPixel = bitmapData.Scan0;
#endif

            Random random;
            int maxOffset, pixelOffset = 0;

            foreach (RegionInfo regionInfo in this.imageInfo.RegionInfo)
            {
                //go to first pixel of this region
#if Use_Unsafe_Pointers
                pFirstPixel = (PixelData*)bitmapData.Scan0.ToPointer();
                pPixel = pFirstPixel + (int)regionInfo.PixelIndices[0];
#else
                pFirstPixel = bitmapData.Scan0;
                pixelPosition = pFirstPixel + (PixelSize * (int)regionInfo.PixelIndices[0]);
#endif
                pixelOffset = 0;

                for (int n = 0; n < regionInfo.Capacity; n++)
                {
                    messageByte = 0;
                    keyByte = GetKeyValue(key);
                    random = new Random(keyByte);

                    for (int messageBitIndex = 0; messageBitIndex < 8; )
                    {
                        //move to the next pixel

                        float countRemainingPixels = regionInfo.CountPixels - pixelOffset - 1;
                        float lengthRemainingStream = regionInfo.Capacity - n;
                        float countMessageBytesPerPixel = (float)regionInfo.CountUsedBitsPerPixel / 8;
                        float lengthBlock = countRemainingPixels * countMessageBytesPerPixel / lengthRemainingStream;

                        maxOffset = (int)Math.Floor(lengthBlock);
                        pixelOffset += random.Next(1, maxOffset);

#if Use_Unsafe_Pointers
                        pPixel = pFirstPixel + (int)regionInfo.PixelIndices[pixelOffset];
#else
                        pixelPosition = pFirstPixel + (PixelSize * (int)regionInfo.PixelIndices[pixelOffset]);
                        ReadPixel(pixelPosition, out pPixel);
#endif

                        //rotate color components
                        currentColorComponent = (currentColorComponent == 2) ? 0 : (currentColorComponent + 1);
                        //get value of Red, Green or Blue
                        colorComponent = GetColorComponent(pPixel, currentColorComponent);

                        for (int carrierBitIndex = 0; carrierBitIndex < regionInfo.CountUsedBitsPerPixel; carrierBitIndex++)
                        {
                            AddBit(messageBitIndex, ref messageByte, carrierBitIndex, colorComponent);
                            messageBitIndex++;
                        }
                    }

                    //add the re-constructed byte to the message
                    messageStream.WriteByte(messageByte);
                }
            }

            //unlock pixels
            bitmap.UnlockBits(bitmapData);

            messageStream.Position = 0;
            StreamReader reader = new StreamReader(messageStream);
            imageInfo.TextMessage = reader.ReadToEnd();

            messageStream.Position = 0;
            byte[] outputBuffer = new byte[imageInfo.TextMessage.Length];
            messageStream.Read(outputBuffer, 0, outputBuffer.Length);
            OutputData = new CStreamWriter(outputBuffer);
            this.OnPropertyChanged("OutputData");

            return messageStream;
        }

        /// <summary>Copy a bit from [messageByte] into to lowest bit of [carrierByte]</summary>
        /// <param name="messageBitIndex">Position of the bit to copy</param>
        /// <param name="messageByte">a byte from the message stream</param>
        /// <param name="carrierBitIndex">Position of the bit in [carrierByte]</param>
        /// <param name="carrierByte">a byte from the carrier file</param>
        /// <returns>Changed [carrierByte]</returns>
        protected byte SetBit(int messageBitIndex, byte messageByte, int carrierBitIndex, byte carrierByte)
        {
            //get one bit of the current message byte...
            bool messageBit = ((messageByte & (1 << messageBitIndex)) > 0);
            //get one bit of the carrier byte
            bool carrierBit = ((carrierByte & (1 << carrierBitIndex)) > 0);

            //place [messageBit] in the corresponding bit of [carrierByte]
            if (messageBit && !carrierBit)
            {
                carrierByte += (byte)(1 << carrierBitIndex);
            }
            else if (!messageBit && carrierBit)
            {
                carrierByte -= (byte)(1 << carrierBitIndex);
            }

            return carrierByte;
        }

        /// <summary>Copy the lowest bit from [carrierByte] into a specific bit of [messageByte]</summary>
        /// <param name="messageBitIndex">Position of the bit in [messageByte]</param>
        /// <param name="messageByte">a byte to write into the message stream</param>
        /// <param name="carrierBitIndex">Position of the bit in [carrierByte]</param>
        /// <param name="carrierByte">a byte from the carrier file</param>
        /// <returns>Changed [messageByte]</returns>
        protected byte GetBit(int messageBitIndex, byte messageByte, int carrierBitIndex, byte carrierByte)
        {
            int carrierBit = ((carrierByte & (1 << carrierBitIndex)) > 0) ? 1 : 0;
            messageByte += (byte)(carrierBit << messageBitIndex);
            return messageByte;
        }

        #endregion
    }
}
