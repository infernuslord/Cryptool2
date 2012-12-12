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
    public class TwofishTest
    {
        public TwofishTest()
        {
        }

        [TestMethod]
        public void TwofishTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Twofish");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "KeyData", ".Action", ".Mode", ".KeySize", ".Mode", ".Padding" }, new[] { "OutputStream" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.key.HexToByteArray(), 0, 0, vector.keysize, 0, 0 });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

            foreach (TestVector vector in testvectors_loop)
            {
                string input, key, cipher;
                input = cipher = key = "00000000000000000000000000000000";

                for (int i = 0; i < 49; i++)
                {
                    key = (input + key).Substring(0, (128 + vector.keysize * 64) / 4);
                    input = cipher;
                    output = scenario.GetOutputs(new object[] { input.HexToStream(), key.HexToByteArray(), 0, 0, vector.keysize, 0, 0 });
                    cipher = output[0].ToHex();
                }
                Assert.AreEqual(vector.output.ToUpper(), cipher, "Unexpected value in test loop #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, key, output;
            public int n, mode, action, keysize;
        }

        //
        // Source of the test vectors: http://www.schneier.com/code/ecb_ival.txt
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, action=0, mode=0, keysize=0, key="00000000000000000000000000000000", input="00000000000000000000000000000000", output="9f589f5cf6122c32b6bfec2f2ae8c35a" },
            new TestVector () { n=1, action=0, mode=0, keysize=0, key="BCA724A54533C6987E14AA827952F921", input="6B459286F3FFD28D49F15B1581B08E42", output="5D9D4EEFFA9151575524F115815A12E0" },
            new TestVector () { n=3, action=0, mode=0, keysize=1, key="0123456789ABCDEFFEDCBA98765432100011223344556677", input="00000000000000000000000000000000", output="cfd1d2e5a9be9cdf501f13b892bd2248" },
            new TestVector () { n=4, action=0, mode=0, keysize=1, key="FB66522C332FCC4C042ABE32FA9E902FDEA4F3DA75EC7A8E", input="F0AB73301125FA21EF70BE5385FB76B6", output="E75449212BEEF9F4A390BD860A640941" },
            new TestVector () { n=5, action=0, mode=0, keysize=2, key="0123456789ABCDEFFEDCBA987654321000112233445566778899AABBCCDDEEFF", input="00000000000000000000000000000000", output="37527be0052334b89f0cfccae87cfa20" },
            new TestVector () { n=6, action=0, mode=0, keysize=2, key="248A7F3528B168ACFDD1386E3F51E30C2E2158BC3E5FC714C1EEECA0EA696D48", input="431058F4DBC7F734DA4F02F04CC4F459", output="37FE26FF1CF66175F5DDF4C33B97A205" },
        };

        TestVector[] testvectors_loop = new TestVector[] {
            new TestVector () { n=0, action=0, mode=0, keysize=0, output="5D9D4EEFFA9151575524F115815A12E0" },
            new TestVector () { n=1, action=0, mode=0, keysize=1, output="E75449212BEEF9F4A390BD860A640941" },
            new TestVector () { n=2, action=0, mode=0, keysize=2, output="37FE26FF1CF66175F5DDF4C33B97A205" },
       };

    }
}