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

using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  /// <summary>
  /// Testclass for Tiger hash
  /// </summary>
  [TestClass]
  public class TigerTest
  {

    public TigerTest()
    {
      // nothing to do
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

    [TestMethod]
    public void TigerTestMethod()
    {

      string TEST_DATA = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
      byte[] TEST_HASH = { 
                      0xF, 0x7B, 0xF9, 0xA1, 0x9B, 0x9C, 0x58, 0xF2,
                      0xB7, 0x61, 0xD, 0xF7, 0xE8, 0x4F, 0xA, 0xC3,
                      0xA7, 0x1C, 0x63, 0x1E, 0x7B, 0x53, 0xF7, 0x8E};

      
      ASCIIEncoding enc = new ASCIIEncoding();
      
      TIGER tg = new TIGER();

      tg.Initialize();

      byte[] hash = tg.ComputeHash(enc.GetBytes(TEST_DATA));

      Assert.AreEqual(hash.Length, TEST_HASH.Length, "invalid hash length.");

      for (int i = 0; i < hash.Length; i++)
        Assert.AreEqual(hash[i], TEST_HASH[i], "Invalid hash value.");
    }
  }
}
