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
using System.IO;
using System.Threading;

namespace Cryptool.PluginBase.IO
{
    public class CStreamWriter : Stream
    {
        #region Private fields and constructors

        private readonly object _monitor;

        private byte[] _buffer;
        private FileStream _writeStream;
        private string _writePath;
        private int _writePtr;

        // Need length information of closed file streams.
        private bool _closed;
        private long _closedLength;

        /// <summary>
        /// Init CStreamWriter with 64 KB memory buffer
        /// </summary>
        public CStreamWriter() : this(64*1024)
        {
        }

        /// <summary>
        /// Init CStreamWriter with custom size memory buffer (in bytes)
        /// </summary>
        /// <param name="bufSize"></param>
        public CStreamWriter(int bufSize)
        {
            _buffer = new byte[bufSize];
            _monitor = new object();
        }

        ~CStreamWriter()
        {
            if (IsSwapped)
            {
                _writeStream.Close();
                File.Delete(_writePath);
            }
        }

        #endregion

        #region Public properties

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Returns whether the underlying buffer is swapped out to filesystem or not.
        /// </summary>
        public bool IsSwapped
        {
            get
            {
                return _writeStream != null;
            }
        }

        public override long Length
        {
            get
            {
                if (IsSwapped)
                {
                    return _closed ? _closedLength : _writeStream.Length;
                }
                else
                {
                    return _writePtr;
                }
            }
        }

        public override long Position
        {
            get
            {
                if (IsSwapped)
                {
                    return _closed ? _closedLength : _writeStream.Position;
                }
                else
                {
                    return _writePtr;
                }
            }

            // can not seek
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Public methods

        public override void Close()
        {
            lock (_monitor)
            {
                base.Close();

                if (IsSwapped)
                {
                    _closed = true;
                    _closedLength = _writeStream.Length;
                    _writeStream.Close();
                }

                Monitor.PulseAll(_monitor);
            }
        }

        public CStreamReader CreateReader()
        {
            return new CStreamReader(this);
        }

        public override void Flush()
        {
            lock (_monitor)
            {
                if (IsSwapped)
                {
                    _writeStream.Flush();
                }

                // wakeup readers
                Monitor.PulseAll(_monitor);
            }
        }

        /// <summary>
        /// can not read, get CStreamReader instead
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        // can not seek
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        // can not seek
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// convenience method for Write(byte[], int, int)
        /// </summary>
        /// <param name="buf"></param>
        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// write to mem/file buffer (switches transparently if required)
        /// </summary>
        public override void Write(byte[] buf, int offset, int count)
        {
            lock(_monitor)
            {
                if (!IsSwapped && hasMemorySpace(count))
                {
                    Array.Copy(buf, offset, _buffer, _writePtr, count);
                    _writePtr += count;
                }
                else
                {
                    if (!IsSwapped)
                    {
                        createSwapFile();
                        _writeStream.Write(_buffer, 0, _writePtr);
                        _writeStream.Flush(); // required before announcing swap event
                        _buffer = null;

                        if (SwitchToSwapEvent != null)
                            SwitchToSwapEvent();
                    }

                    _writeStream.Write(buf, offset, count);
                }

                // don't pulse monitor, wait for flush
            }
        }

        #endregion

        #region Private methods

        private bool hasMemorySpace(int count)
        {
            return _writePtr + count <= _buffer.Length;
        }

        private void createSwapFile()
        {
            _writePath = DirectoryHelper.GetNewTempFilePath();
            _writeStream = new FileStream(_writePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
        }

        #endregion

        #region Internal members for stream readers

        internal object InternalMonitor
        {
            get { return _monitor; }
        }

        internal delegate void UseSwapFileHandler();

        internal event UseSwapFileHandler SwitchToSwapEvent;

        internal byte[] MemBuff
        {
            get { return _buffer; }
        }

        internal string FilePath
        {
            get { return _writePath; }
        }

        #endregion
    }
}
