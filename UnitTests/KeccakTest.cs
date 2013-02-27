using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.PluginBase.IO;
using Cryptool.Plugins.Cryptography.Encryption;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class KeccakTest
    {
        public KeccakTest()
        {
        }

        [TestMethod]
        public void SHATestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Keccak");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", ".KECCAKFunction" }, new[] { "OutputStream" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.ToStream(), vector.mode });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, output;
            public int n, mode;
        }

        //
        // Sources of the test vectors:
        //  http://www.di-mgt.com.au/sha_testvectors.html#KECCAK-KAT
        //  http://en.wikipedia.org/wiki/SHA-3
        //
        TestVector[] testvectors = new TestVector[] {

            // SHA3-224
            new TestVector () { n=0, mode=1, output="F71837502BA8E10837BDD8D365ADB85591895602FC552B48B7390ABD", input="" },
	        new TestVector () { n=2, mode=1, output="C30411768506EBE1C2871B1EE2E87D38DF342317300A9B97A95EC6A8", input="abc" },
	        new TestVector () { n=3, mode=1, output="E51FAA2B4655150B931EE8D700DC202F763CA5F962C529EAE55012B6", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
	        new TestVector () { n=4, mode=1, output="344298994B1B06873EAE2CE739C425C47291A2E24189E01B524F88DC", input="abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" },
	        new TestVector () { n=5, mode=1, output="310AEE6B30C47350576AC2873FA89FD190CDC488442F3EF654CF23FE", input="The quick brown fox jumps over the lazy dog" },
	        new TestVector () { n=6, mode=1, output="C59D4EAEAC728671C635FF645014E2AFA935BEBFFDB5FBD207FFDEAB", input="The quick brown fox jumps over the lazy dog." },

            // SHA3-256
            new TestVector () { n=7, mode=2, output="C5D2460186F7233C927E7DB2DCC703C0E500B653CA82273B7BFAD8045D85A470", input="" },
	        new TestVector () { n=8, mode=2, output="4E03657AEA45A94FC7D47BA826C8D667C0D1E6E33A64A036EC44F58FA12D6C45", input="abc" },
	        new TestVector () { n=9, mode=2, output="45D3B367A6904E6E8D502EE04999A7C27647F91FA845D456525FD352AE3D7371", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
	        new TestVector () { n=10, mode=2, output="F519747ED599024F3882238E5AB43960132572B7345FBEB9A90769DAFD21AD67", input="abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" },
	        new TestVector () { n=11, mode=2, output="4D741B6F1EB29CB2A9B9911C82F56FA8D73B04959D3D9D222895DF6C0B28AA15", input="The quick brown fox jumps over the lazy dog" },
	        new TestVector () { n=12, mode=2, output="578951E24EFD62A3D63A86F7CD19AAA53C898FE287D2552133220370240B572D", input="The quick brown fox jumps over the lazy dog." },

            // SHA3-384
            new TestVector () { n=13, mode=3, output="2C23146A63A29ACF99E73B88F8C24EAA7DC60AA771780CCC006AFBFA8FE2479B2DD2B21362337441AC12B515911957FF", input="" },
	        new TestVector () { n=14, mode=3, output="F7DF1165F033337BE098E7D288AD6A2F74409D7A60B49C36642218DE161B1F99F8C681E4AFAF31A34DB29FB763E3C28E", input="abc" },
	        new TestVector () { n=15, mode=3, output="B41E8896428F1BCBB51E17ABD6ACC98052A3502E0D5BF7FA1AF949B4D3C855E7C4DC2C390326B3F3E74C7B1E2B9A3657", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
	        new TestVector () { n=16, mode=3, output="CC063F34685135368B34F7449108F6D10FA727B09D696EC5331771DA46A923B6C34DBD1D4F77E595689C1F3800681C28", input="abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" },
	        new TestVector () { n=17, mode=3, output="283990FA9D5FB731D786C5BBEE94EA4DB4910F18C62C03D173FC0A5E494422E8A0B3DA7574DAE7FA0BAF005E504063B3", input="The quick brown fox jumps over the lazy dog" },
	        new TestVector () { n=18, mode=3, output="9AD8E17325408EDDB6EDEE6147F13856AD819BB7532668B605A24A2D958F88BD5C169E56DC4B2F89FFD325F6006D820B", input="The quick brown fox jumps over the lazy dog." },

            // SHA3-512
            new TestVector () { n=19, mode=4, output="0EAB42DE4C3CEB9235FC91ACFFE746B29C29A8C366B7C60E4E67C466F36A4304C00FA9CAF9D87976BA469BCBE06713B435F091EF2769FB160CDAB33D3670680E", input="" },
	        new TestVector () { n=20, mode=4, output="18587DC2EA106B9A1563E32B3312421CA164C7F1F07BC922A9C83D77CEA3A1E5D0C69910739025372DC14AC9642629379540C17E2A65B19D77AA511A9D00BB96", input="abc" },
	        new TestVector () { n=21, mode=4, output="6AA6D3669597DF6D5A007B00D09C20795B5C4218234E1698A944757A488ECDC09965435D97CA32C3CFED7201FF30E070CD947F1FC12B9D9214C467D342BCBA5D", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
	        new TestVector () { n=22, mode=4, output="AC2FB35251825D3AA48468A9948C0A91B8256F6D97D8FA4160FAFF2DD9DFCC24F3F1DB7A983DAD13D53439CCAC0B37E24037E7B95F80F59F37A2F683C4BA4682", input="abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" },
	        new TestVector () { n=23, mode=4, output="D135BB84D0439DBAC432247EE573A23EA7D3C9DEB2A968EB31D47C4FB45F1EF4422D6C531B5B9BD6F449EBCC449EA94D0A8F05F62130FDA612DA53C79659F609", input="The quick brown fox jumps over the lazy dog" },
	        new TestVector () { n=24, mode=4, output="AB7192D2B11F51C7DD744E7B3441FEBF397CA07BF812CCEAE122CA4DED6387889064F8DB9230F173F6D1AB6E24B6E50F065B039F799F5592360A6558EB52D760", input="The quick brown fox jumps over the lazy dog." },
        };
    }
}