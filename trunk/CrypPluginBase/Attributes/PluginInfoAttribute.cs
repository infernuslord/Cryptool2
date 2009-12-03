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

namespace Cryptool.PluginBase
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginInfoAttribute : Attribute
    {
        # region multi language properties
        private readonly string caption;
        public string Caption
        {
          get 
          {
            if (MultiLanguage && caption != null)
              return PluginType.GetPluginStringResource(caption);
            else
              return caption; 
          }
        }

        public readonly string toolTip;
        public string ToolTip
        {
          get
          {
            if (MultiLanguage && toolTip != null)
              return PluginType.GetPluginStringResource(toolTip);
            else
              return toolTip;
          }
        }
        # endregion multi language properties

        # region no-translation
        public readonly bool Startable;      
        public readonly string DescriptionUrl;
        public readonly string[] Icons;
        # endregion

        # region translation helpers
        public readonly string ResourceFile;
        private Type pluginType;

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType
        {
          get { return pluginType; }
          set { pluginType = value; }
        }

        private bool MultiLanguage
        {
          get { return ResourceFile != null && PluginType != null; }
        }
        # endregion translation helpers

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginInfoAttribute"/> class.
        /// </summary>
        /// <param name="startable">if set to <c>true</c> [startable].</param>
        /// <param name="caption">General name.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="descriptionUrl">If FlowDocument for description is provided place the URI here. Set XAML file to "Resource" not "Embedded Resource".</param>
        /// <param name="icons">The icons. Set icon files to "Resource" not "Embedded Resource".</param>
        public PluginInfoAttribute(bool startable, string caption, string toolTip, string descriptionUrl, params string[] icons)
          {
            this.Startable = startable;
            this.caption = caption;
            this.toolTip = toolTip;
            this.DescriptionUrl = descriptionUrl;
            this.Icons = icons;
        }

        /// <summary>
        /// Temp. two constructors while not all plugins are switched to new multi language mode.
        /// </summary>
        /// <param name="resourceFile">The resource file.</param>
        /// <param name="startable">if set to <c>true</c> [startable].</param>
        /// <param name="caption">The caption.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="descriptionUrl">The description URL.</param>
        /// <param name="icons">The icons.</param>
        public PluginInfoAttribute(string resourceFile, bool startable, string caption, string toolTip, string descriptionUrl, params string[] icons)
        {
          this.ResourceFile = resourceFile;
          this.Startable = startable;
          this.caption = caption;
          this.toolTip = toolTip;
          this.DescriptionUrl = descriptionUrl;
          this.Icons = icons;
        }
    }
}
