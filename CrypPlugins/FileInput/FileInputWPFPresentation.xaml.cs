using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using FileInput;

namespace FileInput
{
    /// <summary>
    /// Interaction logic for FileInputWPFPresentation.xaml
    /// </summary>
    public partial class FileInputWPFPresentation : UserControl
    {
        public HexBox.HexBox hexBox;

        private FileInputClass  exp;

        public FileInputWPFPresentation(FileInputClass exp )
        {
            InitializeComponent();

            

            this.exp = exp;
            

            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox();
            this.hexBox.OnFileChanged += fileChanged; 
            MainMain.Children.Add(hexBox);

        }

        


        public void CloseFileToGetFileStreamForExecution()
        {
            try
            {
                hexBox.saveData(true, false);

                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, true);

                hexBox.IsEnabled = false;

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to reopen file: {0}", ex), NotificationLevel.Error);
            }

        }

        public void ReopenClosedFile()
        {
            //closedForExecution = false;

            if (File.Exists((exp.Settings as FileInputSettings).OpenFilename))
            {
                // tbFileClosedWhileRunning.Visibility = Visibility.Collapsed;
                // windowsFormsHost.Visibility = Visibility.Visible;
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, false);
               
            }
            hexBox.IsEnabled = true; try
            {

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to reopen file: {0}", ex), NotificationLevel.Error);
            }
        }


        internal void OpenFile(String fileName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate {
                try
                {
                    hexBox.openFile(fileName, false);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error trying to open file: {0}", ex), NotificationLevel.Error);
                }
            }, null);
        }

        internal void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(Object sender, EventArgs eventArgs)
        {
            (exp.Settings as FileInput.FileInputSettings).OpenFilename = hexBox.Pfad;
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            if (this.ActualWidth / 2.25 < this.ActualHeight)
                this.MainMain.RenderTransform = new ScaleTransform(this.ActualWidth / this.MainMain.ActualWidth, this.ActualWidth / this.MainMain.ActualWidth);
            else
                this.MainMain.RenderTransform = new ScaleTransform(this.ActualHeight / this.MainMain.ActualHeight, this.ActualHeight / this.MainMain.ActualHeight);
        }
        
        internal void CloseFile()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.closeFile(true);
            }, null);
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, exp, new GuiLogEventArgs(message, exp, logLevel));
        }
    }
}
