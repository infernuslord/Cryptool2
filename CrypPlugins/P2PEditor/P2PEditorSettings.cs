/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

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

using System.ComponentModel;
using System.Windows; 
using Cryptool.P2PEditor.GUI;
using Cryptool.PluginBase;
using System;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.P2PEditor
{
    internal class P2PEditorSettings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}