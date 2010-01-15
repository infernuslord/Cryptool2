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
    public class CStreamReader : Stream
    {
        private readonly CStreamWriter _writer;

        private FileStream _readStream;
        private int _readPtr = 0;

        public CStreamReader(CStreamWriter writer)
        {
            _writer = writer;

            if (_writer.IsSwapped)
            {
                SwapHandler();
            }
            else
            {
                _writer.SwitchToSwapEvent += SwapHandler;
            }
        }

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

        public override void Flush()
        {
            throw new NotSupportedException();
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
            set
            {
                throw new NotSupportedException();
            }
        }

        private int availableRead()
        {
            long avail = _writer.Position - (IsSwapped ? _readStream.Position : _readPtr);
            return (int)Math.Min(int.MaxValue, avail);
        }

        /// <summary>
        /// Convenience method for Read(byte[], int, int)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// POSIX-like read: Reads 1 to count amount of bytes into given buffer.
        /// Blocks until at least 1 byte has been read. Does not guarantee to read the requested amount of data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>amount of bytes that has been read into buffer</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_writer.InternalMonitor)
            {
                while (availableRead() < 1)
                {
                    Monitor.Wait(_writer.InternalMonitor);
                }

                int readAttempt = Math.Min(availableRead(), count);

                if (IsSwapped)
                {
                    // MUST NOT block, otherwise we're potentially deadlocked
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

        public int ReadFully(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        // seeking is currently not supported
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public bool IsSwapped
        {
            get
            {
                return _readStream != null;
            }
        }

        private void SwapHandler()
        {
            _readStream = new FileStream(_writer.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (_readPtr > 0)
            {
                _readStream.Seek(_readPtr, SeekOrigin.Begin);
            }
        }
    }
}
