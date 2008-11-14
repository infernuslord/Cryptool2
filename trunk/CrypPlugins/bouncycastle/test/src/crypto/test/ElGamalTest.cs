using System;
using System.Text;

using NUnit.Framework;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Crypto.Tests
{
	[TestFixture]
	public class ElGamalTest
		: SimpleTest
	{
		private BigInteger g512 = new BigInteger("153d5d6172adb43045b68ae8e1de1070b6137005686d29d3d73a7749199681ee5b212c9b96bfdcfa5b20cd5e3fd2044895d609cf9b410b7a0f12ca1cb9a428cc", 16);
		private BigInteger p512 = new BigInteger("9494fec095f3b85ee286542b3836fc81a5dd0a0349b4c239dd38744d488cf8e31db8bcb7d33b41abb9e5a33cca9144b1cef332c94bf0573bf047a3aca98cdf3b", 16);

		private BigInteger g768 = new BigInteger("7c240073c1316c621df461b71ebb0cdcc90a6e5527e5e126633d131f87461c4dc4afc60c2cb0f053b6758871489a69613e2a8b4c8acde23954c08c81cbd36132cfd64d69e4ed9f8e51ed6e516297206672d5c0a69135df0a5dcf010d289a9ca1", 16);
		private BigInteger p768 = new BigInteger("8c9dd223debed1b80103b8b309715be009d48860ed5ae9b9d5d8159508efd802e3ad4501a7f7e1cfec78844489148cd72da24b21eddd01aa624291c48393e277cfc529e37075eccef957f3616f962d15b44aeab4039d01b817fde9eaa12fd73f", 16);

		private BigInteger g1024 = new BigInteger("1db17639cdf96bc4eabba19454f0b7e5bd4e14862889a725c96eb61048dcd676ceb303d586e30f060dbafd8a571a39c4d823982117da5cc4e0f89c77388b7a08896362429b94a18a327604eb7ff227bffbc83459ade299e57b5f77b50fb045250934938efa145511166e3197373e1b5b1e52de713eb49792bedde722c6717abf", 16);
		private BigInteger p1024 = new BigInteger("a00e283b3c624e5b2b4d9fbc2653b5185d99499b00fd1bf244c6f0bb817b4d1c451b2958d62a0f8a38caef059fb5ecd25d75ed9af403f5b5bdab97a642902f824e3c13789fed95fa106ddfe0ff4a707c85e2eb77d49e68f2808bcea18ce128b178cd287c6bc00efa9a1ad2a673fe0dceace53166f75b81d6709d5f8af7c66bb7", 16);

		public override string Name
		{
			get { return "ElGamal"; }
		}

		private void doTestEnc(
			int         size,
			int         privateValueSize,
			BigInteger  g,
			BigInteger  p)
		{
			ElGamalParameters dhParams = new ElGamalParameters(p, g, privateValueSize);
			ElGamalKeyGenerationParameters ekgParams = new ElGamalKeyGenerationParameters(new SecureRandom(), dhParams);
			ElGamalKeyPairGenerator kpGen = new ElGamalKeyPairGenerator();

			kpGen.Init(ekgParams);

			//
			// generate pair
			//
			AsymmetricCipherKeyPair pair = kpGen.GenerateKeyPair();

			ElGamalPublicKeyParameters pu = (ElGamalPublicKeyParameters) pair.Public;
			ElGamalPrivateKeyParameters pv = (ElGamalPrivateKeyParameters) pair.Private;

			checkKeySize(privateValueSize, pv);

			ElGamalEngine e = new ElGamalEngine();

			e.Init(true, pu);

			if (e.GetOutputBlockSize() != size / 4)
			{
				Fail(size + " GetOutputBlockSize() on encryption failed.");
			}

			string message = "This is a test";

			byte[] pText = Encoding.ASCII.GetBytes(message);
			byte[] cText = e.ProcessBlock(pText, 0, pText.Length);

			e.Init(false, pv);

			if (e.GetOutputBlockSize() != (size / 8) - 1)
			{
				Fail(size + " GetOutputBlockSize() on decryption failed.");
			}

			pText = e.ProcessBlock(cText, 0, cText.Length);

			if (!message.Equals(Encoding.ASCII.GetString(pText)))
			{
				Fail(size + " bit test failed");
			}



			e.Init(true, pu);
			byte[] bytes = new byte[e.GetInputBlockSize() + 2];

			try
			{
				e.ProcessBlock(bytes, 0, bytes.Length);

				Fail("out of range block not detected");
			}
			catch (DataLengthException)
			{
				// expected
			}

			try
			{
				bytes[0] = (byte)0xff;

				e.ProcessBlock(bytes, 0, bytes.Length - 1);

				Fail("out of range block not detected");
			}
			catch (DataLengthException)
			{
				// expected
			}

			try
			{
				bytes[0] = (byte)0x7f;

				e.ProcessBlock(bytes, 0, bytes.Length - 1);
			}
			catch (DataLengthException)
			{
				Fail("in range block failed");
			}
		}

		private void checkKeySize(
			int							privateValueSize,
			ElGamalPrivateKeyParameters	priv)
		{
			if (privateValueSize != 0)
			{
				if (priv.X.BitLength != privateValueSize)
				{
					Fail("limited key check failed for key size " + privateValueSize);
				}
			}
		}

		/**
		 * this test is can take quite a while
		 */
		private void doTestGeneration(
			int size)
		{
			ElGamalParametersGenerator pGen = new ElGamalParametersGenerator();

			pGen.Init(size, 10, new SecureRandom());

			ElGamalParameters elParams = pGen.GenerateParameters();

			if (elParams.L != 0)
			{
				Fail("ElGamalParametersGenerator failed to set L to 0 in generated ElGamalParameters");
			}

			ElGamalKeyGenerationParameters ekgParams = new ElGamalKeyGenerationParameters(new SecureRandom(), elParams);

			ElGamalKeyPairGenerator kpGen = new ElGamalKeyPairGenerator();

			kpGen.Init(ekgParams);

			//
			// generate first pair
			//
			AsymmetricCipherKeyPair pair = kpGen.GenerateKeyPair();

			ElGamalPublicKeyParameters pu = (ElGamalPublicKeyParameters)pair.Public;
			ElGamalPrivateKeyParameters pv = (ElGamalPrivateKeyParameters)pair.Private;

			ElGamalEngine e = new ElGamalEngine();

			e.Init(true, new ParametersWithRandom(pu, new SecureRandom()));

			string message = "This is a test";
			byte[] pText = Encoding.ASCII.GetBytes(message);
			byte[] cText = e.ProcessBlock(pText, 0, pText.Length);

			e.Init(false, pv);

			pText = e.ProcessBlock(cText, 0, cText.Length);

			if (!message.Equals(Encoding.ASCII.GetString(pText)))
			{
				Fail("generation test failed");
			}
		}

		[Test]
		public void TestEnc512()
		{
			doTestEnc(512, 0, g512, p512);
			doTestEnc(512, 64, g512, p512);
		}

		[Test]
		public void TestEnc768()
		{
			doTestEnc(768, 0, g768, p768);
			doTestEnc(768, 128, g768, p768);
		}

		[Test]
		public void TestEnc1024()
		{
			doTestEnc(1024, 0, g1024, p1024);
		}

		[Test]
		public void TestGeneration258()
		{
			doTestGeneration(258);
		}

		[Test]
		public void TestInitCheck()
		{
			try
			{
				new ElGamalEngine().ProcessBlock(new byte[]{ 1 }, 0, 1);
				Fail("failed initialisation check");
			}
			catch (InvalidOperationException)
			{
				// expected
			}
		}

		public override void PerformTest()
		{
			TestEnc512();
			TestEnc768();
			TestEnc1024();
			TestGeneration258();
			TestInitCheck();
		}

		public static void Main(
			string[] args)
		{
			ITest test = new ElGamalTest();
			ITestResult result = test.Perform();

			Console.WriteLine(result);
		}
	}
}