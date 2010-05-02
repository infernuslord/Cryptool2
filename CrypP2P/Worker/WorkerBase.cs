/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Cryptool.P2P.Worker
{

    abstract class WorkerBase
    {
        protected BackgroundWorker backgroundWorker;

        public WorkerBase()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(PerformWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkComplete);
        }

        public void Start()
        {
            PrePerformWork();
            backgroundWorker.RunWorkerAsync();
        }

        protected abstract void WorkComplete(object sender, RunWorkerCompletedEventArgs e);

        protected abstract void PerformWork(object sender, DoWorkEventArgs e);

        protected abstract void PrePerformWork();
    }
}
