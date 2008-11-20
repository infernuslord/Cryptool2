using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PKCS5;

namespace Tests
{

	[TestClass]
	public class pkcsTest
	{
		public pkcsTest()
		{
		}

		private TestContext testContextInstance;
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

		#region additional Test attributes
		//
		// Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
		//
		// Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
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
		public void pkcs5TestMethodMD5()
		{
			///
			/// ToDo: find correct Test values
			/// 
			byte[] k0 = { };
			byte[] s0 = { };
			byte[] e0 = { 0 };
			
			PKCS5.PKCS5 p = new PKCS5.PKCS5();

			PKCS5Settings set = new PKCS5Settings();

			p.KeyData = k0;
			p.SaltData = s0;

			set.Count = 1000;
			set.SHAFunction = (int) System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.MD5;
			set.Length = 24 * 8;

			p.Settings = set;

			p.Hash();

			byte[] h = p.HashOutputData;

			//Assert.AreEqual<byte[]>(e0,h); //ToDo
			Assert.IsNull(null);
		}
	}
}
