using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Cryptool.PluginBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class TemplateAndPluginTests
    {
        [TestMethod]
        public void CasearTest()
        {
            var pluginInstance = GetPluginInstance("Caesar");
            var scenario = new PluginTestScenario(pluginInstance, new []{"ShiftKey", "InputAlphabet", "InputString"}, new []{"OutputString"});

            //Test 1:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" }, new[] { "Dpylx hyer gk imknjcrr tcpuyfpjmqrcl Ryvg oscp bspaf Zywcpl" }))
            {
                Assert.Fail("Test 1 failed!");
            }

            //Test 2:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" }, new[] { "dPyLX HyER GK IMKNJCRR TCPUyFPJMQRCL ryVG OSCP BSPAF ZyWCPL" }))
            {
                Assert.Fail("Test 2 failed!");
            }
        }

        [TestMethod]
        public void ADFGVXTest()
        {
            var pluginInstance = GetPluginInstance("ADFGVX");
            var scenario = new PluginTestScenario(pluginInstance, new[] { ".SubstitutionPass", ".TranspositionPass", "InputString" }, new[] { "OutputString" });

            //Test 1:
            if (!scenario.Test(new [] { "WIKPEDAZYXVUTSRQONMLHGFCB", "BEOBACHTUNGSLISTE", "Munitionierung beschleunigen Punkt Soweit nicht eingesehen auch bei Tag" }, new[] { "GXGGADDDGDXXAFADDFAAXAFDFFXFDGDXGAGGAAXFAGADFAAADGFAXXADADFFFDDADFGAXGXAFXGXFXDAFAGFXXFAXGFDXFFDFAGXXGXXADGXGFXDFFDGAXXFFFFGDX" }))
            {
                Assert.Fail("Test 1 failed!");
            }
        }

        private static ICrypComponent GetPluginInstance(string pluginName)
        {
            return GetPluginInstance(pluginName, pluginName);
        }

        private static ICrypComponent GetPluginInstance(string pluginName, string assemblyName)
        {
            Assembly a = null;
            try
            {
                a = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format("Can't load assembly {0}: {1}.", assemblyName, ex));
            }

            var pluginType = a.GetTypes().First(x => x.Name == pluginName);
            if (pluginType == null)
            {
                Assert.Fail(string.Format("Can't load plugin {0} from assembly {1}.", pluginName, assemblyName));
            }
            return pluginType.CreateComponentInstance();
        }
    }
}
