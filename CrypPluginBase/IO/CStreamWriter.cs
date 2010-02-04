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
using System.Runtime.ConstrainedExecution;

namespace Cryptool.PluginBase.IO
{
    /// <summary>
    /// Create a stream to pass data with arbitrary size to another CT2 plugin.
    /// The stream is internally backed by a non-cyclic memory buffer and switches automagically to a
    /// temporary file if the membuff exceeds a certain size. Please note that the buffer does not
    /// forget old data, therefore you can derive an arbitary number of stream readers at any time.
    /// 
    /// <para>You SHOULD Flush() the stream when you're writing large data amounts and expect the readers
    /// to perform intermediate processing before writing has been finished.
    /// You MUST Close() the stream when you're finished with writing, otherwise the reader will block
    /// and wait for more data infinitely.</para>
    /// 
    /// <para>You SHOULD Dispose() the stream when you're done using it (or use the C# "using" keyword) in
    /// order to remove the temporary swapfile, however if you forget to, the GC will clean up for you.</para>
    /// </public>
    public class CStreamWriter : Stream, IDisposable
    {
        #region Private fields and constructors

        internal const int FileBufferSize = 8192;

        private readonly object _monitor;
        private bool _closed;
        private bool _disposed;

        // membuff
        private byte[] _buffer;
        private int _bufPtr;

        // swapfile
        private FileStream _writeStream;
        private string _filePath;

        /// <summary>
        /// Init CStreamWriter with 64 KB memory buffer
        /// </summary>
        public CStreamWriter() : this(65536)
        {
        }

        /// <summary>
        /// Init CStreamWriter with custom size memory buffer (in bytes)
        /// </summary>
        public CStreamWriter(int bufSize)
        {
            _buffer = new byte[bufSize];
            _monitor = new object();

            CStream = new CStream(this);
        }

        /// <summary>
        /// Init CStreamWriter and copy some data to memory buffer.
        /// 
        /// Please note: Data is *copied* from passed buffer to internal memory buffer.
        /// </summary>
        /// <param name="buf">Pre-initialized byte array which is copied into internal membuff</param>
        /// <param name="autoClose">close after initialization</param>
        public CStreamWriter(byte[] buf, bool autoClose) : this(buf.Length)
        {
            Array.Copy(buf, _buffer, buf.Length);
            _bufPtr = buf.Length;

            if (autoClose)
            {
                Close();
            }
        }

        #endregion

        #region Public properties

        public CStream CStream
        {
            get;
            private set;
        }

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
        /// File path of swapfile (if any).
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Has the writer stream been marked as closed?
        /// 
        /// Please note: closing the write stream is not equivalent with disposing it.
        /// Readers can still read from a closed stream, but not from a disposed one.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return _closed;
            }
            set
            {
                if (value)
                    Close();
            }
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
                    // TODO: cache FileStream property
                    return _writeStream.Length;
                }
                else
                {
                    return _bufPtr;
                }
            }
        }

        public override long Position
        {
            get
            {
                if (IsSwapped)
                {
                    // TODO: cache FileStream property
                    return _writeStream.Position;
                }
                else
                {
                    return _bufPtr;
                }
            }

            // writer can not seek
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// You MUST call Close() when you're done writing or the readers will be stuck in an infinite loop.
        /// 
        /// Please note: Contrary to the API description of Stream.Close() this method DOES NOT release all
        /// resources associated to this CStream. Call Dispose() to release resources or let the GC call
        /// the finalizer.
        /// </summary>
        public override void Close()
        {
            /*
             * Note 1: We're NOT following the pattern of the Stream class to implement cleanup code in
             * Dispose() without touching Close(), because we explicitly want to mark a stream as closed
             * without disposing the underlying mem/file buffer. That's why we also don't call base.Close().
             * 
             * Note 2: Closing the CStream does not automatically close the underlying file handle, as
             * the file is marked as DeleteOnClose and may be removed too early. File is closed when the
             * CStreamWriter is disposed or garbage collected.
             */

            // do nothing if already closed
            if (_closed)
                return;

            _closed = true;

            Flush();
        }

        /// <summary>
        /// Explicitly destroy object.
        /// </summary>
        public new void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            base.Dispose();

            if (IsSwapped)
            {
                _writeStream.Close(); // release file handle
                _writeStream = null;
            }

            _buffer = null;
            SwapEvent = null;
        }

        /// <summary>
        /// Flush any caches, announce buffer freshness to readers.
        /// </summary>
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
        /// Can not read, use CStream instead.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writer can not seek.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writer can not seek.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Convenience method for Write(byte[] buf, 0, buf.Length)
        /// </summary>
        /// <param name="buf"></param>
        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Write to mem/file buffer (switches transparently if required)
        /// </summary>
        public override void Write(byte[] buf, int offset, int count)
        {
            if (_closed || _disposed)
                throw new InvalidOperationException("Can't write, CStream already closed/disposed");

            lock(_monitor)
            {
                if (!IsSwapped && hasMemorySpace(count))
                {
                    Array.Copy(buf, offset, _buffer, _bufPtr, count);
                    _bufPtr += count;
                }
                else
                {
                    if (!IsSwapped)
                    {
                        createSwapFile();
                        _writeStream.Write(_buffer, 0, _bufPtr);
                        _writeStream.Flush(); // ensure reader can seek before announcing swap event
                        _buffer = null;

                        if (SwapEvent != null)
                            SwapEvent();
                    }

                    _writeStream.Write(buf, offset, count);
                }

                // don't pulse monitor, wait for flush
            }
        }

        #endregion

        #region Private/protected methods

        private bool hasMemorySpace(int count)
        {
            return _bufPtr + count <= _buffer.Length;
        }

        private void createSwapFile()
        {
            _filePath = DirectoryHelper.GetNewTempFilePath();
            _writeStream = new FileStream(_filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, FileBufferSize, FileOptions.DeleteOnClose);
        }

        #endregion

        #region Internal members for stream readers

        internal bool IsDisposed
        {
            get { return _disposed; }
        }

        internal object InternalMonitor
        {
            get { return _monitor; }
        }

        internal delegate void ReaderCallback();

        internal event ReaderCallback SwapEvent;

        internal byte[] MemBuff
        {
            get { return _buffer; }
        }

        #endregion

    }
}
