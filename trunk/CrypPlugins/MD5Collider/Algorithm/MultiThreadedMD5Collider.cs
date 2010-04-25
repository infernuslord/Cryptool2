using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Timers;

namespace Cryptool.Plugins.MD5Collider.Algorithm
{
    class MultiThreadedMD5Collider<T> : IMD5ColliderAlgorithm where T : IMD5ColliderAlgorithm, new()
    {
        private List<ColliderWorkerAdapter<T>> workers = new List<ColliderWorkerAdapter<T>>();
        private List<IMD5ColliderAlgorithm> colliders = new List<IMD5ColliderAlgorithm>();
        private IMD5ColliderAlgorithm successfulCollider;
        private Timer progressUpdateTimer;
        private int workerCount;

        private System.Threading.AutoResetEvent finishedEvent = new System.Threading.AutoResetEvent(false);

        public MultiThreadedMD5Collider()
        {
            workerCount = Math.Max(Environment.ProcessorCount, 1);

            for (int i = 0; i < workerCount; i++)
            {
                IMD5ColliderAlgorithm collider = new T();
                colliders.Add(collider);

                ColliderWorkerAdapter<T> colliderWorkerAdapter = new ColliderWorkerAdapter<T>(this, collider);
                workers.Add(colliderWorkerAdapter);
            }

            progressUpdateTimer = new Timer();
            progressUpdateTimer.Interval = 100;
            progressUpdateTimer.Elapsed += progressUpdateTimer_Tick;
        }

        public byte[] FirstCollidingData { get { return successfulCollider != null ? successfulCollider.FirstCollidingData : null; } }
        public byte[] SecondCollidingData { get { return successfulCollider != null ? successfulCollider.SecondCollidingData : null; } }

        public byte[] RandomSeed
        {
            set
            {
                if (value == null)
                {
                    foreach (IMD5ColliderAlgorithm collider in colliders)
                        collider.RandomSeed = null;
                    return;
                }


                byte[] randomSeedCopy = (byte[])value.Clone();

                foreach (IMD5ColliderAlgorithm collider in colliders)
                {
                    collider.RandomSeed = randomSeedCopy;

                    randomSeedCopy = (byte[])randomSeedCopy.Clone();

                    if (randomSeedCopy.Length == 0)
                        randomSeedCopy = new byte[1];

                    randomSeedCopy[0]++;
                }
            }
        }

        internal void SignalWorkIsFinished(IMD5ColliderAlgorithm successfulCollider)
        {
            this.successfulCollider = successfulCollider;
            updateProgress();
            Stop();

            finishedEvent.Set();
        }

        public byte[] IHV
        {
            set
            {
                foreach (IMD5ColliderAlgorithm collider in colliders)
                    collider.IHV = value;
            }
        }

        public int MatchProgressMax
        {
            get { return colliders.Max(c => c.MatchProgressMax); }
        }

        public int MatchProgress
        {
            get { return colliders.Max(c => c.MatchProgress); }
            set { }
        }

        public long CombinationsTried
        {
            get { return colliders.Sum(c => c.CombinationsTried); }
        }

        public TimeSpan ElapsedTime
        {
            get { return colliders.Max(c => c.ElapsedTime); }
        }

        public void FindCollision()
        {
            progressUpdateTimer.Start();

            finishedEvent.Reset();

            foreach (ColliderWorkerAdapter<T> worker in workers)
                worker.StartWork();

            finishedEvent.WaitOne();

            OnPropertyChanged("FirstCollidingData");
            OnPropertyChanged("SecondCollidingData");
        }

        public void Stop()
        {
            foreach (IMD5ColliderAlgorithm collider in colliders)
                collider.Stop();

            progressUpdateTimer.Stop();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }


        void progressUpdateTimer_Tick(object sender, EventArgs e)
        {
            updateProgress();
        }

        private void updateProgress()
        {
            OnPropertyChanged("MatchProgressMax");
            OnPropertyChanged("MatchProgress");
            OnPropertyChanged("CombinationsTried");
            OnPropertyChanged("ElapsedTime");
        }
    }
}
