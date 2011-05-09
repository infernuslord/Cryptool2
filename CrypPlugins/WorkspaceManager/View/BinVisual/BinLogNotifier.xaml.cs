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
using System.Globalization;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinLogNotifier.xaml
    /// </summary>
    public partial class BinLogNotifier : UserControl, INotifyPropertyChanged
    {
        #region Events
        public EventHandler<RequestLogDisplayArgs> RequestLogDisplay;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region private vars
        private Queue<Log> logStack = new Queue<Log>();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) }; 
        #endregion

        #region DependencyProperties

        private Log currentLog;
        public Log CurrentLog
        {
            get { return currentLog; }
            set { currentLog = value; OnPropertyChanged("CurrentLog"); }
        } 
        #endregion

        #region Constructor
        public BinLogNotifier(ObservableCollection<Log> Logs)
        {
            InitializeComponent();
            timer.Tick += new EventHandler(TickHandler);
            timer.Start();
            Logs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LogCollectionChangedHandler);
        } 
        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            this.DataContext = this;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #region Event Handler
        void TickHandler(object sender, EventArgs e)
        {
            if (logStack.Count == 0)
            {
                timer.Stop();
                CurrentLog = null;
                return;
            }

            CurrentLog = logStack.Dequeue();
        }

        void LogCollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                logStack.Enqueue((Log)e.NewItems[0]);
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

    #region Converter
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    } 
    #endregion

    #region EventArgs

    public class RequestLogDisplayArgs : EventArgs
    {
        public Log Messages { get; set; }
    }

    #endregion
}
