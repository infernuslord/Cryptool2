using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cryptool.Core;
using Cryptool.PluginBase;

namespace StatusGenerator
{
    public class StatusGenerator
    {
        public const string CrypPlugins = "CrypPlugins";

        private IDictionary<string, Type> pluginAssemblies;

        private StreamWriter streamWriter;

        private string pluginPath;

        private IDictionary<string, string> publicSolution;
        private IDictionary<string, string> coreSolution;

        public StatusGenerator(string programRoot, string outputFile)
        {

            pluginAssemblies = LoadPlugins();

            streamWriter = new StreamWriter(outputFile);
            streamWriter.WriteLine("<html><head><style type=\"text/css\">td { border:1px solid; }</style></head>");
            streamWriter.WriteLine("<body><table>");
            streamWriter.WriteLine("<tr><th>Plugin Directory</th><th>Developer Solution</th><th>Nightly Build</th><th>Namespace</th><th>IPlugin</th><th>Documentation</th></tr>");

            pluginPath = Path.Combine(programRoot, CrypPlugins);

            publicSolution = ReadSolution("../../CrypTool 2.0.sln");
            coreSolution = ReadSolution("../../CoreDeveloper/CrypTool 2.0.sln");

            foreach (DirectoryInfo dir in new DirectoryInfo(pluginPath).GetDirectories())
            {
                if (dir.Name.StartsWith("."))
                    continue;

                ProcessDirectory(dir.Name);
            }

            streamWriter.WriteLine("</table></body></html>");
            streamWriter.Close();
        }

        private void ProcessDirectory(string pluginName)
        {
            bool isInDeveloperSolution = publicSolution.ContainsKey(pluginName);
            bool isInNightlyBuild = coreSolution.ContainsKey(pluginName);

            streamWriter.Write(string.Format("<tr><td>{0}</td>", pluginName));
            streamWriter.Write("<td style=\"text-align:center\">");
            streamWriter.Write(isInDeveloperSolution ? "true" : "<span style=\"color:red\">false</span>");
            streamWriter.Write("</td><td style=\"text-align:center\">");
            streamWriter.Write(isInNightlyBuild ? "true" : "<span style=\"color:red\">false</span>");
            streamWriter.Write("</td>");

            var pluginTypes = from type in pluginAssemblies.Values
                              where type.Assembly.GetName().Name == pluginName
                              select type;

            if (pluginTypes.Count() == 0)
            {
                streamWriter.Write("<td colspan=\"3\"><span style=\"color:red\">IPlugin not found</span> (not in CrypBuild, assembly name != directory name or not an IPlugin)</td>");
            }
            else
            {
                streamWriter.Write("<td>");
                foreach(var type in pluginTypes)
                    streamWriter.Write(type.Namespace + "<br>");

                streamWriter.Write("</td><td>");
                foreach (var type in pluginTypes)
                    streamWriter.Write(type.Name + "<br>");

                streamWriter.Write("</td><td>");

                foreach (var type in pluginTypes)
                {
                    string descFile = type.GetPluginInfoAttribute().DescriptionUrl;
                    if (string.IsNullOrWhiteSpace(descFile))
                        streamWriter.Write("<span style=\"color:red\">None</span>");
                    else if (descFile.EndsWith(".xaml"))
                        streamWriter.Write("<span style=\"color:red\">XAML: </span>" + descFile);
                    else if (!File.Exists(Path.Combine(pluginPath, descFile)))
                        streamWriter.Write("<span style=\"color:red\">File not found: </span>" + descFile);
                    else
                        streamWriter.Write("yes, XML");

                    streamWriter.Write("<br>");
                }
                streamWriter.Write("</td>");
            }

            streamWriter.WriteLine("</tr>");
        }

        //example: Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TextInput", "CrypPlugins\TextInput\TextInput.csproj", "{475E8850-4D82-4C5E-AD19-5FDA82BC7576}"
        private readonly Regex slnRegex = new Regex("Project\\(\"{([A-Z0-9-]+)}\"\\) = \"(\\S+)\", \"(\\S+)\", \"{([A-Z0-9-]+)}\"");

        private IDictionary<string, string> ReadSolution(string slnPath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!File.Exists(slnPath))
                return dict; // silently ignore, probably CoreDeveloper sln missing

            StreamReader streamReader = new StreamReader(slnPath);
            while(!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                Match match = slnRegex.Match(line);
                if (match.Success)
                {
                    string pluginName = match.Groups[2].Value;
                    string projectPath = match.Groups[3].Value;

                    while (projectPath.StartsWith("..\\"))
                        projectPath = projectPath.Replace("..\\", "");

                    if (projectPath.StartsWith(CrypPlugins))
                        dict[pluginName] = projectPath;
                }
            }

            streamReader.Close();

            return dict;
        }

        private IDictionary<string, Type> LoadPlugins()
        {
            return new PluginManager(null).LoadTypes(AssemblySigningRequirement.LoadAllAssemblies);
        }
    }
}
