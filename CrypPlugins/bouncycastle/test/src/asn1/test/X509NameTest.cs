using System;
using System.Collections;
using System.IO;
using System.Text;

using NUnit.Framework;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Asn1.Tests
{
	[TestFixture]
	public class X509NameTest
		: SimpleTest
	{
		private static readonly string[] subjects =
		{
			"C=AU,ST=Victoria,L=South Melbourne,O=Connect 4 Pty Ltd,OU=Webserver Team,CN=www2.connect4.com.au,E=webmaster@connect4.com.au",
			"C=AU,ST=Victoria,L=South Melbourne,O=Connect 4 Pty Ltd,OU=Certificate Authority,CN=Connect 4 CA,E=webmaster@connect4.com.au",
			"C=AU,ST=QLD,CN=SSLeay/rsa test cert",
			"C=US,O=National Aeronautics and Space Administration,SERIALNUMBER=16+CN=Steve Schoch",
			"E=cooke@issl.atl.hp.com,C=US,OU=Hewlett Packard Company (ISSL),CN=Paul A. Cooke",
			"O=Sun Microsystems Inc,CN=store.sun.com",
			"unstructuredAddress=192.168.1.33,unstructuredName=pixfirewall.ciscopix.com,CN=pixfirewall.ciscopix.com"
		};

		public override string Name
		{
			get { return "X509Name"; }
		}

		private static X509Name FromBytes(
			byte[] bytes)
		{
			return X509Name.GetInstance(Asn1Object.FromByteArray(bytes));
		}

		private IAsn1Convertible createEntryValue(
			DerObjectIdentifier	oid,
			string				value)
		{
			Hashtable attrs = new Hashtable();
			attrs.Add(oid, value);

			ArrayList ord = new ArrayList();
			ord.Add(oid);

			X509Name name = new X509Name(ord, attrs);

			Asn1Sequence seq = (Asn1Sequence)name.ToAsn1Object();
			Asn1Set set = (Asn1Set)seq[0];
			seq = (Asn1Sequence)set[0];

			return seq[1];
		}

		private IAsn1Convertible createEntryValueFromString(
			DerObjectIdentifier	oid,
			string				val)
		{
			Hashtable attrs = new Hashtable();
			attrs.Add(oid, val);

			ArrayList ord = new ArrayList(attrs.Keys);

			X509Name name = new X509Name(new X509Name(ord, attrs).ToString());

			Asn1Sequence seq = (Asn1Sequence) name.ToAsn1Object();
			Asn1Set asn1Set = (Asn1Set) seq[0];
			seq = (Asn1Sequence) asn1Set[0];

			return seq[1];
		}

		private void doTestEncodingPrintableString(
			DerObjectIdentifier	oid,
			string				value)
		{
			IAsn1Convertible converted = createEntryValue(oid, value);
			if (!(converted is DerPrintableString))
			{
				Fail("encoding for " + oid + " not printable string");
			}
		}

		private void doTestEncodingIA5String(
			DerObjectIdentifier oid,
			string				value)
		{
			IAsn1Convertible converted = createEntryValue(oid, value);
			if (!(converted is DerIA5String))
			{
				Fail("encoding for " + oid + " not IA5String");
			}
		}

		private void doTestEncodingGeneralizedTime(
			DerObjectIdentifier	oid,
			string				val)
		{
			IAsn1Convertible converted = createEntryValue(oid, val);
			if (!(converted is DerGeneralizedTime))
			{
				Fail("encoding for " + oid + " not GeneralizedTime");
			}
			converted = createEntryValueFromString(oid, val);
			if (!(converted is DerGeneralizedTime))
			{
				Fail("encoding for " + oid + " not GeneralizedTime");
			}
		}

		public override void PerformTest()
		{
			doTestEncodingPrintableString(X509Name.C, "AU");
			doTestEncodingPrintableString(X509Name.SerialNumber, "123456");
			doTestEncodingPrintableString(X509Name.DnQualifier, "123456");
			doTestEncodingIA5String(X509Name.EmailAddress, "test@test.com");
			doTestEncodingIA5String(X509Name.DC, "test");
			// correct encoding
			doTestEncodingGeneralizedTime(X509Name.DateOfBirth, "#180F32303032303132323132323232305A");
			// compatability encoding
			doTestEncodingGeneralizedTime(X509Name.DateOfBirth, "20020122122220Z");

			//
			// composite
			//
			Hashtable attrs = new Hashtable();
			attrs.Add(X509Name.C, "AU");
			attrs.Add(X509Name.O, "The Legion of the Bouncy Castle");
			attrs.Add(X509Name.L, "Melbourne");
			attrs.Add(X509Name.ST, "Victoria");
			attrs.Add(X509Name.E, "feedback-crypto@bouncycastle.org");

			ArrayList ord = new ArrayList(attrs.Keys);

			X509Name name1 = new X509Name(ord, attrs);

			if (!name1.Equivalent(name1))
			{
				Fail("Failed same object test");
			}

			if (!name1.Equivalent(name1, true))
			{
				Fail("Failed same object test - in Order");
			}

			X509Name name2 = new X509Name(ord, attrs);

			if (!name1.Equivalent(name2))
			{
				Fail("Failed same name test");
			}

			if (!name1.Equivalent(name2, true))
			{
				Fail("Failed same name test - in Order");
			}

			if (name1.GetHashCode() != name2.GetHashCode())
			{
				Fail("Failed same name test - in Order");
			}

			ArrayList ord1 = new ArrayList();

			ord1.Add(X509Name.C);
			ord1.Add(X509Name.O);
			ord1.Add(X509Name.L);
			ord1.Add(X509Name.ST);
			ord1.Add(X509Name.E);

			ArrayList ord2 = new ArrayList();

			ord2.Add(X509Name.E);
			ord2.Add(X509Name.ST);
			ord2.Add(X509Name.L);
			ord2.Add(X509Name.O);
			ord2.Add(X509Name.C);

			name1 = new X509Name(ord1, attrs);
			name2 = new X509Name(ord2, attrs);

			if (!name1.Equivalent(name2))
			{
				Fail("Failed reverse name test");
			}

			if (name1.Equivalent(name2, true))
			{
				Fail("Failed reverse name test - in Order");
			}

			if (!name1.Equivalent(name2, false))
			{
				Fail("Failed reverse name test - in Order false");
			}

			ArrayList oids = name1.GetOids();
			if (!CompareVectors(oids, ord1))
			{
				Fail("oid comparison test");
			}

			ArrayList val1 = new ArrayList();

			val1.Add("AU");
			val1.Add("The Legion of the Bouncy Castle");
			val1.Add("Melbourne");
			val1.Add("Victoria");
			val1.Add("feedback-crypto@bouncycastle.org");

			name1 = new X509Name(ord1, val1);

			ArrayList values = name1.GetValues();
			if (!CompareVectors(values, val1))
			{
				Fail("value comparison test");
			}

			ord2 = new ArrayList();

			ord2.Add(X509Name.ST);
			ord2.Add(X509Name.ST);
			ord2.Add(X509Name.L);
			ord2.Add(X509Name.O);
			ord2.Add(X509Name.C);

			name1 = new X509Name(ord1, attrs);
			name2 = new X509Name(ord2, attrs);

			if (name1.Equivalent(name2))
			{
				Fail("Failed different name test");
			}

			ord2 = new ArrayList();

			ord2.Add(X509Name.ST);
			ord2.Add(X509Name.L);
			ord2.Add(X509Name.O);
			ord2.Add(X509Name.C);

			name1 = new X509Name(ord1, attrs);
			name2 = new X509Name(ord2, attrs);

			if (name1.Equivalent(name2))
			{
				Fail("Failed subset name test");
			}


			compositeTest();


			//
			// getValues test
			//
			ArrayList v1 = name1.GetValues(X509Name.O);

			if (v1.Count != 1 || !v1[0].Equals("The Legion of the Bouncy Castle"))
			{
				Fail("O test failed");
			}

			ArrayList v2 = name1.GetValues(X509Name.L);

			if (v2.Count != 1 || !v2[0].Equals("Melbourne"))
			{
				Fail("L test failed");
			}

			//
			// general subjects test
			//
			for (int i = 0; i != subjects.Length; i++)
			{
				X509Name name = new X509Name(subjects[i]);
				byte[] encodedName = name.GetEncoded();
				name = X509Name.GetInstance(Asn1Object.FromByteArray(encodedName));

				if (!name.ToString().Equals(subjects[i]))
				{
					Fail("Failed regeneration test " + i);
				}
			}

			//
			// sort test
			//
			X509Name unsorted = new X509Name("SERIALNUMBER=BBB + CN=AA");

			if (!FromBytes(unsorted.GetEncoded()).ToString().Equals("CN=AA+SERIALNUMBER=BBB"))
			{
				Fail("Failed sort test 1");
			}

			unsorted = new X509Name("CN=AA + SERIALNUMBER=BBB");

			if (!FromBytes(unsorted.GetEncoded()).ToString().Equals("CN=AA+SERIALNUMBER=BBB"))
			{
				Fail("Failed sort test 2");
			}

			unsorted = new X509Name("SERIALNUMBER=B + CN=AA");

			if (!FromBytes(unsorted.GetEncoded()).ToString().Equals("SERIALNUMBER=B+CN=AA"))
			{
				Fail("Failed sort test 3");
			}

			unsorted = new X509Name("CN=AA + SERIALNUMBER=B");

			if (!FromBytes(unsorted.GetEncoded()).ToString().Equals("SERIALNUMBER=B+CN=AA"))
			{
				Fail("Failed sort test 4");
			}

			//
			// equality tests
			//
			equalityTest(new X509Name("CN=The     Legion"), new X509Name("CN=The Legion"));
			equalityTest(new X509Name("CN=   The Legion"), new X509Name("CN=The Legion"));
			equalityTest(new X509Name("CN=The Legion   "), new X509Name("CN=The Legion"));
			equalityTest(new X509Name("CN=  The     Legion "), new X509Name("CN=The Legion"));
			equalityTest(new X509Name("CN=  the     legion "), new X509Name("CN=The Legion"));

			//
			// inequality to sequences
			//
			name1 = new X509Name("CN=The Legion");

			if (name1.Equals(new DerSequence()))
			{
				Fail("inequality test with sequence");
			}

			if (name1.Equals(new DerSequence(new DerSet())))
			{
				Fail("inequality test with sequence and set");
			}

			Asn1EncodableVector v = new Asn1EncodableVector(
				new DerObjectIdentifier("1.1"),
				new DerObjectIdentifier("1.1"));

			if (name1.Equals(new DerSequence(new DerSet(new DerSet(v)))))
			{
				Fail("inequality test with sequence and bad set");
			}

//			if (name1.Equals(new DerSequence(new DerSet(new DerSet(v))), true))
//			{
//				Fail("inequality test with sequence and bad set");
//			}
			try
			{
				X509Name.GetInstance(new DerSequence(new DerSet(new DerSet(v))));
				Fail("GetInstance should reject bad sequence");
			}
			catch (ArgumentException)
			{
				//expected
			}

			if (name1.Equals(new DerSequence(new DerSet(new DerSequence()))))
			{
				Fail("inequality test with sequence and short sequence");
			}

//			if (name1.Equals(new DerSequence(new DerSet(new DerSequence())), true))
//			{
//				Fail("inequality test with sequence and short sequence");
//			}
			try
			{
				X509Name.GetInstance(new DerSequence(new DerSet(new DerSequence())));
				Fail("GetInstance should reject short sequence");
			}
			catch (ArgumentException)
			{
				//expected
			}

			v = new Asn1EncodableVector(
				new DerObjectIdentifier("1.1"),
				new DerSequence());

			if (name1.Equals(new DerSequence(new DerSet(new DerSequence(v)))))
			{
				Fail("inequality test with sequence and bad sequence");
			}

			if (name1.Equivalent(null))
			{
				Fail("inequality test with null");
			}

			if (name1.Equivalent(null, true))
			{
				Fail("inequality test with null");
			}

			//
			// this is contrived but it checks sorting of sets with equal elements
			//
			unsorted = new X509Name("CN=AA + CN=AA + CN=AA");

			//
			// tagging test - only works if CHOICE implemented
			//
			/*
			ASN1TaggedObject tag = new DERTaggedObject(false, 1, new X509Name("CN=AA"));

			if (!tag.isExplicit())
			{
				return new SimpleTestResult(false, getName() + ": failed to explicitly tag CHOICE object");
			}

			X509Name name = X509Name.getInstance(tag, false);

			if (!name.equals(new X509Name("CN=AA")))
			{
				return new SimpleTestResult(false, getName() + ": failed to recover tagged name");
			}
			*/



			DerUtf8String testString = new DerUtf8String("The Legion of the Bouncy Castle");
			byte[] encodedBytes = testString.GetEncoded();
			byte[] hexEncodedBytes = Hex.Encode(encodedBytes);
			string hexEncodedString = "#" + Encoding.ASCII.GetString(hexEncodedBytes);

			DerUtf8String converted = (DerUtf8String)
				new X509DefaultEntryConverter().GetConvertedValue(
				X509Name.L , hexEncodedString);

			if (!converted.Equals(testString))
			{
				Fail("Failed X509DefaultEntryConverter test");
			}

			//
			// try a weird value
			//
		}

		private void compositeTest()
		{
			//
			// composite test
			//
			byte[] enc = Hex.Decode("305e310b300906035504061302415531283026060355040a0c1f546865204c6567696f6e206f662074686520426f756e637920436173746c653125301006035504070c094d656c626f75726e653011060355040b0c0a4173636f742056616c65");
			X509Name n = X509Name.GetInstance(Asn1Object.FromByteArray(enc));

			if (!n.ToString().Equals("C=AU,O=The Legion of the Bouncy Castle,L=Melbourne+OU=Ascot Vale"))
			{
				Fail("Failed composite to string test got: " + n.ToString());
			}

			if (!n.ToString(true, X509Name.DefaultSymbols).Equals("L=Melbourne+OU=Ascot Vale,O=The Legion of the Bouncy Castle,C=AU"))
			{
				Fail("Failed composite to string test got: " + n.ToString(true, X509Name.DefaultSymbols));
			}

			n = new X509Name(true, "L=Melbourne+OU=Ascot Vale,O=The Legion of the Bouncy Castle,C=AU");
			if (!n.ToString().Equals("C=AU,O=The Legion of the Bouncy Castle,L=Melbourne+OU=Ascot Vale"))
			{
				Fail("Failed composite to string reversal test got: " + n.ToString());
			}

			n = new X509Name("C=AU, O=The Legion of the Bouncy Castle, L=Melbourne + OU=Ascot Vale");

			MemoryStream bOut = new MemoryStream();
			Asn1OutputStream aOut = new Asn1OutputStream(bOut);

			aOut.WriteObject(n);

			byte[] enc2 = bOut.ToArray();

			if (!Arrays.AreEqual(enc, enc2))
			{
				Fail("Failed composite string to encoding test");
			}
		}

		private void equalityTest(
			X509Name	x509Name,
			X509Name	x509Name1)
		{
			if (!x509Name.Equivalent(x509Name1))
			{
				Fail("equality test failed for " + x509Name + " : " + x509Name1);
			}

			if (!x509Name.Equivalent(x509Name1, true))
			{
				Fail("equality test failed for " + x509Name + " : " + x509Name1);
			}
		}

		private bool CompareVectors(
			ArrayList	one,
			ArrayList	two)
		{
			if (one.Count != two.Count)
				return false;

			for (int i = 0; i < one.Count; ++i)
			{
				if (!one[i].Equals(two[i]))
					return false;
			}

			return true;
		}

		public static void Main(
			string[] args)
		{
			ITest test = new X509NameTest();
			ITestResult result = test.Perform();

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
