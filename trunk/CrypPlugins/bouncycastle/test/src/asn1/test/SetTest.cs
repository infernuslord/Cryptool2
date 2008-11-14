using System;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Asn1.Tests
{
	/// <remarks>Set sorting test example.</remarks>
    [TestFixture]
    public class SetTest
        : ITest
    {
        public string Name
        {
			get { return "Set"; }
        }

		public ITestResult Perform()
        {
            try
            {
                byte[] data = new byte[10];

				Asn1EncodableVector v = new Asn1EncodableVector(
					new DerOctetString(data),
					new DerBitString(data),
					new DerInteger(100),
					DerBoolean.True);

				Asn1Set s = new DerSet(v);

				if (!(s[0] is DerBoolean))
                {
                    return new SimpleTestResult(false, Name + ": sorting failed.");
                }

				s = new BerSet(v);

				if (!(s[0] is DerOctetString))
				{
					return new SimpleTestResult(false, Name + ": BER set sort order changed.");
				}

				// create an implicitly tagged "set" without sorting
                Asn1TaggedObject tag = new DerTaggedObject(false, 1, new DerSequence(v));
                s = Asn1Set.GetInstance(tag, false);

                if (s[0] is DerBoolean)
                {
                    return new SimpleTestResult(false, Name + ": sorted when shouldn't be.");
                }

				// equality test
				s = new DerSet(
					DerBoolean.True,
					DerBoolean.True,
					DerBoolean.True);

				return new SimpleTestResult(true, Name + ": Okay");
            }
            catch (Exception e)
            {
                return new SimpleTestResult(false, Name + ": Exception - " + e.ToString(), e);
            }
        }

        public static void Main(
            string[] args)
        {
            ITest test = new SetTest();
            ITestResult  result = test.Perform();

			Console.WriteLine(result);
        }

		[Test]
        public void TestFunction()
        {
            string resultText = Perform().ToString();

            Assert.AreEqual(Name + ": Okay", resultText);
        }
    }
}
