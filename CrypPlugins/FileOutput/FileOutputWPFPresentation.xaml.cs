using System;
using System.IO;
using System.Windows.Controls;
using FileOutput;

namespace FileOutputWPF
{
    /// <summary>
    /// Interaction logic for FileOutputWPFPresentation.xaml
    /// </summary>
    public partial class FileOutputWPFPresentation : UserControl
    {
        private readonly FileOutputClass exp;
        public HexBox.HexBox hexBox;

        public FileOutputWPFPresentation(FileOutputClass exp)
        {
            InitializeComponent();
            this.exp = exp;
            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox();
            hexBox.OnFileChanged += fileChanged;
            MainMain.Children.Add(hexBox);
            hexBox.collapseControl(false);
        }

        public void CloseFileToGetFileStreamForExecution()
        {          
            hexBox.closeFile(false);         
        }

        public void Clear()
        {
            hexBox.Clear();
        }

        public void ReopenClosedFile()
        {            
            if (File.Exists((exp.Settings as FileOutputSettings).TargetFilename))
            {             
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileOutputSettings).TargetFilename, false);
                hexBox.collapseControl(false);
            }
        }


        internal void OpenFile(String fileName)
        {           
        }

        internal void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(Object sender, EventArgs eventArgs)
        {           
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {            
            hexBox.Width = ActualWidth;
            hexBox.Height = ActualHeight;
        }

        internal void CloseFile()
        {           
        }
    }
}