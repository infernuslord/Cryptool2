using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1.Utilities;

namespace Org.BouncyCastle.Asn1
{
    /**
     * a general purpose ASN.1 decoder - note: this class differs from the
     * others in that it returns null after it has read the last object in
     * the stream. If an ASN.1 Null is encountered a Der/BER Null object is
     * returned.
     */
    public class Asn1InputStream
        : FilterStream
    {
        private class EosAsn1Object
			: Asn1Object
        {
            internal override void Encode(
				DerOutputStream derOut)
            {
                throw new IOException("Eeek!");
            }

			protected override bool Asn1Equals(
				Asn1Object asn1Object)
			{
				return asn1Object is EosAsn1Object;
			}

			protected override int Asn1GetHashCode()
			{
				return 0;
			}
        }

		private static readonly Asn1Object EndOfStream = new EosAsn1Object();

		internal bool eofFound;
		internal int limit = int.MaxValue;

		public Asn1InputStream(
			Stream inputStream)
            : base(inputStream)
        {
        }

		/**
		 * Create an ASN1InputStream where no DER object will be longer than limit.
		 *
		 * @param input stream containing ASN.1 encoded data.
		 * @param limit maximum size of a DER encoded object.
		 */
		public Asn1InputStream(
			Stream	inputStream,
			int		limit)
			: base(inputStream)
		{
			this.limit = limit;
		}

		/**
		 * Create an ASN1InputStream based on the input byte array. The length of DER objects in
		 * the stream is automatically limited to the length of the input array.
		 *
		 * @param input array containing ASN.1 encoded data.
		 */
		public Asn1InputStream(
			byte[] input)
			: this(new MemoryStream(input, false), input.Length)
		{
		}

		internal int ReadLength()
        {
            int length = ReadByte();
            if (length < 0)
            {
                throw new IOException("EOF found when length expected");
            }

            if (length == 0x80)
            {
                return -1;      // indefinite-length encoding
            }

            if (length > 127)
            {
                int size = length & 0x7f;

                if (size > 4)
                {
                    throw new IOException("DER length more than 4 bytes");
                }

                length = 0;
                for (int i = 0; i < size; i++)
                {
                    int next = ReadByte();

                    if (next < 0)
                    {
                        throw new IOException("EOF found reading length");
                    }

                    length = (length << 8) + next;
                }

                if (length < 0)
                {
                    throw new IOException("corrupted steam - negative length found");
                }

				if (length >= limit)   // after all we must have read at least 1 byte
				{
					throw new IOException("corrupted steam - out of bounds length found");
				}
			}

			return length;
        }

        internal void ReadFully(
            byte[]  bytes)
        {
            int left = bytes.Length;
            if (left == 0)
            {
                return;
            }

			int len;
			while ((len = Read(bytes, bytes.Length - left, left)) > 0)
            {
                if ((left -= len) == 0)
                {
                    return;
                }
            }

            if (left != 0)
            {
                throw new EndOfStreamException("EOF encountered in middle of object");
            }
        }

        /**
         * build an object given its tag and a byte stream to construct it
         * from.
         */
        internal Asn1Object BuildObject(
            int		tag,
			int		tagNo,
            byte[]	bytes)
        {
            if ((tag & Asn1Tags.Application) != 0)
            {
				return new DerApplicationSpecific(tagNo, bytes);
			}

			switch (tag)
            {
				case Asn1Tags.Null:
					return DerNull.Instance;
				case Asn1Tags.Sequence | Asn1Tags.Constructed:
				{
					Asn1EncodableVector v = BuildDerEncodableVector(bytes);
					return new DerSequence(v);
				}
				case Asn1Tags.Set | Asn1Tags.Constructed:
				{
					Asn1EncodableVector v = BuildDerEncodableVector(bytes);
					return new DerSet(v, false);
				}
				case Asn1Tags.Boolean:
					return new DerBoolean(bytes);
				case Asn1Tags.Integer:
					return new DerInteger(bytes);
				case Asn1Tags.Enumerated:
					return new DerEnumerated(bytes);
				case Asn1Tags.ObjectIdentifier:
					return new DerObjectIdentifier(bytes);
				case Asn1Tags.BitString:
				{
					int padBits = bytes[0];
					byte[] data = new byte[bytes.Length - 1];
					Array.Copy(bytes, 1, data, 0, bytes.Length - 1);
					return new DerBitString(data, padBits);
				}
				case Asn1Tags.NumericString:
					return new DerNumericString(bytes);
				case Asn1Tags.Utf8String:
					return new DerUtf8String(bytes);
				case Asn1Tags.PrintableString:
					return new DerPrintableString(bytes);
				case Asn1Tags.IA5String:
					return new DerIA5String(bytes);
				case Asn1Tags.T61String:
					return new DerT61String(bytes);
				case Asn1Tags.VisibleString:
					return new DerVisibleString(bytes);
				case Asn1Tags.GeneralString:
					return new DerGeneralString(bytes);
				case Asn1Tags.UniversalString:
					return new DerUniversalString(bytes);
				case Asn1Tags.BmpString:
					return new DerBmpString(bytes);
				case Asn1Tags.OctetString:
					return new DerOctetString(bytes);
				case Asn1Tags.OctetString | Asn1Tags.Constructed:
					return BuildDerConstructedOctetString(bytes);
				case Asn1Tags.UtcTime:
					return new DerUtcTime(bytes);
				case Asn1Tags.GeneralizedTime:
					return new DerGeneralizedTime(bytes);
				default:
				{
					//
					// with tagged object tag number is bottom 5 bits
					//
					if ((tag & Asn1Tags.Tagged) != 0)
					{
						bool isImplicit = ((tag & Asn1Tags.Constructed) == 0);

						if (bytes.Length == 0)        // empty tag!
						{
							Asn1Encodable ae = isImplicit
								?	(Asn1Encodable) DerNull.Instance
								:	new DerSequence();

							return new DerTaggedObject(false, tagNo, ae);
						}

						//
						// simple type - implicit... return an octet string
						//
						if (isImplicit)
						{
							return new DerTaggedObject(false, tagNo, new DerOctetString(bytes));
						}

						Asn1InputStream aIn = new Asn1InputStream(bytes);
						Asn1Encodable dObj = aIn.ReadObject();


						// explicitly tagged (probably!) - if it isn't we'd have to
						// tell from the context

						//if (aIn.available() == 0)
						if (aIn.Position == bytes.Length) //FIXME?
						{
							return new DerTaggedObject(tagNo, dObj);
						}

						//
						// another implicit object, we'll create a sequence...
						//
						Asn1EncodableVector v = new Asn1EncodableVector();

						while (dObj != null)
						{
							v.Add(dObj);
							dObj = aIn.ReadObject();
						}

						return new DerTaggedObject(false, tagNo, new DerSequence(v));
					}

					return new DerUnknownTag(tag, bytes);
				}
            }
        }

		/**
         * read a string of bytes representing an indefinite length object.
         */
        private byte[] ReadIndefiniteLengthFully()
        {
            MemoryStream bOut = new MemoryStream();
			int b1 = ReadByte();

			int b;
			while ((b = ReadByte()) >= 0)
            {
                if (b1 == 0 && b == 0) break;

				bOut.WriteByte((byte) b1);
                b1 = b;
            }

			return bOut.ToArray();
        }

		private BerOctetString BuildConstructedOctetString(
			Asn1Object sentinel)
        {
            ArrayList octs = new ArrayList();
			Asn1Object o;

			while ((o = ReadObject()) != sentinel)
            {
				octs.Add(o);
            }

			return new BerOctetString(octs);
        }

		private BerOctetString BuildDerConstructedOctetString(
			byte[] input)
		{
			Asn1InputStream aIn = new Asn1InputStream(input);

			return aIn.BuildConstructedOctetString(null);
		}

		private Asn1EncodableVector BuildEncodableVector(
			Asn1Object sentinel)
		{
			Asn1EncodableVector v = new Asn1EncodableVector();
			Asn1Object o;

			while ((o = ReadObject()) != sentinel)
			{
				v.Add(o);
			}

			return v;
		}

		private Asn1EncodableVector BuildDerEncodableVector(
			byte[] input)
		{
			Asn1InputStream aIn = new Asn1InputStream(input);

			return aIn.BuildEncodableVector(null);
		}

		public Asn1Object ReadObject()
        {
            int tag = ReadByte();
            if (tag == -1)
            {
                if (eofFound)
                {
                    throw new EndOfStreamException("attempt to read past end of file.");
                }

                eofFound = true;

                return null;
            }

			int tagNo = 0;

			if ((tag & Asn1Tags.Tagged) != 0 || (tag & Asn1Tags.Application) != 0)
			{
				tagNo = ReadTagNumber(tag);
			}

			int length = ReadLength();

			if (length < 0)    // indefinite length method
            {
                switch (tag)
                {
					case Asn1Tags.Null:
						return BerNull.Instance;
					case Asn1Tags.Sequence | Asn1Tags.Constructed:
					{
						Asn1EncodableVector v = BuildEncodableVector(EndOfStream);
						return new BerSequence(v);
					}
					case Asn1Tags.Set | Asn1Tags.Constructed:
					{
						Asn1EncodableVector v = BuildEncodableVector(EndOfStream);
						return new BerSet(v, false);
					}
					case Asn1Tags.OctetString | Asn1Tags.Constructed:
						return BuildConstructedOctetString(EndOfStream);
					default:
					{
						//
						// with tagged object tag number is bottom 5 bits
						//
						if ((tag & Asn1Tags.Tagged) != 0)
						{
							//
							// simple type - implicit... return an octet string
							//
							if ((tag & Asn1Tags.Constructed) == 0)
							{
								byte[]  bytes = ReadIndefiniteLengthFully();

								return new BerTaggedObject(false, tagNo, new DerOctetString(bytes));
							}

							//
							// either constructed or explicitly tagged
							//
							Asn1Object dObj = ReadObject();

							if (dObj == EndOfStream)     // empty tag!
							{
								return new DerTaggedObject(tagNo);
							}

							Asn1Object next = ReadObject();

							//
							// explicitly tagged (probably!) - if it isn't we'd have to
							// tell from the context
							//
							if (next == EndOfStream)
							{
								return new BerTaggedObject(tagNo, dObj);
							}

							//
							// another implicit object, we'll create a sequence...
							//
							Asn1EncodableVector v = new Asn1EncodableVector(dObj);

							do
							{
								v.Add(next);
								next = ReadObject();
							}
							while (next != EndOfStream);

							return new BerTaggedObject(false, tagNo, new BerSequence(v));
						}

						throw new IOException("unknown Ber object encountered");
					}
                }
            }
            else
            {
                if (tag == 0 && length == 0)    // end of contents marker.
                {
                    return EndOfStream;
                }

                byte[]  bytes = new byte[length];

                ReadFully(bytes);

                return BuildObject(tag, tagNo, bytes);
            }
        }

		private int ReadTagNumber(int tag)
		{
			int tagNo = tag & 0x1f;

			if (tagNo == 0x1f)
			{
				int b = ReadByte();

				tagNo = 0;

				while ((b >= 0) && ((b & 0x80) != 0))
				{
					tagNo |= (b & 0x7f);
					tagNo <<= 7;
					b = ReadByte();
				}

				if (b < 0)
				{
					eofFound = true;
					throw new EndOfStreamException("EOF found inside tag value.");
				}

				tagNo |= (b & 0x7f);
			}

			return tagNo;
		}
    }
}
