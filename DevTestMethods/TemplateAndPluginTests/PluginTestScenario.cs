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
            : base(GetProperties(plugin, inputProperties), GetObjectArray(plugin, inputProperties), 
                   GetProperties(plugin, outputProperties), GetObjectArray(plugin, outputProperties))
        {
            _plugin = plugin;
        }

        private static object[] GetObjectArray(IPlugin plugin, string[] properties)
        {
            var settings = plugin.Settings;

            var res = new object[properties.Length];
            for (int i = 0; i < res.Length; i++)
            {
                if (properties[i].StartsWith("."))
                {
                    res[i] = settings;
                }
                else
                {
                    res[i] = plugin;
                }
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