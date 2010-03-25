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

namespace FileOutput.Delegates
{
  public delegate void FindProgressChanged(object sender, FindProgressChangedEventArgs e);
  public delegate void FindFinished(object sender);
  public delegate void SelectionChanged(object sender);
  public delegate void FileOpened(object sender, FileOpendedEventArgs e);
  public delegate void FileClosed(object sender);
  public delegate void ExceptionOccured(object sender, Exception e);
  public delegate void InformationOccured(object sender, Exception e);
}
