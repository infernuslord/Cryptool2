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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Threading;
using System.IO;
using Cryptool.Core;
using Cryptool.CrypWin.Properties;
using System.Reflection;
using System.Security.Principal;
using Cryptool.PluginBase.Miscellaneous;
using Application = System.Windows.Application;


namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UnhandledExceptionDialog.ShowModalDialog((Exception)e.ExceptionObject, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
            }, null);
        }

        

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledExceptionDialog.ShowModalDialog(e.Exception, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
        }
    }
}
