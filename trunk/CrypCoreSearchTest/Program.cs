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
            Provider.CreateIndexes();
            Console.WriteLine("Bitte Suchwort eingeben");
            int counter= 1;
            foreach(var Result in Provider.Search(Console.ReadLine()))
            {
                Console.WriteLine(string.Format("{0}. PlugIn: {1}",counter,Result.Plugin));
                foreach (var context in Result.Contexts)
                {
                    Console.WriteLine(string.Format("     Kontext: {0}", context));
                }
                counter++;
            }
            Console.ReadKey();
        }
    }
}
