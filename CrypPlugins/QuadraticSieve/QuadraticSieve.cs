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
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.Reflection;

namespace Cryptool.Plugins.QuadraticSieve
{
    [Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Quadratic Sieve", "Sieving Primes", "", "QuadraticSieve/iconqs.png")]
    class QuadraticSieve : DependencyObject, IThroughput
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
        private bool sieving_started;
        private int start_relations;
        private ArrayList conf_list;
        private static Assembly msieveDLL;
        private static Type msieve;
        private bool userStopped = false;

        static QuadraticSieve()
        {
            directoryName = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TempDirectoryName), "msieve");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            //Load msieve.dll:
            string s = Directory.GetCurrentDirectory();
            string dllname;
            if (IntPtr.Size == 4)
                dllname = "msieve.dll";
            else
                dllname = "msieve64.dll";
            msieveDLL = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\CrypPlugins\\" + dllname);
            msieve = msieveDLL.GetType("Msieve.msieve");
        }

        public QuadraticSieve()
        {
            QuickWatchPresentation = new QuadraticSievePresentation();
            
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.timeLeft.Text = "?";
                quadraticSieveQuickWatchPresentation.endTime.Text = "?";
                quadraticSieveQuickWatchPresentation.logging.Text = "Currently not sieving.";
            }
            , null);
        }                

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (QuadraticSieveSettings)value; } 
        }           

        public void PreExecution()
        {  
        }

        public void Execute()
        {
            userStopped = false;

            if (InputNumber is Object)
            {
                if (InputNumber.ToString().Length >= 275)
                {
                    GuiLogMessage("Input too big.", NotificationLevel.Error);
                    return;
                }

                String timeLeft_message = "?";
                String endtime_message = "?";
                String logging_message = "Starting quadratic sieve, please wait!";

                GuiLogMessage(logging_message, NotificationLevel.Info);
                quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                    quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
                    quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                }
                , null);   

                DateTime start_time = DateTime.Now;

                //init msieve with callbacks:
                MethodInfo initMsieve = msieve.GetMethod("initMsieve");
                Object callback_struct = Activator.CreateInstance(msieveDLL.GetType("Msieve.callback_struct"));
                FieldInfo showProgressField = msieveDLL.GetType("Msieve.callback_struct").GetField("showProgress");
                FieldInfo prepareSievingField = msieveDLL.GetType("Msieve.callback_struct").GetField("prepareSieving");
                Delegate showProgressDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.showProgressDelegate"), this, "showProgress");
                Delegate prepareSievingDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.prepareSievingDelegate"), this, "prepareSieving");
                showProgressField.SetValue(callback_struct, showProgressDel);
                prepareSievingField.SetValue(callback_struct, prepareSievingDel);
                initMsieve.Invoke(null, new object[1] { callback_struct });

                //Now factorize:
                ArrayList factors;
                try
                {
                    string file = Path.Combine(directoryName, "" + InputNumber + ".dat");
                    if (settings.DeleteCache && File.Exists(file))
                        File.Delete(file);
                    MethodInfo factorize = msieve.GetMethod("factorize");
                    factors = (ArrayList)factorize.Invoke(null, new object[] { InputNumber.ToString(), file });                    
                    obj = IntPtr.Zero;
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Error using msieve. " + ex.Message, NotificationLevel.Error);
                    stopThreads();
                    return;
                }

                if (factors != null && !userStopped)
                {
                    timeLeft_message = "0 seconds left";
                    endtime_message = "" + (DateTime.Now);
                    logging_message = "Sieving finished in " + (DateTime.Now - start_time) + "!";

                    GuiLogMessage(logging_message, NotificationLevel.Info);
                    quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                        quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
                        quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                    }
                    , null);
                    BigInteger[] outs = new BigInteger[factors.Count];
                    for (int i = 0; i < factors.Count; i++)
                    {
                        outs[i] = new BigInteger((string)factors[i], 10);
                    }
                    OutputFactors = outs;
                }
                else
                {
                    timeLeft_message = "0 sec left";
                    endtime_message = "Stopped";
                    logging_message = "Stopped by user!";

                    GuiLogMessage(logging_message, NotificationLevel.Info);
                    quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                        quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
                        quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                    }
                    , null);
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
                if (sieving_started)
                {
                    TimeSpan diff = DateTime.Now - start_sieving_time;
                    double msleft = (diff.TotalMilliseconds / (num_relations - start_relations)) * (max_relations - num_relations);
                    if (msleft > 0)
                    {
                        TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)msleft);
                        String logging_message = "Found " + num_relations + " of " + max_relations + " relations!";
                        String timeLeft_message = showTimeSpan(ts) + " left";
                        String endtime_message = "" + DateTime.Now.AddMilliseconds((long)msleft);
                        
                        GuiLogMessage(logging_message + " " + timeLeft_message + ".", NotificationLevel.Debug);
                        quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                            quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                            quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
                        }
                        , null);

                    }
                }

                if (!sieving_started)
                {
                    start_relations = num_relations;
                    start_sieving_time = DateTime.Now;
                    sieving_started = true;
                }

                ProgressChanged((double)num_relations / max_relations * 0.8 + 0.1, 1.0);                

                while (yieldqueue.Count != 0)       //get all the results from the helper threads, and store them
                {
                    MethodInfo saveYield = msieve.GetMethod("saveYield");
                    saveYield.Invoke(null, new object[] { conf, (IntPtr)yieldqueue.Dequeue() });                    
                }                
            }
        }

        private void prepareSieving (IntPtr conf, int update, IntPtr core_sieve_fcn)
        {
            int threads = Math.Min(settings.CoresUsed, Environment.ProcessorCount-1);
            MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
            this.obj = (IntPtr)getObjFromConf.Invoke(null, new object[] { conf });            
            yieldqueue = Queue.Synchronized(new Queue());
            sieving_started = false;
            conf_list = new ArrayList();

            String message = "Start sieving using " + (threads + 1) + " cores!";
            GuiLogMessage(message, NotificationLevel.Info);
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.logging.Text = message;
            }
            , null);          

            ProgressChanged(0.1, 1.0);

            running = true;
            //start helper threads:
            for (int i = 0; i < threads; i++)
            {
                MethodInfo cloneSieveConf = msieve.GetMethod("cloneSieveConf");
                IntPtr clone = (IntPtr)cloneSieveConf.Invoke(null, new object[] { conf });                
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
                    MethodInfo collectRelations = msieve.GetMethod("collectRelations");
                    collectRelations.Invoke(null, new object[] { clone, update, core_sieve_fcn });
                    MethodInfo getYield = msieve.GetMethod("getYield");
                    IntPtr yield = (IntPtr)getYield.Invoke(null, new object[] { clone });                    
                    yieldqueue.Enqueue(yield);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Error using msieve." + ex.Message, NotificationLevel.Error);
                    threadcount = 0;
                    return;
                }                
            }
            MethodInfo freeSieveConf = msieve.GetMethod("freeSieveConf");
            freeSieveConf.Invoke(null, new object[] { clone });            
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
            this.userStopped = true;
            if (obj != IntPtr.Zero)
            {
                stopThreads();
                MethodInfo stop = msieve.GetMethod("stop");
                stop.Invoke(null, new object[] { obj });
            }
            
        }

        private void stopThreads()
        {
            if (conf_list != null)
            {
                running = false;
                MethodInfo stop = msieve.GetMethod("stop");
                MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
                foreach (IntPtr conf in conf_list)
                    stop.Invoke(null, new object[] { getObjFromConf.Invoke(null, new object[] {conf}) });
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

        #region IPlugin Members

        private QuadraticSievePresentation quadraticSieveQuickWatchPresentation
        {
            get { return QuickWatchPresentation as QuadraticSievePresentation; }
        }

        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        #endregion
    }
}
