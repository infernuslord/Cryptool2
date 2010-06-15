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
using Cryptool.P2P;

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
        private QuadraticSieveSettings settings;
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
        private PeerToPeer peerToPeer;
        private bool usePeer2Peer;
        private bool useGnuplot = false;
        private StreamWriter gnuplotFile;
        private double[] relationsPerMS;

        private static Assembly msieveDLL = null;
        private static Type msieve = null;
        private static bool alreadyInUse = false;
        private static Mutex alreadyInUseMutex = new Mutex();

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
            Settings = new QuadraticSieveSettings();

            directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "msieve");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            QuickWatchPresentation = new QuadraticSievePresentation();

            peerToPeer = new PeerToPeer(quadraticSieveQuickWatchPresentation, yieldEvent);
            peerToPeer.P2PWarning += new PeerToPeer.P2PWarningHandler(peerToPeer_P2PWarning);
            
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.peer2peer.Visibility = settings.UsePeer2Peer ? Visibility.Visible : Visibility.Collapsed;
                quadraticSieveQuickWatchPresentation.Redraw();
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
            set
            {
                this.settings = (QuadraticSieveSettings)value;
                this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            }
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UsePeer2Peer")
            {
                if (quadraticSieveQuickWatchPresentation != null)
                {
                    quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        quadraticSieveQuickWatchPresentation.peer2peer.Visibility = settings.UsePeer2Peer ? Visibility.Visible : Visibility.Collapsed;
                        quadraticSieveQuickWatchPresentation.Redraw();
                    }, null);
                }
            }
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
            if (checkInUse())
                return;
            try
            {
                usePeer2Peer = settings.UsePeer2Peer;
                if (usePeer2Peer && !P2PManager.IsConnected)
                {
                    GuiLogMessage("No connection to Peer2Peer network. Sieving locally now!", NotificationLevel.Warning);
                    usePeer2Peer = false;
                }
                if (usePeer2Peer && settings.Channel.Trim() == "")
                {
                    GuiLogMessage("No channel for Peer2Peer network specified. Sieving locally now!", NotificationLevel.Warning);
                    usePeer2Peer = false;
                }
                if (usePeer2Peer)
                {
                    peerToPeer.SetChannel(settings.Channel);
                    peerToPeer.SetNumber(InputNumber);
                }

                if (useGnuplot)
                    gnuplotFile = new StreamWriter(Path.Combine(directoryName, "gnuplot.dat"), false);

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
                        quadraticSieveQuickWatchPresentation.ProgressYields.Clear();
                        quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                        quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
                        quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                        quadraticSieveQuickWatchPresentation.factorList.Items.Clear();
                        quadraticSieveQuickWatchPresentation.factorInfo.Content = "Searching trivial factors!";
                        if (usePeer2Peer)
                            quadraticSieveQuickWatchPresentation.relationsInfo.Content = "";
                        else
                            quadraticSieveQuickWatchPresentation.relationsInfo.Content = "Only local sieving!";
                    }
                    , null);

                    DateTime start_time = DateTime.Now;

                    initMsieveDLL();
                    factorManager = new FactorManager(msieve.GetMethod("getPrimeFactors"), msieve.GetMethod("getCompositeFactors"), InputNumber);
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
                if (useGnuplot)
                    gnuplotFile.Close();
            }
            finally
            {
                alreadyInUse = false;
            }
        }

        private bool checkInUse()
        {
            try
            {
                alreadyInUseMutex.WaitOne();
                if (alreadyInUse)
                {
                    GuiLogMessage("QuadraticSieve plugin is only allowed to execute ones at a time due to technical restrictions.", NotificationLevel.Error);
                    return true;
                }
                else
                {
                    alreadyInUse = true;
                    return false;
                }
            }
            finally
            {
                alreadyInUseMutex.ReleaseMutex();
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
                if (usePeer2Peer)
                    quadraticSieveQuickWatchPresentation.relationsInfo.Content = "";
            }
            , null);          

            ProgressChanged(0.1, 1.0);

            running = true;
            //start helper threads:
            relationsPerMS = new double[threads + 1];
            for (int i = 0; i < threads+1; i++)
            {
                MethodInfo cloneSieveConf = msieve.GetMethod("cloneSieveConf");
                IntPtr clone = (IntPtr)cloneSieveConf.Invoke(null, new object[] { conf });                
                conf_list.Add(clone);
                WaitCallback worker = new WaitCallback(MSieveJob);
                ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, yieldqueue, i });
            }

            //manage the yields of the other threads:
            manageYields(conf, max_relations);  //this method returns as soon as there are enough relations found
            if (userStopped)
                return;

            //sieving is finished now, so give some informations and stop threads:
            ProgressChanged(0.9, 1.0);
            GuiLogMessage("Sieving finished", NotificationLevel.Info);
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.timeLeft.Text = "";
                quadraticSieveQuickWatchPresentation.endTime.Text = "";
                quadraticSieveQuickWatchPresentation.factorInfo.Content = "Found enough relations! Please wait...";
            }, null);
            
            stopThreads();
            if (yieldqueue != null)
                yieldqueue.Clear();
        }

        /// <summary>
        /// Manages the whole yields that are created during the sieving process by the other threads (and other peers).
        /// Returns true, if enough relations have been found.
        /// </summary>
        private void manageYields(IntPtr conf, int max_relations)
        {
            MethodInfo serializeYield = msieve.GetMethod("serializeYield");
            MethodInfo deserializeYield = msieve.GetMethod("deserializeYield");
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

                    if (usePeer2Peer)
                    {
                        byte[] serializedYield = (byte[])serializeYield.Invoke(null, new object[] { yield });
                        peerToPeer.Put(serializedYield);
                    }

                    saveYield.Invoke(null, new object[] { conf, yield });
                }

                if (usePeer2Peer)
                {
                    Queue dhtqueue = peerToPeer.GetLoadedYieldsQueue();
                    while (dhtqueue.Count != 0)       //get all the loaded results from the DHT, and store them
                    {
                        byte[] yield = (byte[])dhtqueue.Dequeue();
                        IntPtr deserializedYield = (IntPtr)deserializeYield.Invoke(null, new object[] { yield });
                        saveYield.Invoke(null, new object[] { conf, deserializedYield });
                    }
                }

                num_relations = (int)getNumRelations.Invoke(null, new object[] { conf });
                showProgressPresentation(max_relations, num_relations, start_relations, start_sieving_time);

                if (usePeer2Peer && !peerToPeer.SyncFactorManager(factorManager))   //another peer already finished sieving
                {
                    throw new AlreadySievedException();
                }
            }            
        }

        private void showProgressPresentation(int max_relations, int num_relations, int start_relations, DateTime start_sieving_time)
        {
            String logging_message = "Found " + num_relations + " of " + max_relations + " relations!";
            double msleft = 0;

            //calculate global performance in relations per ms:
            double globalPerformance = 0;
            foreach (double r in relationsPerMS)
                globalPerformance += r;
            if (usePeer2Peer)
                globalPerformance += peerToPeer.GetP2PPerformance();

            //Calculate the total time assuming that we can sieve 1 minute with the same performance:
            double relationsCalculatableIn1Minute = 1000 * 60 * 1 * globalPerformance;
            if (relationsCalculatableIn1Minute <= max_relations)
            {
                double p = ApproximatedPolynom(relationsCalculatableIn1Minute / max_relations);
                double estimatedTotalTime = 1000 * 60 * 1 / p;

                //Calculate the elapsed time assuming that we sieved with the same performance the whole time:
                p = ApproximatedPolynom((double)num_relations / max_relations);
                double estimatedElapsedTime = estimatedTotalTime * p;

                //Calculate time left:
                msleft = estimatedTotalTime - estimatedElapsedTime;
                /*GuiLogMessage("Total: " + new TimeSpan(0, 0, 0, 0, (int)estimatedTotalTime), NotificationLevel.Info);
                GuiLogMessage("Elapsed: " + new TimeSpan(0, 0, 0, 0, (int)estimatedElapsedTime), NotificationLevel.Info);*/
            }            
            
            String timeLeft_message = "very soon";
            String endtime_message = "very soon";
            if (msleft > 0 && !double.IsInfinity(msleft))
            {
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)msleft);
                timeLeft_message = showTimeSpan(ts) + " left";
                endtime_message = "" + DateTime.Now.AddMilliseconds((long)msleft);
            }

            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.logging.Text = logging_message;
                quadraticSieveQuickWatchPresentation.timeLeft.Text = timeLeft_message;
                quadraticSieveQuickWatchPresentation.endTime.Text = endtime_message;
            }, null);

            if (useGnuplot)
            {
                double percentage = (double)num_relations / max_relations;
                double time = (DateTime.Now - start_sieving_time).TotalSeconds;
                gnuplotFile.WriteLine("" + time + "\t\t" + percentage);
            }
        }

        private static double ApproximatedPolynom(double x)
        {
            double a = -3.55504;
            double b = 8.62296;
            double c = -7.75103;
            double d = 3.65871;
            double progress = a * x * x * x * x + b * x * x * x + c * x * x + d * x;
            return progress;
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

            if (usePeer2Peer)
                peerToPeer.SyncFactorManager(factorManager);
            
            MethodInfo msieve_run_core = msieve.GetMethod("msieve_run_core");

            //Now factorize as often as needed:
            while (!factorManager.OnlyPrimes())
            {
                //get one composite factor, which we want to sieve now:
                BigInteger compositeFactor = factorManager.GetCompositeFactor();
                showFactorInformations(compositeFactor);
                if (usePeer2Peer)
                    peerToPeer.SetFactor(compositeFactor);

                try
                {
                    //now start quadratic sieve on it:                
                    IntPtr resultList = (IntPtr)msieve_run_core.Invoke(null, new object[2] { obj, compositeFactor.ToString() });
                    if (userStopped)
                        return;

                    factorManager.ReplaceCompositeByFactors(compositeFactor, resultList);   //add the result list to factorManager

                    if (usePeer2Peer)
                        peerToPeer.SyncFactorManager(factorManager);
                }
                catch (AlreadySievedException)
                {
                    GuiLogMessage("Another peer already finished factorization of composite factor" + compositeFactor + ". Sieving next one...", NotificationLevel.Info);
                }
            }
        }

        private void showFactorInformations(BigInteger compositeFactor)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                String compRep;
                if (compositeFactor.ToString().Length < 6)
                    compRep = compositeFactor.ToString();
                else
                    compRep = compositeFactor.ToString().Substring(0, 4) + "...";
                quadraticSieveQuickWatchPresentation.factorInfo.Content = "Now sieving first composite factor! (" + compRep + ")";
            }, null);
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
            int threadNR = (int)parameters[4];

            MethodInfo collectRelations = msieve.GetMethod("collectRelations");
            MethodInfo getYield = msieve.GetMethod("getYield");
            MethodInfo getAmountOfRelationsInYield = msieve.GetMethod("getAmountOfRelationsInYield");

            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (running)
            {
                try
                {
                    //Calculate the performance of this thread:
                    DateTime beginning = DateTime.Now;                    
                    collectRelations.Invoke(null, new object[] { clone, update, core_sieve_fcn });                    
                    IntPtr yield = (IntPtr)getYield.Invoke(null, new object[] { clone });

                    int amountOfFullRelations = (int)getAmountOfRelationsInYield.Invoke(null, new object[] { yield });
                    relationsPerMS[threadNR] = amountOfFullRelations / (DateTime.Now - beginning).TotalMilliseconds;

                    if (usePeer2Peer)
                        peerToPeer.SetOurPerformance(relationsPerMS);

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

            if (conf_list != null)
                conf_list[threadNR] = null;
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
                if (usePeer2Peer)
                    peerToPeer.StopLoadStoreThread();

                MethodInfo stop = msieve.GetMethod("stop");
                MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
                foreach (IntPtr conf in conf_list)
                    if (conf != null)
                        stop.Invoke(null, new object[] { getObjFromConf.Invoke(null, new object[] { conf }) });

                conf_list = null;

                GuiLogMessage("Waiting for threads to stop!", NotificationLevel.Debug);
                while (threadcount > 0)
                {
                    Thread.Sleep(0);
                }
                GuiLogMessage("Threads stopped!", NotificationLevel.Debug);
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
                if (IntPtr.Size == 4)
                    dllname = "msieve.dll";
                else
                    dllname = "msieve64.dll";

                msieveDLL = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\AppReferences\\"  + dllname);
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

        private void peerToPeer_P2PWarning(string warning)
        {
            GuiLogMessage(warning, NotificationLevel.Warning);
        }

        #endregion

    }
}
