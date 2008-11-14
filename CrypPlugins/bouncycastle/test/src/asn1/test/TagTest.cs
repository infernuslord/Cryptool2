using System;

using NUnit.Framework;

using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Asn1.Tests
{
	/**
	* X.690 test example
	*/
	[TestFixture]
	public class TagTest
		: SimpleTest
	{
		private static readonly byte[] longTagged = Base64.Decode(
			  "ZSRzIp8gEEZFRENCQTk4NzY1NDMyMTCfIQwyMDA2MDQwMTEyMzSUCCAFERVz"
			+ "A4kCAHEXGBkalAggBRcYGRqUCCAFZS6QAkRFkQlURUNITklLRVKSBQECAwQF"
			+ "kxAREhMUFRYXGBkalAggBREVcwOJAgBxFxgZGpQIIAUXGBkalAggBWUukAJE"
			+ "RZEJVEVDSE5JS0VSkgUBAgMEBZMQERITFBUWFxgZGpQIIAURFXMDiQIAcRcY"
			+ "GRqUCCAFFxgZGpQIIAVlLpACREWRCVRFQ0hOSUtFUpIFAQIDBAWTEBESExQV"
			+ "FhcYGRqUCCAFERVzA4kCAHEXGBkalAggBRcYGRqUCCAFFxgZGpQIIAUXGBka"
			+ "lAg=");

		private static readonly byte[] longAppSpecificTag = Hex.Decode("5F610101");

		public override string Name
		{
			get { return "Tag"; }
		}

		public override void PerformTest()
		{
			DerApplicationSpecific app = (DerApplicationSpecific)
				Asn1Object.FromByteArray(longTagged);

			app = (DerApplicationSpecific) Asn1Object.FromByteArray(app.GetContents());

			Asn1InputStream aIn = new Asn1InputStream(app.GetContents());

			Asn1TaggedObject tagged = (Asn1TaggedObject) aIn.ReadObject();

			if (tagged.TagNo != 32)
			{
				Fail("unexpected tag value found - not 32");
			}

			tagged = (Asn1TaggedObject) aIn.ReadObject();

			if (tagged.TagNo != 33)
			{
				Fail("unexpected tag value found - not 32");
			}

			aIn = new Asn1InputStream(longAppSpecificTag);

			app = (DerApplicationSpecific)aIn.ReadObject();

			if (app.ApplicationTag != 97)
			{
				Fail("incorrect tag number read");
			}
		}

		public static void Main(
			string[] args)
		{
			RunTest(new TagTest());
		}

		[Test]
		public void TestFunction()
		{
			string resultText = Perform().ToString();

			Assert.AreEqual(Name + ": Okay", resultText);
		}
	}
}
