using System;
using System.Collections;
using System.IO;
using System.Text;

using NUnit.Core;
using NUnit.Framework;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Cms.Tests
{
	[TestFixture]
	public class SignedDataTest
	{
		private const string OrigDN = "O=Bouncy Castle, C=AU";
		private static AsymmetricCipherKeyPair origKP;
		private static X509Certificate origCert;

		private const string SignDN = "CN=Bob, OU=Sales, O=Bouncy Castle, C=AU";
		private static AsymmetricCipherKeyPair signKP;
		private static X509Certificate signCert;

		private const string ReciDN = "CN=Doug, OU=Sales, O=Bouncy Castle, C=AU";
//		private static AsymmetricCipherKeyPair reciKP;
//		private static X509Certificate reciCert;

		private static X509Crl signCrl;

		private static AsymmetricCipherKeyPair signGostKP;
		private static X509Certificate signGostCert;

		private static AsymmetricCipherKeyPair signDsaKP;
		private static X509Certificate signDsaCert;

		private static AsymmetricCipherKeyPair signECGostKP;
		private static X509Certificate signECGostCert;

		private static AsymmetricCipherKeyPair signECDsaKP;
		private static X509Certificate signECDsaCert;

		private static AsymmetricCipherKeyPair OrigKP
		{
			get { return origKP == null ? (origKP = CmsTestUtil.MakeKeyPair()) : origKP; }
		}

		private static AsymmetricCipherKeyPair SignKP
		{
			get { return signKP == null ? (signKP = CmsTestUtil.MakeKeyPair()) : signKP; }
		}

//		private static AsymmetricCipherKeyPair ReciKP
//		{
//			get { return reciKP == null ? (reciKP = CmsTestUtil.MakeKeyPair()) : reciKP; }
//		}

		private static AsymmetricCipherKeyPair SignGostKP
		{
			get { return signGostKP == null ? (signGostKP = CmsTestUtil.MakeGostKeyPair()) : signGostKP; }
		}

		private static AsymmetricCipherKeyPair SignDsaKP
		{
			get { return signDsaKP == null ? (signDsaKP = CmsTestUtil.MakeDsaKeyPair()) : signDsaKP; }
		}

		private static AsymmetricCipherKeyPair SignECGostKP
		{
			get { return signECGostKP == null ? (signECGostKP = CmsTestUtil.MakeECGostKeyPair()) : signECGostKP; }
		}

		private static AsymmetricCipherKeyPair SignECDsaKP
		{
			get { return signECDsaKP == null ? (signECDsaKP = CmsTestUtil.MakeECDsaKeyPair()) : signECDsaKP; }
		}

		private static X509Certificate OrigCert
		{
			get { return origCert == null ? (origCert = CmsTestUtil.MakeCertificate(OrigKP, OrigDN, OrigKP, OrigDN)) : origCert; }
		}

		private static X509Certificate SignCert
		{
			get { return signCert == null ? (signCert = CmsTestUtil.MakeCertificate(SignKP, SignDN, OrigKP, OrigDN)) : signCert; }
		}

//		private static X509Certificate ReciCert
//		{
//			get { return reciCert == null ? (reciCert = CmsTestUtil.MakeCertificate(ReciKP, ReciDN, SignKP, SignDN)) : reciCert; }
//		}

		private static X509Crl SignCrl
		{
			get { return signCrl == null ? (signCrl = CmsTestUtil.MakeCrl(SignKP)) : signCrl; }
		}

		private static X509Certificate SignGostCert
		{
			get { return signGostCert == null ? (signGostCert = CmsTestUtil.MakeCertificate(SignGostKP, SignDN, OrigKP, OrigDN)) : signGostCert; }
		}

		private static X509Certificate SignECGostCert
		{
			get { return signECGostCert == null ? (signECGostCert = CmsTestUtil.MakeCertificate(SignECGostKP, SignDN, OrigKP, OrigDN)) : signECGostCert; }
		}

		private static X509Certificate SignDsaCert
		{
			get { return signDsaCert == null ? (signDsaCert = CmsTestUtil.MakeCertificate(SignDsaKP, SignDN, OrigKP, OrigDN)) : signDsaCert; }
		}

		private static X509Certificate SignECDsaCert
		{
			get { return signECDsaCert == null ? (signECDsaCert = CmsTestUtil.MakeCertificate(SignECDsaKP, SignDN, OrigKP, OrigDN)) : signECDsaCert; }
		}

		private static readonly byte[] disorderedMessage = Base64.Decode(
			"SU9fc3RkaW5fdXNlZABfX2xpYmNfc3RhcnRfbWFpbgBnZXRob3N0aWQAX19n"
			+ "bW9uX3M=");

		private static readonly byte[] disorderedSet = Base64.Decode(
			"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"
			+ "JyQFn5msmhPQT/6XSGOlrWRoFummXN9lQzAjCj1sgTcmoLCVQ5s5WpCAOXFw"
			+ "VWu16qndz3sPItn3jJ0F3Kh3w79NglvPAgUAwAAAATAKBgYrJAMDAQIFAAOB"
			+ "gQBpSRdnDb6AcNVaXSmGo6+kVPIBhot1LzJOGaPyDNpGXxd7LV4tMBF1U7gr"
			+ "4k1g9BO6YiMWvw9uiTZmn0CfV8+k4fWEuG/nmafRoGIuay2f+ILuT+C0rnp1"
			+ "4FgMsEhuVNJJAmb12QV0PZII+UneyhAneZuQQzVUkTcVgYxogxdSOzCCAlUw"
			+ "ggHBoAMCAQICBACdekowCgYGKyQDAwECBQAwbzELMAkGA1UEBhMCREUxPTA7"
			+ "BgNVBAoUNFJlZ3VsaWVydW5nc2JlaMhvcmRlIGbIdXIgVGVsZWtvbW11bmlr"
			+ "YXRpb24gdW5kIFBvc3QxITAMBgcCggYBCgcUEwExMBEGA1UEAxQKNlItQ2Eg"
			+ "MTpQTjAiGA8yMDAxMDIwMTEzNDcwN1oYDzIwMDUwMzIyMDg1NTUxWjBvMQsw"
			+ "CQYDVQQGEwJERTE9MDsGA1UEChQ0UmVndWxpZXJ1bmdzYmVoyG9yZGUgZsh1"
			+ "ciBUZWxla29tbXVuaWthdGlvbiB1bmQgUG9zdDEhMAwGBwKCBgEKBxQTATEw"
			+ "EQYDVQQDFAo1Ui1DQSAxOlBOMIGhMA0GCSqGSIb3DQEBAQUAA4GPADCBiwKB"
			+ "gQCKHkFTJx8GmoqFTxEOxpK9XkC3NZ5dBEKiUv0Ife3QMqeGMoCUnyJxwW0k"
			+ "2/53duHxtv2yHSZpFKjrjvE/uGwdOMqBMTjMzkFg19e9JPv061wyADOucOIa"
			+ "NAgha/zFt9XUyrHF21knKCvDNExv2MYIAagkTKajLMAw0bu1J0FadQIFAMAA"
			+ "AAEwCgYGKyQDAwECBQADgYEAV1yTi+2gyB7sUhn4PXmi/tmBxAfe5oBjDW8m"
			+ "gxtfudxKGZ6l/FUPNcrSc5oqBYxKWtLmf3XX87LcblYsch617jtNTkMzhx9e"
			+ "qxiD02ufcrxz2EVt0Akdqiz8mdVeqp3oLcNU/IttpSrcA91CAnoUXtDZYwb/"
			+ "gdQ4FI9l3+qo/0UwggJVMIIBwaADAgECAgQAxIymMAoGBiskAwMBAgUAMG8x"
			+ "CzAJBgNVBAYTAkRFMT0wOwYDVQQKFDRSZWd1bGllcnVuZ3NiZWjIb3JkZSBm"
			+ "yHVyIFRlbGVrb21tdW5pa2F0aW9uIHVuZCBQb3N0MSEwDAYHAoIGAQoHFBMB"
			+ "MTARBgNVBAMUCjZSLUNhIDE6UE4wIhgPMjAwMTEwMTUxMzMxNThaGA8yMDA1"
			+ "MDYwMTA5NTIxN1owbzELMAkGA1UEBhMCREUxPTA7BgNVBAoUNFJlZ3VsaWVy"
			+ "dW5nc2JlaMhvcmRlIGbIdXIgVGVsZWtvbW11bmlrYXRpb24gdW5kIFBvc3Qx"
			+ "ITAMBgcCggYBCgcUEwExMBEGA1UEAxQKN1ItQ0EgMTpQTjCBoTANBgkqhkiG"
			+ "9w0BAQEFAAOBjwAwgYsCgYEAiokD/j6lEP4FexF356OpU5teUpGGfUKjIrFX"
			+ "BHc79G0TUzgVxqMoN1PWnWktQvKo8ETaugxLkP9/zfX3aAQzDW4Zki6x6GDq"
			+ "fy09Agk+RJvhfbbIzRkV4sBBco0n73x7TfG/9NTgVr/96U+I+z/1j30aboM6"
			+ "9OkLEhjxAr0/GbsCBQDAAAABMAoGBiskAwMBAgUAA4GBAHWRqRixt+EuqHhR"
			+ "K1kIxKGZL2vZuakYV0R24Gv/0ZR52FE4ECr+I49o8FP1qiGSwnXB0SwjuH2S"
			+ "iGiSJi+iH/MeY85IHwW1P5e+bOMvEOFhZhQXQixOD7totIoFtdyaj1XGYRef"
			+ "0f2cPOjNJorXHGV8wuBk+/j++sxbd/Net3FtMIICVTCCAcGgAwIBAgIEAMSM"
			+ "pzAKBgYrJAMDAQIFADBvMQswCQYDVQQGEwJERTE9MDsGA1UEChQ0UmVndWxp"
			+ "ZXJ1bmdzYmVoyG9yZGUgZsh1ciBUZWxla29tbXVuaWthdGlvbiB1bmQgUG9z"
			+ "dDEhMAwGBwKCBgEKBxQTATEwEQYDVQQDFAo3Ui1DQSAxOlBOMCIYDzIwMDEx"
			+ "MDE1MTMzNDE0WhgPMjAwNTA2MDEwOTUyMTdaMG8xCzAJBgNVBAYTAkRFMT0w"
			+ "OwYDVQQKFDRSZWd1bGllcnVuZ3NiZWjIb3JkZSBmyHVyIFRlbGVrb21tdW5p"
			+ "a2F0aW9uIHVuZCBQb3N0MSEwDAYHAoIGAQoHFBMBMTARBgNVBAMUCjZSLUNh"
			+ "IDE6UE4wgaEwDQYJKoZIhvcNAQEBBQADgY8AMIGLAoGBAIOiqxUkzVyqnvth"
			+ "ihnltsE5m1Xn5TZKeR/2MQPStc5hJ+V4yptEtIx+Fn5rOoqT5VEVWhcE35wd"
			+ "bPvgJyQFn5msmhPQT/6XSGOlrWRoFummXN9lQzAjCj1sgTcmoLCVQ5s5WpCA"
			+ "OXFwVWu16qndz3sPItn3jJ0F3Kh3w79NglvPAgUAwAAAATAKBgYrJAMDAQIF"
			+ "AAOBgQBi5W96UVDoNIRkCncqr1LLG9vF9SGBIkvFpLDIIbcvp+CXhlvsdCJl"
			+ "0pt2QEPSDl4cmpOet+CxJTdTuMeBNXxhb7Dvualog69w/+K2JbPhZYxuVFZs"
			+ "Zh5BkPn2FnbNu3YbJhE60aIkikr72J4XZsI5DxpZCGh6xyV/YPRdKSljFjCC"
			+ "AlQwggHAoAMCAQICAwyDqzAKBgYrJAMDAQIFADBvMQswCQYDVQQGEwJERTE9"
			+ "MDsGA1UEChQ0UmVndWxpZXJ1bmdzYmVoyG9yZGUgZsh1ciBUZWxla29tbXVu"
			+ "aWthdGlvbiB1bmQgUG9zdDEhMAwGBwKCBgEKBxQTATEwEQYDVQQDFAo1Ui1D"
			+ "QSAxOlBOMCIYDzIwMDAwMzIyMDk0MTI3WhgPMjAwNDAxMjExNjA0NTNaMG8x"
			+ "CzAJBgNVBAYTAkRFMT0wOwYDVQQKFDRSZWd1bGllcnVuZ3NiZWjIb3JkZSBm"
			+ "yHVyIFRlbGVrb21tdW5pa2F0aW9uIHVuZCBQb3N0MSEwDAYHAoIGAQoHFBMB"
			+ "MTARBgNVBAMUCjRSLUNBIDE6UE4wgaEwDQYJKoZIhvcNAQEBBQADgY8AMIGL"
			+ "AoGBAI8x26tmrFJanlm100B7KGlRemCD1R93PwdnG7svRyf5ZxOsdGrDszNg"
			+ "xg6ouO8ZHQMT3NC2dH8TvO65Js+8bIyTm51azF6clEg0qeWNMKiiXbBXa+ph"
			+ "hTkGbXiLYvACZ6/MTJMJ1lcrjpRF7BXtYeYMcEF6znD4pxOqrtbf9z5hAgUA"
			+ "wAAAATAKBgYrJAMDAQIFAAOBgQB99BjSKlGPbMLQAgXlvA9jUsDNhpnVm3a1"
			+ "YkfxSqS/dbQlYkbOKvCxkPGA9NBxisBM8l1zFynVjJoy++aysRmcnLY/sHaz"
			+ "23BF2iU7WERy18H3lMBfYB6sXkfYiZtvQZcWaO48m73ZBySuiV3iXpb2wgs/"
			+ "Cs20iqroAWxwq/W/9jCCAlMwggG/oAMCAQICBDsFZ9UwCgYGKyQDAwECBQAw"
			+ "bzELMAkGA1UEBhMCREUxITAMBgcCggYBCgcUEwExMBEGA1UEAxQKNFItQ0Eg"
			+ "MTpQTjE9MDsGA1UEChQ0UmVndWxpZXJ1bmdzYmVoyG9yZGUgZsh1ciBUZWxl"
			+ "a29tbXVuaWthdGlvbiB1bmQgUG9zdDAiGA8xOTk5MDEyMTE3MzUzNFoYDzIw"
			+ "MDQwMTIxMTYwMDAyWjBvMQswCQYDVQQGEwJERTE9MDsGA1UEChQ0UmVndWxp"
			+ "ZXJ1bmdzYmVoyG9yZGUgZsh1ciBUZWxla29tbXVuaWthdGlvbiB1bmQgUG9z"
			+ "dDEhMAwGBwKCBgEKBxQTATEwEQYDVQQDFAozUi1DQSAxOlBOMIGfMA0GCSqG"
			+ "SIb3DQEBAQUAA4GNADCBiQKBgI4B557mbKQg/AqWBXNJhaT/6lwV93HUl4U8"
			+ "u35udLq2+u9phns1WZkdM3gDfEpL002PeLfHr1ID/96dDYf04lAXQfombils"
			+ "of1C1k32xOvxjlcrDOuPEMxz9/HDAQZA5MjmmYHAIulGI8Qg4Tc7ERRtg/hd"
			+ "0QX0/zoOeXoDSEOBAgTAAAABMAoGBiskAwMBAgUAA4GBAIyzwfT3keHI/n2P"
			+ "LrarRJv96mCohmDZNpUQdZTVjGu5VQjVJwk3hpagU0o/t/FkdzAjOdfEw8Ql"
			+ "3WXhfIbNLv1YafMm2eWSdeYbLcbB5yJ1od+SYyf9+tm7cwfDAcr22jNRBqx8"
			+ "wkWKtKDjWKkevaSdy99sAI8jebHtWz7jzydKMIID9TCCA16gAwIBAgICbMcw"
			+ "DQYJKoZIhvcNAQEFBQAwSzELMAkGA1UEBhMCREUxEjAQBgNVBAoUCVNpZ250"
			+ "cnVzdDEoMAwGBwKCBgEKBxQTATEwGAYDVQQDFBFDQSBTSUdOVFJVU1QgMTpQ"
			+ "TjAeFw0wNDA3MzAxMzAyNDZaFw0wNzA3MzAxMzAyNDZaMDwxETAPBgNVBAMM"
			+ "CFlhY29tOlBOMQ4wDAYDVQRBDAVZYWNvbTELMAkGA1UEBhMCREUxCjAIBgNV"
			+ "BAUTATEwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAIWzLlYLQApocXIp"
			+ "pgCCpkkOUVLgcLYKeOd6/bXAnI2dTHQqT2bv7qzfUnYvOqiNgYdF13pOYtKg"
			+ "XwXMTNFL4ZOI6GoBdNs9TQiZ7KEWnqnr2945HYx7UpgTBclbOK/wGHuCdcwO"
			+ "x7juZs1ZQPFG0Lv8RoiV9s6HP7POqh1sO0P/AgMBAAGjggH1MIIB8TCBnAYD"
			+ "VR0jBIGUMIGRgBQcZzNghfnXoXRm8h1+VITC5caNRqFzpHEwbzELMAkGA1UE"
			+ "BhMCREUxPTA7BgNVBAoUNFJlZ3VsaWVydW5nc2JlaMhvcmRlIGbIdXIgVGVs"
			+ "ZWtvbW11bmlrYXRpb24gdW5kIFBvc3QxITAMBgcCggYBCgcUEwExMBEGA1UE"
			+ "AxQKNVItQ0EgMTpQToIEALs8rjAdBgNVHQ4EFgQU2e5KAzkVuKaM9I5heXkz"
			+ "bcAIuR8wDgYDVR0PAQH/BAQDAgZAMBIGA1UdIAQLMAkwBwYFKyQIAQEwfwYD"
			+ "VR0fBHgwdjB0oCygKoYobGRhcDovL2Rpci5zaWdudHJ1c3QuZGUvbz1TaWdu"
			+ "dHJ1c3QsYz1kZaJEpEIwQDEdMBsGA1UEAxMUQ1JMU2lnblNpZ250cnVzdDE6"
			+ "UE4xEjAQBgNVBAoTCVNpZ250cnVzdDELMAkGA1UEBhMCREUwYgYIKwYBBQUH"
			+ "AQEEVjBUMFIGCCsGAQUFBzABhkZodHRwOi8vZGlyLnNpZ250cnVzdC5kZS9T"
			+ "aWdudHJ1c3QvT0NTUC9zZXJ2bGV0L2h0dHBHYXRld2F5LlBvc3RIYW5kbGVy"
			+ "MBgGCCsGAQUFBwEDBAwwCjAIBgYEAI5GAQEwDgYHAoIGAQoMAAQDAQH/MA0G"
			+ "CSqGSIb3DQEBBQUAA4GBAHn1m3GcoyD5GBkKUY/OdtD6Sj38LYqYCF+qDbJR"
			+ "6pqUBjY2wsvXepUppEler+stH8mwpDDSJXrJyuzf7xroDs4dkLl+Rs2x+2tg"
			+ "BjU+ABkBDMsym2WpwgA8LCdymmXmjdv9tULxY+ec2pjSEzql6nEZNEfrU8nt"
			+ "ZCSCavgqW4TtMYIBejCCAXYCAQEwUTBLMQswCQYDVQQGEwJERTESMBAGA1UE"
			+ "ChQJU2lnbnRydXN0MSgwDAYHAoIGAQoHFBMBMTAYBgNVBAMUEUNBIFNJR05U"
			+ "UlVTVCAxOlBOAgJsxzAJBgUrDgMCGgUAoIGAMBgGCSqGSIb3DQEJAzELBgkq"
			+ "hkiG9w0BBwEwIwYJKoZIhvcNAQkEMRYEFIYfhPoyfGzkLWWSSLjaHb4HQmaK"
			+ "MBwGCSqGSIb3DQEJBTEPFw0wNTAzMjQwNzM4MzVaMCEGBSskCAYFMRgWFi92"
			+ "YXIvZmlsZXMvdG1wXzEvdGVzdDEwDQYJKoZIhvcNAQEFBQAEgYA2IvA8lhVz"
			+ "VD5e/itUxbFboKxeKnqJ5n/KuO/uBCl1N14+7Z2vtw1sfkIG+bJdp3OY2Cmn"
			+ "mrQcwsN99Vjal4cXVj8t+DJzFG9tK9dSLvD3q9zT/GQ0kJXfimLVwCa4NaSf"
			+ "Qsu4xtG0Rav6bCcnzabAkKuNNvKtH8amSRzk870DBg==");

		private void VerifySignatures(
			CmsSignedData	s,
			byte[]			contentDigest)
		{
			IX509Store x509Certs = s.GetCertificates("Collection");

			SignerInformationStore signers = s.GetSignerInfos();
			ICollection c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));

				if (contentDigest != null)
				{
					Assert.IsTrue(Arrays.AreEqual(contentDigest, signer.GetContentDigest()));
				}
			}
		}

		private void VerifySignatures(
			CmsSignedData s)
		{
			VerifySignatures(s, null);
		}

		[Test]
		public void TestSha1AndMD5WithRsaEncapsulatedRepeated()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(OrigCert);
			certList.Add(SignCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestMD5);

			gen.AddCertificates(x509Certs);

			CmsSignedData s = gen.Generate(msg, true);

			s = new CmsSignedData(ContentInfo.GetInstance(Asn1Object.FromByteArray(s.GetEncoded())));

			x509Certs = s.GetCertificates("Collection");

			SignerInformationStore signers = s.GetSignerInfos();

			Assert.AreEqual(2, signers.Count);

			SignerID sid = null;
			ICollection c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				sid = signer.SignerID;

				Assert.IsTrue(signer.Verify(cert));

				//
				// check content digest
				//

				byte[] contentDigest = (byte[])gen.GetGeneratedDigests()[signer.DigestAlgOid];

				AttributeTable table = signer.SignedAttributes;
				Asn1.Cms.Attribute hash = table[CmsAttributes.MessageDigest];

				Assert.IsTrue(Arrays.AreEqual(contentDigest, ((Asn1OctetString)hash.AttrValues[0]).GetOctets()));
			}

			c = signers.GetSigners(sid);

			Assert.AreEqual(2, c.Count);

			//
			// try using existing signer
			//

			gen = new CmsSignedDataGenerator();

			gen.AddSigners(s.GetSignerInfos());

			gen.AddCertificates(s.GetCertificates("Collection"));
			gen.AddCrls(s.GetCrls("Collection"));

			s = gen.Generate(msg, true);

			s = new CmsSignedData(ContentInfo.GetInstance(Asn1Object.FromByteArray(s.GetEncoded())));

			x509Certs = s.GetCertificates("Collection");

			signers = s.GetSignerInfos();
			c = signers.GetSigners();

			Assert.AreEqual(2, c.Count);

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.AreEqual(true, signer.Verify(cert));
			}

			CheckSignerStoreReplacement(s, signers);
		}

		// NB: C# build doesn't support "no attributes" version of CmsSignedDataGenerator.Generate
