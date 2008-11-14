using System;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Icao;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Asn1.Tests
{
    [TestFixture]
    public class LDSSecurityObjectUnitTest
        : SimpleTest
    {
        public override string Name
        {
			get { return "LDSSecurityObject"; }
        }

		private byte[] GenerateHash()
        {
            Random rand = new Random();
            byte[] bytes = new byte[20];
            rand.NextBytes(bytes);
            return bytes;
        }

        public override void PerformTest()
        {
            AlgorithmIdentifier  algoId = new AlgorithmIdentifier("1.3.14.3.2.26");
            DataGroupHash[] datas = new DataGroupHash[2];

            datas[0] = new DataGroupHash(1, new DerOctetString(GenerateHash()));
            datas[1] = new DataGroupHash(2, new DerOctetString(GenerateHash()));

            LdsSecurityObject so = new LdsSecurityObject(algoId, datas);

            CheckConstruction(so, algoId, datas);


            so = LdsSecurityObject.GetInstance(null);

            if (so != null)
            {
                Fail("null GetInstance() failed.");
            }

			try
            {
                LdsSecurityObject.GetInstance(new object());

                Fail("GetInstance() failed to detect bad object.");
            }
            catch (ArgumentException)
            {
                // expected
            }

			try
            {
				new LdsSecurityObject(new DerSequence());

				Fail("constructor failed to detect empty sequence.");
            }
            catch (ArgumentException)
            {
                // expected
            }

			try
            {
                new LdsSecurityObject(algoId, new DataGroupHash[1]);

				Fail("constructor failed to detect small DataGroupHash array.");
            }
            catch (ArgumentException)
            {
                // expected
            }

			try
            {
                new LdsSecurityObject(algoId, new DataGroupHash[LdsSecurityObject.UBDataGroups + 1]);

				Fail("constructor failed to out of bounds DataGroupHash array.");
            }
            catch (ArgumentException)
            {
                // expected
            }
        }

		private void CheckConstruction(
            LdsSecurityObject	so,
            AlgorithmIdentifier	digestAlgorithmIdentifier,
            DataGroupHash[]		datagroupHash)
        {
            CheckStatement(so, digestAlgorithmIdentifier, datagroupHash);

			so = LdsSecurityObject.GetInstance(so);

			CheckStatement(so, digestAlgorithmIdentifier, datagroupHash);

			Asn1Sequence seq = (Asn1Sequence) Asn1Object.FromByteArray(
				so.ToAsn1Object().GetEncoded());

			so = LdsSecurityObject.GetInstance(seq);

			CheckStatement(so, digestAlgorithmIdentifier, datagroupHash);
        }

		private void CheckStatement(
            LdsSecurityObject	so,
            AlgorithmIdentifier	digestAlgorithmIdentifier,
            DataGroupHash[]		datagroupHash)
        {
            if (digestAlgorithmIdentifier != null)
            {
                if (!so.DigestAlgorithmIdentifier.Equals(digestAlgorithmIdentifier))
                {
                    Fail("ids don't match.");
                }
            }
            else if (so.DigestAlgorithmIdentifier != null)
            {
                Fail("digest algorithm Id found when none expected.");
            }

			if (datagroupHash != null)
            {
                DataGroupHash[] datas = so.GetDatagroupHash();

                for (int i = 0; i != datas.Length; i++)
                {
                    if (!datagroupHash[i].Equals(datas[i]))
                    {
                        Fail("name registration authorities don't match.");
                    }
                }
            }
            else if (so.GetDatagroupHash() != null)
            {
                Fail("data hash groups found when none expected.");
            }
        }

		public static void Main(
            string[]    args)
        {
            RunTest(new LDSSecurityObjectUnitTest());
        }

        [Test]
        public void TestFunction()
        {
            string resultText = Perform().ToString();

            Assert.AreEqual(Name + ": Okay", resultText);
        }
    }
}
