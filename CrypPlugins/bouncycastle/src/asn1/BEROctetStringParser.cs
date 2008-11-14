using System;
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
	public class BerOctetStringParser
		: Asn1OctetStringParser
	{
		private readonly Asn1ObjectParser _parser;

		internal BerOctetStringParser(
			Asn1ObjectParser parser)
		{
			_parser = parser;
		}

		public Stream GetOctetStream()
		{
			return new ConstructedOctetStream(_parser);
		}

		public Asn1Object ToAsn1Object()
		{
			try
			{
				return new BerOctetString(Streams.ReadAll(GetOctetStream()));
			}
			catch (IOException e)
			{
				throw new InvalidOperationException("IOException converting stream to byte array: " + e.Message, e);
			}
		}
	}
}
