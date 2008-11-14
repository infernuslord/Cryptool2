using System;
using System.IO;
using Org.BouncyCastle.Bcpg.Attr;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg
{
    /**
    * reader for user attribute sub-packets
    */
    public class UserAttributeSubpacketsParser
    {
        private readonly Stream input;

		public UserAttributeSubpacketsParser(
            Stream input)
        {
            this.input = input;
        }

		private void ReadFully(
            byte[]	buf,
            int		off,
            int		len)
        {
			if (Streams.ReadFully(input, buf, off, len) < len)
				throw new EndOfStreamException();
		}

		public UserAttributeSubpacket ReadPacket()
        {
            int l = input.ReadByte();

            if (l < 0)
            {
                return null;
            }
            int bodyLen = 0;
            if (l < 192)
            {
                bodyLen = l;
            }
            else if (l < 223)
            {
                bodyLen = ((l - 192) << 8) + (input.ReadByte()) + 192;
            }
            else if (l == 255)
            {
                bodyLen = (input.ReadByte() << 24) | (input.ReadByte() << 16)
                    |  (input.ReadByte() << 8)  | input.ReadByte();
            }
            int tag = input.ReadByte();
            if (tag < 0)
            {
                throw new EndOfStreamException("unexpected EOF reading user attribute sub packet");
            }
            byte[] data = new byte[bodyLen - 1];
            this.ReadFully(data, 0, data.Length);
            UserAttributeSubpacketTag type = (UserAttributeSubpacketTag) tag;
            switch (type)
            {
                case UserAttributeSubpacketTag.ImageAttribute:
                    return new ImageAttrib(data);
            }
            return new UserAttributeSubpacket(type, data);
        }
    }
}
