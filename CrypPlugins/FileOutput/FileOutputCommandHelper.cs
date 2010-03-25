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
using FileOutput.Helper;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace FileOutput
{
  public partial class FileOutputPresentation
  {


    public void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private void PreviewNew_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      uscHexBox.NewFile();
      e.Handled = true;
    }

    public void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    { 
      e.CanExecute = true;
      e.Handled = true;
    }

    private void PreviewOpen_Executed(object sender, ExecutedRoutedEventArgs e)    
    {
      // SelectedFile = FileHelper.OpenFile();
      //fileOutputClass.SelectedFile = FileHelper.OpenFile();
      //if (fileOutputClass.SelectedFile != null)
      //{
      //  uscHexBox.OpenFile(fileOutputClass.SelectedFile);
      //}
      //e.Handled = true;
    }

    public void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (uscHexBox != null) e.CanExecute = uscHexBox.CanSave;
      else e.CanExecute = false;
    }

    private void PreviewSave_Executed(object sender, ExecutedRoutedEventArgs e)
    { uscHexBox.SaveFile(); }

    public void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      //e.CanExecute = true;
      if (uscHexBox != null)
      {
        e.CanExecute = uscHexBox.CanClose;
      }
      else e.CanExecute = false;

      // e.Handled = true;
    }

    private void PreviewClose_Executed(object sender, ExecutedRoutedEventArgs e)
    { 
      uscHexBox.CloseFile();
      e.Handled = true;
    }

    public void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (uscHexBox != null) e.CanExecute = uscHexBox.CanCut;
      else e.CanExecute = false;
    }

    private void PreviewCut_Executed(object sender, ExecutedRoutedEventArgs e)
    { uscHexBox.Cut(); }

    public void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (uscHexBox != null) e.CanExecute = uscHexBox.CanCopy;
      else e.CanExecute = false;
    }

    private void PreviewCopy_Executed(object sender, ExecutedRoutedEventArgs e)
    { uscHexBox.Copy(); }

    public void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (uscHexBox != null) e.CanExecute = uscHexBox.CanPaste;
      else e.CanExecute = false;
    }

    private void PreviewPaste_Executed(object sender, ExecutedRoutedEventArgs e)
    { uscHexBox.Paste(); }

    public void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute =
        textBoxSearch.Text != null && textBoxSearch.Text != string.Empty &&
        (uscHexBox != null && uscHexBox.CanFind);
      e.Handled = true;
    }

    private void PreviewFind_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      progressBar.Value = 1;
      progressBar.Maximum = 100;
      uscHexBox.Find(textBoxSearch.Text);
    }

    public void FindNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute =
        textBoxSearch.Text != null && textBoxSearch.Text != string.Empty &&
        (uscHexBox != null && uscHexBox.CanFind);
    }

    private void PreviewFindNext_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      uscHexBox.Find(textBoxSearch.Text);
    }
   
    public void Abort_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = uscHexBox != null && uscHexBox.CanCancel;
    }

    private void PreviewAbort_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      uscHexBox.AbortFind();
    }
  }

}
