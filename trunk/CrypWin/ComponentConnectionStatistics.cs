using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using WorkspaceManager.Model;

namespace Cryptool.CrypWin
{
    public static class ComponentConnectionStatistics
    {
        public delegate void GuiLogMessageHandler(string message, NotificationLevel logLevel);
        public static event GuiLogMessageHandler OnGuiLogMessageOccured;

        public class ComponentConnector
        {
            private readonly Type _component;
            private readonly string _connectorName;

            public string ConnectorName
            {
                get { return _connectorName; }
            }

            public Type Component
            {
                get { return _component; }
            }

            public ComponentConnector(Type component, string connectorName)
            {
                _component = component;
                _connectorName = connectorName;
            }
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (ComponentConnector)) return false;
                return Equals((ComponentConnector) obj);
            }

            public bool Equals(ComponentConnector other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._component, _component) && Equals(other._connectorName, _connectorName);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_component != null ? _component.GetHashCode() : 0)*397) ^ (_connectorName != null ? _connectorName.GetHashCode() : 0);
                }
            }
        }

        public class ConnectorStatistics
        {
            private readonly Dictionary<ComponentConnector, uint> _connectorUsages = new Dictionary<ComponentConnector, uint>();

            public Dictionary<ComponentConnector, uint> ConnectorUsages
            {
                get { return _connectorUsages; }
            }

            public void IncrementConnectorUsage(ComponentConnector otherConnector)
            {
                IncrementConnectorUsage(otherConnector, 1);
            }

            public void IncrementConnectorUsage(ComponentConnector otherConnector, uint add)
            {
                if (!_connectorUsages.ContainsKey(otherConnector))
                {
                    _connectorUsages.Add(otherConnector, 0);
                }
                _connectorUsages[otherConnector] += add;
            }

            public IEnumerable<ComponentConnector> GetSortedConnectorUsages()
            {
                return _connectorUsages.OrderBy(x => x.Value).Select(x => x.Key);
            }
        }

        public static readonly Dictionary<ComponentConnector, ConnectorStatistics> Statistics = new Dictionary<ComponentConnector, ConnectorStatistics>();

        public static void SaveCurrentStatistics(string file)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("componentConnectionStatistics");
            doc.AppendChild(root);

            foreach (var connectorStatistic in Statistics)
            {
                var cs = doc.CreateElement("connectorStatistic");
                root.AppendChild(cs);
                var attr = doc.CreateAttribute("component");
                attr.Value = connectorStatistic.Key.Component.FullName;
                cs.Attributes.Append(attr);
                attr = doc.CreateAttribute("connector");
                attr.Value = connectorStatistic.Key.ConnectorName;
                cs.Attributes.Append(attr);

                foreach (var otherConnector in connectorStatistic.Value.ConnectorUsages)
                {
                    var cc = doc.CreateElement("connectedConnector");
                    cs.AppendChild(cc);
                    attr = doc.CreateAttribute("component");
                    attr.Value = otherConnector.Key.Component.FullName;
                    cc.Attributes.Append(attr);
                    attr = doc.CreateAttribute("connector");
                    attr.Value = otherConnector.Key.ConnectorName;
                    cc.Attributes.Append(attr);
                    attr = doc.CreateAttribute("count");
                    attr.Value = otherConnector.Value.ToString();
                    cc.Attributes.Append(attr);
                }
            }

            doc.Save(file);
        }

        public static void LoadCurrentStatistics(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            var root = doc.DocumentElement;
            foreach (var child in root.ChildNodes)
            {
                var cs = (XmlElement) child;
                var component = cs.GetAttribute("component");
                var connector = cs.GetAttribute("connector");

                if (ComponentInformations.AllLoadedPlugins.ContainsKey(component))
                {
                    var componentConnector = new ComponentConnector(ComponentInformations.AllLoadedPlugins[component], connector);
                    foreach (var innerChild in cs.ChildNodes)
                    {
                        var cc = (XmlElement) innerChild;
                        component = cc.GetAttribute("component");
                        connector = cc.GetAttribute("connector");
                        if (ComponentInformations.AllLoadedPlugins.ContainsKey(component))
                        {
                            var count = uint.Parse(cc.GetAttribute("count"));
                            var otherComponentConnector = new ComponentConnector(ComponentInformations.AllLoadedPlugins[component], connector);
                            if (!Statistics.ContainsKey(componentConnector))
                            {
                                Statistics.Add(componentConnector, new ConnectorStatistics());
                                Statistics[componentConnector].IncrementConnectorUsage(otherComponentConnector, count);
                            }
                        }
                    }
                }
                else
                {
                    GuiLogMessageOccured(string.Format("Can't find component type {0} in running system.", component), NotificationLevel.Warning);
                }
            }
        }

        public static void GenerateStatisticsFromTemplate(string templateDir)
        {
            var modelLoader = new ModelPersistance();

            foreach (var file in Directory.GetFiles(templateDir, "*.cwm", SearchOption.AllDirectories))
            {
                var templateFile = new FileInfo(file);
                if (templateFile.Name.StartsWith("."))
                {
                    continue;
                }

                using (var model = modelLoader.loadModel(templateFile.FullName))
                {
                    //Analyse model connections:
                    foreach (var pluginModel in model.GetAllPluginModels())
                    {
                        foreach (var inputConnector in pluginModel.GetInputConnectors())
                        {
                            AnalyseConnectorUsage(inputConnector);
                        }
                        foreach (var outputConnector in pluginModel.GetOutputConnectors())
                        {
                            AnalyseConnectorUsage(outputConnector);
                        }
                    }
                }
            }
        }

        private static void AnalyseConnectorUsage(ConnectorModel connectorModel)
        {
            var componentConnector = new ComponentConnector(connectorModel.PluginModel.PluginType, connectorModel.PropertyName);
            foreach (var otherConnector in AllConnectedConnectors(connectorModel))
            {
                if (!Statistics.ContainsKey(componentConnector))
                {
                    Statistics.Add(componentConnector, new ConnectorStatistics());
                }
                Statistics[componentConnector].IncrementConnectorUsage(otherConnector);
            }
        }

        private static IEnumerable<ComponentConnector> AllConnectedConnectors(ConnectorModel connectorModel)
        {
            foreach (var inputConnection in connectorModel.GetInputConnections())
            {
                yield return new ComponentConnector(inputConnection.From.PluginModel.PluginType, inputConnection.From.PropertyName);
            }
            foreach (var outputConnection in connectorModel.GetOutputConnections())
            {
                yield return new ComponentConnector(outputConnection.To.PluginModel.PluginType, outputConnection.To.PropertyName);
            }
        }

        private static void GuiLogMessageOccured(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogMessageOccured != null)
                OnGuiLogMessageOccured(message, loglevel);
        }
    }
}
