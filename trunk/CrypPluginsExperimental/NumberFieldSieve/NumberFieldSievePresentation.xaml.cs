using System;
using System.Collections;
using System.Collections.Generic;
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

namespace NumberFieldSieve
{
    /// <summary>
    /// Interaction logic for NumberFieldSievePresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NumberFieldSieve.Properties.Resources")]
    public partial class NumberFieldSievePresentation : UserControl
    {
        private readonly Queue _appendTextQueue;
        private readonly Timer _textTimer;
        private bool _timerOn = false;
        private const int TextTimerPeriod = 1000;

        public NumberFieldSievePresentation()
        {
            _appendTextQueue = Queue.Synchronized(new Queue());
            _textTimer = new Timer(TextTimerCallback, null, Timeout.Infinite, TextTimerPeriod);

            InitializeComponent();
        }

        private void TextTimerCallback(object state)
        {
            if (_appendTextQueue.Count > 0)
            {
                lock (_appendTextQueue)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        do
                        {
                            var el = _appendTextQueue.Dequeue();
                            TextOut.AppendText((string)el);
                        } while (_appendTextQueue.Count > 0);
                        TextOut.ScrollToEnd();
                    }, null);
                }
            }
            else
            {
                TimerOut();
            }
        }

        public void Append(string text)
        {
            _appendTextQueue.Enqueue(text);
            TimerOn();
        }
        
        private void TimerOn()
        {
            if (!_timerOn)
            {
                lock (_textTimer)
                {
                    if (!_timerOn)
                    {
                        _timerOn = true;
                        _textTimer.Change(0, TextTimerPeriod);
                    }
                }
            }
        }

        private void TimerOut()
        {
            if (_timerOn)
            {
                lock (_textTimer)
                {
                    if (_timerOn)
                    {
                        _timerOn = false;
                        _textTimer.Change(Timeout.Infinite, TextTimerPeriod);
                    }
                }
            }
        }

        public void Clear()
        {
            _appendTextQueue.Clear();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                TextOut.Clear();
            }, null);
        }
    }
}
