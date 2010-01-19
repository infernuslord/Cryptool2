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
using System.Diagnostics;

namespace Cryptool.PluginBase.IO
{
    /// <summary>
    /// Read from a corresponding CStreamWriter.
    /// </summary>
    public class CStream : Stream, IDisposable
    {
        #region Private fields and constructors

        private readonly CStreamWriter _writer;

        private FileStream _readStream;
        private int _readPtr;

        private bool _disposed;

        public CStream(CStreamWriter writer)
        {
            _writer = writer;
            _writer.ShutdownEvent += shutdownHandler;
            _writer.SwapEvent += swapHandler;

            if (_writer.IsSwapped)
            {
                swapHandler();
            }
        }

        ~CStream()
        {
            Dispose(false);
        }

        #endregion

        #region Public properties

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public bool IsSwapped
        {
            get
            {
                return _readStream != null;
            }
        }

        public override long Length
        {
            get
            {
                return _writer.Length;
            }
        }

        public override long Position
        {
            get
            {
                if (IsSwapped)
                    return _readStream.Position;
                else
                    return _readPtr;
            }

            // Seeking currently not supported.
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Public methods

        public override void Close()
        {
            Dispose();
        }

        public new void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Convenience method for Read(byte[] buf, 0, buf.Length)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Read POSIX-like 1 to count amount of bytes into given byte array.
        /// Blocks until at least 1 byte has been read or underlying stream has been closed.
        /// Does not guarantee to read the requested amount of data, can read less.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>amount of bytes that has been read into buffer or 0 if EOF</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_writer.InternalMonitor)
            {
                int available;

                while ((available = availableRead()) < 1)
                {
                    if (_writer.IsClosed)
                        return 0; // EOF

                    Monitor.Wait(_writer.InternalMonitor);
                }

                int readAttempt = Math.Min(available, count);

                if (IsSwapped)
                {
                    // MUST NOT block, otherwise we're potentially deadlocked
                    Debug.Assert(_writer.Length - _readStream.Position > 0);
                    return _readStream.Read(buffer, offset, readAttempt);
                }
                else
                {
                    Array.Copy(_writer.MemBuff, _readPtr, buffer, offset, readAttempt);
                    _readPtr += readAttempt;

                    return readAttempt;
                }
            }
        }

        /// <summary>
        /// Convenience method for Read (non-POSIX): block until array is full or EOF occurs.
        /// </summary>
        public int ReadFully(byte[] buffer)
        {
            return ReadFully(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Convenience method for Read (non-POSIX): block until required amount of data has
        /// been retrieved or EOF occurs.
        /// </summary>
        public int ReadFully(byte[] buffer, int offset, int count)
        {
            int readSum = 0;
            while (readSum < count)
            {
                int read = Read(buffer, offset, (count - readSum));
                
                if (read == 0) // EOF
                    return readSum;

                readSum += read;
            }

            return readSum;
        }

        /// <summary>
        /// Seeking is currently not supported
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reader can't write.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Private/protected methods

        private int availableRead()
        {
            long avail = _writer.Position - (IsSwapped ? _readStream.Position : _readPtr);
            return (int)Math.Min(int.MaxValue, avail);
        }

        /// <summary>
        /// Switch from membuff to swapfile
        /// </summary>
        private void swapHandler()
        {
            _readStream = new FileStream(_writer.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (_readPtr > 0)
            {
                _readStream.Seek(_readPtr, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Writer shutting down, release file.
        /// </summary>
        private void shutdownHandler()
        {
            if (IsSwapped)
            {
                _readStream.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
            }

            if (IsSwapped)
            {
                _readStream.Close();
                _readStream = null;
            }

            _writer.ShutdownEvent -= shutdownHandler;
            _writer.SwapEvent -= swapHandler;

            _disposed = true;
        }

        #endregion
    }
}
