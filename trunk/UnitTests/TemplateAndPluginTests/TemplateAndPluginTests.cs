using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Cryptool.PluginBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkspaceManager.Model;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class TemplateAndPluginTests
    {
        private static DirectoryInfo _templateDirectory;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var dir = Directory.GetParent(System.Environment.CurrentDirectory);
            while (dir != null && dir.GetDirectories("Templates").Length == 0)
            {
                dir = dir.Parent;
            }
            if (dir == null)
            {
                Assert.Fail("Template directory not found!");
            }
            _templateDirectory = dir.GetDirectories("Templates")[0];
        }

        [TestMethod]
        public void CasearTest()
        {
            var pluginInstance = GetPluginInstance("Caesar");
            var scenario = new PluginTestScenario(pluginInstance, new []{"ShiftKey", "InputAlphabet", "InputString"}, new []{"OutputString"});
            
            //Test 1:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" }, new[] { "Dpylx hyer gk imknjcrr tcpuyfpjmqrcl Ryvg oscp bspaf Zywcpl" }))
            {
                TestFail(1);
            }

            //Test 2:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" }, new[] { "dPyLX HyER GK IMKNJCRR TCPUyFPJMQRCL ryVG OSCP BSPAF ZyWCPL" }))
            {
                TestFail(2);
            }
        }

        [TestMethod]
        public void CasearTemplateTest()
        {
            //This test doesn't work yet!

            var model = GetWorkspaceModel(@"Cryptography\Classic\Caesar.cwm");
            var scenario = new TemplateTestScenario(model, new[] { "Message>.Text" }, new[] { "Result>CurrentValue" });

            //Test 1:
            if (!scenario.Test(new object[] { "Input your message here." }, new[] { "Nsuzy dtzw rjxxflj mjwj." }))
            {
                TestFail(1);
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
                TestFail(1);
            }
        }

        #region private

        private void TestFail(int number)
        {
            Assert.Fail(string.Format("Test {0} failed!", number));
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

        private static WorkspaceModel GetWorkspaceModel(string fileName)
        {
            var modelLoader = new ModelPersistance();
            var model = modelLoader.loadModel(Path.Combine(_templateDirectory.FullName, fileName));
            return model;
        }

        #endregion
    }
}
