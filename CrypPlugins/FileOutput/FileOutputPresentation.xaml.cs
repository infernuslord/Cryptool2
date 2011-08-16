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
using FileOutput.Delegates;
using FileOutput.WindowsFormsUserControl;
using System.Threading;
using FileOutput.Helper;
using Cryptool.PluginBase;
using System.IO;
using System.Windows.Threading;
using FileOutput;

namespace FileOutput
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  [Cryptool.PluginBase.Attributes.Localization("FileOutput.Properties.Resources")]
  public partial class FileOutputPresentation : UserControl
  {
    // public string SelectedFile { get; set; }
    public HexBoxContainer UscHexBoc { get { return uscHexBox; } }
    private FileOutputClass fileOutputClass;

    public FileOutputPresentation(FileOutputClass FileOutputClass)
    {
      InitializeComponent();
      this.fileOutputClass = FileOutputClass;
      this.fileOutputClass.settings.PropertyChanged += Settings_PropertyChanged;
      this.uscHexBox.OnSelectionChanged += uscHexBox_OnSelectionChanged;
      this.Width = double.NaN;
      this.Height = double.NaN;
      uscHexBox.OnFileOpened += uscHexBox_OnFileOpened;
      uscHexBox.OnFileClosed += uscHexBox_OnFileClosed;
      uscHexBox.OnStatusBarProgressbarValueChanged += uscHexBox_OnStatusBarProgressbarValueChanged;
      // buttonClose.IsEnabled = false;
      menuItemClose.IsEnabled = false;
      buttonSave.IsEnabled = false;
      textBoxNoFileMessage.Focus();
      //menuItemSave.IsEnabled = false;            
    }

    void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      try
      {
        if (e.PropertyName == "TargetFilename")
        {
          targetFile.Text = System.IO.Path.GetFileName(fileOutputClass.settings.TargetFilename);
          targetFile.ToolTip = fileOutputClass.settings.TargetFilename;
          if (File.Exists(fileOutputClass.settings.TargetFilename))
            OpenPresentationFile();
        }
      }
      catch {}
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
      //menuItemSave.IsEnabled = false;
      buttonSave.IsEnabled = false;
      if (!closedForExecution)
      {
        fileOutputClass.InputFile = string.Empty;
        fileOutputClass.UpdateQuickWatch();
      }
    }

    void uscHexBox_OnFileOpened(object sender, FileOpendedEventArgs e)
    {
      // buttonClose.IsEnabled = true;
      menuItemClose.IsEnabled = true;
      //menuItemSave.IsEnabled = true;
      buttonSave.IsEnabled = true;
      fileOutputClass.InputFile = e.Filename;

      if (!closedForExecution)
      {        
        fileOutputClass.UpdateQuickWatch();
      }
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

    public void ClosePresentationFile()
    {
      closedForExecution = true;

      if (uscHexBox.CanCancel) uscHexBox.AbortFind();

      uscHexBox.CloseFile();
    }
    
    public void OpenPresentationFile()
    {
      Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
      {
        closedForExecution = false;
        if (!uscHexBox.CanClose)
        {
          windowsFormsHost.Visibility = Visibility.Visible;
          textBoxNoFileMessage.Visibility = Visibility.Collapsed;

          //if (fileOutputClass.settings.TargetFilename != null)
          //  uscHexBox.OpenFile(fileOutputClass.settings.TargetFilename);
          if (fileOutputClass.InputFile != null)
            uscHexBox.OpenFile(fileOutputClass.InputFile);

          progressBar.Value = 0;
        }
      }, null);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      fileOutputClass.InputFile = string.Empty;
      if (uscHexBox.CanCancel) uscHexBox.AbortFind();
      uscHexBox.CloseFile();
      windowsFormsHost.Visibility = Visibility.Collapsed;
      textBoxNoFileMessage.Visibility = Visibility.Visible;
      progressBar.Value = 0;
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
      About about = new About();
      about.ShowDialog();
    }

    private void SelectTarget_Click(object sender, RoutedEventArgs e)
    {
      try
      {
      if (fileOutputClass != null && fileOutputClass.settings != null)
      {
        string target = FileHelper.SaveFile();
        if (target != null) ClosePresentationFile();
        fileOutputClass.settings.TargetFilename = target;
      }
      }
      catch{}
    }

  }
}
