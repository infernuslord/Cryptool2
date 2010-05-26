﻿/*                              
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
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cryptool.Plugins.QuadraticSieve
{
    /// <summary>
    /// This class wraps the msieve algorithm in version 1.42 which you can find at http://www.boo.net/~jasonp/qs.html
    /// It also extends the msieve functionality to multi threading 
    /// Many thanks to the author of msieve "jasonp_sf"
    /// 
    /// For further information on quadratic sieve or msieve please have a look at the above mentioned URL
    /// </summary>
    [Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Quadratic Sieve", "Sieving Primes", "QuadraticSieve/DetailedDescription/Description.xaml", "QuadraticSieve/iconqs.png")]
    class QuadraticSieve : DependencyObject, IThroughput
    {
        #region private variables

        private readonly string directoryName;
        private QuadraticSieveSettings settings = new QuadraticSieveSettings();
        private BigInteger inputNumber;
        private BigInteger[] outputFactors;
        private bool running;
        private Queue yieldqueue;
        private AutoResetEvent yieldEvent = new AutoResetEvent(false);
        private IntPtr obj = IntPtr.Zero;
        private volatile int threadcount = 0;
        private ArrayList conf_list;
        private bool userStopped = false;
        private FactorManager factorManager;
        private PeerToPeer peerToPeer = new PeerToPeer();
        private BigInteger sumSize = 0;

        private static Assembly msieveDLL = null;
        private static Type msieve = null;

        #endregion

        #region events

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event PluginProgressChangedEventHandler OnPluginProcessChanged;        

        #endregion

        #region public

        /// <summary>
        /// Constructor
        /// 
        /// constructs a new QuadraticSieve plugin
        /// </summary>
        public QuadraticSieve()
        {
            directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "msieve");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            QuickWatchPresentation = new QuadraticSievePresentation();
            
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.timeLeft.Text = "?";
                quadraticSieveQuickWatchPresentation.endTime.Text = "?";
                quadraticSieveQuickWatchPresentation.logging.Text = "Currently not sieving.";
            }
            , null);
        }                

        /// <summary>
        /// Getter / Setter for the settings of this plugin
        /// </summary>
        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (QuadraticSieveSettings)value; } 
        }           

        /// <summary>
        /// Called by the environment before executing this plugin
        /// </summary>
        public void PreExecution()
        {  
        }
        
        /// <summary>
        /// Called by the environment to execute this plugin
        /// </summary>
        public void Execute()
        {
            sumSize = 0;
            userStopped = false;

            if (InputNumber != 0)
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
                    quadraticSieveQuickWatchPresentation.factorList.Items.Clear();
                    quadraticSieveQuickWatchPresentation.factorInfo.Content = "Searching trivial factors!";                    
                }
                , null);   

                DateTime start_time = DateTime.Now;

                initMsieveDLL();
                factorManager = new FactorManager(msieve.GetMethod("getPrimeFactors"), msieve.GetMethod("getCompositeFactors"));
                factorManager.FactorsChanged += this.FactorsChanged;

                //Now factorize:                
                try
                {
                    string file = Path.Combine(directoryName, "" + InputNumber + ".dat");
                    if (settings.DeleteCache && File.Exists(file))
                        File.Delete(file);
                    MethodInfo start = msieve.GetMethod("start");
                    start.Invoke(null, new object[] { InputNumber.ToString(), file });
                    obj = IntPtr.Zero;
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Error using msieve. " + ex.Message, NotificationLevel.Error);
                    stopThreads();
                    return;
                }

                if (!userStopped)
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
                        quadraticSieveQuickWatchPresentation.factorInfo.Content = "";
                    }
                    , null);

                    Debug.Assert(factorManager.CalculateNumber() == InputNumber);
                    OutputFactors = factorManager.getPrimeFactors();
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
                        quadraticSieveQuickWatchPresentation.factorInfo.Content = "";
                    }
                    , null);
                }
                    
                ProgressChanged(1, 1);
                
            }
        }
        
        /// <summary>
        /// Called by the environment after execution
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Called by the environment to pause execution
        /// </summary>
        public void Pause()
        {
        }

        /// <summary>
        /// Called by the environment to stop execution
        /// </summary>
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

        /// <summary>
        /// Called by the environment to initialize this plugin
        /// </summary>
        public void Initialize()
        {
            settings.Initialize();
        }

        /// <summary>
        /// Called by the environment to dispose this plugin
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Getter / Setter for the input number which should be factorized
        /// </summary>
        [PropertyInfo(Direction.InputData, "Number input", "Enter the number you want to factorize", "", DisplayLevel.Beginner)]
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

        /// <summary>
        /// Getter / Setter for the factors calculated by msieve
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Factors output", "Your factors will be sent here", "", DisplayLevel.Beginner)]
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
        
        /// <summary>
        /// Called when a property of this plugin changes
        /// </summary>
        /// <param name="name">name</param>
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Getter / Setter for the presentation of this plugin
        /// </summary>
        public UserControl Presentation { get; private set; }

        /// <summary>
        /// Getter / Setter for the QuickWatchPresentation of this plugin
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        #endregion

        #region private

        /// <summary>
        /// calculate a String which shows the timespan
        /// 
        /// example
        /// 
        ///     4 days
        /// or
        ///     2 minutes
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Callback method to prepare sieving
        /// Called by msieve
        /// 
        /// </summary>
        /// <param name="conf">pointer to configuration</param>
        /// <param name="update">number of relations found</param>
        /// <param name="core_sieve_fcn">pointer to internal sieve function of msieve</param>
        private void prepareSieving(IntPtr conf, int update, IntPtr core_sieve_fcn, int max_relations)
        {
            int threads = Math.Min(settings.CoresUsed, Environment.ProcessorCount-1);
            MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
            this.obj = (IntPtr)getObjFromConf.Invoke(null, new object[] { conf });            
            yieldqueue = Queue.Synchronized(new Queue());
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
            for (int i = 0; i < threads+1; i++)
            {
                MethodInfo cloneSieveConf = msieve.GetMethod("cloneSieveConf");
                IntPtr clone = (IntPtr)cloneSieveConf.Invoke(null, new object[] { conf });                
                conf_list.Add(clone);
                WaitCallback worker = new WaitCallback(MSieveJob);
                ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, yieldqueue });
            }

            //manage the yields of the other threads:
            manageYields(conf, max_relations);  //this method returns as soon as there are enough relations found
            if (userStopped)
                return;

            //sieving is finished now, so give some informations and stop threads:
            GuiLogMessage("Data size: " + (sumSize / 1024) / 1024 + " MB!", NotificationLevel.Debug);
            ProgressChanged(0.9, 1.0);
            GuiLogMessage("Sieving finished", NotificationLevel.Info);
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.factorInfo.Content = "Found enough relations! Please wait...";
            }, null);
            stopThreads();
            if (yieldqueue != null)
                yieldqueue.Clear();
        }

        private void manageYields(IntPtr conf, int max_relations)
        {
            MethodInfo serializeYield = msieve.GetMethod("serializeYield");
            MethodInfo getNumRelations = msieve.GetMethod("getNumRelations");
            int num_relations = (int)getNumRelations.Invoke(null, new object[] { conf });
            int start_relations = num_relations;
            DateTime start_sieving_time = DateTime.Now;
            MethodInfo saveYield = msieve.GetMethod("saveYield");

            while (num_relations < max_relations)
            {
                ProgressChanged((double)num_relations / max_relations * 0.8 + 0.1, 1.0);
                
                yieldEvent.WaitOne();               //wait until queue is not empty
                if (userStopped)
                    return;
                while (yieldqueue.Count != 0)       //get all the results from the helper threads, and store them
                {
                    IntPtr yield = (IntPtr)yieldqueue.Dequeue();
                    byte[] serializedYield = (byte[])serializeYield.Invoke(null, new object[] { yield });                    

                    int compressedSize = peerToPeer.put(serializedYield);
                    sumSize += compressedSize;

                    saveYield.Invoke(null, new object[] { conf, yield });
                }
                num_relations = (int)getNumRelations.Invoke(null, new object[] { conf });
                showProgressPresentation(max_relations, num_relations, start_relations, start_sieving_time);
            }            
        }

        private void showProgressPresentation(int max_relations, int num_relations, int start_relations, DateTime start_sieving_time)
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

        /// <summary>
        /// This callback method is called by msieve. "list" is the trivial factor list (i.e. it consists of the factors that have been found without
        /// using the quadratic sieve algorithm).
        /// The method then factors all the factors that are still composite by using the quadratic sieve.
        /// </summary>
        private void getTrivialFactorlist(IntPtr list, IntPtr obj)
        {
            //add the trivial factors to the factor list:
            factorManager.AddFactors(list);

            MethodInfo msieve_run_core = msieve.GetMethod("msieve_run_core");

            //Now factorize as often as needed:
            while (!factorManager.OnlyPrimes())
            {
                //get one composite factor, which we want to sieve now:
                BigInteger compositeFactor = factorManager.GetCompositeFactor();
                quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    quadraticSieveQuickWatchPresentation.factorInfo.Content = "Now sieving first composite factor!";
                }, null);

                //now start quadratic sieve on it:                
                IntPtr resultList = (IntPtr)msieve_run_core.Invoke(null, new object[2] { obj, compositeFactor.ToString() });
                if (userStopped)
                    return;
                factorManager.ReplaceCompositeByFactors(compositeFactor, resultList);
            }
        }

        /// <summary>
        /// Helper Thread for msieve, which sieves for relations:
        /// </summary>
        /// <param name="param">params</param>
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
                    yieldEvent.Set();
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

        /// <summary>
        /// Stop all running threads
        /// </summary>
        private void stopThreads()
        {
            if (conf_list != null)
            {
                running = false;
                MethodInfo stop = msieve.GetMethod("stop");
                MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
                foreach (IntPtr conf in conf_list)
                    stop.Invoke(null, new object[] { getObjFromConf.Invoke(null, new object[] { conf }) });
                GuiLogMessage("Waiting for threads to stop!", NotificationLevel.Debug);
                while (threadcount > 0)
                {
                    Thread.Sleep(0);
                }
                GuiLogMessage("Threads stopped!", NotificationLevel.Debug);
                conf_list.Clear();
            }
        }    

        /// <summary>
        /// Change the progress of this plugin
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="max">max</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void FactorsChanged(List<BigInteger> primeFactors, List<BigInteger> compositeFactors)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.factorList.Items.Clear();

                foreach (BigInteger pf in primeFactors)         
                    quadraticSieveQuickWatchPresentation.factorList.Items.Add("Prime Factor: " + pf.ToString());            

                foreach (BigInteger cf in compositeFactors)
                    quadraticSieveQuickWatchPresentation.factorList.Items.Add("Composite Factor: " + cf.ToString());
            }, null);
        }

        /// <summary>
        /// Logs a message to the CrypTool gui
        /// </summary>
        /// <param name="p">p</param>
        /// <param name="notificationLevel">notificationLevel</param>
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// Getter / Setter for the QuickWatchPresentation
        /// </summary>
        private QuadraticSievePresentation quadraticSieveQuickWatchPresentation
        {
            get { return QuickWatchPresentation as QuadraticSievePresentation; }
        }

        /// <summary>
        /// dynamically loads the msieve dll file and sets the callbacks
        /// </summary>
        private void initMsieveDLL()
        {
            //Load msieve.dll (if necessary):
            if (msieve == null || msieveDLL == null)
            {
                string s = Directory.GetCurrentDirectory();
                string dllname;
                string relPath;
                if (IntPtr.Size == 4)
                {
                    dllname = "msieve.dll";
                    relPath = "x86";
                }
                else
                {
                    dllname = "msieve64.dll";
                    relPath = "x64";
                }
                msieveDLL = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\AppReferences\\"  + relPath + "\\" + dllname);
                msieve = msieveDLL.GetType("Msieve.msieve");
            }

            //init msieve with callbacks:
            MethodInfo initMsieve = msieve.GetMethod("initMsieve");
            Object callback_struct = Activator.CreateInstance(msieveDLL.GetType("Msieve.callback_struct"));            
            FieldInfo prepareSievingField = msieveDLL.GetType("Msieve.callback_struct").GetField("prepareSieving");
            FieldInfo getTrivialFactorlistField = msieveDLL.GetType("Msieve.callback_struct").GetField("getTrivialFactorlist");            
            Delegate prepareSievingDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.prepareSievingDelegate"), this, "prepareSieving");
            Delegate getTrivialFactorlistDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.getTrivialFactorlistDelegate"), this, "getTrivialFactorlist");            
            prepareSievingField.SetValue(callback_struct, prepareSievingDel);
            getTrivialFactorlistField.SetValue(callback_struct, getTrivialFactorlistDel);
            initMsieve.Invoke(null, new object[1] { callback_struct });
        }
        #endregion

    }
}
