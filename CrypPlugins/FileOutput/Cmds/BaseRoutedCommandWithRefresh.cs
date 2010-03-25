/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Windows.Input;

namespace FileOutput.Cmds
{

  /// <summary>
  /// This bases class implements the EventHandler CanExecuteChanged, cause
  /// we need this to manual execute it after mouse was clicked in the area
  /// of the hexeditor which is located in a WindowsFormsHost. We can't directly
  /// use WPF Command-Binding feature when clickin in WindowsFormsHost.
  /// </summary>
  public class BaseRoutedCommandWithRefresh : RoutedUICommand, ICommand
  {
   private event EventHandler changed;

   event EventHandler ICommand.CanExecuteChanged
   {
      add 
      { 
         base.CanExecuteChanged += value;
         changed += value;
      }
      remove 
      { 
         base.CanExecuteChanged -= value;
         changed -= value;
      }
   }

   public void Refresh()
   {     
      if (changed != null) changed(this, EventArgs.Empty);
   }

   public BaseRoutedCommandWithRefresh(string text, string name, Type owner) : 
     base(text, name, owner)
   {            
   }

  }
}
