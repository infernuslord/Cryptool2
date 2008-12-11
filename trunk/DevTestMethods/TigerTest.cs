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
    public void TigerTestMethod1()
    {

      string TEST_DATA = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
      byte[] TEST_HASH = { 
                      0x0F, 0x7B, 0xF9, 0xA1, 0x9B, 0x9C, 0x58, 0xF2,
                      0xB7, 0x61, 0x0D, 0xF7, 0xE8, 0x4F, 0x0A, 0xC3,
                      0xA7, 0x1C, 0x63, 0x1E, 0x7B, 0x53, 0xF7, 0x8E};

      
      ASCIIEncoding enc = new ASCIIEncoding();
      
      HMACTIGER tg = new HMACTIGER();

      tg.Initialize();

      byte[] hash = tg.ComputeHash(enc.GetBytes(TEST_DATA));

      Assert.AreEqual(hash.Length, TEST_HASH.Length, "invalid hash length.");

      for (int i = 0; i < hash.Length; i++)
        Assert.AreEqual(hash[i], TEST_HASH[i], "Invalid hash value.");
    }

    [TestMethod]
    public void TigerTestMethod2()
    {
      // test vectors from 
      // http://www.cs.technion.ac.il/~biham/Reports/Tiger/tiger2-test-vectors-nessie-format.dat
      //
      string[] source = 
      {
        "",
        "a",
        "abc",
        "message digest",
        "abcdefghijklmnopqrstuvwxyz",
        "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
        "12345678901234567890123456789012345678901234567890123456789012345678901234567890",
        "The quick brown fox jumps over the lazy dog" // wikipedia
      };

      string[] result1 = 
      {
        "3293AC630C13F0245F92BBB1766E16167A4E58492DDE73F3", // ""
        "77BEFBEF2E7EF8AB2EC8F93BF587A7FC613E247F5F247809", // "a"
        "2AAB1484E8C158F2BFB8C5FF41B57A525129131C957B5F93", // "abc"
        "D981F8CB78201A950DCF3048751E441C517FCA1AA55A29F6", // "message digest"
        "1714A472EEE57D30040412BFCC55032A0B11602FF37BEEE9", // "a..z"
        "0F7BF9A19B9C58F2B7610DF7E84F0AC3A71C631E7B53F78E", // "a...q"
        "8DCEA680A17583EE502BA38A3C368651890FFBCCDC49A8CC", // "A...Za...z0...9"
        "1C14795529FD9F207A958F84C52F11E887FA0CABDFD91BFD", // 8 times "1234567890"
        "6D12A41E72E644F017B6F0E2F7B44C6285F06DD5D2C5B075"  // The quick brown fox ...
        
      };

      string[] result2 = 
      {
        "4441BE75F6018773C206C22745374B924AA8313FEF919F41", // ""
        "67E6AE8E9E968999F70A23E72AEAA9251CBC7C78A7916636", // "a"
        "F68D7BC5AF4B43A06E048D7829560D4A9415658BB0B1F3BF", // "abc"
        "E29419A1B5FA259DE8005E7DE75078EA81A542EF2552462D", // "message digest"
        "F5B6B6A78C405C8547E91CD8624CB8BE83FC804A474488FD", // "a..z"
        "A6737F3997E8FBB63D20D2DF88F86376B5FE2D5CE36646A9", // "a...q"
        "EA9AB6228CEE7B51B77544FCA6066C8CBB5BBAE6319505CD", // "A...Za...z0...9"
        "D85278115329EBAA0EEC85ECDC5396FDA8AA3A5820942FFF", // 8 times "1234567890"
        "976ABFF8062A2E9DCEA3A1ACE966ED9C19CB85558B4976D8"  // The quick brown fox ...
      };

      ASCIIEncoding enc = new ASCIIEncoding();

      for (int i = 0; i < source.Length; i++)
      {
        testContextInstance.WriteLine(" Test " + i.ToString());
        testContextInstance.WriteLine(" data = " + source[i]);

        HMACTIGER  tg1 = new HMACTIGER();
        HMACTIGER2 tg2 = new HMACTIGER2();

        tg1.Initialize();
        tg2.Initialize();
        
        byte[] code = enc.GetBytes(source[i]);
        byte[] hash1 = tg1.ComputeHash(code);
        byte[] hash2 = tg2.ComputeHash(code);

        string tmp = "";
        foreach (byte b in hash1)
        {
          if (b < 0x10)
            tmp += "0";
          tmp += b.ToString("X");
        }

        testContextInstance.WriteLine(" expected   = " + result1[i]);
        testContextInstance.WriteLine(" calculated = " + tmp);

        Assert.AreEqual(result1[i], tmp, "Hash(1) is invalid.");

        tmp = "";
        foreach (byte b in hash2)
        {
          if (b < 0x10)
            tmp += "0";
          tmp += b.ToString("X");
        }

        testContextInstance.WriteLine(" expected   = " + result2[i]);
        testContextInstance.WriteLine(" calculated = " + tmp);

        Assert.AreEqual(result2[i], tmp, "Hash(2) is invalid.");
      }
    }

  }
}
