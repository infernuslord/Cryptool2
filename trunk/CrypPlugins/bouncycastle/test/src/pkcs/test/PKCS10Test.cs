#region Using directives

using System;
using System.Collections;
using System.Text;

using NUnit.Framework;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Test;
using Org.BouncyCastle.Security;

#endregion

namespace Org.BouncyCastle.Pkcs.Tests
{
    [TestFixture]
    public class Pkcs10Test
        : ITest
    {
        public string Name
        {
			get { return "Pkcs10"; }
        }

		public ITestResult Perform()
        {
            try
            {
                IAsymmetricCipherKeyPairGenerator pGen = GeneratorUtilities.GetKeyPairGenerator("RSA");
                RsaKeyGenerationParameters genParam = new RsaKeyGenerationParameters(
					BigInteger.ValueOf(0x10001), new SecureRandom(), 512, 25);

                pGen.Init(genParam);

                AsymmetricCipherKeyPair pair = pGen.GenerateKeyPair();

                Hashtable attrs = new Hashtable();

                attrs.Add(X509Name.C, "AU");
                attrs.Add(X509Name.O, "The Legion of the Bouncy Castle");
                attrs.Add(X509Name.L, "Melbourne");
                attrs.Add(X509Name.ST, "Victoria");
                attrs.Add(X509Name.EmailAddress, "feedback-crypto@bouncycastle.org");

                X509Name subject = new X509Name(new ArrayList(attrs.Keys), attrs);

                Pkcs10CertificationRequest req1 = new Pkcs10CertificationRequest(
					"SHA1withRSA",
					subject,
					pair.Public,
					null,
					pair.Private);

				byte[] bytes = req1.GetEncoded();

                Pkcs10CertificationRequest req2 = new Pkcs10CertificationRequest(bytes);

				if (!req2.Verify())
                {
                    return new SimpleTestResult(false, Name + ": Failed verify check.");
                }

                if (!req2.GetPublicKey().Equals(req1.GetPublicKey()))
                {
                    return new SimpleTestResult(false, Name + ": Failed public key check.");
                }

                return new SimpleTestResult(true, Name + ": Okay");
            }
            catch (Exception e)
            {
                return new SimpleTestResult(false, Name + ": exception - " + e.Message);
            }
        }

        public static void Main(
            string[] args)
        {
            ITest test = new Pkcs10Test();
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
