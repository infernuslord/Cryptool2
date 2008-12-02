//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Whirlpool
{
	/// <summary>
	/// Zusammenfassungsbeschreibung für UnitTest1
	/// </summary>
	[TestClass]
	public class Whirlpool
	{
		public Whirlpool()
		{
		}

		private TestContext testContextInstance;

		/// <summary>
		///Ruft den Textkontext mit Informationen über
		///den aktueen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Zusätzliche Testattribute
		//
		// Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
		//
		// Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Verwenden Sie ClassCleanup, um nach Ausführung aer Tests in einer Klasse Code auszuführen.
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestMethodAbc()
		{
			// iso_test_vectors

			string[] source =
			{
				"",
				"a",
				"abc",
				"message digest",
				"abcdefghijklmnopqrstuvwxyz",
				"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
				"12345678901234567890123456789012345678901234567890123456789012345678901234567890",
				"abcdbcdecdefdefgefghfghighijhijk"
			};

			string[] result =
			{
				"19FA61D75522A4669B44E39C1D2E1726C530232130D407F89AFEE0964997F7A73E83BE698B288FEBCF88E3E03C4F0757EA8964E59B63D93708B138CC42A66EB3",
				"8ACA2602792AEC6F11A67206531FB7D7F0DFF59413145E6973C45001D0087B42D11BC645413AEFF63A42391A39145A591A92200D560195E53B478584FDAE231A",
				"4E2448A4C6F486BB16B6562C73B4020BF3043E3A731BCE721AE1B303D97E6D4C7181EEBDB6C57E277D0E34957114CBD6C797FC9D95D8B582D225292076D4EEF5",
				"378C84A4126E2DC6E56DCC7458377AAC838D00032230F53CE1F5700C0FFB4D3B8421557659EF55C106B4B52AC5A4AAA692ED920052838F3362E86DBD37A8903E",
				"F1D754662636FFE92C82EBB9212A484A8D38631EAD4238F5442EE13B8054E41B08BF2A9251C30B6A0B8AAE86177AB4A6F68F673E7207865D5D9819A3DBA4EB3B",
				"DC37E008CF9EE69BF11F00ED9ABA26901DD7C28CDEC066CC6AF42E40F82F3A1E08EBA26629129D8FB7CB57211B9281A65517CC879D7B962142C65F5A7AF01467",
				"466EF18BABB0154D25B9D38A6414F5C08784372BCCB204D6549C4AFADB6014294D5BD8DF2A6C44E538CD047B2681A51A2C60481E88C5A20B2C2A80CF3A9A083B",
				"2A987EA40F917061F5D6F0A0E4644F488A7A5A52DEEE656207C562F988E95C6916BDC8031BC5BE1B7B947639FE050B56939BAAA0ADFF9AE6745B7B181C3BE3FD"
			};


			int tests = source.Length;
			for (int t = 0; t < tests; t++)
			{
				WhirlpoolHash wh = new WhirlpoolHash();

				testContextInstance.WriteLine(" Test " + t.ToString());
				testContextInstance.WriteLine(" data = " + source[t]);

				char[] src = source[t].ToCharArray();
				int length = src.Length;
				byte[] data = new byte[length];

				for (uint i = 0; i < length; i++)
					data[i] = (byte)(src[i]);

				wh.Add(data, (ulong)(length * 8));
				wh.Finish();

				byte[] res = wh.Hash;

				Assert.AreEqual(res.Length, 64,"Hash Length invalid.");

				string tmp = "";
				foreach (byte b in res)
				{
					if (b < 0x10)
						tmp += "0";
					tmp += b.ToString("X");
				}
				Assert.AreEqual(result[t], tmp, "Hash is invalid.");

				testContextInstance.WriteLine(" expected   = " + result[t]);
				testContextInstance.WriteLine(" calculated = " + tmp);
			}
		}
	}
}
