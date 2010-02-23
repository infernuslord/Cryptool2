using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Timers;

namespace Cryptool.Plugins.MD5Collider.Algorithm
{
    abstract class MD5ColliderBase : IMD5ColliderAlgorithm
    {
        public byte[] FirstCollidingData { get; protected set; }
        public byte[] SecondCollidingData { get; protected set; }
        public byte[] RandomSeed { get; set; }

        private String _status;
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }

        public MD5ColliderBase()
        {
            MatchProgressMax = 1;
            MatchProgress = 0;

            progressUpdateTimer.Interval = 100;
            progressUpdateTimer.Elapsed += progressUpdateTimer_Tick;

            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        private int _matchProgressMax;
        public int MatchProgressMax
        {
            get { return _matchProgressMax; }
            set { int old = _matchProgressMax; _matchProgressMax = value; }
        }

        private int _matchProgress;
        public int MatchProgress
        {
            get { return _matchProgress; }
            set { int old = _matchProgress; _matchProgress = value; }
        }

        private Timer progressUpdateTimer = new Timer();

        void progressUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        abstract protected void PerformFindCollision();

        abstract protected void PerformStop();

        private DateTime startTime;

        private Timer timer = new Timer();
        private void StartTimer()
        {
            startTime = DateTime.Now;
            ElapsedTime = TimeSpan.Zero;

            timer.Start();
            progressUpdateTimer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            ElapsedTime = DateTime.Now - startTime;
        }

        private void StopTimer()
        {
            timer.Stop();
            progressUpdateTimer.Stop();

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            OnPropertyChanged("MatchProgressMax");
            OnPropertyChanged("MatchProgress");
        }

        public void FindCollision()
        {
            StartTimer();
            PerformFindCollision();
            StopTimer();
        }

        public void Stop()
        {
            PerformStop();
            StopTimer();
        }

        #region IMD5ColliderAlgorithm Member

        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; OnPropertyChanged("ElapsedTime"); }
        }

        #endregion
    }
}
