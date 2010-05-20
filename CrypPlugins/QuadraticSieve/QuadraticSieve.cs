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
using System.Numerics;

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
        private IntPtr obj = IntPtr.Zero;
        private volatile int threadcount = 0;
        private DateTime start_sieving_time;
        private bool sieving_started;
        private int start_relations;
        private ArrayList conf_list;
        private static Assembly msieveDLL;
        private static Type msieve;
        private bool userStopped = false;
        private IntPtr factorList;
        private ArrayList factors;

        #endregion

        #region events

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        #endregion

        /// <summary>
        /// Static constructor
        /// 
        /// loads the msieve / msieve64 dll
        /// </summary>
        static QuadraticSieve()
        {
            //Load msieve.dll:
            string s = Directory.GetCurrentDirectory();
            string dllname;
            if (IntPtr.Size == 4)
                dllname = "msieve.dll";
            else
                dllname = "msieve64.dll";
            msieveDLL = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\AppReferences\\" + dllname);
            msieve = msieveDLL.GetType("Msieve.msieve");
        }
        
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
                FieldInfo getTrivialFactorlistField = msieveDLL.GetType("Msieve.callback_struct").GetField("getTrivialFactorlist");
                Delegate showProgressDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.showProgressDelegate"), this, "showProgress");
                Delegate prepareSievingDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.prepareSievingDelegate"), this, "prepareSieving");
                Delegate getTrivialFactorlistDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.getTrivialFactorlistDelegate"), this, "getTrivialFactorlist");
                showProgressField.SetValue(callback_struct, showProgressDel);
                prepareSievingField.SetValue(callback_struct, prepareSievingDel);
                getTrivialFactorlistField.SetValue(callback_struct, getTrivialFactorlistDel);
                initMsieve.Invoke(null, new object[1] { callback_struct });

                //Now factorize:                
                try
                {
                    string file = Path.Combine(directoryName, "" + InputNumber + ".dat");
                    if (settings.DeleteCache && File.Exists(file))
                        File.Delete(file);
                    MethodInfo factorize = msieve.GetMethod("start");
                    factorize.Invoke(null, new object[] { InputNumber.ToString(), file });
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
                        outs[i] = BigInteger.Parse((string)factors[i]);
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
        /// Actualize the progress 
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="num_relations"></param>
        /// <param name="max_relations"></param>
        private void showProgress(IntPtr conf, int num_relations, int max_relations)
        {
            if (num_relations == -1)    //sieving finished
            {
                showFactorList();
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
                    showFactorList();

                    MethodInfo getCurrentFactor = msieve.GetMethod("getCurrentFactor");
                    //GuiLogMessage((String)(getCurrentFactor.Invoke(null, new object[] { conf })), NotificationLevel.Debug);

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

        private void showFactorList()
        {
            MethodInfo getPrimeFactors = msieve.GetMethod("getPrimeFactors");
            ArrayList fl = (ArrayList)(getPrimeFactors.Invoke(null, new object[] { factorList }));
            foreach (Object o in fl)
                GuiLogMessage("Prim Faktoren: " + (String)o, NotificationLevel.Debug);

            MethodInfo getCompositeFactors = msieve.GetMethod("getCompositeFactors");
            ArrayList fl2 = (ArrayList)(getCompositeFactors.Invoke(null, new object[] { factorList }));
            foreach (Object o in fl2)
                GuiLogMessage("Zusammengesetzte Faktoren: " + (String)o, NotificationLevel.Debug);
        }

        /// <summary>
        /// Callback method to prepare sieving
        /// Called by msieve
        /// 
        /// </summary>
        /// <param name="conf">pointer to configuration</param>
        /// <param name="update">number of relations found</param>
        /// <param name="core_sieve_fcn">pointer to internal sieve function of msieve</param>
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

        private void getTrivialFactorlist(IntPtr list, IntPtr obj)
        {
            factorList = list;
            MethodInfo getPrimeFactors = msieve.GetMethod("getPrimeFactors");
            factors = (ArrayList)(getPrimeFactors.Invoke(null, new object[] { factorList }));
            GuiLogMessage("TEST", NotificationLevel.Debug);

            //showFactorList();
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

                    //Just for testing the serialize mechanism:
                    MethodInfo serializeYield = msieve.GetMethod("serializeYield");
                    byte[] serializedYield = (byte[])serializeYield.Invoke(null, new object[] { yield });
                    /*MethodInfo deserializeYield = msieve.GetMethod("deserializeYield");
                    yield = (IntPtr)deserializeYield.Invoke(null, new object[] { serializedYield });*/

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

        #endregion

    }
}
