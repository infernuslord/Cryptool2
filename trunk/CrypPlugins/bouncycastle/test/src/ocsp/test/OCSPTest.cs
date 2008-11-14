using System;
using System.Collections;

using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.Ocsp.Tests
{
	[TestFixture]
	public class OcspTest
		: SimpleTest
	{
		private static readonly byte[] testResp1 = Base64.Decode(
			  "MIIFnAoBAKCCBZUwggWRBgkrBgEFBQcwAQEEggWCMIIFfjCCARehgZ8wgZwx"
			+ "CzAJBgNVBAYTAklOMRcwFQYDVQQIEw5BbmRocmEgcHJhZGVzaDESMBAGA1UE"
			+ "BxMJSHlkZXJhYmFkMQwwCgYDVQQKEwNUQ1MxDDAKBgNVBAsTA0FUQzEeMBwG"
			+ "A1UEAxMVVENTLUNBIE9DU1AgUmVzcG9uZGVyMSQwIgYJKoZIhvcNAQkBFhVv"
			+ "Y3NwQHRjcy1jYS50Y3MuY28uaW4YDzIwMDMwNDAyMTIzNDU4WjBiMGAwOjAJ"
			+ "BgUrDgMCGgUABBRs07IuoCWNmcEl1oHwIak1BPnX8QQUtGyl/iL9WJ1VxjxF"
			+ "j0hAwJ/s1AcCAQKhERgPMjAwMjA4MjkwNzA5MjZaGA8yMDAzMDQwMjEyMzQ1"
			+ "OFowDQYJKoZIhvcNAQEFBQADgYEAfbN0TCRFKdhsmvOdUoiJ+qvygGBzDxD/"
			+ "VWhXYA+16AphHLIWNABR3CgHB3zWtdy2j7DJmQ/R7qKj7dUhWLSqclAiPgFt"
			+ "QQ1YvSJAYfEIdyHkxv4NP0LSogxrumANcDyC9yt/W9yHjD2ICPBIqCsZLuLk"
			+ "OHYi5DlwWe9Zm9VFwCGgggPMMIIDyDCCA8QwggKsoAMCAQICAQYwDQYJKoZI"
			+ "hvcNAQEFBQAwgZQxFDASBgNVBAMTC1RDUy1DQSBPQ1NQMSYwJAYJKoZIhvcN"
			+ "AQkBFhd0Y3MtY2FAdGNzLWNhLnRjcy5jby5pbjEMMAoGA1UEChMDVENTMQww"
			+ "CgYDVQQLEwNBVEMxEjAQBgNVBAcTCUh5ZGVyYWJhZDEXMBUGA1UECBMOQW5k"
			+ "aHJhIHByYWRlc2gxCzAJBgNVBAYTAklOMB4XDTAyMDgyOTA3MTE0M1oXDTAz"
			+ "MDgyOTA3MTE0M1owgZwxCzAJBgNVBAYTAklOMRcwFQYDVQQIEw5BbmRocmEg"
			+ "cHJhZGVzaDESMBAGA1UEBxMJSHlkZXJhYmFkMQwwCgYDVQQKEwNUQ1MxDDAK"
			+ "BgNVBAsTA0FUQzEeMBwGA1UEAxMVVENTLUNBIE9DU1AgUmVzcG9uZGVyMSQw"
			+ "IgYJKoZIhvcNAQkBFhVvY3NwQHRjcy1jYS50Y3MuY28uaW4wgZ8wDQYJKoZI"
			+ "hvcNAQEBBQADgY0AMIGJAoGBAM+XWW4caMRv46D7L6Bv8iwtKgmQu0SAybmF"
			+ "RJiz12qXzdvTLt8C75OdgmUomxp0+gW/4XlTPUqOMQWv463aZRv9Ust4f8MH"
			+ "EJh4ekP/NS9+d8vEO3P40ntQkmSMcFmtA9E1koUtQ3MSJlcs441JjbgUaVnm"
			+ "jDmmniQnZY4bU3tVAgMBAAGjgZowgZcwDAYDVR0TAQH/BAIwADALBgNVHQ8E"
			+ "BAMCB4AwEwYDVR0lBAwwCgYIKwYBBQUHAwkwNgYIKwYBBQUHAQEEKjAoMCYG"
			+ "CCsGAQUFBzABhhpodHRwOi8vMTcyLjE5LjQwLjExMDo3NzAwLzAtBgNVHR8E"
			+ "JjAkMCKgIKAehhxodHRwOi8vMTcyLjE5LjQwLjExMC9jcmwuY3JsMA0GCSqG"
			+ "SIb3DQEBBQUAA4IBAQB6FovM3B4VDDZ15o12gnADZsIk9fTAczLlcrmXLNN4"
			+ "PgmqgnwF0Ymj3bD5SavDOXxbA65AZJ7rBNAguLUo+xVkgxmoBH7R2sBxjTCc"
			+ "r07NEadxM3HQkt0aX5XYEl8eRoifwqYAI9h0ziZfTNes8elNfb3DoPPjqq6V"
			+ "mMg0f0iMS4W8LjNPorjRB+kIosa1deAGPhq0eJ8yr0/s2QR2/WFD5P4aXc8I"
			+ "KWleklnIImS3zqiPrq6tl2Bm8DZj7vXlTOwmraSQxUwzCKwYob1yGvNOUQTq"
			+ "pG6jxn7jgDawHU1+WjWQe4Q34/pWeGLysxTraMa+Ug9kPe+jy/qRX2xwvKBZ"
//			+ "====");
			+ "");

		private static readonly byte[] testResp2 = Base64.Decode(
			  "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"
			+ "A5oyOoNv0w+8bbtMNEc1"
//			+ "====");
			+ "");

		public override string Name
		{
			get { return "OCSP"; }
		}

		private void doTestECDsa()
		{
			string signDN = "O=Bouncy Castle, C=AU";
			AsymmetricCipherKeyPair signKP = OcspTestUtil.MakeECKeyPair();
			X509Certificate testCert = OcspTestUtil.MakeECDsaCertificate(signKP, signDN, signKP, signDN);

			string origDN = "CN=Eric H. Echidna, E=eric@bouncycastle.org, O=Bouncy Castle, C=AU";
			GeneralName origName = new GeneralName(new X509Name(origDN));

			//
			// general id value for our test issuer cert and a serial number.
			//
			CertificateID id = new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One);

			//
			// basic request generation
			//
			OcspReqGenerator gen = new OcspReqGenerator();

			gen.AddRequest(new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			OcspReq req = gen.Generate();

			if (req.IsSigned)
			{
				Fail("signed but shouldn't be");
			}

			X509Certificate[] certs = req.GetCerts();

			if (certs != null)
			{
				Fail("null certs expected, but not found");
			}

			Req[] requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			//
			// request generation with signing
			//
			X509Certificate[] chain = new X509Certificate[1];

			gen = new OcspReqGenerator();

			gen.SetRequestorName(new GeneralName(GeneralName.DirectoryName, new X509Name("CN=fred")));

			gen.AddRequest(new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			chain[0] = testCert;

			req = gen.Generate("SHA1withECDSA", signKP.Private, chain);

			if (!req.IsSigned)
			{
				Fail("not signed but should be");
			}

			if (!req.Verify(signKP.Public))
			{
				Fail("signature failed to verify");
			}

			requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			certs = req.GetCerts();

			if (certs == null)
			{
				Fail("null certs found");
			}

			if (certs.Length != 1 || !certs[0].Equals(testCert))
			{
				Fail("incorrect certs found in request");
			}

			//
			// encoding test
			//
			byte[] reqEnc = req.GetEncoded();

			OcspReq newReq = new OcspReq(reqEnc);

			if (!newReq.Verify(signKP.Public))
			{
				Fail("newReq signature failed to verify");
			}

			//
			// request generation with signing and nonce
			//
			chain = new X509Certificate[1];

			gen = new OcspReqGenerator();

			ArrayList oids = new ArrayList();
			ArrayList values = new ArrayList();
			byte[] sampleNonce = new byte[16];
			Random rand = new Random();

			rand.NextBytes(sampleNonce);

			gen.SetRequestorName(new GeneralName(GeneralName.DirectoryName, new X509Name("CN=fred")));

			oids.Add(OcspObjectIdentifiers.PkixOcspNonce);
			values.Add(new X509Extension(false, new DerOctetString(new DerOctetString(sampleNonce))));

			gen.SetRequestExtensions(new X509Extensions(oids, values));

			gen.AddRequest(new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			chain[0] = testCert;

			req = gen.Generate("SHA1withECDSA", signKP.Private, chain);

			if (!req.IsSigned)
			{
				Fail("not signed but should be");
			}

			if (!req.Verify(signKP.Public))
			{
				Fail("signature failed to verify");
			}

			//
			// extension check.
			//
			ISet extOids = req.GetCriticalExtensionOids();

			if (extOids.Count != 0)
			{
				Fail("wrong number of critical extensions in OCSP request.");
			}

			extOids = req.GetNonCriticalExtensionOids();

			if (extOids.Count != 1)
			{
				Fail("wrong number of non-critical extensions in OCSP request.");
			}

			Asn1OctetString extValue = req.GetExtensionValue(OcspObjectIdentifiers.PkixOcspNonce);

			Asn1Encodable extObj = X509ExtensionUtilities.FromExtensionValue(extValue);

			if (!(extObj is Asn1OctetString))
			{
				Fail("wrong extension type found.");
			}

			if (!AreEqual(((Asn1OctetString)extObj).GetOctets(), sampleNonce))
			{
				Fail("wrong extension value found.");
			}

			//
			// request list check
			//
			requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			//
			// response generation
			//
			BasicOcspRespGenerator respGen = new BasicOcspRespGenerator(signKP.Public);

			respGen.AddResponse(id, CertificateStatus.Good);

			respGen.Generate("SHA1withECDSA", signKP.Private, chain, DateTime.UtcNow);
		}

		public override void PerformTest()
		{
			string signDN = "O=Bouncy Castle, C=AU";
			AsymmetricCipherKeyPair signKP = OcspTestUtil.MakeKeyPair();
			X509Certificate testCert = OcspTestUtil.MakeCertificate(signKP, signDN, signKP, signDN);

			string origDN = "CN=Eric H. Echidna, E=eric@bouncycastle.org, O=Bouncy Castle, C=AU";
			GeneralName origName = new GeneralName(new X509Name(origDN));

			//
			// general id value for our test issuer cert and a serial number.
			//
			CertificateID   id = new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One);

			//
			// basic request generation
			//
			OcspReqGenerator gen = new OcspReqGenerator();

			gen.AddRequest(
				new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			OcspReq req = gen.Generate();

			if (req.IsSigned)
			{
				Fail("signed but shouldn't be");
			}

			X509Certificate[] certs = req.GetCerts();

			if (certs != null)
			{
				Fail("null certs expected, but not found");
			}

			Req[] requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			//
			// request generation with signing
			//
			X509Certificate[] chain = new X509Certificate[1];

			gen = new OcspReqGenerator();

			gen.SetRequestorName(new GeneralName(GeneralName.DirectoryName, new X509Name("CN=fred")));

			gen.AddRequest(
				new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			chain[0] = testCert;

			req = gen.Generate("SHA1withRSA", signKP.Private, chain);

			if (!req.IsSigned)
			{
				Fail("not signed but should be");
			}

			if (!req.Verify(signKP.Public))
			{
				Fail("signature failed to Verify");
			}

			requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			certs = req.GetCerts();

			if (certs == null)
			{
				Fail("null certs found");
			}

			if (certs.Length != 1 || !testCert.Equals(certs[0]))
			{
				Fail("incorrect certs found in request");
			}

			//
			// encoding test
			//
			byte[] reqEnc = req.GetEncoded();

			OcspReq newReq = new OcspReq(reqEnc);

			if (!newReq.Verify(signKP.Public))
			{
				Fail("newReq signature failed to Verify");
			}

			//
			// request generation with signing and nonce
			//
			chain = new X509Certificate[1];

			gen = new OcspReqGenerator();

			ArrayList oids = new ArrayList();
			ArrayList values = new ArrayList();
			byte[] sampleNonce = new byte[16];
			Random rand = new Random();

			rand.NextBytes(sampleNonce);

			gen.SetRequestorName(new GeneralName(GeneralName.DirectoryName, new X509Name("CN=fred")));

			oids.Add(OcspObjectIdentifiers.PkixOcspNonce);
			values.Add(new X509Extension(false, new DerOctetString(new DerOctetString(sampleNonce))));

			gen.SetRequestExtensions(new X509Extensions(oids, values));

			gen.AddRequest(
				new CertificateID(CertificateID.HashSha1, testCert, BigInteger.One));

			chain[0] = testCert;

			req = gen.Generate("SHA1withRSA", signKP.Private, chain);

			if (!req.IsSigned)
			{
				Fail("not signed but should be");
			}

			if (!req.Verify(signKP.Public))
			{
				Fail("signature failed to Verify");
			}

			//
			// extension check.
			//
			ISet extOids = req.GetCriticalExtensionOids();

			if (extOids.Count != 0)
			{
				Fail("wrong number of critical extensions in OCSP request.");
			}

			extOids = req.GetNonCriticalExtensionOids();

			if (extOids.Count != 1)
			{
				Fail("wrong number of non-critical extensions in OCSP request.");
			}

			Asn1OctetString extValue = req.GetExtensionValue(OcspObjectIdentifiers.PkixOcspNonce);
			Asn1Object extObj = X509ExtensionUtilities.FromExtensionValue(extValue);

			if (!(extObj is Asn1OctetString))
			{
				Fail("wrong extension type found.");
			}

			byte[] compareNonce = ((Asn1OctetString) extObj).GetOctets();

			if (!AreEqual(compareNonce, sampleNonce))
			{
				Fail("wrong extension value found.");
			}

			//
			// request list check
			//
			requests = req.GetRequestList();

			if (!requests[0].GetCertID().Equals(id))
			{
				Fail("Failed isFor test");
			}

			//
			// response parsing - test 1
			//
			OcspResp response = new OcspResp(testResp1);

			if (response.Status != 0)
			{
				Fail("response status not zero.");
			}

			BasicOcspResp brep = (BasicOcspResp) response.GetResponseObject();
			chain = brep.GetCerts();

			if (!brep.Verify(chain[0].GetPublicKey()))
			{
				Fail("response 1 failed to Verify.");
			}

			//
			// test 2
			//
			SingleResp[] singleResp = brep.Responses;

			response = new OcspResp(testResp2);

			if (response.Status != 0)
			{
				Fail("response status not zero.");
			}

			brep = (BasicOcspResp)response.GetResponseObject();
			chain = brep.GetCerts();

			if (!brep.Verify(chain[0].GetPublicKey()))
			{
				Fail("response 2 failed to Verify.");
			}

			singleResp = brep.Responses;

			//
			// simple response generation
			//
			OCSPRespGenerator respGen = new OCSPRespGenerator();
			OcspResp resp = respGen.Generate(OCSPRespGenerator.Successful, response.GetResponseObject());

			if (!resp.GetResponseObject().Equals(response.GetResponseObject()))
			{
				Fail("response fails to match");
			}

			doTestECDsa();
		}

		public static void Main(
			string[] args)
		{
			ITest test = new OcspTest();
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
