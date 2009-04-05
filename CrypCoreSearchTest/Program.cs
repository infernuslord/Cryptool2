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
                                       (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\HelpFiles"),
                                   IndexPath =
                                       (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\HelpIndex")
                               };
            Provider.CreateIndexes();
            foreach(var Result in Provider.Search(Console.ReadLine()))
            {
                Console.WriteLine(Result.Plugin);
                Console.WriteLine(Result.Score);
                Console.WriteLine(Result.Context);
            }
            Console.ReadKey();
        }
    }
}
