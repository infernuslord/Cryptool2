using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.Core;
using Cryptool.PluginBase;
using System.Reflection;

namespace WorkspaceManager.View.Converter
{
    class DragDropDataObjectToPluginConverter
    {
        public static PluginManager PluginManager { get; set; }

        private static Type type;

        public static Type CreatePluginInstance(string assemblyQualifiedName, string typeVar)
        {
            if (PluginManager != null && assemblyQualifiedName != null && typeVar != null)
            {
                AssemblyName assName = new AssemblyName(assemblyQualifiedName);
                type = PluginManager.LoadType(assName.Name, typeVar);

                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
