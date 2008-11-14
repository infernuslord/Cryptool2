using System;

using NUnit.Framework;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Crypto.Tests
{
	/**
	* HC-128 and HC-256 Tests. Based on the test vectors in the official reference
	* papers, respectively:
	* 
	* http://www.ecrypt.eu.org/stream/p3ciphers/hc/hc128_p3.pdf
	* http://www.ecrypt.eu.org/stream/p3ciphers/hc/hc256_p3.pdf
	*/
	[TestFixture]
	public class HCFamilyTest
		: SimpleTest
	{
		private static readonly byte[] MSG = new byte[64];

		private static readonly byte[] K256A = new byte[32];
		private static readonly byte[] K256B = Hex.Decode(
			  "55000000000000000000000000000000"
			+ "00000000000000000000000000000000");

		private static readonly byte[] IVA = new byte[32];
		private static readonly byte[] IVB = new byte[]{0x1};

		private static readonly byte[] HC256A = Hex.Decode(
			  "8589075b0df3f6d82fc0c5425179b6a6"
			+ "3465f053f2891f808b24744e18480b72"
			+ "ec2792cdbf4dcfeb7769bf8dfa14aee4"
			+ "7b4c50e8eaf3a9c8f506016c81697e32");
		private static readonly byte[] HC256B = Hex.Decode(
			  "bfa2e2afe9ce174f8b05c2feb18bb1d1"
			+ "ee42c05f01312b71c61f50dd502a080b"
			+ "edfec706633d9241a6dac448af8561ff"
			+ "5e04135a9448c4342de7e9f337520bdf");
		private static readonly byte[] HC256C = Hex.Decode(
			  "fe4a401ced5fe24fd19a8f956fc036ae"
			+ "3c5aa68823e2abc02f90b3aea8d30e42"
			+ "59f03a6c6e39eb448f7579fb70137a5e"
			+ "6d10b7d8add0f7cd723423daf575dde6");
		private static readonly byte[] HC256D = Hex.Decode(
			  "c6b6fb99f2ae1440a7d4ca342011694e"
			+ "6f36b4be420db05d4745fd907c630695"
			+ "5f1d7bda13ae7e36aebc5399733b7f37"
			+ "95f34066b601d21f2d8cf830a9c08937");

		private static readonly byte[] K128A = new byte[16];
		private static readonly byte[] K128B = Hex.Decode("55000000000000000000000000000000");

		private static readonly byte[] HC128A = Hex.Decode(
			  "731500823bfd03a0fb2fd77faa63af0e"
			+ "de122fc6a7dc29b662a685278b75ec68"
			+ "9036db1e8189600500ade078491fbf9a"
			+ "1cdc30136c3d6e2490f664b29cd57102");
		private static readonly byte[] HC128B = Hex.Decode(
			  "c01893d5b7dbe9588f65ec9864176604"
			+ "36fc6724c82c6eec1b1c38a7c9b42a95"
			+ "323ef1230a6a908bce757b689f14f7bb"
			+ "e4cde011aeb5173f89608c94b5cf46ca");
		private static readonly byte[] HC128C = Hex.Decode(
			  "518251a404b4930ab02af9310639f032"
			+ "bcb4a47a5722480b2bf99f72cdc0e566"
			+ "310f0c56d3cc83e8663db8ef62dfe07f"
			+ "593e1790c5ceaa9cab03806fc9a6e5a0");
		private static readonly byte[] HC128D = Hex.Decode(
			  "a4eac0267e4911266a2a384f5c4e1329"
			+ "da407fa155e6b1ae05c6fdf3bbdc8a86"
			+ "7a699aa01a4dc11763658cccd3e62474"
			+ "9cf8236f0131be21c3a51de9d12290de");

		public override string Name
		{
			get { return "HC-128 and HC-256"; }
		}

		public override void PerformTest()
		{
			IStreamCipher hc = new HC256Engine();
			HCTest(hc, "HC-256 - A", K256A, IVA, HC256A);
			HCTest(hc, "HC-256 - B", K256A, IVB, HC256B);
			HCTest(hc, "HC-256 - C", K256B, IVA, HC256C);
			HCTest2(hc, "HC-256 - D", K256A, IVA, HC256D, 0x10000);

			hc = new HC128Engine();
			HCTest(hc, "HC-128 - A", K128A, IVA, HC128A);
			HCTest(hc, "HC-128 - B", K128A, IVB, HC128B);
			HCTest(hc, "HC-128 - C", K128B, IVA, HC128C);
			HCTest2(hc, "HC-128 - D", K128A, IVA, HC128D, 0x100000);
		}

		private void HCTest(
			IStreamCipher	hc,
			string			test,
			byte[]			key,
			byte[]			IV,
			byte[]			expected)
		{
			KeyParameter kp = new KeyParameter(key);
			ParametersWithIV ivp = new ParametersWithIV(kp, IV);
			hc.Init(true, ivp);
			for (int i = 0; i < 64; i++)
			{
				if (hc.ReturnByte(MSG[i]) != expected[i])
				{
					Fail(test + " failure");
				}
			}
		}

		private void HCTest2(
			IStreamCipher	hc,
			string			test,
			byte[]			key,
			byte[]			IV,
			byte[]			expected,
			int				times)
		{
			KeyParameter kp = new KeyParameter(key);
			ParametersWithIV ivp = new ParametersWithIV(kp, IV);
			hc.Init(true, ivp);
			byte[] result = new byte[64];
			for (int j = 0; j < times; j++)
			{
				for (int i = 0; i < 64; i++)
				{
					result[i] = hc.ReturnByte(result[i]);
				}
			}

			for (int i = 0; i < 64; i++)
			{
				if (result[i] != expected[i])
				{
					Fail(test + " failure at byte " + i);
				}
			}
		}

		public static void Main(
			string[] args)
		{
			RunTest(new HCFamilyTest());
		}

		[Test]
		public void TestFunction()
		{
			string resultText = Perform().ToString();
			Assert.AreEqual(Name + ": Okay", resultText);
		}
	}
}
