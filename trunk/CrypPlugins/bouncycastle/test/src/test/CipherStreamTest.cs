using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Tests
{
	/// <remarks>Check that cipher input/output streams are working correctly</remarks>
	[TestFixture]
	public class CipherStreamTest
		: SimpleTest
	{
		private static readonly byte[] RK = Hex.Decode("0123456789ABCDEF");
		private static readonly byte[] RIN = Hex.Decode("4e6f772069732074");
		private static readonly byte[] ROUT = Hex.Decode("3afbb5c77938280d");

		private static byte[] SIN = Hex.Decode(
			"00000000000000000000000000000000"
			+ "00000000000000000000000000000000"
			+ "00000000000000000000000000000000"
			+ "00000000000000000000000000000000");
		private static readonly byte[] SK = Hex.Decode("80000000000000000000000000000000");
		private static readonly byte[] SIV = Hex.Decode("0000000000000000");
		private static readonly byte[] SOUT = Hex.Decode(
			  "4DFA5E481DA23EA09A31022050859936"
			+ "DA52FCEE218005164F267CB65F5CFD7F"
			+ "2B4F97E0FF16924A52DF269515110A07"
			+ "F9E460BC65EF95DA58F740B7D1DBB0AA");

		private static readonly byte[] HCIN = new byte[64];
		private static readonly byte[] HCIV = new byte[32];

		private static readonly byte[] HCK256A = new byte[32];
		private static readonly byte[] HC256A = Hex.Decode(
			  "8589075b0df3f6d82fc0c5425179b6a6"
			+ "3465f053f2891f808b24744e18480b72"
			+ "ec2792cdbf4dcfeb7769bf8dfa14aee4"
			+ "7b4c50e8eaf3a9c8f506016c81697e32");

		private static readonly byte[] HCK128A = new byte[16];
		private static readonly byte[] HC128A = Hex.Decode(
			  "731500823bfd03a0fb2fd77faa63af0e"
			+ "de122fc6a7dc29b662a685278b75ec68"
			+ "9036db1e8189600500ade078491fbf9a"
			+ "1cdc30136c3d6e2490f664b29cd57102");

		private void doRunTest(
			string	name,
			int		ivLength)
		{
			string lCode = "ABCDEFGHIJKLMNOPQRSTUVWXY0123456789";

			string baseName = name;
			if (name.IndexOf('/') >= 0)
			{
				baseName = name.Substring(0, name.IndexOf('/'));
			}

			CipherKeyGenerator kGen = GeneratorUtilities.GetKeyGenerator(baseName);

			IBufferedCipher inCipher = CipherUtilities.GetCipher(name);
			IBufferedCipher outCipher = CipherUtilities.GetCipher(name);
			KeyParameter key = ParameterUtilities.CreateKeyParameter(baseName, kGen.GenerateKey());
			MemoryStream bIn = new MemoryStream(Encoding.ASCII.GetBytes(lCode), false);
			MemoryStream bOut = new MemoryStream();

			// In the Java build, this IV would be implicitly created and then retrieved with getIV()
			ICipherParameters cipherParams = key;
			if (ivLength > 0)
			{
				cipherParams = new ParametersWithIV(cipherParams, new byte[ivLength]);
			}

			inCipher.Init(true, cipherParams);

			// TODO Should we provide GetIV() method on IBufferedCipher?
			//if (inCipher.getIV() != null)
			//{
			//	outCipher.Init(false, new ParametersWithIV(key, inCipher.getIV()));
			//}
			//else
			//{
			//	outCipher.Init(false, key);
			//}
			outCipher.Init(false, cipherParams);

			CipherStream cIn = new CipherStream(bIn, inCipher, null);
			CipherStream cOut = new CipherStream(bOut, null, outCipher);

			int c;

			while ((c = cIn.ReadByte()) >= 0)
			{
				cOut.WriteByte((byte)c);
			}

			cIn.Close();

			cOut.Flush();
			cOut.Close();

			string res = Encoding.ASCII.GetString(bOut.ToArray());

			if (!res.Equals(lCode))
			{
				Fail("Failed - decrypted data doesn't match.");
			}
		}

		private void doTestAlgorithm(
			string	name,
			byte[]	keyBytes,
			byte[]	iv,
			byte[]	plainText,
			byte[]	cipherText)
		{
			KeyParameter key = ParameterUtilities.CreateKeyParameter(name, keyBytes);

			IBufferedCipher inCipher = CipherUtilities.GetCipher(name);
			IBufferedCipher outCipher = CipherUtilities.GetCipher(name);

			if (iv != null)
			{
				inCipher.Init(true, new ParametersWithIV(key, iv));
				outCipher.Init(false, new ParametersWithIV(key, iv));
			}
			else
			{
				inCipher.Init(true, key);
				outCipher.Init(false, key);
			}

			byte[] enc = inCipher.DoFinal(plainText);
			if (!AreEqual(enc, cipherText))
			{
				Fail(name + ": cipher text doesn't match");
			}

			byte[] dec = outCipher.DoFinal(enc);

			if (!AreEqual(dec, plainText))
			{
				Fail(name + ": plain text doesn't match");
			}
		}

		private void doTestException(
			string	name,
			int		ivLength)
		{
			try
			{
				byte[] key128 = {
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143,
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143
				};

				byte[] key256 = {
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143,
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143,
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143,
					(byte)128, (byte)131, (byte)133, (byte)134,
					(byte)137, (byte)138, (byte)140, (byte)143 };

				byte[] keyBytes;
				if (name.Equals("HC256"))
				{
					keyBytes = key256;
				}
				else
				{
					keyBytes = key128;
				}

				KeyParameter cipherKey = ParameterUtilities.CreateKeyParameter(name, keyBytes);

				ICipherParameters cipherParams = cipherKey;
				if (ivLength > 0)
				{
					cipherParams = new ParametersWithIV(cipherParams, new byte[ivLength]);
				}

				IBufferedCipher ecipher = CipherUtilities.GetCipher(name);
				ecipher.Init(true, cipherParams);

				byte[] cipherText = new byte[0];
				try
				{
					// According specification Method engineUpdate(byte[] input,
					// int inputOffset, int inputLen, byte[] output, int
					// outputOffset)
					// throws ShortBufferException - if the given output buffer is
					// too
					// small to hold the result
					ecipher.ProcessBytes(new byte[20], 0, 20, cipherText, 0);

//					Fail("failed exception test - no ShortBufferException thrown");
					Fail("failed exception test - no DataLengthException thrown");
				}
//				catch (ShortBufferException e)
				catch (DataLengthException)
				{
					// ignore
				}

				// NB: The lightweight engine doesn't take public/private keys
//				try
//				{
//					IBufferedCipher c = CipherUtilities.GetCipher(name);
//
//					//                Key k = new PublicKey()
//					//                {
//					//
//					//                    public string getAlgorithm()
//					//                    {
//					//                        return "STUB";
//					//                    }
//					//
//					//                    public string getFormat()
//					//                    {
//					//                        return null;
//					//                    }
//					//
//					//                    public byte[] getEncoded()
//					//                    {
//					//                        return null;
//					//                    }
//					//
//					//                };
//					AsymmetricKeyParameter k = new AsymmetricKeyParameter(false);
//					c.Init(true, k);
//
//					Fail("failed exception test - no InvalidKeyException thrown for public key");
//				}
//				catch (InvalidKeyException)
//				{
//					// okay
//				}
//
//				try
//				{
//					IBufferedCipher c = CipherUtilities.GetCipher(name);
//
//					//				Key k = new PrivateKey()
//					//                {
//					//
//					//                    public string getAlgorithm()
//					//                    {
//					//                        return "STUB";
//					//                    }
//					//
//					//                    public string getFormat()
//					//                    {
//					//                        return null;
//					//                    }
//					//
//					//                    public byte[] getEncoded()
//					//                    {
//					//                        return null;
//					//                    }
//					//
//					//                };
//
//					AsymmetricKeyParameter k = new AsymmetricKeyParameter(true);
//					c.Init(false, k);
//
//					Fail("failed exception test - no InvalidKeyException thrown for private key");
//				}
//				catch (InvalidKeyException)
//				{
//					// okay
//				}
			}
			catch (Exception e)
			{
				Fail("unexpected exception.", e);
			}
		}

		[Test]
		public void TestRC4()
		{
			doRunTest("RC4", 0);
		}

		[Test]
		public void TestRC4Exception()
		{
			doTestException("RC4", 0);
		}

		[Test]
		public void TestRC4Algorithm()
		{
			doTestAlgorithm("RC4", RK, null, RIN, ROUT);
		}

		[Test]
		public void TestSalsa20()
		{
			doRunTest("Salsa20", 8);
		}

		[Test]
		public void TestSalsa20Exception()
		{
			doTestException("Salsa20", 8);
		}

		[Test]
		public void TestSalsa20Algorithm()
		{
			doTestAlgorithm("Salsa20", SK, SIV, SIN, SOUT);
		}

		[Test]
		public void TestHC128()
		{
			doRunTest("HC128", 16);
		}

		[Test]
		public void TestHC128Exception()
		{
			doTestException("HC128", 16);
		}

		[Test]
		public void TestHC128Algorithm()
		{
			doTestAlgorithm("HC128", HCK128A, HCIV, HCIN, HC128A);
		}

		[Test]
		public void TestHC256()
		{
			doRunTest("HC256", 32);
		}

		[Test]
		public void TestHC256Exception()
		{
			doTestException("HC256", 32);
		}

		[Test]
		public void TestHC256Algorithm()
		{
			doTestAlgorithm("HC256", HCK256A, HCIV, HCIN, HC256A);
		}

		[Test]
		public void TestDesEcbPkcs7()
		{
			doRunTest("DES/ECB/PKCS7Padding", 0);
		}

		[Test]
		public void TestDesCfbNoPadding()
		{
			doRunTest("DES/CFB8/NoPadding", 0);
		}

		public override void PerformTest()
		{
			TestRC4();
			TestRC4Exception();
			TestRC4Algorithm();
			TestSalsa20();
			TestSalsa20Exception();
			TestSalsa20Algorithm();
			TestHC128();
			TestHC128Exception();
			TestHC128Algorithm();
			TestHC256();
			TestHC256Exception();
			TestHC256Algorithm();
			TestDesEcbPkcs7();
			TestDesCfbNoPadding();
		}

		public override string Name
		{
			get { return "CipherStreamTest"; }
		}

		public static void Main(
			string[] args)
		{
			RunTest(new CipherStreamTest());
		}
	}
}
