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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.PluginBase
{
    /// <summary>
    /// See Wiki for more information: https://www.cryptool.org/trac/CrypTool2/wiki/IPluginHints
    /// </summary>
    public interface IPlugin : INotifyPropertyChanged
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
        /// Gets the quick watch presentation - will be displayed inside of the plugin presentation-element. You
        /// can return the existing Presentation if it makes sense to display it inside a small area. But be aware that
        /// if Presentation is displayed in QuickWatchPresentation you can't open Presentation it in a tab before you
        /// you close QuickWatchPresentation;
        /// Return null if your plugin has no QuickWatchPresentation. 
        /// </summary>
        /// <value>The quick watch presentation.</value>
        UserControl QuickWatchPresentation { get; }

        /// <summary>
        /// Will be called from editor before right before chain-run starts
        /// </summary>
        void PreExecution();

        /// <summary>
        /// Will be called from editor while chain-run is active and after last necessary input
        /// for plugin has been set. 
        /// </summary>
        void Execute();

        /// <summary>
        /// Will be called from editor after last plugin in chain has finished its work.
        /// </summary>
        void PostExecution();

        /// <summary>
        /// Not defined yet.
        /// </summary>
        void Pause();

        /// <summary>
        /// Will be called from editor while chain-run is active. Plugin hast to stop work immediately. 
        /// </summary>
        void Stop();

        /// <summary>
        /// Will be called from editor after restoring settings and before adding to workspace.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Will be called from editor when element is deleted from worksapce.
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        void Dispose();
    }
}
