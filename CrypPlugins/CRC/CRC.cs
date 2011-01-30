/*
   Copyright 2009-2010 Matthäus Wander, University of Duisburg-Essen

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
        private CStreamWriter outputStreamWriter;

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

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be hashed", "", true, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream InputStream
        {
            get;
            set;
            }

        [PropertyInfo(Direction.OutputData, "Hash value", "Output hash value as Stream", "", false, false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
            }

            set
            {
            }
        }

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
            if (outputStreamWriter != null)
            {
                outputStreamWriter.Dispose();
                outputStreamWriter = null;
            }
            }

        public void Execute()
        {
            ProgressChanged(0.0, 1.0);

            if (InputStream == null)
            {
                GuiLogMessage("Received null value for input CStream, not processing.", NotificationLevel.Warning);
                return;
            }

            byte[] input = new byte[BUFSIZE];
            uint crc = 0xffffffff;

            using (CStreamReader reader = InputStream.CreateReader())
            {
                // read and process portions of up to BUFSIZE bytes of input stream
                int readCount;
                while ((readCount = reader.Read(input)) > 0)
                {
                for (int i = 0; i < readCount; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ input[i]);
                    crc = (uint)((crc >> 8) ^ table[index]);
                }

                    ProgressChanged((double)reader.Position / reader.Length, 1.0);
            }
            }

            crc ^= 0xffffffff;

            byte[] outputData = new byte[4];

            outputData[0] = (byte)(crc >> 24);
            outputData[1] = (byte)(crc >> 16);
            outputData[2] = (byte)(crc >> 8);
            outputData[3] = (byte)(crc);

            // create new one
            outputStreamWriter = new CStreamWriter(outputData);

            ProgressChanged(1.0, 1.0);
            OnPropertyChanged("OutputStream");
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
