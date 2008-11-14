using System;

namespace Org.BouncyCastle.Bcpg.Attr
{
	/// <remarks>Basic type for a image attribute packet.</remarks>
    public class ImageAttrib
		: UserAttributeSubpacket
    {
        private int     hdrLength;
        private int     _version;
        private int     _encoding;
        private byte[]  imageData;

        public ImageAttrib(
            byte[] data)
            : base(UserAttributeSubpacketTag.ImageAttribute, data)
        {
            hdrLength = ((data[1] & 0xff) << 8) | (data[0] & 0xff);
            _version = data[2] & 0xff;
            _encoding = data[3] & 0xff;

            imageData = new byte[data.Length - hdrLength];
            Array.Copy(data, hdrLength, imageData, 0, imageData.Length);
        }

        public int Version
        {
			get { return _version; }
        }

        public int Encoding
        {
			get { return _encoding; }
        }

		public byte[] GetImageData()
        {
            return imageData;
        }
    }
}
