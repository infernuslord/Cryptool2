using System;
using System.IO;
using System.Reflection;
using Cryptool.Core;

namespace CrypCoreSearchTest
{
    class Program
    {
        static void Main()
        {
            var Provider = new SearchProvider
                               {
                                   HelpFilePath =
                                       Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\..\\..\\HelpFiles"),
                                   IndexPath =
                                       Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\..\\..\\HelpIndex")
                               };
            Console.WriteLine(string.Format("HelpFilePath: {0}\r\nIndexPath: {1}",Provider.HelpFilePath,Provider.IndexPath));
            Console.WriteLine("Indizes neu erstellen? (y/n)");
            if(Console.ReadLine().ToLower() == "y")
            {
                Console.WriteLine("Indizes werden neu erstellt...");
                Provider.CreateIndexes();
            }
            Console.WriteLine("Bitte Suchwort eingeben");
            int counter= 1;
            var results = Provider.Search(Console.ReadLine());
            if(results.Count  > 0)
            {
                foreach (var Result in results)
                {
                    Console.WriteLine(string.Format("\r\n{0}. PlugIn: {1}", counter, Result.Plugin));
                    foreach (var context in Result.Contexts)
                    {
                        Console.WriteLine(string.Format("     Kontext: {0}", context));
                    }
                    counter++;
                }
            }
            else
            {
                Console.WriteLine("Nichts gefunden...");
            }
            
            Console.ReadKey();
        }
    }
}
