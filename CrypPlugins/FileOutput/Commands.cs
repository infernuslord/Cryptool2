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
using FileOutput.Cmds;
using System.Windows;

namespace FileOutput
{
  public static class Commands
  {
    public static RoutedUICommand FindNextCommand = new RoutedUICommand
      ("FindNext", "FindNext", typeof(FileOutputPresentation));

    public static RoutedUICommand GotoCommand = new RoutedUICommand
      ("Goto", "Goto", typeof(FileOutputPresentation));

    public static RoutedUICommand AbortCommand = new RoutedUICommand
      ("Abort", "Abort", typeof(FileOutputPresentation));

    public static CutCommandRefresh CutCommand = new CutCommandRefresh(
      "Cut", "Cut", typeof(FileOutputPresentation));

    public static CopyCommandRefresh CopyCommand = new CopyCommandRefresh(
      "Copy", "Copy", typeof(FileOutputPresentation));

    static Commands()
    {
      FindNextCommand.InputGestures.Add(new KeyGesture(Key.F3, ModifierKeys.None));
      GotoCommand.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));


      CutCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
      CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));

      ApplicationCommands.Open.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
    }
  }
}
