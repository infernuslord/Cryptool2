using System;

using NUnit.Framework;

using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Asn1.Tests
{
	[TestFixture]
	public class DerApplicationSpecificTest
		: SimpleTest
	{
		private static readonly byte[] impData = Hex.Decode("430109");

		public override string Name
		{
			get { return "DerApplicationSpecific"; }
		}

		public override void PerformTest()
		{
			DerInteger val = new DerInteger(9);

			DerApplicationSpecific tagged = new DerApplicationSpecific(false, 3, val);

			if (!AreEqual(impData, tagged.GetEncoded()))
			{
				Fail("implicit encoding failed");
			}

			DerInteger recVal = (DerInteger) tagged.GetObject(Asn1Tags.Integer);

			if (!val.Equals(recVal))
			{
				Fail("implicit read back failed");
			}
		}

		public static void Main(
			string[] args)
		{
			RunTest(new DerApplicationSpecificTest());
		}

		[Test]
		public void TestFunction()
		{
			string resultText = Perform().ToString();

			Assert.AreEqual(Name + ": Okay", resultText);
		}
	}
}
