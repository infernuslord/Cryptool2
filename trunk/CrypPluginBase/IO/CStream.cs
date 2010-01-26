using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.IO
{
    /// <summary>
    /// CStream reference to membuffer/swapfile. CStream instances can be passed safely
    /// to other plugins or components.
    /// To create a new CStream, please use CStreamWriter.
    /// 
    /// You MAY create 1 to n readers upon one CStream. They do not interfere.
    /// You MAY pass the CStream object safely to other components. They can create new
    /// readers, but not modify the stream.
    /// 
    /// For more usage notes, see CStreamWriter and CStreamReader.
    /// </summary>
    public class CStream
    {

        private CStreamWriter _writer;

        internal CStream(CStreamWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Create a new instance to read from this CStream.
        /// </summary>
        public CStreamReader CreateReader()
        {
            return new CStreamReader(_writer);
        }

        /// <summary>
        /// Attempts to read 4096 bytes from CStream, interpreted as string using default encoding.
        /// May return less if CStream doesn't contain that many bytes.
        /// </summary>
        public override string ToString()
        {
            using (CStreamReader reader = CreateReader())
            {
                byte[] buf = new byte[4096];
                int read = reader.Read(buf);

                return Encoding.Default.GetString(buf, 0, read);
            }
        }
    }
}
