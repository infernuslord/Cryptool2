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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase
{
  /// <summary>
  /// Used to mark Plugin-classes that will not be displayed in the normal plugin-area. 
  /// Instead this types can be used by an editor to display plugins in a speparate place.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class EditorSpecificPluginAttribute : Attribute
  {
  }

  public class EditorSpecificPluginInfo
  {
    public readonly Type Type;
    public readonly string Name;
    public readonly string Identifier;
    public readonly string GroupName;
    public readonly ContextMenuEntry[] ContextMenuEntries;

    public EditorSpecificPluginInfo(string name, string identifier, Type type, string groupName, params ContextMenuEntry[] contextMenuEntries)
    {
      this.Type = type;
      this.Name = name;
      this.Identifier = identifier;
      this.GroupName = groupName;
      this.ContextMenuEntries = contextMenuEntries;
    }
  }

  public class ContextMenuEntry
  {
    public readonly string Name;
    public readonly string MethodName;

    public ContextMenuEntry(string name, string methodName)
    {
      this.Name = name;
      this.MethodName = methodName;
    }
  }
}
