using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cryptool.PluginBase;

namespace Tests.TemplateAndPluginTests
{
    internal class PluginTestScenario : TestScenario
    {
        private readonly IPlugin _plugin;

        public PluginTestScenario(IPlugin plugin, string[] inputProperties, string[] outputProperties)
            : base(GetProperties(plugin, inputProperties), GetDublicatedArray(plugin, inputProperties.Length), 
                   GetProperties(plugin, outputProperties), GetDublicatedArray(plugin, outputProperties.Length))
        {
            _plugin = plugin;
        }

        private static object[] GetDublicatedArray(object o, int length)
        {
            var res = new object[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = o;
            }
            return res;
        }

        private static PropertyInfo[] GetProperties(IPlugin plugin, string[] properties)
        {
            var settings = plugin.Settings;

            var res = new List<PropertyInfo>();
            foreach (var property in properties)
            {
                if (property.StartsWith("."))
                {
                    res.Add(settings.GetType().GetProperty(property.Substring(1)));
                }
                else
                {
                    res.Add(plugin.GetType().GetProperty(property));
                }
            }
            return res.ToArray();
        }

        protected override void Execute()
        {
            _plugin.Execute();
        }

        protected override void Initialize()
        {
            _plugin.PreExecution();
        }
    }
}