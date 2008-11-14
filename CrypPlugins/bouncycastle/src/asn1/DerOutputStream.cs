using System;
using System.IO;

using Org.BouncyCastle.Asn1.Utilities;

namespace Org.BouncyCastle.Asn1
{
    public class DerOutputStream
        : FilterStream
    {
        public DerOutputStream(Stream os) : base(os)
        {
        }

        private void WriteLength(
            int length)
        {
            if (length > 127)
            {
                int size = 1;
                uint val = (uint) length;

                while ((val >>= 8) != 0)
                {
                    size++;
                }

                WriteByte((byte)(size | 0x80));

                for (int i = (size - 1) * 8; i >= 0; i -= 8)
                {
                    WriteByte((byte)(length >> i));
                }
            }
            else
            {
                WriteByte((byte)length);
            }
        }

        internal void WriteEncoded(
            int		tag,
            byte[]	bytes)
        {
            WriteByte((byte) tag);
            WriteLength(bytes.Length);
            Write(bytes, 0, bytes.Length);
        }

		internal void WriteEncoded(
			int		tag,
			byte[]	bytes,
			int		offset,
			int		length)
		{
			WriteByte((byte) tag);
			WriteLength(length);
			Write(bytes, offset, length);
		}

		protected void WriteNull()
        {
            WriteByte(Asn1Tags.Null);
            WriteByte(0x00);
        }

        public virtual void WriteObject(
            object obj)
        {
            if (obj == null)
            {
                WriteNull();
            }
            else if (obj is Asn1Object)
            {
                ((Asn1Object)obj).Encode(this);
            }
            else if (obj is Asn1Encodable)
            {
                ((Asn1Encodable)obj).ToAsn1Object().Encode(this);
            }
            else
            {
                throw new IOException("object not Asn1Object");
            }
        }
    }
}
