using System;

using NUnit.Framework;

using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Crypto.Tests
{
	/**
	 * ECDSA tests are taken from X9.62.
	 */
	[TestFixture]
	public class ECTest
		: SimpleTest
	{
		/**
		* X9.62 - 1998,<br/>
		* J.3.1, Page 152, ECDSA over the field Fp<br/>
		* an example with 192 bit prime
		*/
		[Test]
		public void TestECDsa192bitPrime()
		{
			BigInteger r = new BigInteger("3342403536405981729393488334694600415596881826869351677613");
			BigInteger s = new BigInteger("5735822328888155254683894997897571951568553642892029982342");

			byte[] kData = BigIntegers.AsUnsignedByteArray(new BigInteger("6140507067065001063065065565667405560006161556565665656654"));

			SecureRandom k = FixedSecureRandom.From(kData);

			FpCurve curve = new FpCurve(
				new BigInteger("6277101735386680763835789423207666416083908700390324961279"), // q
				new BigInteger("fffffffffffffffffffffffffffffffefffffffffffffffc", 16), // a
				new BigInteger("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012")), // G
				new BigInteger("6277101735386680763835789423176059013767194773182842284081")); // n

			ECPrivateKeyParameters priKey = new ECPrivateKeyParameters(
				"ECDSA",
				new BigInteger("651056770906015076056810763456358567190100156695615665659"), // d
				parameters);

			ParametersWithRandom param = new ParametersWithRandom(priKey, k);

			ECDsaSigner ecdsa = new ECDsaSigner();

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			if (!r.Equals(sig[0]))
			{
				Fail("r component wrong." + Environment.NewLine
					+ " expecting: " + r + Environment.NewLine
					+ " got      : " + sig[0]);
			}

			if (!s.Equals(sig[1]))
			{
				Fail("s component wrong." + Environment.NewLine
					+ " expecting: " + s + Environment.NewLine
					+ " got      : " + sig[1]);
			}

			// Verify the signature
			ECPublicKeyParameters pubKey = new ECPublicKeyParameters(
				"ECDSA",
				curve.DecodePoint(Hex.Decode("0262b12d60690cdcf330babab6e69763b471f994dd702d16a5")), // Q
				parameters);

			ecdsa.Init(false, pubKey);
			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("verification fails");
			}
		}

		[Test]
		public void TestDecode()
		{
			FpCurve curve = new FpCurve(
				new BigInteger("6277101735386680763835789423207666416083908700390324961279"), // q
				new BigInteger("fffffffffffffffffffffffffffffffefffffffffffffffc", 16), // a
				new BigInteger("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1", 16)); // b

			ECPoint p = curve.DecodePoint(Hex.Decode("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012"));

			if (!p.X.ToBigInteger().Equals(new BigInteger("188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012", 16)))
			{
				Fail("x uncompressed incorrectly");
			}

			if (!p.Y.ToBigInteger().Equals(new BigInteger("7192b95ffc8da78631011ed6b24cdd573f977a11e794811", 16)))
			{
				Fail("y uncompressed incorrectly");
			}

			byte[] encoding = p.GetEncoded();

			if (!AreEqual(encoding, Hex.Decode("03188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012")))
			{
				Fail("point compressed incorrectly");
			}
		}

		/**
		 * X9.62 - 1998,<br/>
		 * J.3.2, Page 155, ECDSA over the field Fp<br/>
		 * an example with 239 bit prime
		 */
		[Test]
		public void TestECDsa239bitPrime()
		{
			BigInteger r = new BigInteger("308636143175167811492622547300668018854959378758531778147462058306432176");
			BigInteger s = new BigInteger("323813553209797357708078776831250505931891051755007842781978505179448783");

			byte[] kData = BigIntegers.AsUnsignedByteArray(new BigInteger("700000017569056646655505781757157107570501575775705779575555657156756655"));

			SecureRandom k = FixedSecureRandom.From(kData);

			FpCurve curve = new FpCurve(
				new BigInteger("883423532389192164791648750360308885314476597252960362792450860609699839"), // q
				new BigInteger("7fffffffffffffffffffffff7fffffffffff8000000000007ffffffffffc", 16), // a
				new BigInteger("6b016c3bdcf18941d0d654921475ca71a9db2fb27d1d37796185c2942c0a", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("020ffa963cdca8816ccc33b8642bedf905c3d358573d3f27fbbd3b3cb9aaaf")), // G
				new BigInteger("883423532389192164791648750360308884807550341691627752275345424702807307")); // n

			ECPrivateKeyParameters priKey = new ECPrivateKeyParameters(
				"ECDSA",
				new BigInteger("876300101507107567501066130761671078357010671067781776716671676178726717"), // d
				parameters);

			ECDsaSigner ecdsa = new ECDsaSigner();
			ParametersWithRandom param = new ParametersWithRandom(priKey, k);

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			if (!r.Equals(sig[0]))
			{
				Fail("r component wrong." + Environment.NewLine
					+ " expecting: " + r + Environment.NewLine
					+ " got      : " + sig[0]);
			}

			if (!s.Equals(sig[1]))
			{
				Fail("s component wrong." + Environment.NewLine
					+ " expecting: " + s + Environment.NewLine
					+ " got      : " + sig[1]);
			}

			// Verify the signature
			ECPublicKeyParameters pubKey = new ECPublicKeyParameters(
				"ECDSA",
				curve.DecodePoint(Hex.Decode("025b6dc53bc61a2548ffb0f671472de6c9521a9d2d2534e65abfcbd5fe0c70")), // Q
				parameters);

			ecdsa.Init(false, pubKey);
			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("signature fails");
			}
		}


		/**
		 * X9.62 - 1998,<br/>
		 * J.2.1, Page 100, ECDSA over the field F2m<br/>
		 * an example with 191 bit binary field
		 */
		[Test]
		public void TestECDsa191bitBinary()
		{
			BigInteger r = new BigInteger("87194383164871543355722284926904419997237591535066528048");
			BigInteger s = new BigInteger("308992691965804947361541664549085895292153777025772063598");

			byte[] kData = BigIntegers.AsUnsignedByteArray(new BigInteger("1542725565216523985789236956265265265235675811949404040041"));

			SecureRandom k = FixedSecureRandom.From(kData);

			F2mCurve curve = new F2mCurve(
				191, // m
				9, //k
				new BigInteger("2866537B676752636A68F56554E12640276B649EF7526267", 16), // a
				new BigInteger("2E45EF571F00786F67B0081B9495A3D95462F5DE0AA185EC", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("0436B3DAF8A23206F9C4F299D7B21A9C369137F2C84AE1AA0D765BE73433B3F95E332932E70EA245CA2418EA0EF98018FB")), // G
				new BigInteger("1569275433846670190958947355803350458831205595451630533029"), // n
				BigInteger.Two); // h

			ECPrivateKeyParameters priKey = new ECPrivateKeyParameters(
				"ECDSA",
				new BigInteger("1275552191113212300012030439187146164646146646466749494799"), // d
				parameters);

			ECDsaSigner ecdsa = new ECDsaSigner();
			ParametersWithRandom param = new ParametersWithRandom(priKey, k);

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			if (!r.Equals(sig[0]))
			{
				Fail("r component wrong." + Environment.NewLine
					+ " expecting: " + r + Environment.NewLine
					+ " got      : " + sig[0]);
			}

			if (!s.Equals(sig[1]))
			{
				Fail("s component wrong." + Environment.NewLine
					+ " expecting: " + s + Environment.NewLine
					+ " got      : " + sig[1]);
			}

			// Verify the signature
			ECPublicKeyParameters pubKey = new ECPublicKeyParameters(
				"ECDSA",
				curve.DecodePoint(Hex.Decode("045DE37E756BD55D72E3768CB396FFEB962614DEA4CE28A2E755C0E0E02F5FB132CAF416EF85B229BBB8E1352003125BA1")), // Q
				parameters);

			ecdsa.Init(false, pubKey);
			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("signature fails");
			}
		}

		/**
		 * X9.62 - 1998,<br/>
		 * J.2.1, Page 100, ECDSA over the field F2m<br/>
		 * an example with 191 bit binary field
		 */
		[Test]
		public void TestECDsa239bitBinary()
		{
			BigInteger r = new BigInteger("21596333210419611985018340039034612628818151486841789642455876922391552");
			BigInteger s = new BigInteger("197030374000731686738334997654997227052849804072198819102649413465737174");

			byte[] kData = BigIntegers.AsUnsignedByteArray(new BigInteger("171278725565216523967285789236956265265265235675811949404040041670216363"));

			SecureRandom k = FixedSecureRandom.From(kData);

			F2mCurve curve = new F2mCurve(
				239, // m
				36, //k
				new BigInteger("32010857077C5431123A46B808906756F543423E8D27877578125778AC76", 16), // a
				new BigInteger("790408F2EEDAF392B012EDEFB3392F30F4327C0CA3F31FC383C422AA8C16", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("0457927098FA932E7C0A96D3FD5B706EF7E5F5C156E16B7E7C86038552E91D61D8EE5077C33FECF6F1A16B268DE469C3C7744EA9A971649FC7A9616305")), // G
				new BigInteger("220855883097298041197912187592864814557886993776713230936715041207411783"), // n
				BigInteger.ValueOf(4)); // h

			ECPrivateKeyParameters priKey = new ECPrivateKeyParameters(
				"ECDSA",
				new BigInteger("145642755521911534651321230007534120304391871461646461466464667494947990"), // d
				parameters);

			ECDsaSigner ecdsa = new ECDsaSigner();
			ParametersWithRandom param = new ParametersWithRandom(priKey, k);

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			if (!r.Equals(sig[0]))
			{
				Fail("r component wrong." + Environment.NewLine
					+ " expecting: " + r + Environment.NewLine
					+ " got      : " + sig[0]);
			}

			if (!s.Equals(sig[1]))
			{
				Fail("s component wrong." + Environment.NewLine
					+ " expecting: " + s + Environment.NewLine
					+ " got      : " + sig[1]);
			}

			// Verify the signature
			ECPublicKeyParameters pubKey = new ECPublicKeyParameters(
				"ECDSA",
				curve.DecodePoint(Hex.Decode("045894609CCECF9A92533F630DE713A958E96C97CCB8F5ABB5A688A238DEED6DC2D9D0C94EBFB7D526BA6A61764175B99CB6011E2047F9F067293F57F5")), // Q
				parameters);

			ecdsa.Init(false, pubKey);
			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("signature fails");
			}
		}

		/**
		 * General test for long digest.
		 */
		[Test]
		public void TestECDsa239bitBinaryAndLargeDigest()
		{
			BigInteger r = new BigInteger("21596333210419611985018340039034612628818151486841789642455876922391552");
			BigInteger s = new BigInteger("87626799441093658509023277770579403014298417038607966989658087651831660");

			byte[] kData = BigIntegers.AsUnsignedByteArray(
				new BigInteger("171278725565216523967285789236956265265265235675811949404040041670216363"));

			SecureRandom k = FixedSecureRandom.From(kData);

			F2mCurve curve = new F2mCurve(
				239, // m
				36, //k
				new BigInteger("32010857077C5431123A46B808906756F543423E8D27877578125778AC76", 16), // a
				new BigInteger("790408F2EEDAF392B012EDEFB3392F30F4327C0CA3F31FC383C422AA8C16", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(
					Hex.Decode("0457927098FA932E7C0A96D3FD5B706EF7E5F5C156E16B7E7C86038552E91D61D8EE5077C33FECF6F1A16B268DE469C3C7744EA9A971649FC7A9616305")), // G
				new BigInteger("220855883097298041197912187592864814557886993776713230936715041207411783"), // n
				BigInteger.ValueOf(4)); // h

			ECPrivateKeyParameters priKey = new ECPrivateKeyParameters(
				"ECDSA",
				new BigInteger("145642755521911534651321230007534120304391871461646461466464667494947990"), // d
				parameters);

			ECDsaSigner ecdsa = new ECDsaSigner();
			ParametersWithRandom param = new ParametersWithRandom(priKey, k);

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517968236873715988614170569073515315707566766479517968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			if (!r.Equals(sig[0]))
			{
				Fail("r component wrong." + Environment.NewLine
					+ " expecting: " + r + Environment.NewLine
					+ " got      : " + sig[0]);
			}

			if (!s.Equals(sig[1]))
			{
				Fail("s component wrong." + Environment.NewLine
					+ " expecting: " + s + Environment.NewLine
					+ " got      : " + sig[1]);
			}

			// Verify the signature
			ECPublicKeyParameters pubKey = new ECPublicKeyParameters(
				"ECDSA",
				curve.DecodePoint(
					Hex.Decode("045894609CCECF9A92533F630DE713A958E96C97CCB8F5ABB5A688A238DEED6DC2D9D0C94EBFB7D526BA6A61764175B99CB6011E2047F9F067293F57F5")), // Q
				parameters);

			ecdsa.Init(false, pubKey);
			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("signature fails");
			}
		}

		/**
		 * key generation test
		 */
		[Test]
		public void TestECDsaKeyGenTest()
		{
			SecureRandom random = new SecureRandom();

			FpCurve curve = new FpCurve(
				new BigInteger("883423532389192164791648750360308885314476597252960362792450860609699839"), // q
				new BigInteger("7fffffffffffffffffffffff7fffffffffff8000000000007ffffffffffc", 16), // a
				new BigInteger("6b016c3bdcf18941d0d654921475ca71a9db2fb27d1d37796185c2942c0a", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("020ffa963cdca8816ccc33b8642bedf905c3d358573d3f27fbbd3b3cb9aaaf")), // G
				new BigInteger("883423532389192164791648750360308884807550341691627752275345424702807307")); // n


			ECKeyPairGenerator pGen = new ECKeyPairGenerator();
			ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(
				parameters,
				random);

			pGen.Init(genParam);

			AsymmetricCipherKeyPair pair = pGen.GenerateKeyPair();

			ParametersWithRandom param = new ParametersWithRandom(pair.Private, random);

			ECDsaSigner ecdsa = new ECDsaSigner();

			ecdsa.Init(true, param);

			byte[] message = new BigInteger("968236873715988614170569073515315707566766479517").ToByteArray();
			BigInteger[] sig = ecdsa.GenerateSignature(message);

			ecdsa.Init(false, pair.Public);

			if (!ecdsa.VerifySignature(message, sig[0], sig[1]))
			{
				Fail("signature fails");
			}
		}

		/**
		 * Basic Key Agreement Test
		 */
		[Test]
		public void TestECBasicAgreementTest()
		{
			SecureRandom random = new SecureRandom();

			FpCurve curve = new FpCurve(
				new BigInteger("883423532389192164791648750360308885314476597252960362792450860609699839"), // q
				new BigInteger("7fffffffffffffffffffffff7fffffffffff8000000000007ffffffffffc", 16), // a
				new BigInteger("6b016c3bdcf18941d0d654921475ca71a9db2fb27d1d37796185c2942c0a", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("020ffa963cdca8816ccc33b8642bedf905c3d358573d3f27fbbd3b3cb9aaaf")), // G
				new BigInteger("883423532389192164791648750360308884807550341691627752275345424702807307")); // n


			ECKeyPairGenerator pGen = new ECKeyPairGenerator();
			ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(parameters, random);

			pGen.Init(genParam);

			AsymmetricCipherKeyPair p1 = pGen.GenerateKeyPair();
			AsymmetricCipherKeyPair p2 = pGen.GenerateKeyPair();

			//
			// two way
			//
			IBasicAgreement e1 = new ECDHBasicAgreement();
			IBasicAgreement e2 = new ECDHBasicAgreement();

			e1.Init(p1.Private);
			e2.Init(p2.Private);

			BigInteger   k1 = e1.CalculateAgreement(p2.Public);
			BigInteger   k2 = e2.CalculateAgreement(p1.Public);

			if (!k1.Equals(k2))
			{
				Fail("calculated agreement test failed");
			}

			//
			// two way
			//
			e1 = new ECDHCBasicAgreement();
			e2 = new ECDHCBasicAgreement();

			e1.Init(p1.Private);
			e2.Init(p2.Private);

			k1 = e1.CalculateAgreement(p2.Public);
			k2 = e2.CalculateAgreement(p1.Public);

			if (!k1.Equals(k2))
			{
				Fail("calculated agreement test failed");
			}
		}

		[Test]
		public void TestECMqvTestVector1()
		{
			// Test Vector from GEC-2

			X9ECParameters x9 = SecNamedCurves.GetByName("secp160r1");
			ECDomainParameters p = new ECDomainParameters(
				x9.Curve, x9.G, x9.N, x9.H, x9.GetSeed());

			AsymmetricCipherKeyPair U1 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("0251B4496FECC406ED0E75A24A3C03206251419DC0")), p),
				new ECPrivateKeyParameters(
					new BigInteger("AA374FFC3CE144E6B073307972CB6D57B2A4E982", 16), p));

			AsymmetricCipherKeyPair U2 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("03D99CE4D8BF52FA20BD21A962C6556B0F71F4CA1F")), p),
				new ECPrivateKeyParameters(
					new BigInteger("149EC7EA3A220A887619B3F9E5B4CA51C7D1779C", 16), p));

			AsymmetricCipherKeyPair V1 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("0349B41E0E9C0369C2328739D90F63D56707C6E5BC")), p),
				new ECPrivateKeyParameters(
					new BigInteger("45FB58A92A17AD4B15101C66E74F277E2B460866", 16), p));

			AsymmetricCipherKeyPair V2 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("02706E5D6E1F640C6E9C804E75DBC14521B1E5F3B5")), p),
				new ECPrivateKeyParameters(
					new BigInteger("18C13FCED9EADF884F7C595C8CB565DEFD0CB41E", 16), p));

			ECPoint keyA = CalculateMqvAgreement(
				p,
				(ECPrivateKeyParameters) U1.Private,
				(ECPrivateKeyParameters) U2.Private,
				(ECPublicKeyParameters) U2.Public,
				(ECPublicKeyParameters) V1.Public,
				(ECPublicKeyParameters) V2.Public);

			ECPoint keyB = CalculateMqvAgreement(
				p,
				(ECPrivateKeyParameters) V1.Private,
				(ECPrivateKeyParameters) V2.Private,
				(ECPublicKeyParameters) V2.Public,
				(ECPublicKeyParameters) U1.Public,
				(ECPublicKeyParameters) U2.Public);

			// Note: In the actual algorithm, we would need to ensure !keyA.IsInfinity
			// and the secret is just the ECFieldElement keyA.X

			Assert.AreEqual(keyA, keyB);
			Assert.AreEqual(
				keyA.X.ToBigInteger(),
				new BigInteger("5A6955CEFDB4E43255FB7FCF718611E4DF8E05AC", 16));
		}

		[Test]
		public void TestECMqvTestVector2()
		{
			// Test Vector from GEC-2

			X9ECParameters x9 = SecNamedCurves.GetByName("sect163k1");
			ECDomainParameters p = new ECDomainParameters(
				x9.Curve, x9.G, x9.N, x9.H, x9.GetSeed());

			AsymmetricCipherKeyPair U1 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("03037D529FA37E42195F10111127FFB2BB38644806BC")), p),
				new ECPrivateKeyParameters(
					new BigInteger("03A41434AA99C2EF40C8495B2ED9739CB2155A1E0D", 16), p));

			AsymmetricCipherKeyPair U2 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("02015198E74BC2F1E5C9A62B80248DF0D62B9ADF8429")), p),
				new ECPrivateKeyParameters(
					new BigInteger("032FC4C61A8211E6A7C4B8B0C03CF35F7CF20DBD52", 16), p));

			AsymmetricCipherKeyPair V1 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("03072783FAAB9549002B4F13140B88132D1C75B3886C")), p),
				new ECPrivateKeyParameters(
					new BigInteger("57E8A78E842BF4ACD5C315AA0569DB1703541D96", 16), p));

			AsymmetricCipherKeyPair V2 = new AsymmetricCipherKeyPair(
				new ECPublicKeyParameters(
					p.Curve.DecodePoint(Hex.Decode("03067E3AEA3510D69E8EDD19CB2A703DDC6CF5E56E32")), p),
				new ECPrivateKeyParameters(
					new BigInteger("02BD198B83A667A8D908EA1E6F90FD5C6D695DE94F", 16), p));

			ECPoint keyA = CalculateMqvAgreement(
				p,
				(ECPrivateKeyParameters) U1.Private,
				(ECPrivateKeyParameters) U2.Private,
				(ECPublicKeyParameters) U2.Public,
				(ECPublicKeyParameters) V1.Public,
				(ECPublicKeyParameters) V2.Public);

			ECPoint keyB = CalculateMqvAgreement(
				p,
				(ECPrivateKeyParameters) V1.Private,
				(ECPrivateKeyParameters) V2.Private,
				(ECPublicKeyParameters) V2.Public,
				(ECPublicKeyParameters) U1.Public,
				(ECPublicKeyParameters) U2.Public);

			// Note: In the actual algorithm, we would need to ensure !keyA.IsInfinity
			// and the secret is just the ECFieldElement keyA.X

			Assert.AreEqual(keyA, keyB);
			Assert.AreEqual(
				keyA.X.ToBigInteger(),
				new BigInteger("038359FFD30C0D5FC1E6154F483B73D43E5CF2B503", 16));
		}

		[Test]
		public void TestECMqvRandom()
		{
			SecureRandom random = new SecureRandom();

			FpCurve curve = new FpCurve(
				new BigInteger("883423532389192164791648750360308885314476597252960362792450860609699839"), // q
				new BigInteger("7fffffffffffffffffffffff7fffffffffff8000000000007ffffffffffc", 16), // a
				new BigInteger("6b016c3bdcf18941d0d654921475ca71a9db2fb27d1d37796185c2942c0a", 16)); // b

			ECDomainParameters parameters = new ECDomainParameters(
				curve,
				curve.DecodePoint(Hex.Decode("020ffa963cdca8816ccc33b8642bedf905c3d358573d3f27fbbd3b3cb9aaaf")), // G
				new BigInteger("883423532389192164791648750360308884807550341691627752275345424702807307")); // n

			ECKeyPairGenerator pGen = new ECKeyPairGenerator();

			pGen.Init(new ECKeyGenerationParameters(parameters, random));


			// Pre-established key pairs
			AsymmetricCipherKeyPair U1 = pGen.GenerateKeyPair();
			AsymmetricCipherKeyPair V1 = pGen.GenerateKeyPair();

			// Ephemeral key pairs
			AsymmetricCipherKeyPair U2 = pGen.GenerateKeyPair();
			AsymmetricCipherKeyPair V2 = pGen.GenerateKeyPair();

			ECPoint keyA = CalculateMqvAgreement(
				parameters,
				(ECPrivateKeyParameters) U1.Private,
				(ECPrivateKeyParameters) U2.Private,
				(ECPublicKeyParameters) U2.Public,
				(ECPublicKeyParameters) V1.Public,
				(ECPublicKeyParameters) V2.Public);

			ECPoint keyB = CalculateMqvAgreement(
				parameters,
				(ECPrivateKeyParameters) V1.Private,
				(ECPrivateKeyParameters) V2.Private,
				(ECPublicKeyParameters) V2.Public,
				(ECPublicKeyParameters) U1.Public,
				(ECPublicKeyParameters) U2.Public);

			// Note: In the actual algorithm, we would need to ensure !keyA.IsInfinity
			// and the secret is just the ECFieldElement keyA.X

			Assert.AreEqual(keyA, keyB);
		}

		// The ECMQV Primitive as described in SEC-1, 3.4
		private ECPoint CalculateMqvAgreement(
			ECDomainParameters		parameters,
			ECPrivateKeyParameters	d1U,
			ECPrivateKeyParameters	d2U,
			ECPublicKeyParameters	Q2U,
			ECPublicKeyParameters	Q1V,
			ECPublicKeyParameters	Q2V)
		{
			BigInteger n = parameters.N;
			int e = (parameters.N.BitLength + 1) / 2;
			BigInteger powE = BigInteger.One.ShiftLeft(e);

			BigInteger x = Q2U.Q.X.ToBigInteger();
			BigInteger xBar = x.Mod(powE);
			BigInteger Q2UBar = xBar.SetBit(e);
			BigInteger s = d1U.D.Multiply(Q2UBar).Mod(n).Add(d2U.D).Mod(n);

			BigInteger xPrime = Q2V.Q.X.ToBigInteger();
			BigInteger xPrimeBar = xPrime.Mod(powE);
			BigInteger Q2VBar = xPrimeBar.SetBit(e);

			BigInteger hs = parameters.H.Multiply(s).Mod(n);

//			ECPoint p = Q1V.Q.Multiply(Q2VBar).Add(Q2V.Q).Multiply(hs);
			ECPoint p = ECAlgorithms.ShamirsTrick(
				Q1V.Q, Q2VBar.Multiply(hs).Mod(n), Q2V.Q, hs);

			return p;
		}

		public override string Name
		{
			get { return "EC"; }
		}

		public override void PerformTest()
		{
			TestDecode();
			TestECDsa192bitPrime();
			TestECDsa239bitPrime();
			TestECDsa191bitBinary();
			TestECDsa239bitBinary();
			TestECDsaKeyGenTest();
			TestECBasicAgreementTest();
			TestECDsa239bitBinaryAndLargeDigest();
		}

		public static void Main(
			string[] args)
		{
			RunTest(new ECTest());
		}
	}
}
