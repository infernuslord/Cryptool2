/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

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

namespace Cryptool.PluginBase.IO
{
    public class CryptoolStream : Stream
    {
        private const string TempFileExtension = "csf";

        private string fileName;
        private FileStream fileStream;
        private bool isReadOnly;
        private bool isDisposed;
        private bool deleteOnDispose;

        public string FileName 
        {
            get { return this.fileName; }
        }

        private long position;
        public override long Position
        {
          get
          {
            try
            {
              if (isReadOnly) return position;
              else return fileStream.Position;
            }
            catch
            {
              return 0;
            }
          }
          set
          {
            position = value;
            fileStream.Position = value;
          }
        }

        //public long Length
        //{
        //    get { return this.fileStream.Length; }
        //}
        /// <summary>
        /// Length and Position have to be cached => performance issues:
        /// http://www.codeproject.com/KB/files/fastbinaryfileinput.aspx    
        /// </summary>
        private long length;
        public override long Length
        {
          get
          {
            try
            {
              if (isReadOnly) return length;
              else return fileStream.Length;
            }
            catch
            {
              return 0;
            }
          }
        }

        ~CryptoolStream()
        {
            Dispose(false);
        }

        public new void Dispose()
        {
            if (!isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected override void Dispose(bool publicDispose)
        {
            if (!isDisposed)
            {
                // public dispose code only
                if (publicDispose)
                {
                    base.Dispose(publicDispose);
                }

                // finalization and public dispose code
                isDisposed = true;
                Close();

                if (deleteOnDispose)
                {
                    File.Delete(fileName);
                }
            }
        }

        public override void SetLength(long value)
        {
          length = value;
          this.fileStream.SetLength(value);
        }

        #region read
        /// <summary>
        /// Create a new file with given bytes. After writing the bytes to file, the
        /// file will be reopend with FileAccess.Read.
        /// </summary>
        /// <param name="Plugin"></param>
        /// <param name="Bytes"></param>
        public void OpenRead(byte[] Bytes)
        {
          if (Bytes == null) throw new ArgumentException("Bytes");

          if (!Directory.Exists(DirectoryHelper.DirectoryLocalTemp)) Directory.CreateDirectory(DirectoryHelper.DirectoryLocalTemp);

          this.fileName = DirectoryHelper.GetNewTempFilePath(TempFileExtension);
          this.fileStream = new FileStream(this.fileName, FileMode.CreateNew);

          fileStream.Write(Bytes, 0, Bytes.Length);

          fileStream.Flush();
          fileStream.Close();

          fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          length = fileStream.Length;
          position = fileStream.Position;
          isReadOnly = true;
          deleteOnDispose = true;
        }

        [Obsolete("pluginCaption is not used anymore, use OpenRead(byte[]) instead")]
        public void OpenRead(string pluginCaption, byte[] Bytes)
        {
          OpenRead(Bytes);
        }

        public void OpenRead(string fileName)
        {
            this.fileName = fileName;
            this.Close();
            this.fileStream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            length = fileStream.Length;
            isReadOnly = true; 
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
          this.position += count;
          return this.fileStream.Read(buffer, offset, count);
        }

        public byte[] Read(int offset, int count)
        {
            this.position += count;
            byte[] buffer = new byte[count];
            this.fileStream.Read(buffer, offset, count);
            return buffer;
        }

        public byte Read()
        {
            this.position++;
            return (byte) this.fileStream.ReadByte();
        }
        #endregion read

        # region write
        [Obsolete("pluginCaption is not used anymore, use OpenWrite() instead")]
        public void OpenWrite(string pluginCaption)
        {
            OpenWrite();
        }

        public void OpenWrite()
        {
            if (this.fileStream != null)
                this.fileStream.Position = 0;
            else
            {
                if (!Directory.Exists(DirectoryHelper.DirectoryLocalTemp)) Directory.CreateDirectory(DirectoryHelper.DirectoryLocalTemp);

                this.fileName = DirectoryHelper.GetNewTempFilePath(TempFileExtension);
                this.fileStream = new FileStream(this.fileName, FileMode.CreateNew);
                File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Temporary);
            }
            isReadOnly = false;
            deleteOnDispose = true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
          this.fileStream.Write(buffer, offset, count);
        }   

        public void Write(byte[] buffer)
        {
            this.fileStream.Write(buffer, (int)this.fileStream.Position, buffer.Length);
        }

        public void Write(byte value)
        {
            this.fileStream.WriteByte(value);
        }
        # endregion write

        public override void Close()
        {
            if (this.fileStream != null)
            {                
                this.fileStream.Close();
                this.fileStream = null;
            }
        }

        public override bool CanRead
        {
          get { return fileStream != null && fileStream.CanRead; }
        }

        public override bool CanSeek
        {
          get { return fileStream != null && fileStream.CanSeek; }
        }

        public override bool CanWrite
        {
          get { return fileStream != null && fileStream.CanWrite; }
        }

        public override void Flush()
        {
          if (fileStream != null) fileStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
          this.position = offset; // BROKEN: does not support other origins than Begin
          return this.fileStream.Seek(offset, origin);
        }

    }
}
