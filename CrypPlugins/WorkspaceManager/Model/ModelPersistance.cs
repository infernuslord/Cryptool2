/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class with static methods for loading and saving of WorkspaceModels
    /// </summary>
    public class ModelPersistance
    {
        /// <summary>
        /// Deserializes a model from the given file with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static WorkspaceModel loadModel(string filename)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryFormatter binaryFormatter = new BinaryFormatter();                
                return (WorkspaceModel)binaryFormatter.Deserialize(fileStream);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        /// <summary>
        /// Serializes the given model to a file with the given filename
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static void saveModel(WorkspaceModel workspaceModel, string filename)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, workspaceModel);               
            }                
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }
    }
}
