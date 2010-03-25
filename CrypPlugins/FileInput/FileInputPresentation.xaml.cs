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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileInput.Delegates;
using FileInput.WindowsFormsUserControl;
using System.Threading;
using FileInput.Helper;
using Cryptool.PluginBase;
using System.Windows.Threading;
using System.IO;

namespace FileInput
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class FileInputPresentation : UserControl
  {
    private bool ignoreNextFileOpenEvent = false;
    public HexBoxContainer UscHexBoc { get { return uscHexBox; } }
    private FileInputClass fileInputClass;

    public FileInputPresentation(FileInputClass FileInputClass)
    {
      InitializeComponent();
      this.fileInputClass = FileInputClass;
      this.uscHexBox.OnSelectionChanged += uscHexBox_OnSelectionChanged;
      this.Width = double.NaN;
      this.Height = double.NaN;
      uscHexBox.OnFileOpened += uscHexBox_OnFileOpened;
      uscHexBox.OnFileClosed += uscHexBox_OnFileClosed;
      uscHexBox.OnStatusBarProgressbarValueChanged += uscHexBox_OnStatusBarProgressbarValueChanged;
      menuItemClose.IsEnabled = false;
      buttonSave.IsEnabled = false;
      menuItemSave.IsEnabled = false;
    }

    void uscHexBox_OnStatusBarProgressbarValueChanged(object sender, PluginProgressEventArgs args)
    {
      progressBar.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
      {
        progressBar.Value = args.Value;
        progressBar.Maximum = args.Max;
      }, null);
    }

    void uscHexBox_OnFileClosed(object sender)
    {
      // buttonClose.IsEnabled = false;
      menuItemClose.IsEnabled = false;
      menuItemSave.IsEnabled = false;
      buttonSave.IsEnabled = false;
      if (!closedForExecution)
      {
        // fileInputClass.SelectedFile = string.Empty;
        (fileInputClass.Settings as FileInputSettings).OpenFilename = string.Empty;
        // fileInputClass.UpdateQuickWatch();
      }
    }

    void uscHexBox_OnFileOpened(object sender, FileOpendedEventArgs e)
    {
      // buttonClose.IsEnabled = true;
      menuItemClose.IsEnabled = true;
      menuItemSave.IsEnabled = true;
      buttonSave.IsEnabled = true;
      // fileInputClass.SelectedFile = e.Filename;      

      //if (!closedForExecution)
      //{        
      //  fileInputClass.UpdateQuickWatch();
      //}
    }

    void uscHexBox_OnSelectionChanged(object sender)
    {
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
      {
        Commands.CutCommand.Refresh();
        Commands.CopyCommand.Refresh();
      }, null);
    }

    private bool closedForExecution;
    public void CloseFileToGetFileStreamForExecution()
    {
      closedForExecution = true;

      if (uscHexBox.CanCancel) uscHexBox.AbortFind();

      if (uscHexBox.CanClose)
      {
        string fileName = uscHexBox.FileName;
        uscHexBox.CloseFile();
        uscHexBox.OpenFile(fileName, true);
        // windowsFormsHost.Visibility = Visibility.Collapsed;
        // tbFileClosedWhileRunning.Visibility = Visibility.Visible;
      }
    }

    public void ReopenClosedFile()
    {
      closedForExecution = false;

      if (File.Exists((fileInputClass.Settings as FileInputSettings).OpenFilename) && uscHexBox.CanClose)
      {
        // tbFileClosedWhileRunning.Visibility = Visibility.Collapsed;
        // windowsFormsHost.Visibility = Visibility.Visible;
        uscHexBox.CloseFile();
        uscHexBox.OpenFile((fileInputClass.Settings as FileInputSettings).OpenFilename, false);
      }
    }

    public void OpenFile(string FileName)
    {
      if (!ignoreNextFileOpenEvent)
      {
        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
        {
          uscHexBox.OpenFile(FileName, false);
        }, null);
        ignoreNextFileOpenEvent = false;
      }      
    }

    public void CloseFile()
    {
      uscHexBox.CloseFile();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      // fileInputClass.SelectedFile = string.Empty;
      (fileInputClass.Settings as FileInputSettings).OpenFilename = string.Empty;
      if (uscHexBox.CanCancel) uscHexBox.AbortFind();
      uscHexBox.CloseFile();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
      About about = new About();
      about.ShowDialog();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string file = FileHelper.OpenFile();
        if (File.Exists(file))
        {
          uscHexBox.OpenFile(file, false);
          ignoreNextFileOpenEvent = true;
          ((FileInputSettings)fileInputClass.Settings).OpenFilename = file;
        }
      }
      catch { }
    }

  }
}