//		[Test]
//		public void TestSha1WithRsaNoAttributes()
//		{
//			IList certList = new ArrayList();
//			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello world!"));
//
//			certList.Add(OrigCert);
//			certList.Add(SignCert);
//
//			IX509Store x509Certs = X509StoreFactory.Create(
//				"Certificate/Collection",
//				new X509CollectionStoreParameters(certList));
//
//			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();
//
//			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);
//
//			gen.AddCertificates(x509Certs);
//
//			CmsSignedData s = gen.Generate(CmsSignedDataGenerator.Data, msg, false, false);
//
//			//
//			// compute expected content digest
//			//
//			IDigest md = DigestUtilities.GetDigest("SHA1");
//
//			byte[] testBytes = Encoding.ASCII.GetBytes("Hello world!");
//			md.BlockUpdate(testBytes, 0, testBytes.Length);
//			byte[] hash = DigestUtilities.DoFinal(md);
//
//			VerifySignatures(s, hash);
//		}

		[Test]
		public void TestSha1WithRsaAndAttributeTable()
		{
			byte[] testBytes = Encoding.ASCII.GetBytes("Hello world!");

			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(testBytes);

			certList.Add(OrigCert);
			certList.Add(SignCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			IDigest md = DigestUtilities.GetDigest("SHA1");
			md.BlockUpdate(testBytes, 0, testBytes.Length);
			byte[] hash = DigestUtilities.DoFinal(md);

			Asn1.Cms.Attribute attr = new Asn1.Cms.Attribute(CmsAttributes.MessageDigest,
				new DerSet(new DerOctetString(hash)));

			Asn1EncodableVector v = new Asn1EncodableVector(attr);

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1,
				new AttributeTable(v), null);

			gen.AddCertificates(x509Certs);

			CmsSignedData s = gen.Generate(CmsSignedDataGenerator.Data, null, false);

			//
			// the signature is detached, so need to add msg before passing on
			//
			s = new CmsSignedData(msg, s.GetEncoded());

			//
			// compute expected content digest
			//
			VerifySignatures(s, hash);
		}

		[Test]
		public void TestSha1WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestSha1);
		}

		[Test]
		public void TestSha224WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestSha224);
		}

		[Test]
		public void TestSha256WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestSha256);
		}

		[Test]
		public void TestRipeMD128WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestRipeMD128);
		}

		[Test]
		public void TestRipeMD160WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestRipeMD160);
		}

		[Test]
		public void TestRipeMD256WithRsaEncapsulated()
		{
			EncapsulatedTest(SignKP, SignCert, CmsSignedDataGenerator.DigestRipeMD256);
		}

		[Test]
		public void TestECDsaEncapsulated()
		{
			EncapsulatedTest(SignECDsaKP, SignECDsaCert, CmsSignedDataGenerator.DigestSha1);
		}

		[Test]
		public void TestECDsaSha224Encapsulated()
		{
			EncapsulatedTest(SignECDsaKP, SignECDsaCert, CmsSignedDataGenerator.DigestSha224);
		}

		[Test]
		public void TestECDsaSha256Encapsulated()
		{
			EncapsulatedTest(SignECDsaKP, SignECDsaCert, CmsSignedDataGenerator.DigestSha256);
		}

		[Test]
		public void TestECDsaSha384Encapsulated()
		{
			EncapsulatedTest(SignECDsaKP, SignECDsaCert, CmsSignedDataGenerator.DigestSha384);
		}

		[Test]
		public void TestECDsaSha512Encapsulated()
		{
			EncapsulatedTest(SignECDsaKP, SignECDsaCert, CmsSignedDataGenerator.DigestSha512);
		}

		[Test]
		public void TestECDsaSha512EncapsulatedWithKeyFactoryAsEC()
		{
//			X509EncodedKeySpec  pubSpec = new X509EncodedKeySpec(_signEcDsaKP.getPublic().getEncoded());
			byte[] pubEnc = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(SignECDsaKP.Public).GetDerEncoded();
//			PKCS8EncodedKeySpec privSpec = new PKCS8EncodedKeySpec(_signEcDsaKP.getPrivate().getEncoded());
			byte[] privEnc = PrivateKeyInfoFactory.CreatePrivateKeyInfo(SignECDsaKP.Private).GetDerEncoded();
//			KeyFactory          keyFact = KeyFactory.getInstance("EC", "BC");
//			KeyPair             kp = new KeyPair(keyFact.generatePublic(pubSpec), keyFact.generatePrivate(privSpec));
			AsymmetricCipherKeyPair kp = new AsymmetricCipherKeyPair(
                PublicKeyFactory.CreateKey(pubEnc),
				PrivateKeyFactory.CreateKey(privEnc));

			EncapsulatedTest(kp, SignECDsaCert, CmsSignedDataGenerator.DigestSha512);
		}

		[Test]
		public void TestDsaEncapsulated()
		{
			EncapsulatedTest(SignDsaKP, SignDsaCert, CmsSignedDataGenerator.DigestSha1);
		}

		[Test]
		public void TestGost3411WithGost3410Encapsulated()
		{
			EncapsulatedTest(SignGostKP, SignGostCert, CmsSignedDataGenerator.DigestGost3411);
		}

		[Test]
		public void TestGost3411WithECGost3410Encapsulated()
		{
			EncapsulatedTest(SignECGostKP, SignECGostCert, CmsSignedDataGenerator.DigestGost3411);
		}

		private void EncapsulatedTest(
			AsymmetricCipherKeyPair	signaturePair,
			X509Certificate			signatureCert,
			string					digestAlgorithm)
		{
			IList certList = new ArrayList();
			IList crlList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(signatureCert);
			certList.Add(OrigCert);

			crlList.Add(SignCrl);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));
			IX509Store x509Crls = X509StoreFactory.Create(
				"CRL/Collection",
				new X509CollectionStoreParameters(crlList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(signaturePair.Private, signatureCert, digestAlgorithm);

			gen.AddCertificates(x509Certs);
			gen.AddCrls(x509Crls);

			CmsSignedData s = gen.Generate(msg, true);

			s = new CmsSignedData(ContentInfo.GetInstance(Asn1Object.FromByteArray(s.GetEncoded())));

			x509Certs = s.GetCertificates("Collection");
			x509Crls = s.GetCrls("Collection");

			SignerInformationStore signers = s.GetSignerInfos();
			ICollection c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));
			}

			//
			// check for CRLs
			//
			ArrayList crls = new ArrayList(x509Crls.GetMatches(null));

			Assert.AreEqual(1, crls.Count);

			Assert.IsTrue(crls.Contains(SignCrl));

			//
			// try using existing signer
			//

			gen = new CmsSignedDataGenerator();

			gen.AddSigners(s.GetSignerInfos());

			gen.AddCertificates(s.GetCertificates("Collection"));
			gen.AddCrls(s.GetCrls("Collection"));

			s = gen.Generate(msg, true);

			s = new CmsSignedData(ContentInfo.GetInstance(Asn1Object.FromByteArray(s.GetEncoded())));

			x509Certs = s.GetCertificates("Collection");
			x509Crls = s.GetCrls("Collection");

			signers = s.GetSignerInfos();
			c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));
			}

			CheckSignerStoreReplacement(s, signers);
		}

		//
		// signerInformation store replacement test.
		//
		private void CheckSignerStoreReplacement(
			CmsSignedData orig,
			SignerInformationStore signers)
		{
			CmsSignedData s = CmsSignedData.ReplaceSigners(orig, signers);

			IX509Store x509Certs = s.GetCertificates("Collection");

			signers = s.GetSignerInfos();
			ICollection c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));
			}
		}

		[Test]
		public void TestUnsortedAttributes()
		{
			CmsSignedData s = new CmsSignedData(new CmsProcessableByteArray(disorderedMessage), disorderedSet);

			IX509Store x509Certs = s.GetCertificates("Collection");

			SignerInformationStore	signers = s.GetSignerInfos();
			ICollection				c = signers.GetSigners();

			foreach (SignerInformation signer in c)
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate) certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));
			}
		}

		[Test]
		public void TestNullContentWithSigner()
		{
			IList certList = new ArrayList();

			certList.Add(OrigCert);
			certList.Add(SignCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData s = gen.Generate(null, false);

			s = new CmsSignedData(ContentInfo.GetInstance(Asn1Object.FromByteArray(s.GetEncoded())));

			VerifySignatures(s);
		}

		[Test]
		public void TestWithAttributeCertificate()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(SignDsaCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			IX509AttributeCertificate attrCert = CmsTestUtil.GetAttributeCertificate();

			ArrayList attrCerts = new ArrayList();
			attrCerts.Add(attrCert);

			IX509Store store = X509StoreFactory.Create(
				"AttributeCertificate/Collection",
				new X509CollectionStoreParameters(attrCerts));

			gen.AddAttributeCertificates(store);

			CmsSignedData sd = gen.Generate(msg);

			Assert.AreEqual(4, sd.Version);

			store = sd.GetAttributeCertificates("Collection");

			ArrayList coll = new ArrayList(store.GetMatches(null));

			Assert.AreEqual(1, coll.Count);

			Assert.IsTrue(coll.Contains(attrCert));

			//
			// create new certstore
			//
			certList = new ArrayList();
			certList.Add(OrigCert);
			certList.Add(SignCert);

			x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			//
			// replace certs
			//
			sd = CmsSignedData.ReplaceCertificatesAndCrls(sd, x509Certs, null, null);

			VerifySignatures(sd);
		}

		[Test]
		public void TestCertStoreReplacement()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(SignDsaCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData sd = gen.Generate(msg);

			//
			// create new certstore
			//
			certList = new ArrayList();
			certList.Add(OrigCert);
			certList.Add(SignCert);

			x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			//
			// replace certs
			//
			sd = CmsSignedData.ReplaceCertificatesAndCrls(sd, x509Certs, null, null);

			VerifySignatures(sd);
		}

		[Test]
		public void TestEncapsulatedCertStoreReplacement()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(SignDsaCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData sd = gen.Generate(msg, true);

			//
			// create new certstore
			//
			certList = new ArrayList();
			certList.Add(OrigCert);
			certList.Add(SignCert);

			x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			//
			// replace certs
			//
			sd = CmsSignedData.ReplaceCertificatesAndCrls(sd, x509Certs, null, null);

			VerifySignatures(sd);
		}

		[Test]
		public void TestCertOrdering1()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(OrigCert);
			certList.Add(SignCert);
			certList.Add(SignDsaCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData sd = gen.Generate(msg, true);

			x509Certs = sd.GetCertificates("Collection");
			ArrayList a = new ArrayList(x509Certs.GetMatches(null));

			Assert.AreEqual(3, a.Count);
			Assert.AreEqual(OrigCert, a[0]);
			Assert.AreEqual(SignCert, a[1]);
			Assert.AreEqual(SignDsaCert, a[2]);
		}

		[Test]
		public void TestCertOrdering2()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(SignCert);
			certList.Add(SignDsaCert);
			certList.Add(OrigCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));
	
			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData sd = gen.Generate(msg, true);

			x509Certs = sd.GetCertificates("Collection");
			ArrayList a = new ArrayList(x509Certs.GetMatches(null));

			Assert.AreEqual(3, a.Count);
			Assert.AreEqual(SignCert, a[0]);
			Assert.AreEqual(SignDsaCert, a[1]);
			Assert.AreEqual(OrigCert, a[2]);
		}

		[Test]
		public void TestSignerStoreReplacement()
		{
			IList certList = new ArrayList();
			CmsProcessable msg = new CmsProcessableByteArray(Encoding.ASCII.GetBytes("Hello World!"));

			certList.Add(OrigCert);
			certList.Add(SignCert);

			IX509Store x509Certs = X509StoreFactory.Create(
				"Certificate/Collection",
				new X509CollectionStoreParameters(certList));

			CmsSignedDataGenerator gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha1);

			gen.AddCertificates(x509Certs);

			CmsSignedData original = gen.Generate(msg, true);

			//
			// create new Signer
			//
			gen = new CmsSignedDataGenerator();

			gen.AddSigner(OrigKP.Private, OrigCert, CmsSignedDataGenerator.DigestSha224);

			gen.AddCertificates(x509Certs);

			CmsSignedData newSD = gen.Generate(msg, true);

			//
			// replace signer
			//
			CmsSignedData sd = CmsSignedData.ReplaceSigners(original, newSD.GetSignerInfos());

			IEnumerator signerEnum = sd.GetSignerInfos().GetSigners().GetEnumerator();
			signerEnum.MoveNext();
			SignerInformation signer = (SignerInformation) signerEnum.Current;

			Assert.AreEqual(CmsSignedDataGenerator.DigestSha224, signer.DigestAlgOid);

			// we use a parser here as it requires the digests to be correct in the digest set, if it
			// isn't we'll get a NullPointerException
			CmsSignedDataParser sp = new CmsSignedDataParser(sd.GetEncoded());

			sp.GetSignedContent().Drain();

			VerifySignatures(sp);
		}

		private void VerifySignatures(
			CmsSignedDataParser sp)
		{
			IX509Store x509Certs = sp.GetCertificates("Collection");
			SignerInformationStore signers = sp.GetSignerInfos();

			foreach (SignerInformation signer in signers.GetSigners())
			{
				ICollection certCollection = x509Certs.GetMatches(signer.SignerID);

				IEnumerator certEnum = certCollection.GetEnumerator();
				certEnum.MoveNext();
				X509Certificate cert = (X509Certificate)certEnum.Current;

				Assert.IsTrue(signer.Verify(cert));
			}
		}
	}
}
