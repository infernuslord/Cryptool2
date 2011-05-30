using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StatusGenerator
{
    public class StatusGenerator
    {
        public const string CrypPlugins = "CrypPlugins";

        private IDictionary<string, string> publicSolution;
        private IDictionary<string, string> coreSolution;

        private StreamWriter streamWriter;

        public void Generate(string programRoot, string outputFile)
        {
            streamWriter = new StreamWriter(outputFile);
            streamWriter.WriteLine("<html><body><table>");
            streamWriter.WriteLine("<tr><th>Plugin</th><th>Developer Solution</th><th>Nightly Build</th></tr>");

            string pluginPath = Path.Combine(programRoot, CrypPlugins);

            publicSolution = ReadSolution("../../CrypTool 2.0.sln");
            coreSolution = ReadSolution("../../CoreDeveloper/CrypTool 2.0.sln");

            foreach (DirectoryInfo dir in new DirectoryInfo(pluginPath).GetDirectories())
            {
                if (dir.Name.StartsWith("."))
                    continue;

                ProcessDirectory(dir);
            }

            streamWriter.WriteLine("</table></body></html>");
            streamWriter.Close();
        }

        private void ProcessDirectory(DirectoryInfo dir)
        {
            string dirShortName = dir.Name;

            bool isInDeveloperSolution = publicSolution.ContainsKey(dirShortName);
            bool isInNightlyBuild = coreSolution.ContainsKey(dirShortName);

            streamWriter.Write(string.Format("<tr><td>{0}</td>", dirShortName));
            streamWriter.Write(isInDeveloperSolution ? "<td>true</td>" : "<td><font color=\"red\">false</td>");
            streamWriter.Write(isInNightlyBuild ? "<td>true</td>" : "<td><font color=\"red\">false</td>");
            streamWriter.WriteLine("</tr>");
        }

        private Regex slnRegex = new Regex("Project\\(\"{([A-Z0-9-]+)}\"\\) = \"(\\S+)\", \"(\\S+)\", \"{(\\w+)-(\\w+)-(\\w+)-(\\w+)-(\\w+)}\"");

        private IDictionary<string, string> ReadSolution(string slnPath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

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

            return dict;
        }
    }
}
