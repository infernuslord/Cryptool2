using System;
using System.Text;

using NUnit.Framework;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.Crypto.Tests
{
	[TestFixture]
	public class EaxTest
		: SimpleTest
	{
		private static readonly byte[] K1 = Hex.Decode("233952DEE4D5ED5F9B9C6D6FF80FF478");
		private static readonly byte[] N1 = Hex.Decode("62EC67F9C3A4A407FCB2A8C49031A8B3");
		private static readonly byte[] A1 = Hex.Decode("6BFB914FD07EAE6B");
		private static readonly byte[] P1 = Hex.Decode("");
		private static readonly byte[] C1 = Hex.Decode("E037830E8389F27B025A2D6527E79D01");
		private static readonly byte[] T1 = Hex.Decode("E037830E8389F27B025A2D6527E79D01");

		private static readonly byte[] K2 = Hex.Decode("91945D3F4DCBEE0BF45EF52255F095A4");
		private static readonly byte[] N2 = Hex.Decode("BECAF043B0A23D843194BA972C66DEBD");
		private static readonly byte[] A2 = Hex.Decode("FA3BFD4806EB53FA");
		private static readonly byte[] P2 = Hex.Decode("F7FB");
		private static readonly byte[] C2 = Hex.Decode("19DD5C4C9331049D0BDAB0277408F67967E5");
		private static readonly byte[] T2 = Hex.Decode("5C4C9331049D0BDAB0277408F67967E5");

		private static readonly byte[] K3 = Hex.Decode("01F74AD64077F2E704C0F60ADA3DD523");
		private static readonly byte[] N3 = Hex.Decode("70C3DB4F0D26368400A10ED05D2BFF5E");
		private static readonly byte[] A3 = Hex.Decode("234A3463C1264AC6");
		private static readonly byte[] P3 = Hex.Decode("1A47CB4933");
		private static readonly byte[] C3 = Hex.Decode("D851D5BAE03A59F238A23E39199DC9266626C40F80");
		private static readonly byte[] T3 = Hex.Decode("3A59F238A23E39199DC9266626C40F80");

		private static readonly byte[] K4 = Hex.Decode("D07CF6CBB7F313BDDE66B727AFD3C5E8");
		private static readonly byte[] N4 = Hex.Decode("8408DFFF3C1A2B1292DC199E46B7D617");
		private static readonly byte[] A4 = Hex.Decode("33CCE2EABFF5A79D");
		private static readonly byte[] P4 = Hex.Decode("481C9E39B1");
		private static readonly byte[] C4 = Hex.Decode("632A9D131AD4C168A4225D8E1FF755939974A7BEDE");
		private static readonly byte[] T4 = Hex.Decode("D4C168A4225D8E1FF755939974A7BEDE");

		private static readonly byte[] K5 = Hex.Decode("35B6D0580005BBC12B0587124557D2C2");
		private static readonly byte[] N5 = Hex.Decode("FDB6B06676EEDC5C61D74276E1F8E816");
		private static readonly byte[] A5 = Hex.Decode("AEB96EAEBE2970E9");
		private static readonly byte[] P5 = Hex.Decode("40D0C07DA5E4");
		private static readonly byte[] C5 = Hex.Decode("071DFE16C675CB0677E536F73AFE6A14B74EE49844DD");
		private static readonly byte[] T5 = Hex.Decode("CB0677E536F73AFE6A14B74EE49844DD");

		private static readonly byte[] K6 = Hex.Decode("BD8E6E11475E60B268784C38C62FEB22");
		private static readonly byte[] N6 = Hex.Decode("6EAC5C93072D8E8513F750935E46DA1B");
		private static readonly byte[] A6 = Hex.Decode("D4482D1CA78DCE0F");
		private static readonly byte[] P6 = Hex.Decode("4DE3B35C3FC039245BD1FB7D");
		private static readonly byte[] C6 = Hex.Decode("835BB4F15D743E350E728414ABB8644FD6CCB86947C5E10590210A4F");
		private static readonly byte[] T6 = Hex.Decode("ABB8644FD6CCB86947C5E10590210A4F");

		private static readonly byte[] K7 = Hex.Decode("7C77D6E813BED5AC98BAA417477A2E7D");
		private static readonly byte[] N7 = Hex.Decode("1A8C98DCD73D38393B2BF1569DEEFC19");
		private static readonly byte[] A7 = Hex.Decode("65D2017990D62528");
		private static readonly byte[] P7 = Hex.Decode("8B0A79306C9CE7ED99DAE4F87F8DD61636");
		private static readonly byte[] C7 = Hex.Decode("02083E3979DA014812F59F11D52630DA30137327D10649B0AA6E1C181DB617D7F2");
		private static readonly byte[] T7 = Hex.Decode("137327D10649B0AA6E1C181DB617D7F2");

		private static readonly byte[] K8 = Hex.Decode("5FFF20CAFAB119CA2FC73549E20F5B0D");
		private static readonly byte[] N8 = Hex.Decode("DDE59B97D722156D4D9AFF2BC7559826");
		private static readonly byte[] A8 = Hex.Decode("54B9F04E6A09189A");
		private static readonly byte[] P8 = Hex.Decode("1BDA122BCE8A8DBAF1877D962B8592DD2D56");
		private static readonly byte[] C8 = Hex.Decode("2EC47B2C4954A489AFC7BA4897EDCDAE8CC33B60450599BD02C96382902AEF7F832A");
		private static readonly byte[] T8 = Hex.Decode("3B60450599BD02C96382902AEF7F832A");

		private static readonly byte[] K9 = Hex.Decode("A4A4782BCFFD3EC5E7EF6D8C34A56123");
		private static readonly byte[] N9 = Hex.Decode("B781FCF2F75FA5A8DE97A9CA48E522EC");
		private static readonly byte[] A9 = Hex.Decode("899A175897561D7E");
		private static readonly byte[] P9 = Hex.Decode("6CF36720872B8513F6EAB1A8A44438D5EF11");
		private static readonly byte[] C9 = Hex.Decode("0DE18FD0FDD91E7AF19F1D8EE8733938B1E8E7F6D2231618102FDB7FE55FF1991700");
		private static readonly byte[] T9 = Hex.Decode("E7F6D2231618102FDB7FE55FF1991700");

		private static readonly byte[] K10 = Hex.Decode("8395FCF1E95BEBD697BD010BC766AAC3");
		private static readonly byte[] N10 = Hex.Decode("22E7ADD93CFC6393C57EC0B3C17D6B44");
		private static readonly byte[] A10 = Hex.Decode("126735FCC320D25A");
		private static readonly byte[] P10 = Hex.Decode("CA40D7446E545FFAED3BD12A740A659FFBBB3CEAB7");
		private static readonly byte[] C10 = Hex.Decode("CB8920F87A6C75CFF39627B56E3ED197C552D295A7CFC46AFC253B4652B1AF3795B124AB6E");
		private static readonly byte[] T10 = Hex.Decode("CFC46AFC253B4652B1AF3795B124AB6E");

		private static readonly byte[] K11 = Hex.Decode("8395FCF1E95BEBD697BD010BC766AAC3");
		private static readonly byte[] N11 = Hex.Decode("22E7ADD93CFC6393C57EC0B3C17D6B44");
		private static readonly byte[] A11 = Hex.Decode("126735FCC320D25A");
		private static readonly byte[] P11 = Hex.Decode("CA40D7446E545FFAED3BD12A740A659FFBBB3CEAB7");
		private static readonly byte[] C11 = Hex.Decode("CB8920F87A6C75CFF39627B56E3ED197C552D295A7CFC46AFC");
		private static readonly byte[] T11 = Hex.Decode("CFC46AFC");

		public override string Name
		{
			get { return "EAX"; }
		}

		public override void PerformTest()
		{
			EaxBlockCipher eax = new EaxBlockCipher(new AesEngine());

			checkVectors(1, eax, K1, 128, N1, A1, P1, T1, C1);
			checkVectors(2, eax, K2, 128, N2, A2, P2, T2, C2);
			checkVectors(3, eax, K3, 128, N3, A3, P3, T3, C3);
			checkVectors(4, eax, K4, 128, N4, A4, P4, T4, C4);
			checkVectors(5, eax, K5, 128, N5, A5, P5, T5, C5);
			checkVectors(6, eax, K6, 128, N6, A6, P6, T6, C6);
			checkVectors(7, eax, K7, 128, N7, A7, P7, T7, C7);
			checkVectors(8, eax, K8, 128, N8, A8, P8, T8, C8);
			checkVectors(9, eax, K9, 128, N9, A9, P9, T9, C9);
			checkVectors(10, eax, K10, 128, N10, A10, P10, T10, C10);
			checkVectors(11, eax, K11, 32, N11, A11, P11, T11, C11);

			ivParamTest(1, eax, K1, N1);

			//
			// exception tests
			//

			try
			{
				eax.Init(false, new CcmParameters(new KeyParameter(K1), 32, N2, A2));

				byte[] enc = new byte[C2.Length]; 
				int len = eax.ProcessBytes(C2, 0, C2.Length, enc, 0);

				len += eax.DoFinal(enc, len);

				Fail("invalid cipher text not picked up");
			}
			catch (InvalidCipherTextException)
			{
				// expected
			}

			try
			{
				eax.Init(false, new KeyParameter(K1));

				Fail("illegal argument not picked up");
			}
			catch (ArgumentException)
			{
				// expected
			}
		}

		private void checkVectors(
			int				count,
			EaxBlockCipher	eax,
			byte[]			k,
			int				macSize,
			byte[]			n,
			byte[]			a,
			byte[]			p,
			byte[]			t,
			byte[]			c)
		{
			eax.Init(true, new AeadParameters(new KeyParameter(k), macSize, n, a));

			byte[] enc = new byte[c.Length];

			int len = eax.ProcessBytes(p, 0, p.Length, enc, 0);

			len += eax.DoFinal(enc, len);

			if (!AreEqual(c, enc))
			{
				Fail("encrypted stream fails to match in test " + count);
			}

			eax.Init(false, new AeadParameters(new KeyParameter(k), macSize, n, a));

			byte[] tmp = new byte[enc.Length];

			len = eax.ProcessBytes(enc, 0, enc.Length, tmp, 0);

			len += eax.DoFinal(tmp, len);

			byte[] dec = new byte[len];
	        
			Array.Copy(tmp, 0, dec, 0, len);

			if (!AreEqual(p, dec))
			{
				Fail("decrypted stream fails to match in test " + count);
			}

			if (!AreEqual(t, eax.GetMac()))
			{
				Fail("MAC fails to match in test " + count);
			}
		}

		private void ivParamTest(
			int					count,
			IAeadBlockCipher	ccm,
			byte[]				k,
			byte[]				n)
		{
			byte[] p = Encoding.ASCII.GetBytes("hello world!!");

			ccm.Init(true, new ParametersWithIV(new KeyParameter(k), n));

			byte[] enc = new byte[p.Length + 8];

			int len = ccm.ProcessBytes(p, 0, p.Length, enc, 0);

			len += ccm.DoFinal(enc, len);

			ccm.Init(false, new ParametersWithIV(new KeyParameter(k), n));

			byte[] tmp = new byte[enc.Length];

			len = ccm.ProcessBytes(enc, 0, enc.Length, tmp, 0);

			len += ccm.DoFinal(tmp, len);

			byte[] dec = new byte[len];

			Array.Copy(tmp, 0, dec, 0, len);

			if (!AreEqual(p, dec))
			{
				Fail("decrypted stream fails to match in test " + count);
			}
		}

		public static void Main(
			string[] args)
		{
			RunTest(new EaxTest());
		}

		[Test]
		public void TestFunction()
		{
			string resultText = Perform().ToString();

			Assert.AreEqual(Name + ": Okay", resultText);
		}
	}
}
