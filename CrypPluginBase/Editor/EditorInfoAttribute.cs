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

namespace Cryptool.PluginBase.Editor
{
    /// <summary>
    /// The default file-extension for the editor used by CrypWin to display 
    /// Open/Save FileDialog with correct filter.
    /// </summary>
    public class EditorInfoAttribute : Attribute
    {
        public readonly bool ShowAsNewButton;
        public readonly bool ShowLogPanel;
        public readonly bool ShowSettingsPanel;
        public readonly bool ShowComponentPanel;
        public readonly string DefaultExtension;

        public EditorInfoAttribute(string defaultExtension, bool showAsNewButton = true, bool showLogPanel = true, bool showSettingsPanel = true, bool showComponentPanel = true)
        {
            ShowAsNewButton = showAsNewButton;
            DefaultExtension = defaultExtension;
            ShowComponentPanel = showComponentPanel;
            ShowLogPanel = showLogPanel;
            ShowSettingsPanel = showSettingsPanel;
        }
    }
}
