using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Printing;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Markup;
using WorkspaceManager.View.Container;
using WorkspaceManager.View.VisualComponents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;

namespace WorkspaceManager
{
    class WorkspaceManagerSettings : ISettings
    {
        #region ISettings Members
        private bool hasChanges = false;

        private WorkspaceManager WorkspaceManager { get; set; }

        public WorkspaceManagerSettings(WorkspaceManager manager)
        {
            this.Threads = "" + System.Environment.ProcessorCount;
            this.WorkspaceManager = manager;
        }

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        private String guiUpdateInterval = "100";
        [TaskPane("GuiUpdateInterval", "The interval the gui should be updated in miliseconds.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String GuiUpdateInterval
        {
            get
            {
                return guiUpdateInterval;
            }
            set
            {
                guiUpdateInterval = value;
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        private String sleepTime = "0";
        [TaskPane("SleepTime", "The time which the execution will sleep after executing a plugin.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String SleepTime
        {
            get
            {
                return sleepTime;
            }
            set
            {
                sleepTime = value;
                OnPropertyChanged("SleepTime");
            }
        }

        private String threads = "0";
        [TaskPane("Threads", "The amount of used threads for scheduling.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Threads
        {
            get
            {
                return threads;
            }
            set
            {
                threads = value;
                OnPropertyChanged("Threads");
            }
        }

        private int threadPriority = 4;
        [TaskPane("ThreadPriority", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new String[] { "AboveNormal", "BelowNormal", "Highest", "Lowest", "Normal" })]
        public int ThreadPriority
        {
            get
            {
                return threadPriority;
            }
            set
            {
                threadPriority = value;
                OnPropertyChanged("ThreadPriority");
            }
        }  

        private bool benchmarkPlugins = false;
        [TaskPane("BenchmarkPlugins", "Should the WorkspaceManager benchmark the amount of executed plugins per second?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool BenchmarkPlugins
        {
            get
            {
                return benchmarkPlugins;
            }
            set
            {
                benchmarkPlugins = value;
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        private bool synchronousEvents = false;
        [TaskPane("SynchronousEvents", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool SynchronousEvents
        {
            get
            {
                return synchronousEvents;
            }
            set
            {
                synchronousEvents = value;
                OnPropertyChanged("SynchronousEvents");
            }
        }

        private int logLevel = 0;
        [TaskPane("LogLevel", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Debug", "Info", "Warning", "Error"})]
        public int LogLevel
        {
            get
            {
                return logLevel;
            }
            set
            {
                logLevel = value;
                OnPropertyChanged("LogLevel");
            }
        }

        [TaskPane("Print Workspace", "Print the current Workspace", null, 1, false, DisplayLevel.Beginner, ControlType.Button, null)]
        public void PrintWorkspace()
        {
            try
            {
                const int factor = 4;
                ModifiedCanvas control = (ModifiedCanvas)((WorkSpaceEditorView)this.WorkspaceManager.Presentation).ViewBox.Content;
                PrintDialog dialog = new PrintDialog();
                dialog.PageRangeSelection = PageRangeSelection.AllPages;
                dialog.UserPageRangeEnabled = true;

                Nullable<Boolean> print = dialog.ShowDialog();
                if (print == true)
                {
                    WorkspaceManager.GuiLogMessage("Printing document \"" + WorkspaceManager.CurrentFilename + "\" now", NotificationLevel.Info);
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)control.ActualWidth*factor, (int)control.ActualHeight*factor, 96*factor, 96*factor, PixelFormats.Pbgra32);
                    bmp.Render(control);

                    PrintCapabilities capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
                    System.Windows.Size pageSize = new System.Windows.Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                    System.Windows.Size visibleSize = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);

                    FixedDocument fixedDoc = new FixedDocument();
                    control.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    control.Arrange(new Rect(new System.Windows.Point(0, 0), control.DesiredSize));
                    System.Windows.Size size = control.DesiredSize;
                   
                    double xOffset = 0;
                    double yOffset = 0;
                    while (xOffset < size.Width)
                    {
                        yOffset = 0;
                        while (yOffset < size.Height)
                        {                            
                            PageContent pageContent = new PageContent();
                            FixedPage page = new FixedPage();
                            ((IAddChild)pageContent).AddChild(page);
                            fixedDoc.Pages.Add(pageContent);
                            page.Width = pageSize.Width;
                            page.Height = pageSize.Height;
                            System.Windows.Controls.Image croppedImage = new System.Windows.Controls.Image();
                            int width = xOffset + visibleSize.Width > size.Width ? (int)(size.Width - xOffset) : (int)visibleSize.Width;
                            int height = yOffset + visibleSize.Height > size.Height ? (int)(size.Height - yOffset) : (int)visibleSize.Height;                            

                            CroppedBitmap cb = new CroppedBitmap(bmp, new Int32Rect((int)xOffset * factor, (int)yOffset * factor, width * factor, height * factor));
                            croppedImage.Source = cb;
                            croppedImage.Width = width;
                            croppedImage.Height = height;
                            page.Children.Add(croppedImage);
                            yOffset += visibleSize.Height;
                        }
                        xOffset += visibleSize.Width;
                    }                    
                    dialog.PrintDocument(fixedDoc.DocumentPaginator, "WorkspaceManager_" + WorkspaceManager.CurrentFilename);
                    WorkspaceManager.GuiLogMessage("Printed \"" + fixedDoc.DocumentPaginator.PageCount + "\" pages of document \"" + WorkspaceManager.CurrentFilename + "\"", NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                WorkspaceManager.GuiLogMessage("Exception:" + ex.Message, NotificationLevel.Error);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
