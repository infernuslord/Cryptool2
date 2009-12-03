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
using System.Reflection;
using Cryptool.PluginBase.Control;
using System.Collections.Generic;

namespace Cryptool.PluginBase
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyInfoAttribute : Attribute
    {
        #region messages
        protected const string EMPTY_GUID = "Empty or null GUID is not allowed.";
        #endregion

        # region multi language properties
        public readonly string caption;
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

        # region normal properties
        public readonly string DescriptionUrl;
        public readonly Direction Direction;
        public readonly bool Mandatory;        
        public readonly DisplayLevel DisplayLevel;        
        public QuickWatchFormat QuickWatchFormat;
        public string PropertyName; // will be set in extension-method
        public PropertyInfo PropertyInfo { get; set; } // will be set in extension-method
        public readonly string QuickWatchConversionMethod;
        public readonly bool HasDefaultValue;        
        #endregion normal properties

        # region translation helpers
        private Type pluginType;

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType // will be set in extension-method
        {
          get { return pluginType; }
          set { pluginType = value; }
        }

        private bool MultiLanguage
        {
          get { return PluginType != null && PluginType.GetPluginInfoAttribute().ResourceFile != null; }
        }
        # endregion translation helpers     

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoAttribute"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="descriptionUrl">The description URL.</param>
        /// <param name="mandatory">if set to <c>true</c> [mandatory].</param>
        /// <param name="displayLevel">The display level.</param>
        /// <param name="quickWatchFormat">The quick watch format.</param>
        /// <param name="quickWatchConversion">Methodname of converstion method.</param>
        public PropertyInfoAttribute(Direction direction, string caption, string toolTip, string descriptionUrl, bool mandatory, bool hasDefaultValue, DisplayLevel displayLevel, QuickWatchFormat quickWatchFormat, string quickWatchConversionMethod)
        {
            this.caption = caption == null ? "" : caption;
            this.toolTip = toolTip == null ? "" : toolTip;
            this.DescriptionUrl = descriptionUrl == null ? "" : descriptionUrl;
            this.Direction = direction;
            this.Mandatory = mandatory;
            this.DisplayLevel = displayLevel;
            this.QuickWatchFormat = quickWatchFormat;
            this.QuickWatchConversionMethod = quickWatchConversionMethod;
            this.HasDefaultValue = hasDefaultValue;
        }

        public PropertyInfoAttribute(Direction direction, string caption, string toolTip, string descriptionUrl, DisplayLevel displayLevel) 
          : this (direction, caption, toolTip, descriptionUrl, false, false, displayLevel, QuickWatchFormat.None, null)
        {
        }

        #endregion constructor
    }
  }
