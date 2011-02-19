/*                              
   Copyright 2010 Nils Kopal

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent the Connection between two Connector Models
    /// </summary>
    [Serializable]
    public class ConnectionModel : VisualElementModel
    {
        internal ConnectionModel()
        {

        }      

        #region private members

        /// <summary>
        /// Name of the Connection type
        /// </summary>
        private string ConnectionTypeName = null;

        /// <summary>
        /// Name of the Connection assembly
        /// </summary>
        private string ConnectionTypeAssemblyName = null;

        #endregion

        #region public members

        /// <summary>
        /// The starting Point of this Connection Model
        /// </summary>
        public ConnectorModel From { get; internal set; }

        /// <summary>
        /// The ending Point of this Connection Model
        /// </summary>
        public ConnectorModel To { get; internal set; }

        /// <summary>
        /// Is the Connection active?
        /// </summary>        
        public bool Active { get; internal set; }

        /// <summary>
        /// The Type of the Wrapped IPlugin of this PluginModel
        /// Depending on this the Plugin of this PluginModel will be instanciated
        /// </summary>        
        public Type ConnectionType
        {
            get
            {
                if (this.ConnectionTypeName != null)
                {
                    if (ConnectionTypeName.Equals("System.Numerics.BigInteger"))
                    {
                        return typeof(System.Numerics.BigInteger);
                    }
                    if (ConnectionTypeName.Equals("System.Numerics.BigInteger[]"))
                    {
                        return typeof(System.Numerics.BigInteger[]);
                    }
                    Assembly assembly = Assembly.Load(ConnectionTypeAssemblyName);
                    Type t = assembly.GetType(ConnectionTypeName);
                    return t;
                }
                else
                {
                    return null;
                }
            }
            internal set
            {
                this.ConnectionTypeName = value.FullName;
                this.ConnectionTypeAssemblyName = value.Assembly.GetName().Name;
            }
        }

        /// <summary>
        /// The WorkspaceModel of this ConnectionModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; internal set; }

        #endregion

    }
}
