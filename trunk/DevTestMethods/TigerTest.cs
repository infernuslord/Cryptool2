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

namespace Tiger
{
	/// <summary>
	/// Zusammenfassungsbeschreibung für UnitTest1
	/// </summary>
	[TestClass]
	public class Tiger
	{
		public Tiger()
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
		public void TigerTestMethod()
		{
			// iso_test_vectors

			string[] source =
			{
				""
			};

			// ToDo !!!!!!!!!!!!
			string[] result =
			{
				"19FA61D75522A4669B44E39C1D2E1726C530232130D407F89AFEE0964997F7A73E83BE698B288FEBCF88E3E03C4F0757EA8964E59B63D93708B138CC42A66EB3"
			};


			int tests = source.Length;
			for (int t = 0; t < tests; t++)
			{
				//TigerHash wh = new TigerHash();

				//testContextInstance.WriteLine(" Test " + t.ToString());
				//testContextInstance.WriteLine(" data = " + source[t]);

				//char[] src = source[t].ToCharArray();
				//int length = src.Length;
				//byte[] data = new byte[length];

				//for (uint i = 0; i < length; i++)
				//  data[i] = (byte)(src[i]);

				//wh.Add(data, (ulong)(length * 8));
				//wh.Finish();

				//byte[] res = wh.Hash;

				//Assert.AreEqual(res.Length, 64, "Hash Length invalid.");

				//string tmp = "";
				//foreach (byte b in res)
				//{
				//  if (b < 0x10)
				//    tmp += "0";
				//  tmp += b.ToString("X");
				//}
				//Assert.AreEqual(result[t], tmp, "Hash is invalid.");

				//testContextInstance.WriteLine(" expected   = " + result[t]);
				//testContextInstance.WriteLine(" calculated = " + tmp);
			}
		}
	}
}
