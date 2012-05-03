using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.PluginBase.IO;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class TigerTest
    {
        public TigerTest()
        {
        }

        [TestMethod]
        public void TigerTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Tiger");

            // Parameters as ByteArrays
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputData" }, new[] { "HashOutputData" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.ToByteArray() });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

            // Parameters as Streams
            scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream" }, new[] { "HashOutputStream" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.ToStream() });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, output;
            public int n;
        }

        //
        // Source of the test vectors: http://www.cs.technion.ac.il/~biham/Reports/Tiger/tiger2-test-vectors-nessie-format.dat
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, output="4441BE75F6018773C206C22745374B924AA8313FEF919F41", input="" },
            new TestVector () { n=1, output="67E6AE8E9E968999F70A23E72AEAA9251CBC7C78A7916636", input="a" },
            new TestVector () { n=2, output="F68D7BC5AF4B43A06E048D7829560D4A9415658BB0B1F3BF", input="abc" },
            new TestVector () { n=3, output="E29419A1B5FA259DE8005E7DE75078EA81A542EF2552462D", input="message digest" },
            new TestVector () { n=4, output="F5B6B6A78C405C8547E91CD8624CB8BE83FC804A474488FD", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=5, output="A6737F3997E8FBB63D20D2DF88F86376B5FE2D5CE36646A9", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=6, output="EA9AB6228CEE7B51B77544FCA6066C8CBB5BBAE6319505CD", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=7, output="D85278115329EBAA0EEC85ECDC5396FDA8AA3A5820942FFF", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            new TestVector () { n=8, output="976ABFF8062A2E9DCEA3A1ACE966ED9C19CB85558B4976D8", input="The quick brown fox jumps over the lazy dog" },  // Wikipedia
            //new TestVector () { n=0, output="3293AC630C13F0245F92BBB1766E16167A4E58492DDE73F3", input="" },
            //new TestVector () { n=1, output="77BEFBEF2E7EF8AB2EC8F93BF587A7FC613E247F5F247809", input="a" },
            //new TestVector () { n=2, output="2AAB1484E8C158F2BFB8C5FF41B57A525129131C957B5F93", input="abc" },
            //new TestVector () { n=3, output="D981F8CB78201A950DCF3048751E441C517FCA1AA55A29F6", input="message digest" },
            //new TestVector () { n=4, output="1714A472EEE57D30040412BFCC55032A0B11602FF37BEEE9", input="abcdefghijklmnopqrstuvwxyz" },
            //new TestVector () { n=5, output="0F7BF9A19B9C58F2B7610DF7E84F0AC3A71C631E7B53F78E", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            //new TestVector () { n=6, output="8DCEA680A17583EE502BA38A3C368651890FFBCCDC49A8CC", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            //new TestVector () { n=7, output="1C14795529FD9F207A958F84C52F11E887FA0CABDFD91BFD", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            //new TestVector () { n=8, output="6D12A41E72E644F017B6F0E2F7B44C6285F06DD5D2C5B075", input="The quick brown fox jumps over the lazy dog" }  // Wikipedia
        };

    }
}