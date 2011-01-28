/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace Cryptool.Core
{
    /// <summary>
    /// PluginManager Class  
    /// </summary>
    public class PluginManager
    {
        private readonly HashSet<string> disabledAssemblies = new HashSet<string>();

        /// <summary>
        /// Counter for the dll files that were found
        /// </summary>
        private int availablePluginsApproximation = 0;

        /// <summary>
        /// Subdirectory to store plugins
        /// </summary>
        private const string PluginDirecory = "CrypPlugins";

        /// <summary>
        /// Fires if an exception occurs
        /// </summary>
        public event CrypCoreExceptionEventHandler OnExceptionOccured;

        /// <summary>
        /// Fires if an info occurs
        /// </summary>
        public event CrypCoreDebugEventHandler OnDebugMessageOccured;

        /// <summary>
        /// Occurs when a plugin was loaded
        /// </summary>
        public event CrypCorePluginLoadedHandler OnPluginLoaded;
        
        /// <summary>
        /// Custom Plugin Store Directory
        /// </summary>
        private readonly string customPluginStore;

        /// <summary>
        /// Global Plugin Store Directory
        /// </summary>
        private readonly string globalPluginStore;

        /// <summary>
        /// Loaded Assemblies
        /// </summary>
        private readonly Dictionary<string, Assembly> loadedAssemblies;

        /// <summary>
        /// Loaded Types
        /// </summary>
        private readonly Dictionary<string, Type> loadedTypes;

        Dictionary<string, Assembly> foundAssemblies = new Dictionary<string, Assembly>();
        
        /// <summary>
        /// cTor
        /// </summary>
        public PluginManager(HashSet<string> disabledAssemblies)
        {
            this.disabledAssemblies = disabledAssemblies;
            
            this.customPluginStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PluginDirecory);
            this.globalPluginStore = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirecory);
            this.loadedAssemblies = new Dictionary<string, Assembly>();
            this.loadedTypes = new Dictionary<string, Type>();          
        }

        /// <summary>
        /// Returns a type of a loaded assembly
        /// </summary>
        /// <param name="assemblyName">Assembly Name</param>
        /// <param name="typeName">Type Name</param>
        /// <returns>Return the type or null if no type could be found</returns>
        public Type LoadType(string assemblyName, string typeName)
        {
            if (this.loadedAssemblies.ContainsKey(assemblyName))
                return this.loadedAssemblies[assemblyName].GetType(typeName, false);
            return null;
        }

        /// <summary>
        /// Returns all types found in the plugins
        /// </summary>
        /// <param name="state">Load type from all plugins or from signed only</param>
        /// <returns></returns>
        public Dictionary<string, Type> LoadTypes(AssemblySigningRequirement state)
        {
            if (Directory.Exists(globalPluginStore))
            {
              availablePluginsApproximation = AvailablePluginsApproximation(new DirectoryInfo(globalPluginStore));
              FindAssemblies(new DirectoryInfo(globalPluginStore), state, foundAssemblies);
            }

            // custom plugin store is not supported yet
            //if (!Directory.Exists(customPluginStore))
            //    Directory.CreateDirectory(customPluginStore);
            //FindAssemblies(new DirectoryInfo(customPluginStore), state, foundAssemblies);
            LoadTypes(foundAssemblies);
            return this.loadedTypes;
        }


        [Obsolete("will be removed soon")]
        private int AvailablePluginsApproximation(DirectoryInfo directory)
        {
          int count = 0;
          foreach (DirectoryInfo subDirectory in directory.GetDirectories())
          {
            count = AvailablePluginsApproximation(subDirectory);
          }
          return directory.GetFiles("*.dll").Length;
        }

        /// <summary>
        /// Search all subdirectories for assemblies
        /// </summary>
        /// <param name="directory">Root directory</param>
        /// <param name="state">Search for all or only for signed assemblies</param>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private void FindAssemblies(DirectoryInfo directory, AssemblySigningRequirement state, Dictionary<string, Assembly> foundAssemblies)
        {
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                FindAssemblies(subDirectory, state, foundAssemblies);
            }

            int currentPosition = 0;
            foreach (FileInfo fileInfo in directory.GetFiles("*.dll"))
            {
                if (disabledAssemblies != null && disabledAssemblies.Contains(fileInfo.Name))
                    continue;

                currentPosition++;
                try
                {
                    Assembly asm = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));
                    
                    string key = GetAssemblyKey(asm.FullName, state);
                    if (key == null)
                        throw new UnknownFileFormatException(fileInfo.FullName);

                    bool sendMessage = false;
                    if (!foundAssemblies.ContainsKey(key))
                    {
                      foundAssemblies.Add(key, asm);
                      sendMessage = true;
                    }
                    else
                      if (new AssemblyName(asm.FullName).Version > new AssemblyName(foundAssemblies[key].FullName).Version)
                      {
                        foundAssemblies[key] = asm;
                        sendMessage = true;
                      }

                    if (sendMessage)
                    {
                        SendDebugMessage("Loaded Assembly \"" + asm.FullName + "\" from file: " + fileInfo.FullName);
                        if (OnPluginLoaded != null)
                        {
                          OnPluginLoaded(this, new PluginLoadedEventArgs(currentPosition, this.availablePluginsApproximation, asm.GetName().Name + " Version=" + asm.GetName().Version.ToString()));
                        }                          
                    }
                }
                catch (BadImageFormatException)
                {
                  SendExceptionMessage(string.Format(Resources.Exceptions.non_plugin_file, fileInfo.Name));
                }
                catch (Exception ex)
                {
                    SendExceptionMessage(ex);
                }
            }
        }

        /// <summary>
        /// Interate the found assemblies and add well known types
        /// </summary>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private void LoadTypes(Dictionary<string, Assembly> foundAssemblies)
        {
            string interfaceName = "Cryptool.PluginBase.IPlugin";

            foreach (Assembly asm in foundAssemblies.Values)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                try
                {
                  foreach (Type type in asm.GetTypes())
                  {
                    if (type.GetInterface(interfaceName) != null && !this.loadedTypes.ContainsKey(type.FullName))
                    {
                      this.loadedTypes.Add(type.FullName, type);
                      if (!this.loadedAssemblies.ContainsKey(assemblyName.Name))
                      {
                        this.loadedAssemblies.Add(assemblyName.Name, asm);
                      }
                    }
                  }
                }
                catch (ReflectionTypeLoadException tle)
                {
                  if (OnExceptionOccured != null)
                  {
                    OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + tle.LoaderExceptions[0].Message)));
                  }
                }
                catch (Exception exception)
                {
                  if (OnExceptionOccured != null)
                  {
                    OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + exception.Message)));
                  }
                }
            }
        }

        /// <summary>
       /// Create a unique key for each assembly
       /// </summary>
       /// <param name="assemblyFullName">Full name of the assembly</param>
       /// <param name="state">Signed or unsigned</param>
       /// <returns>Returns the key or null if public key is null and signing is required</returns>
        private string GetAssemblyKey(string assemblyFullName, AssemblySigningRequirement state)
        {
            AssemblyName asmName = new AssemblyName(assemblyFullName);
            if (state == AssemblySigningRequirement.LoadSignedAssemblies)
            {
                if (asmName.KeyPair.PublicKey == null)
                    return null;
                return asmName.Name + "__" + asmName.KeyPair.ToString();
            }
            return asmName.Name;
        }

        /// <summary>
        /// Adds an assembly to the store
        /// </summary>
        /// <param name="buffer">byte[] of the assembly</param>
        /// <param name="state">Signed or unsigned</param>
        /// <param name="pluginStore">Global or Custom Store</param>
        public void AddPlugin(byte[] buffer, AssemblySigningRequirement state, PluginStore pluginStore)
        {
            try
            {
                Assembly asm = Assembly.ReflectionOnlyLoad(buffer);
                AssemblyName asmName = new AssemblyName(asm.FullName);

                string publicKeyToken = GetPublicToken(asm);
                if ((state == AssemblySigningRequirement.StoreSignedAssemblies) && (publicKeyToken == string.Empty))
                    throw new AssemblyNotSignedException();

                string pluginStoreDirectory = GetPluginDirectory(asmName, publicKeyToken, pluginStore);
                if (!Directory.Exists(pluginStoreDirectory))
                    Directory.CreateDirectory(pluginStoreDirectory);

                WriteFile(buffer, pluginStoreDirectory, asmName);
            }
            catch (AssemblyNotSignedException ex)
            {
                if (OnExceptionOccured != null)
                    OnExceptionOccured(this, new PluginManagerEventArgs(ex));
            }
            catch
            {
                if (OnExceptionOccured != null)
                    OnExceptionOccured(this, new PluginManagerEventArgs(new StoreAddingException()));
            }
        }

        /// <summary>
        /// Writes the buffer to disk
        /// </summary>
        /// <param name="buffer">byte[] of the assembly</param>
        /// <param name="pluginStoreDirectory">Directory to store the assembly</param>
        /// <param name="assemblyName">Name of the assembly</param>
        private void WriteFile(byte[] buffer, string pluginStoreDirectory, AssemblyName assemblyName)
        {
            string assemblyFileName = Path.Combine(pluginStoreDirectory, assemblyName.Name + ".dll");
            if (!File.Exists(assemblyFileName))
            {
                FileStream fs = File.Create(assemblyFileName);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(buffer);
                bw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// Get the directory for the assembly
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <param name="publicKeyToken">Public token</param>
        /// <param name="pluginStore">Global or Custom Store</param>
        /// <returns>Returns the full directory name</returns>
        private string GetPluginDirectory(AssemblyName assemblyName, string publicKeyToken, PluginStore pluginStore)
        {
            string versionName = string.Empty;
            if (publicKeyToken == String.Empty)
                versionName = Path.Combine(assemblyName.Name, assemblyName.Version.ToString());
            else
                versionName = Path.Combine(assemblyName.Name, assemblyName.Version.ToString() + "__" + publicKeyToken);

            switch (pluginStore)
            {
                case PluginStore.GlobalPluginStore:
                    return Path.Combine(globalPluginStore, versionName);
                case PluginStore.CustomPluginStore:
                    return Path.Combine(customPluginStore, versionName);
            }
            return String.Empty;
        }

        /// <summary>
        /// Returns the public token
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        private string GetPublicToken(Assembly asm)
        {
            byte[] hexBytes = asm.GetName().GetPublicKeyToken();
            string hexString = String.Empty;
            for (int i = 0; i < hexBytes.Length; i++)
            {
                hexString += hexBytes[i].ToString("x");
            }
            return hexString;
        }

        /// <summary>
        /// Sends a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendDebugMessage(string message)
        {
          if (OnDebugMessageOccured != null)
            OnDebugMessageOccured(this, new PluginManagerEventArgs(message));
        }

        private void SendExceptionMessage(Exception ex)
        {
          if (OnExceptionOccured != null)
            OnExceptionOccured(this, new PluginManagerEventArgs(ex));
        }

        private void SendExceptionMessage(string message)
        {
          if (OnExceptionOccured != null)
            OnExceptionOccured(this, new PluginManagerEventArgs(message));
        }
    }
}
