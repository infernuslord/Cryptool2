using System;
using System.Collections.Generic;
using System.IO;
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
using FileOutput;

namespace FileOutputWPF
{
    /// <summary>
    /// Interaction logic for FileOutputWPFPresentation.xaml
    /// </summary>
    public partial class FileOutputWPFPresentation : UserControl
    {
    public HexBox.HexBox hexBox;

    private FileOutputClass exp;

    public FileOutputWPFPresentation( FileOutputClass exp)
        {
            InitializeComponent();
            
            

            this.exp = exp;
            

            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox();
            this.hexBox.OnFileChanged += fileChanged; 
            MainMain.Children.Add(hexBox);
            this.hexBox.collapseControl(false);

        }

        


        public void CloseFileToGetFileStreamForExecution()
        {
            
            //hexBox.saveData(true,false);

            hexBox.closeFile(false);
            //hexBox.openFile((exp.Settings as Cryptool.Plugins.FileInputWPF.ExamplePluginCT2Settings).OpenFilename, true);

            //hexBox.IsEnabled = false;
            //hexBox.openFile(exp.settings.OpenFilename);
        }

        public void ReopenClosedFile()
        {
            //closedForExecution = false;

            if (File.Exists((exp.Settings as FileOutputSettings).TargetFilename))
            {
                // tbFileClosedWhileRunning.Visibility = Visibility.Collapsed;
                // windowsFormsHost.Visibility = Visibility.Visible;
                hexBox.closeFile(false);
                hexBox.openFile(
                        (exp.Settings as FileOutputSettings).TargetFilename, false);
                hexBox.collapseControl(false);
            }
           
        }


        internal void OpenFile(String fileName)
        {
           /* Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate {
                                                                                                hexBox.openFile(fileName,false);
            }, null);*/
        }

        internal void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(Object sender, EventArgs eventArgs)
        {
            /*exp.settings.OpenFilename = hexBox.Pfad;*/
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            //if (this.ActualWidth / 2.25 < this.ActualHeight)
            //    this.MainMain.RenderTransform = new ScaleTransform(this.ActualWidth / this.MainMain.ActualWidth, this.ActualWidth / this.MainMain.ActualWidth);
            //else
            //    this.MainMain.RenderTransform = new ScaleTransform(this.ActualHeight / this.MainMain.ActualHeight, this.ActualHeight / this.MainMain.ActualHeight);

            hexBox.Width = this.ActualWidth;
            hexBox.Height = this.ActualHeight;
        }
        
        internal void CloseFile()
        {
          /*  Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.closeFile();
            }, null);*/
        }
    }
}
