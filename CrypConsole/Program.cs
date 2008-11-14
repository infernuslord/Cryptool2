using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.AccessControl;
using Cryptool.Core;
using Cryptool.PluginBase;

namespace Cryptool.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            PluginManager pm = new PluginManager();

            Arguments cmdLine = new Arguments(args);

            #region GETTER
            if (cmdLine["get"] != null)
            {
                //List all available plug-ins
                if (cmdLine["showplugins"] != null)
                {
                    pm.LoadPlugins(PluginLoader.LoadAllPlugins);
                    foreach ( IEncryptionAlgorithm algo in pm.EncryptionAlgorithms)
                    {
                        System.Console.WriteLine(algo.GetPluginInfo().Name);
                    }
                    foreach (IHashAlgorithm hash in pm.HashAlgorithms)
                    {
                        System.Console.WriteLine(hash.GetPluginInfo().Name);
                    }
                }
                if (cmdLine["md5"] != null)
                {

                }
            }
            #endregion

            #region SETTER
            if (cmdLine["set"] != null)
            {
                // add new plugin
                if (cmdLine["newplugin"] != null)
                {
                    //add new plugin from lokal hard drive
                    if (cmdLine["file"] != null)
                    {
                        FileStream stream = File.Open(cmdLine["newplugin"], FileMode.Open);
                        byte[] buffer = new byte[(int)stream.Length];
                        BinaryReader reader = new BinaryReader(stream);
                        buffer = reader.ReadBytes((int)stream.Length);
                        reader.Close();
                        stream.Close();

                        pm.AddPlugin(buffer, Cryptool.Core.PluginStore.CustomPluginStore);
                    }

                    //add new plugin from cryptool plugin server
                    if (cmdLine["server"] != null)
                    {
                        //To-Do
                    }
                }
            }
            #endregion

            //Assembly asm = Assembly.LoadFrom (@"c:\CrypCore.dll");
            //AssemblyName name = new AssemblyName(asm.FullName);

            //System.Console.WriteLine(asm.GetPublicToken());

            //System.Console.WriteLine(Encoding.Unicode.GetString(asm.GetName().GetPublicKeyToken()));

    
            //string.Format(System.Globalization.NumberStyles.HexNumber,

            //System.Console.WriteLine(asm.GetName().GetPublicKeyToken().ToString());

            //PluginManager pm = new PluginManager();
            //FileStream fs = File.Open(@"c:\CrypCore.dll", FileMode.Open);

            //BinaryReader br = new BinaryReader(fs);
            //byte[] data = br.ReadBytes((int)fs.Length);

            //br.Close();
            //fs.Close();


            //pm.AddPlugin(data, PluginStore.GlobalPluginStore);

            //System.Console.WriteLine("Done");
            //System.Console.ReadLine();
        }
    }
}
