using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

/*
 * CRC hash algorithm (currently just CRC-32).
 * 
 * TODO/Wishlist:
 * - add support for other generator polynoms and hash lengths.
 * 
 * The CRC-32 algorithm implementation is based on a code snippet by NullFX with the following license:
 * 
 * ******************************************************
 * *          NULLFX FREE SOFTWARE LICENSE              *
 * ******************************************************
 * *  CRC32 Library                                     *
 * *  by: Steve Whitley                                 *
 * *  © 2005 NullFX Software                            *
 * *                                                    *
 * * NULLFX SOFTWARE DISCLAIMS ALL WARRANTIES,          *
 * * RESPONSIBILITIES, AND LIABILITIES ASSOCIATED WITH  *
 * * USE OF THIS CODE IN ANY WAY, SHAPE, OR FORM        *
 * * REGARDLESS HOW IMPLICIT, EXPLICIT, OR OBSCURE IT   *
 * * IS. IF THERE IS ANYTHING QUESTIONABLE WITH REGARDS *
 * * TO THIS SOFTWARE BREAKING AND YOU GAIN A LOSS OF   *
 * * ANY NATURE, WE ARE NOT THE RESPONSIBLE PARTY. USE  *
 * * OF THIS SOFTWARE CREATES ACCEPTANCE OF THESE TERMS *
 * *                                                    *
 * * USE OF THIS CODE MUST RETAIN ALL COPYRIGHT NOTICES *
 * * AND LICENSES (MEANING THIS TEXT).                  *
 * *                                                    *
 * ******************************************************
 */
namespace Cryptool.CRC
{
    [Author("Matthäus Wander", "wander@cryptool.org", "Fachgebiet Verteilte Systeme, Universität Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "CRC32", "Cyclic Redundancy Check 32-Bit", null, "CRC/icon.png")]
    public class CRC : ICheckSumHash
    {
        #region Constants and private variables

        const int BUFSIZE = 1024;

        private ISettings settings = new CRCSettings();
        private CryptoolStream inputData;
        private byte[] outputData;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        private uint[] table;

        #endregion

        #region Public interface

        public CRC()
        {
        }

        public ISettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputData
        {
            get
            {
                if (inputData == null) { return null; }

                GuiLogMessage("Filename of input stream: " + inputData.FileName, NotificationLevel.Debug);

                CryptoolStream cs = new CryptoolStream();
                cs.OpenRead(inputData.FileName);
                listCryptoolStreamsOut.Add(cs);
                return cs;
            }
            set
            {
                inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.OutputData, "Hash value", "Output hash value as byte array", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] OutputData
        {
            get
            {
                if (outputData == null) { return null; }

                GuiLogMessage("Got request for hash (byte array)", NotificationLevel.Debug);
                return outputData;
            }
            set
            {
            }
        }

        [PropertyInfo(Direction.OutputData, "Hash value", "Output hash value as Stream", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputDataStream
        {
            get
            {
                if (outputData == null) { return null; }

                CryptoolStream cs = new CryptoolStream();
                listCryptoolStreamsOut.Add(cs);
                cs.OpenRead(this.GetPluginInfoAttribute().Caption, outputData);
                GuiLogMessage("Got request for hash (stream)", NotificationLevel.Debug);
                return cs;
            }
            set
            {
            }
        }

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
            if (inputData != null)
            {
                inputData.Close();
                inputData = null;
            }

            foreach (CryptoolStream cs in listCryptoolStreamsOut)
            {
                cs.Close();
            }
        }

        public void Execute()
        {
            ProgressChanged(0.0, 1.0);

            if (inputData == null)
            {
                GuiLogMessage("Received null value for input CryptoolStream, not processing.", NotificationLevel.Warning);
                return;
            }

            byte[] input = new byte[BUFSIZE];
            uint crc = 0xffffffff;
            int readCount;

            // read and process 1024 bytes long portions of input stream
            for(long bytesLeft = inputData.Length - inputData.Position; inputData.Position < inputData.Length; bytesLeft -= readCount)
            {
                ProgressChanged((double)inputData.Position / inputData.Length, 1.0);
                readCount = bytesLeft < input.Length ? (int)bytesLeft : input.Length;
                GuiLogMessage("Trying to fill working buffer with " + readCount + " bytes", NotificationLevel.Debug);
                readCount = inputData.Read(input, 0, readCount);

                for (int i = 0; i < readCount; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ input[i]);
                    crc = (uint)((crc >> 8) ^ table[index]);
                }
            }

            crc ^= 0xffffffff;

            outputData = new byte[4];

            outputData[0] = (byte)(crc >> 24);
            outputData[1] = (byte)(crc >> 16);
            outputData[2] = (byte)(crc >> 8);
            outputData[3] = (byte)(crc);

            ProgressChanged(1.0, 1.0);
            GuiLogMessage("CRC calculation has finished", NotificationLevel.Debug);

            OnPropertyChanged("OutputData");
            OnPropertyChanged("OutputDataStream");
        }

        /// <summary>
        /// This methods builds the lookup table depending on generator polynom to allow a fast hashing implementation.
        /// </summary>
        public void Initialize()
        {
            uint poly = 0xEDB88320; // reversed presentation of polynom 0x04C11DB7

            // build lookup table
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }

        public void Pause()
        {
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
