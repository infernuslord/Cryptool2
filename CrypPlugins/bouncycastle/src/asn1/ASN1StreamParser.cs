using System;
using System.IO;

namespace Org.BouncyCastle.Asn1
{
	public class Asn1StreamParser
	{
		Stream _in;

		private int _limit;
		private bool _eofFound;

		public Asn1StreamParser(
			Stream inStream)
			: this(inStream, int.MaxValue)
		{
		}

		public Asn1StreamParser(
			Stream	inStream,
			int		limit)
		{
			if (!inStream.CanRead)
				throw new ArgumentException("Expected stream to be readable", "inStream");

			this._in = inStream;
			this._limit = limit;
		}

		public Asn1StreamParser(
			byte[] encoding)
			: this(new MemoryStream(encoding, false), encoding.Length)
		{
		}

		internal Stream GetParentStream()
		{
			return _in;
		}

		private int ReadLength()
		{
			int length = _in.ReadByte();
			if (length < 0)
				throw new EndOfStreamException("EOF found when length expected");

			if (length == 0x80)
				return -1;      // indefinite-length encoding

			if (length > 127)
			{
				int size = length & 0x7f;

				if (size > 4)
					throw new IOException("DER length more than 4 bytes");

				length = 0;
				for (int i = 0; i < size; i++)
				{
					int next = _in.ReadByte();

					if (next < 0)
						throw new EndOfStreamException("EOF found reading length");

					length = (length << 8) + next;
				}

				if (length < 0)
					throw new IOException("corrupted steam - negative length found");

				if (length >= _limit)   // after all we must have read at least 1 byte
					throw new IOException("corrupted steam - out of bounds length found");
			}

			return length;
		}

		public IAsn1Convertible ReadObject()
		{
			int tag = _in.ReadByte();
			if (tag == -1)
			{
				if (_eofFound)
					throw new EndOfStreamException("attempt to read past end of file.");

				_eofFound = true;

				return null;
			}

			// turn of looking for "00" while we resolve the tag
			Set00Check(false);

			//
			// calculate tag number
			//
			int baseTagNo = tag & ~Asn1Tags.Constructed;
			int tagNo = baseTagNo;

			if ((tag & Asn1Tags.Tagged) != 0)
			{
				tagNo = tag & 0x1f;

				//
				// with tagged object tag number is bottom 5 bits, or stored at the start of the content
				//
				if (tagNo == 0x1f)
				{
					tagNo = 0;

					int b = _in.ReadByte();

					while ((b >= 0) && ((b & 0x80) != 0))
					{
						tagNo |= (b & 0x7f);
						tagNo <<= 7;
						b = _in.ReadByte();
					}

					if (b < 0)
					{
						_eofFound = true;

						throw new EndOfStreamException("EOF encountered inside tag value.");
					}

					tagNo |= (b & 0x7f);
				}
			}

			//
			// calculate length
			//
			int length = ReadLength();

			if (length < 0)  // indefinite length
			{
				IndefiniteLengthInputStream indIn = new IndefiniteLengthInputStream(_in);

				switch (baseTagNo)
				{
					case Asn1Tags.Null:
						while (indIn.ReadByte() >= 0)
						{
							// make sure we skip to end of object
						}
						return BerNull.Instance;
					case Asn1Tags.OctetString:
						return new BerOctetStringParser(new Asn1ObjectParser(tag, tagNo, indIn));
					case Asn1Tags.Sequence:
						return new BerSequenceParser(new Asn1ObjectParser(tag, tagNo, indIn));
					case Asn1Tags.Set:
						return new BerSetParser(new Asn1ObjectParser(tag, tagNo, indIn));
					default:
						return new BerTaggedObjectParser(tag, tagNo, indIn);
				}
			}
			else
			{
				DefiniteLengthInputStream defIn = new DefiniteLengthInputStream(_in, length);

				switch (baseTagNo)
				{
					case Asn1Tags.Integer:
						return new DerInteger(defIn.ToArray());
					case Asn1Tags.Null:
						defIn.ToArray(); // make sure we read to end of object bytes.
						return DerNull.Instance;
					case Asn1Tags.ObjectIdentifier:
						return new DerObjectIdentifier(defIn.ToArray());
					case Asn1Tags.OctetString:
						return new DerOctetString(defIn.ToArray());
					case Asn1Tags.Sequence:
						return new DerSequence(loadVector(defIn.ToArray())).Parser;
					case Asn1Tags.Set:
						return new DerSet(loadVector(defIn.ToArray())).Parser;
					default:
						return new BerTaggedObjectParser(tag, tagNo, defIn);
				}
			}
		}

		private void Set00Check(
			bool enabled)
		{
			if (_in is IndefiniteLengthInputStream)
			{
				((IndefiniteLengthInputStream) _in).SetEofOn00(enabled);
			}
		}

		private Asn1EncodableVector loadVector(byte[] bytes)
		{
			Asn1EncodableVector v = new Asn1EncodableVector();
			Asn1InputStream aIn = new Asn1InputStream(bytes);

			Asn1Object obj;
			while ((obj = aIn.ReadObject()) != null)
			{
				v.Add(obj);
			}

			return v;
		}
	}
}
