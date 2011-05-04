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
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinLogNotifier.xaml
    /// </summary>
    public partial class BinLogNotifier : UserControl
    {
        #region Events
        public EventHandler<RequestLogDisplayArgs> RequestLogDisplay; 
        #endregion

        #region private vars
        private Stack<Log> logStack = new Stack<Log>();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) }; 
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty CurrentLogProperty = DependencyProperty.Register("CurrentLog",
    typeof(Log), typeof(BinLogVisual), new FrameworkPropertyMetadata(null));

        public Log CurrentLog
        {
            get { return (Log)base.GetValue(CurrentLogProperty); }
            set { base.SetValue(CurrentLogProperty, value); }
        } 
        #endregion

        #region Constructor
        public BinLogNotifier(ObservableCollection<Log> Logs)
        {
            InitializeComponent();
            timer.Tick += new EventHandler(TickHandler);
            Logs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LogCollectionChangedHandler);
        } 
        #endregion

        #region Event Handler
        void TickHandler(object sender, EventArgs e)
        {
            if (logStack.Count == 0)
            {
                timer.Stop();
            }

            CurrentLog = logStack.Pop();
        }

        void LogCollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                logStack.Push((Log)e.NewItems[0]);
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            }

#warning if something gets deleted this means all messages has been deleted
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                logStack.Clear();
            }
        } 
        #endregion
    }

    #region EventArgs

    public class RequestLogDisplayArgs : EventArgs
    {
        public Log Messages { get; set; }
    }

    #endregion
}
