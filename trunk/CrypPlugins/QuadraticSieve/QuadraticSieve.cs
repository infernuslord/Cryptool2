/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Collections;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using QuadraticSieve;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Threading;
using Msieve;
using System.IO;

namespace Cryptool.Plugins.QuadraticSieve
{
    [Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Quadratic Sieve", "Sieving Primes", "", "QuadraticSieve/iconqs.png")]
    class QuadraticSieve : IThroughput
    {
        #region IPlugin Members

        private const string TempDirectoryName = "CrypTool Temp Files";
        private static readonly string directoryName;
        private QuadraticSieveSettings settings = new QuadraticSieveSettings();
        private BigInteger inputNumber;
        private BigInteger[] outputFactors;
        private bool running;
        private Queue yieldqueue;
        private IntPtr conf;
        private volatile int threadcount = 0;

        static QuadraticSieve()
        {
            directoryName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TempDirectoryName), "msieve");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (QuadraticSieveSettings)value; } 
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {            
        }

        public void Execute()
        {
            if (InputNumber is Object)
            {
                callback_struct callbacks = new callback_struct();
                callbacks.showProgress = showProgress;
                callbacks.prepareSieving = prepareSieving;
                msieve.initMsieve(callbacks);

                ArrayList factors;
                try
                {
                     factors = msieve.factorize(InputNumber.ToString(), Path.Combine(directoryName, "msieve.dat"));
                }
                catch (Exception)
                {
                    GuiLogMessage("Error using msieve.", NotificationLevel.Error);
                    running = false;
                    return;
                }

                if (factors != null)
                {
                    BigInteger[] outs = new BigInteger[factors.Count];
                    for (int i = 0; i < factors.Count; i++)
                    {
                        outs[i] = new BigInteger((string)factors[i], 10);
                    }
                    OutputFactors = outs;
                }
                    
                ProgressChanged(1, 1);
            }
        }

        private void showProgress(int num_relations, int max_relations)
        {
            if (num_relations == -1)    //sieving finished
            {
                ProgressChanged(0.9, 1.0);
                GuiLogMessage("Sieving finished", NotificationLevel.Info);
                running = false;
                if (threadcount > 0)
                {                    
                    GuiLogMessage("Waiting for threads to stop!", NotificationLevel.Debug);
                    while (threadcount > 0)
                    {
                        Thread.Sleep(0);
                    }
                    GuiLogMessage("Threads stopped!", NotificationLevel.Debug);
                }
                yieldqueue.Clear();
            }
            else
            {
                ProgressChanged((double)num_relations / max_relations * 0.8 + 0.1, 1.0);
                GuiLogMessage("" + num_relations + " of " + max_relations + " relations!", NotificationLevel.Debug);
                
                while (yieldqueue.Count != 0)       //get all the results from the helper threads, and store them
                {
                    msieve.saveYield(conf, (IntPtr)yieldqueue.Dequeue());
                }
            }
        }

        private void prepareSieving (IntPtr conf, int update, IntPtr core_sieve_fcn)
        {
            this.conf = conf;
            yieldqueue = Queue.Synchronized(new Queue());
            GuiLogMessage("Start sieving", NotificationLevel.Info);
            ProgressChanged(0.1, 1.0);

            running = true;
            //start helper threads:
            for (int i = 1; i < Math.Min(settings.CoresUsed, Environment.ProcessorCount); i++)
            {
                IntPtr clone = msieve.cloneSieveConf(conf);
                WaitCallback worker = new WaitCallback(MSieveJob);
                ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, yieldqueue });
            }
        }

        //Helper Thread for msieve, which sieves for relations:
        private void MSieveJob(object param)
        {
            threadcount++;
            object[] parameters = (object[])param;
            IntPtr clone = (IntPtr)parameters[0];
            int update = (int)parameters[1];
            IntPtr core_sieve_fcn = (IntPtr)parameters[2];
            Queue yieldqueue = (Queue)parameters[3];

            while (running)
            {
                try
                {
                    msieve.collectRelations(clone, update, core_sieve_fcn);
                    IntPtr yield = msieve.getYield(clone);
                    yieldqueue.Enqueue(yield);
                }
                catch (Exception)
                {
                    GuiLogMessage("Error using msieve.", NotificationLevel.Error);
                    threadcount = 0;
                    return;
                }                
            }

            msieve.freeSieveConf(clone);
            threadcount--;
        }

        public void PostExecution()
        {           
        }

        public void Pause()
        {            
        }

        public void Stop()
        {
            try
            {
                msieve.stop();
            }
            catch (Exception)
            {
            }
            running = false;
        }

        public void Initialize()
        {            
        }

        public void Dispose()
        {
        }

        #endregion

        #region QuadraticSieveInOut

        [PropertyInfo(Direction.InputData, "Number Input", "Put the number you want to factorize here", "", DisplayLevel.Beginner)]
        public BigInteger InputNumber
        {
            get
            {
                return inputNumber;
            }
            set
            {
                this.inputNumber = value;
                OnPropertyChanged("InputNumber");
            }
        }


        [PropertyInfo(Direction.OutputData, "Factors Output", "Your factors will be sent here", "", DisplayLevel.Beginner)]
        public BigInteger[] OutputFactors
        {
            get
            {
                return outputFactors;
            }
            set
            {
                this.outputFactors = value;
                OnPropertyChanged("OutputFactors");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members



        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
