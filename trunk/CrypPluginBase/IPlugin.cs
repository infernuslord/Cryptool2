/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.PluginBase
{
    /// <summary>
    /// See Wiki for more information: https://www.cryptool.org/trac/CrypTool2/wiki/IPluginHints
    /// </summary>
    public interface IPlugin : INotifyPropertyChanged, IDisposable
    {
        event StatusChangedEventHandler OnPluginStatusChanged;
        event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        event PluginProgressChangedEventHandler OnPluginProgressChanged;
                
        ISettings Settings { get; }

        /// <summary>
        /// Provide all presentation stuff in this user control, it will be opened in an tab.
        /// Return null if your plugin has no presentation. 
        /// </summary>
        /// <value>The presentation.</value>
        UserControl Presentation { get; }

        /// <summary>
        /// Will be called each time the plugin is run during workflow and after the inputs have been set.
        /// </summary>
        void Execute();

        /// <summary>
        /// Triggered when user clicked Stop button. Plugin must shut down long running tasks.
        /// PostExecution() will be called afterwards.
        /// </summary>
        void Stop();

        /// <summary>
        /// Will be called Will be called from editor after restoring settings and before adding to workspace.
        /// </summary>
        void Initialize();
    }
}
