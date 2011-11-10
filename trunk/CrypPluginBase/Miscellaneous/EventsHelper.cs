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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class EventsHelper
    {

        private static bool asynchronousPropertyChanged = true;
        public static bool AsynchronousPropertyChanged
        {
            get { return asynchronousPropertyChanged; }
            set { asynchronousPropertyChanged = value; }
        }

        private static bool asynchronousGuiLogMessage = true;
        public static bool AsynchronousGuiLogMessage
        {
            get { return asynchronousGuiLogMessage; }
            set { asynchronousGuiLogMessage = value; }
        }

        private static bool asynchronousProgressChanged = true;
        public static bool AsynchronousProgressChanged
        {
            get { return asynchronousProgressChanged; }
            set { asynchronousProgressChanged = value; }
        }

        private static bool asynchronousStatusChanged = true;
        public static bool AsynchronousStatusChanged
        {
            get { return asynchronousStatusChanged; }
            set { asynchronousStatusChanged = value; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler del, IPlugin plugin, string message)
        {
            GuiLogMessage(del, plugin, new GuiLogEventArgs(message, plugin, NotificationLevel.Debug));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler del, IPlugin plugin, string message, NotificationLevel level)
        {
            GuiLogMessage(del, plugin, new GuiLogEventArgs(message, plugin, level));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler del, IPlugin plugin, GuiLogEventArgs args)
        {
            if (del == null)
            {
                return;
            }
            Delegate[] delegates = del.GetInvocationList();
            if (AsynchronousGuiLogMessage)
            {
                AsyncCallback cleanUp = delegate(IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (GuiLogNotificationEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {

                foreach (GuiLogNotificationEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PropertyChanged(PropertyChangedEventHandler del, object sender, string propertyName)
        {
            PropertyChanged(del, sender, new PropertyChangedEventArgs(propertyName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PropertyChanged(PropertyChangedEventHandler del, object sender, PropertyChangedEventArgs args)
        {
            if (del == null)
            {
                return;
            }
            Delegate[] delegates = del.GetInvocationList();
            if (AsynchronousPropertyChanged)
            {
                AsyncCallback cleanUp = delegate(IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (PropertyChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(sender, args, cleanUp, null);
                }
            }
            else
            {
                foreach (PropertyChangedEventHandler sink in delegates)
                {
                    sink.Invoke(sender, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProgressChanged(PluginProgressChangedEventHandler del, IPlugin plugin, double value, double max)
        {
            ProgressChanged(del, plugin, new PluginProgressEventArgs(value, max));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProgressChanged(PluginProgressChangedEventHandler del, IPlugin plugin, PluginProgressEventArgs args)
        {
            if (del == null)
            {
                return;
            }
            Delegate[] delegates = del.GetInvocationList();
            if (AsynchronousProgressChanged)
            {
                AsyncCallback cleanUp = delegate(IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (PluginProgressChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {
                foreach (PluginProgressChangedEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StatusChanged(StatusChangedEventHandler del, IPlugin plugin, StatusEventArgs args)
        {
            if (del == null)
            {
                return;
            }
            Delegate[] delegates = del.GetInvocationList();
            if (AsynchronousStatusChanged)
            {
                AsyncCallback cleanUp = delegate(IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (StatusChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {
                foreach (StatusChangedEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }

    }
}
