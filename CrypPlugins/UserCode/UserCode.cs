/*                              
   Copyright 2011, Nils Kopal, Uni Duisburg-Essen

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
using System.CodeDom.Compiler;
using System.Numerics;
using System.Reflection;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using Microsoft.CSharp;

namespace Cryptool.Plugins.UserCode
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.UserCode.Properties.Resources", "PluginCaption", "PluginTooltip", null, "UserCode/icons/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class UserCode : ICrypComponent
    {
        private readonly UserCodePresentation _presentation = new UserCodePresentation();

        public UserCode()
        {
            _settings = new UserCodeSettings();
            _presentation.TextBox.TextChanged +=new TextChangedEventHandler(TextBox_TextChanged);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            _settings.Sourcecode = _presentation.TextBox.Text;            
        }

        #region Properties

        private object _input1;
        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip")]
        public object Input1
        {
            get
            {
                return _input1;
            }
            set
            {
                _input1 = value;
                OnPropertyChanged("Input1");
            }
        }

        private object _input2;
        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip")]
        public object Input2
        {
            get
            {
                return _input2;
            }
            set
            {
                _input2 = value;
                OnPropertyChanged("Input2");
            }
        }

        private object _input3;
        [PropertyInfo(Direction.InputData, "Input3Caption", "Input3Tooltip")]
        public object Input3
        {
            get
            {
                return _input3;
            }
            set
            {
                _input3 = value;
                OnPropertyChanged("Input3");
            }
        }

        private object _input4;
        [PropertyInfo(Direction.InputData, "Input4Caption", "Input4Tooltip")]
        public object Input4
        {
            get
            {
                return _input4;
            }
            set
            {
                _input4 = value;
                OnPropertyChanged("Input4");
            }
        }

        private object _input5;
        [PropertyInfo(Direction.InputData, "Input5Caption", "Input5Tooltip")]
        public object Input5
        {
            get
            {
                return _input5;
            }
            set
            {
                _input5 = value;
                OnPropertyChanged("Input5");
            }
        }

        private object _output1;
        [PropertyInfo(Direction.OutputData, "Output1Caption", "Input1Tooltip")]
        public object Output1
        {
            get
            {
                return _output1;
            }
            set
            {
                _output1 = value;
                OnPropertyChanged("Output1");
            }
        }

        private object _output2;
        [PropertyInfo(Direction.OutputData, "Output2Caption", "Input2Tooltip")]
        public object Output2
        {
            get
            {
                return _output2;
            }
            set
            {
                _output2 = value;
                OnPropertyChanged("Output2");
            }
        }

        private object _output3;
        [PropertyInfo(Direction.OutputData, "Output3Caption", "Input3Tooltip")]
        public object Output3
        {
            get
            {
                return _output3;
            }
            set
            {
                _output3 = value;
                OnPropertyChanged("Output3");
            }
        }

        private object _output4;
        [PropertyInfo(Direction.OutputData, "Output4Caption", "Input4Tooltip")]
        public object Output4
        {
            get
            {
                return _output4;
            }
            set
            {
                _output4 = value;
                OnPropertyChanged("Output4");
            }
        }

        private object _output5;
        [PropertyInfo(Direction.OutputData, "Output5Caption", "Input5Tooltip")]
        public object Output5
        {
            get
            {
                return _output5;
            }
            set
            {
                _output5 = value;
                OnPropertyChanged("Output5");
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private UserCodeSettings _settings;
        private object _compiledAssembly;

        public ISettings Settings
        {
            get { return _settings; }
            set { _settings = (UserCodeSettings)value; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void PreExecution()
        {
            try
            {
                var cs = new CSharpCodeProvider();
                var cc = cs.CreateCompiler();
                var cp = new CompilerParameters {GenerateInMemory = true};
                cp.ReferencedAssemblies.Add(GetType().Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof (IPlugin).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof (Exception).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof (INotifyPropertyChanged).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof (BigInteger).Assembly.Location);
                var code = Properties.Resources.UserClass.Replace("//USERCODE//", _settings.Sourcecode);
                var assembly = cc.CompileAssemblyFromSource(cp, code);
                if(assembly.Errors.Count>0)
                {
                    foreach (var error in assembly.Errors)
                    {
                        GuiLogMessage(string.Format("Compile error: {0}",error.ToString()),NotificationLevel.Error);
                    }
                    return;
                }
                _compiledAssembly = assembly.CompiledAssembly.CreateInstance("Cryptool.Plugins.UserCode.UserClass",
                    true,BindingFlags.Default,null,new object[]{this},null,null);
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Exception during code compilation: {0}",ex.Message),NotificationLevel.Error);
            }
        }

        public void Execute()
        {
            try
            {
                _compiledAssembly.GetType().GetMethod("UserMethod").Invoke(_compiledAssembly, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during code execution: {0}", ex.Message), NotificationLevel.Error);
            }
            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.TextBox.Text = ((UserCodeSettings)_settings).Sourcecode;
            }
            , null);            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
    
}
