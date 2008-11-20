//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://www.cryptool.org/svn/CrypTool2/trunk/CrypPlugins/PKCS5/SSCpkcs5.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 30                                                                             $://
// $Author:: junker                                                                           $://
// $Date:: 2008-11-19 14:13:40 +0100 (Mi, 19 Nov 2008)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PKCS5;

namespace Tests
{
    /// <summary>
    /// test methods for PKCS#5 Plugin
    /// </summary>
    [TestClass]
    public class pkcsTest
    {
        public pkcsTest()
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

        private void pkcs5part(byte[] key, byte[] salt, int hmac, byte[] result )
        {
            PKCS5Settings set = new PKCS5Settings();
            set.Count = 2048;
            set.SHAFunction = hmac;
            set.Length = 24 * 8; // Length must be in bits and not in bytes

            PKCS5.PKCS5 p = new PKCS5.PKCS5();
            p.Settings = set;
            p.KeyData = key;
            p.SaltData = salt;

            p.Hash();
            byte[] h = p.HashOutputData;
            p.Dispose();

            //set = (PKCS5.PKCS5Settings)p.Settings;
            //testContextInstance.WriteLine("Settings: hash length is   {0} bits.", set.Length);
            //testContextInstance.WriteLine("Settings: hash function is {0}.", set.SHAFunction);
            //testContextInstance.WriteLine("Settings: hash count is    {0}.", set.Count);

            // both arrays of same size?
            Assert.AreEqual(h.Length, result.Length, "Different hash sizes found");

            string tmp = "expected hash is  ";
            foreach (byte b in result)
                tmp += b.ToString("x2") + " ";
            testContextInstance.WriteLine(tmp);

            tmp = "calculated hash is ";
            foreach (byte b in h)
                tmp += b.ToString("x2") + " ";
            testContextInstance.WriteLine(tmp);

            // the next compares references etc ... but not the array content :-(
            // Assert.AreEqual<byte[]>(result, h, "Different hash values found");
            // compare by hand ...
            for (int i = 0; i < h.Length; i++)
            {
                Assert.AreEqual(result[i], h[i], "Different hash values found");
            }
        }

        [TestMethod]
        public void pkcs5TestMethodMD5()
        {
            byte[] key = { 0x70, 0x61, 0x73, 0x73, 0x77, 0x6f, 0x72, 0x64 };   // "password"
            byte[] salt = { 0x78, 0x57, 0x8E, 0x5A, 0x5D, 0x63, 0xCB, 0x06 };

            ///
            /// referenced test values taken from
            /// http://cryptosys.net/cgi-bin/manual.cgi?m=api&name=PBE_Kdf2 
            /// 
            //Derived key {HMAC-MD5}    = 66991b7f8010a0ba5d8a2e1e1a38341007f2eda8a79619d6 // reference needed
            //Derived key {HMAC-SHA1}   = BFDE6BE94DF7E11DD409BCE20A0255EC327CB936FFE93643
            //Derived key {HMAC-SHA256} = 97B5A91D35AF542324881315C4F849E327C4707D1BC9D322
            //Derived key {HMAC-SHA384} = bd6078731cef2cf5bdc48748a9da182ddc7b48a3cc28069e // reference needed
            //Derived key {HMAC-SHA512} = e6fa68fec0a2be2477809f8983e2719eb29415c61efacf34 // reference needed

            byte[] result_MD5    = { 0x66, 0x99, 0x1b, 0x7f, 0x80, 0x10, 0xa0, 0xba, 0x5d, 0x8a, 0x2e, 0x1e, 0x1a, 0x38, 0x34, 0x10, 0x07, 0xf2, 0xed, 0xa8, 0xa7, 0x96, 0x19, 0xd6 };
            byte[] result_SHA1   = { 0xBF, 0xDE, 0x6B, 0xE9, 0x4D, 0xF7, 0xE1, 0x1D, 0xD4, 0x09, 0xBC, 0xE2, 0x0A, 0x02, 0x55, 0xEC, 0x32, 0x7C, 0xB9, 0x36, 0xFF, 0xE9, 0x36, 0x43 };
            byte[] result_SHA256 = { 0x97, 0xB5, 0xA9, 0x1D, 0x35, 0xAF, 0x54, 0x23, 0x24, 0x88, 0x13, 0x15, 0xC4, 0xF8, 0x49, 0xE3, 0x27, 0xC4, 0x70, 0x7D, 0x1B, 0xC9, 0xD3, 0x22 };
            byte[] result_SHA384 = { 0xbd, 0x60, 0x78, 0x73, 0x1c, 0xef, 0x2c, 0xf5, 0xbd, 0xc4, 0x87, 0x48, 0xa9, 0xda, 0x18, 0x2d, 0xdc, 0x7b, 0x48, 0xa3, 0xcc, 0x28, 0x06, 0x9e }; 
            byte[] result_SHA512 = { 0xe6, 0xfa, 0x68, 0xfe, 0xc0, 0xa2, 0xbe, 0x24, 0x77, 0x80, 0x9f, 0x89, 0x83, 0xe2, 0x71, 0x9e, 0xb2, 0x94, 0x15, 0xc6, 0x1e, 0xfa, 0xcf, 0x34};

            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.MD5, result_MD5);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, result_SHA1);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA256, result_SHA256);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA384, result_SHA384);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA512, result_SHA512);
        }
    }
}
