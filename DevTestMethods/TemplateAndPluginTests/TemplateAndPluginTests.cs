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
            var scenario = new PluginTestScenario(pluginInstance, new []{""}, new []{""});

        }

        private static IPlugin GetPluginInstance(string pluginName)
        {
            return GetPluginInstance(pluginName, pluginName);
        }

        private static IPlugin GetPluginInstance(string pluginName, string assemblyName)
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
            var pluginInstance = (IPlugin)pluginType.GetConstructor(new Type[0]).Invoke(new object[0]);
            return pluginInstance;
        }
    }
}
