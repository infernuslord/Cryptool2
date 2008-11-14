using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    abstract class LimitedInputStream
        : BaseInputStream
    {
        protected readonly Stream _in;

        internal LimitedInputStream(
            Stream inStream)
        {
            this._in = inStream;
        }

		internal Stream GetUnderlyingStream()
        {
            return _in;
        }

		protected void SetParentEofDetect(bool on)
        {
            if (_in is IndefiniteLengthInputStream)
            {
                ((IndefiniteLengthInputStream)_in).SetEofOn00(on);
            }
        }
    }
}
