using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Cryptool.Plugins.MD5Collider.Algorithm
{
    class ColliderWorkerAdapter<T> where T : IMD5ColliderAlgorithm, new()
    {
        private IMD5ColliderAlgorithm wrappedCollider;

        private MultiThreadedMD5Collider<T> multiThreadedCollider;

        private BackgroundWorker worker;

        public bool IsStarted { get { return worker.IsBusy; } }

        public ColliderWorkerAdapter(MultiThreadedMD5Collider<T> multiThreadedCollider, IMD5ColliderAlgorithm wrappedCollider)
        {
            this.multiThreadedCollider = multiThreadedCollider;
            this.wrappedCollider = wrappedCollider;

            worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            wrappedCollider.FindCollision();
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            multiThreadedCollider.SignalWorkIsFinished(wrappedCollider);
        }

        public void StartWork()
        {
            worker.RunWorkerAsync();
        }
    }
}
