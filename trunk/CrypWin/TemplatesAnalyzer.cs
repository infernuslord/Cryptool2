using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.Miscellaneous;
using WorkspaceManager.Model;

namespace Cryptool.CrypWin
{
    public static class TemplatesAnalyzer
    {
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
            var componentConnector = new ComponentConnectionStatistics.ComponentConnector(connectorModel.PluginModel.PluginType, connectorModel.PropertyName);
            foreach (var otherConnector in AllConnectedConnectors(connectorModel))
            {
                ComponentConnectionStatistics.IncrementConnectionUsage(componentConnector, otherConnector);
            }
        }

        private static IEnumerable<ComponentConnectionStatistics.ComponentConnector> AllConnectedConnectors(ConnectorModel connectorModel)
        {
            foreach (var inputConnection in connectorModel.GetInputConnections())
            {
                yield return new ComponentConnectionStatistics.ComponentConnector(inputConnection.From.PluginModel.PluginType, inputConnection.From.PropertyName);
            }
            foreach (var outputConnection in connectorModel.GetOutputConnections())
            {
                yield return new ComponentConnectionStatistics.ComponentConnector(outputConnection.To.PluginModel.PluginType, outputConnection.To.PropertyName);
            }
        }
    }
}
