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
        private IntPtr obj = IntPtr.Zero;
        private volatile int threadcount = 0;
        private DateTime start_sieving_time;        
        private ArrayList conf_list;

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
                if (InputNumber.ToString().Length >= 275)
                {
                    GuiLogMessage("Input too big.", NotificationLevel.Error);
                    return;
                }

                DateTime start_time = DateTime.Now;

                callback_struct callbacks = new callback_struct();
                callbacks.showProgress = showProgress;
                callbacks.prepareSieving = prepareSieving;
                msieve.initMsieve(callbacks);

                ArrayList factors;
                try
                {
                    string file = Path.Combine(directoryName, "" + InputNumber + ".dat");
                    if (settings.DeleteCache && File.Exists(file))
                        File.Delete(file);
                    factors = msieve.factorize(InputNumber.ToString(), file);
                    obj = IntPtr.Zero;
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Error using msieve. " + ex.Message, NotificationLevel.Error);
                    stopThreads();
                    return;
                }

                GuiLogMessage("Factorization finished in " + (DateTime.Now - start_time) + "!", NotificationLevel.Info);

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

        private String showTimeSpan(TimeSpan ts)
        {
            String res = "";
            if (ts.Days != 0)
                res = ts.Days + " days ";
            if (ts.Hours != 0 || res.Length != 0)
                res += ts.Hours + " hours ";
            if (ts.Minutes != 0)
                res += ts.Minutes + " minutes";
            if (res.Length == 0)
                res += ts.Seconds + " seconds";
            return res;
        }

        private void showProgress(IntPtr conf, int num_relations, int max_relations)
        {
            if (num_relations == -1)    //sieving finished
            {
                ProgressChanged(0.9, 1.0);
                GuiLogMessage("Sieving finished", NotificationLevel.Info);
                stopThreads();
                yieldqueue.Clear();
            }
            else
            {
                ProgressChanged((double)num_relations / max_relations * 0.8 + 0.1, 1.0);                
                TimeSpan diff = DateTime.Now - start_sieving_time;
                double msleft = (diff.TotalMilliseconds / num_relations) * (max_relations - num_relations);                                
                if (msleft > 0)
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)msleft);
                    GuiLogMessage("" + num_relations + " of " + max_relations + " relations! About " + showTimeSpan(ts) + " left.", NotificationLevel.Debug);
                }

                while (yieldqueue.Count != 0)       //get all the results from the helper threads, and store them
                {
                    msieve.saveYield(conf, (IntPtr)yieldqueue.Dequeue());
                }
            }
        }

        private void prepareSieving (IntPtr conf, int update, IntPtr core_sieve_fcn)
        {
            int threads = Math.Min(settings.CoresUsed, Environment.ProcessorCount-1);
            this.obj = msieve.getObjFromConf(conf) ;
            yieldqueue = Queue.Synchronized(new Queue());
            conf_list = new ArrayList();
            GuiLogMessage("Starting sieving using " + threads + " cores!", NotificationLevel.Info);
            ProgressChanged(0.1, 1.0);
            start_sieving_time = DateTime.Now;            

            running = true;
            //start helper threads:
            for (int i = 0; i < threads; i++)
            {
                IntPtr clone = msieve.cloneSieveConf(conf);
                conf_list.Add(clone);
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
                catch (Exception ex)
                {
                    GuiLogMessage("Error using msieve." + ex.Message, NotificationLevel.Error);
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
            if (obj != IntPtr.Zero)
            {
                stopThreads();
                msieve.stop(obj);
            }
        }

        private void stopThreads()
        {
            if (conf_list != null)
            {
                running = false;
                foreach (IntPtr conf in conf_list)
                    msieve.stop(msieve.getObjFromConf(conf));
                GuiLogMessage("Waiting for threads to stop!", NotificationLevel.Debug);
                while (threadcount > 0)
                {
                    Thread.Sleep(0);
                }
                GuiLogMessage("Threads stopped!", NotificationLevel.Debug);
                conf_list.Clear();
            }
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
