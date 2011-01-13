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
        private Queue relationPackageQueue;
        private AutoResetEvent newRelationPackageEvent = new AutoResetEvent(false);
        private IntPtr obj = IntPtr.Zero;
        private volatile int threadcount = 0;
        private ArrayList conf_list;
        private Mutex conf_listMutex = new Mutex();
        private bool userStopped = false;
        private FactorManager factorManager;
        private PeerToPeer peerToPeer;
        private PeerToPeerStatusUpdater peerToPeerStatusUpdater;
        private bool usePeer2Peer;
        private bool otherPeerFinished;
        private bool useGnuplot = false;
        private StreamWriter gnuplotFile;
        private double[] relationsPerMS;
        private DateTime start_time;

        private static Assembly msieveDLL = null;
        private static Type msieve = null;
        private static bool alreadyInUse = false;
        private static Mutex alreadyInUseMutex = new Mutex();
        private AutoResetEvent waitForConnection = new AutoResetEvent(false);

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
            Settings = new QuadraticSieveSettings(this);

            directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "msieve");
            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

            QuickWatchPresentation = new QuadraticSievePresentation();

            peerToPeer = new PeerToPeer(quadraticSieveQuickWatchPresentation, newRelationPackageEvent);
            peerToPeerStatusUpdater = new PeerToPeerStatusUpdater(peerToPeer);
            peerToPeer.P2PWarning += new PeerToPeer.P2PWarningHandler(peerToPeer_P2PWarning);
            
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.peer2peer.Visibility = settings.UsePeer2Peer ? Visibility.Visible : Visibility.Collapsed;                
                quadraticSieveQuickWatchPresentation.timeLeft.Content = "-";
                quadraticSieveQuickWatchPresentation.endTime.Content = "-";
                quadraticSieveQuickWatchPresentation.coresUsed.Content = "-";
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
                if (usePeer2Peer && !P2PManager.IsConnected && P2PManager.IsConnecting)
                {
                    P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += HandleConnectionStateChange;
                    GuiLogMessage("Connecting to Peer2Peer network... Please wait!", NotificationLevel.Info);
                    waitForConnection.WaitOne();                    
                    P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred -= HandleConnectionStateChange;
                }
                else if (usePeer2Peer && !P2PManager.IsConnected)
                {
                    GuiLogMessage("No connection to Peer2Peer network. Sieving locally now but waiting for connection...", NotificationLevel.Warning);
                    P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += OnP2PConnectionStateChangeOccurred;
                    usePeer2Peer = false;
                }

                if (usePeer2Peer && (settings.Channel == null ||settings.Channel.Trim() == ""))
                {
                    GuiLogMessage("No channel for Peer2Peer network specified. Sieving locally now!", NotificationLevel.Warning);
                    usePeer2Peer = false;
                }
                if (settings.UsePeer2Peer)
                {
                    peerToPeer.SetChannel(settings.Channel);
                    peerToPeer.SetNumber(InputNumber);
                }

                if (useGnuplot)
                    gnuplotFile = new StreamWriter(Path.Combine(directoryName, "gnuplot.dat"), false);

                userStopped = false;
                otherPeerFinished = false;

                if (InputNumber != 0)
                {
                    if (InputNumber.ToString().Length >= 275)
                    {
                        GuiLogMessage("Input too big.", NotificationLevel.Error);
                        return;
                    }

                    String info_message = "Starting quadratic sieve, please wait!";

                    start_time = DateTime.Now;

                    GuiLogMessage(info_message, NotificationLevel.Info);
                    quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        quadraticSieveQuickWatchPresentation.ProgressRelationPackages.Clear();
                        quadraticSieveQuickWatchPresentation.information.Content = info_message;
                        quadraticSieveQuickWatchPresentation.endTime.Content = "-";
                        quadraticSieveQuickWatchPresentation.timeLeft.Content = "-";
                        quadraticSieveQuickWatchPresentation.elapsedTime.Content = "-";
                        quadraticSieveQuickWatchPresentation.startTime.Content = ""+start_time;
                        quadraticSieveQuickWatchPresentation.factorList.Items.Clear();
                        quadraticSieveQuickWatchPresentation.factorInfo.Content = "Searching trivial factors!";
                        if (usePeer2Peer)
                            quadraticSieveQuickWatchPresentation.localSieving.Visibility = Visibility.Hidden;
                        else
                            quadraticSieveQuickWatchPresentation.localSieving.Visibility = Visibility.Visible;
                    }
                    , null);

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
                        String timeLeft_message = "0 seconds left";
                        String endtime_message = "" + (DateTime.Now);

                        GuiLogMessage(info_message, NotificationLevel.Info);
                        quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            quadraticSieveQuickWatchPresentation.information.Content = "Sieving finished!";
                            quadraticSieveQuickWatchPresentation.endTime.Content = endtime_message;
                            quadraticSieveQuickWatchPresentation.timeLeft.Content = timeLeft_message;
                            quadraticSieveQuickWatchPresentation.factorInfo.Content = "";
                        }
                        , null);

                        Debug.Assert(factorManager.CalculateNumber() == InputNumber);
                        OutputFactors = factorManager.getPrimeFactors();

                        ProgressChanged(1, 1);
                    }
                    else
                    {                        
                        info_message = "Stopped by user!";

                        GuiLogMessage(info_message, NotificationLevel.Info);
                        quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            quadraticSieveQuickWatchPresentation.information.Content = info_message;
                            quadraticSieveQuickWatchPresentation.endTime.Content = "-";
                            quadraticSieveQuickWatchPresentation.timeLeft.Content = "-";
                            quadraticSieveQuickWatchPresentation.startTime.Content = "-";
                            quadraticSieveQuickWatchPresentation.elapsedTime.Content = "-";
                            quadraticSieveQuickWatchPresentation.factorInfo.Content = "";
                        }
                        , null);
                    }
                }
                if (useGnuplot)
                    gnuplotFile.Close();
            }
            finally
            {
                alreadyInUse = false;
                P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred -= OnP2PConnectionStateChangeOccurred;
            }
        }

        private void OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            usePeer2Peer = newState;
            if (usePeer2Peer)
                GuiLogMessage("Using Peer2Peer network now!", NotificationLevel.Info);
            else
                GuiLogMessage("Lost Peer2Peer network connection... Sieving locally now!", NotificationLevel.Info);
        }

        private void HandleConnectionStateChange(object sender, bool newState)
        {
            waitForConnection.Set();
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
            if (obj != IntPtr.Zero)
            {
                stopThreads();
                MethodInfo stop = msieve.GetMethod("stop");
                stop.Invoke(null, new object[] { obj });
            }
            this.userStopped = true;
            newRelationPackageEvent.Set();
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
        [PropertyInfo(Direction.InputData, "Number input", "Enter the number you want to factorize", "")]
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
        [PropertyInfo(Direction.OutputData, "Factors output", "Your factors will be sent here", "")]
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
        private String timeSpanString(TimeSpan ts)
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
        /// <returns>true if enough relations found, false if not</returns>
        private bool prepareSieving(IntPtr conf, int update, IntPtr core_sieve_fcn, int max_relations)
        {
            try
            {
                int threads = Math.Min(settings.CoresUsed, Environment.ProcessorCount - 1);
                MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");
                this.obj = (IntPtr)getObjFromConf.Invoke(null, new object[] { conf });
                relationPackageQueue = Queue.Synchronized(new Queue());
                conf_list = new ArrayList();

                String message = "Sieving now!";
                GuiLogMessage(message, NotificationLevel.Info);
                quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    quadraticSieveQuickWatchPresentation.coresUsed.Content = (threads+1);
                    quadraticSieveQuickWatchPresentation.information.Content = message;
                    if (usePeer2Peer)
                        quadraticSieveQuickWatchPresentation.localSieving.Visibility = Visibility.Hidden;
                }
                , null);

                ProgressChanged(0.1, 1.0);

                running = true;
                //start helper threads:
                relationsPerMS = new double[threads + 1];
                for (int i = 0; i < threads + 1; i++)
                {
                    MethodInfo cloneSieveConf = msieve.GetMethod("cloneSieveConf");
                    IntPtr clone = (IntPtr)cloneSieveConf.Invoke(null, new object[] { conf });
                    conf_list.Add(clone);
                    WaitCallback worker = new WaitCallback(MSieveJob);
                    ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, relationPackageQueue, i });
                }

                //manage the relation packages of the other threads:
                manageRelationPackages(conf, max_relations);  //this method returns as soon as there are enough relations found
                if (userStopped)
                    return false;

                //sieving is finished now, so give some informations and stop threads:
                ProgressChanged(0.9, 1.0);
                GuiLogMessage("Sieving finished", NotificationLevel.Info);
                quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    quadraticSieveQuickWatchPresentation.timeLeft.Content = "-";
                    quadraticSieveQuickWatchPresentation.endTime.Content = "-";
                    quadraticSieveQuickWatchPresentation.factorInfo.Content = "Found enough relations! Please wait...";
                }, null);

                if (relationPackageQueue != null)
                    relationPackageQueue.Clear();
            }
            catch (AlreadySievedException)
            {
                ProgressChanged(0.9, 1.0);
                GuiLogMessage("Another peer already finished factorization of composite factor. Sieving next one...", NotificationLevel.Info);
                quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    quadraticSieveQuickWatchPresentation.timeLeft.Content = "-";
                    quadraticSieveQuickWatchPresentation.endTime.Content = "-";
                    quadraticSieveQuickWatchPresentation.factorInfo.Content = "Other peer finished sieving!";
                }, null);
                otherPeerFinished = true;
                return false;
            }
            finally
            {
                stopThreads();
            }
            return true;
        }

        /// <summary>
        /// Manages the whole relation packages that are created during the sieving process by the other threads (and other peers).
        /// Returns true, if enough relations have been found.
        /// </summary>
        private void manageRelationPackages(IntPtr conf, int max_relations)
        {
            MethodInfo serializeRelationPackage = msieve.GetMethod("serializeRelationPackage");
            MethodInfo deserializeRelationPackage = msieve.GetMethod("deserializeRelationPackage");
            MethodInfo getNumRelations = msieve.GetMethod("getNumRelations");
            int num_relations = (int)getNumRelations.Invoke(null, new object[] { conf });
            int start_relations = num_relations;
            DateTime start_sieving_time = DateTime.Now;
            MethodInfo saveRelationPackage = msieve.GetMethod("saveRelationPackage");

            while (num_relations < max_relations)
            {
                double progress = (double)num_relations / max_relations * 0.8 + 0.1;
                ProgressChanged(progress, 1.0);                
                
                newRelationPackageEvent.WaitOne();               //wait until there is a new relation package in the queue
                if (userStopped)
                    return;

                while (relationPackageQueue.Count != 0)       //get all the results from the helper threads, and store them
                {
                    IntPtr relationPackage = (IntPtr)relationPackageQueue.Dequeue();

                    if (settings.UsePeer2Peer)
                    {
                        byte[] serializedRelationPackage = (byte[])serializeRelationPackage.Invoke(null, new object[] { relationPackage });
                        peerToPeer.Put(serializedRelationPackage);
                    }

                    saveRelationPackage.Invoke(null, new object[] { conf, relationPackage });
                }

                if (usePeer2Peer)
                {
                    Queue dhtqueue = peerToPeer.GetLoadedRelationPackagesQueue();
                    while (dhtqueue.Count != 0)       //get all the loaded results from the DHT, and store them
                    {
                        byte[] relationPackage = (byte[])dhtqueue.Dequeue();
                        IntPtr deserializedRelationPackage = (IntPtr)deserializeRelationPackage.Invoke(null, new object[] { relationPackage });
                        saveRelationPackage.Invoke(null, new object[] { conf, deserializedRelationPackage });
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
            }            
            
            String timeLeft_message = "very soon";
            String endtime_message = "very soon";
            DateTime now = DateTime.Now;
            if (msleft > 0 && !double.IsInfinity(msleft))
            {
                TimeSpan ts = new TimeSpan((long)(msleft * TimeSpan.TicksPerMillisecond));
                timeLeft_message = timeSpanString(ts) + " left";
                endtime_message = "" + now.AddMilliseconds((long)msleft);
            }

            if (globalPerformance == 0 || double.IsInfinity(msleft))
            {
                timeLeft_message = "-";
                endtime_message = "-";
            }

            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.foundRelations.Content = num_relations;
                quadraticSieveQuickWatchPresentation.maxRelations.Content = max_relations;
                quadraticSieveQuickWatchPresentation.timeLeft.Content = timeLeft_message;
                quadraticSieveQuickWatchPresentation.endTime.Content = endtime_message;
                quadraticSieveQuickWatchPresentation.elapsedTime.Content = timeSpanString(now.Subtract(start_time));
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
        private void putTrivialFactorlist(IntPtr list, IntPtr obj)
        {
            if (userStopped)
                return;

            //add the trivial factors to the factor list:
            factorManager.AddFactors(list);

            if (usePeer2Peer)
                peerToPeer.SyncFactorManager(factorManager);
            
            MethodInfo msieve_run_core = msieve.GetMethod("msieve_run_core");

            try
            {
                //Now factorize as often as needed:
                while (!factorManager.OnlyPrimes())
                {
                    //get one composite factor, which we want to sieve now:
                    BigInteger compositeFactor = factorManager.GetCompositeFactor();
                    showFactorInformations(compositeFactor);
                    if (settings.UsePeer2Peer)
                        peerToPeer.SetFactor(compositeFactor);

                    //now start quadratic sieve on it:                
                    IntPtr resultList = (IntPtr)msieve_run_core.Invoke(null, new object[2] { obj, compositeFactor.ToString() });
                    if (otherPeerFinished)
                    {
                        otherPeerFinished = false;
                        continue;
                    }

                    if (resultList == IntPtr.Zero)
                        throw new NotSievableException();

                    if (userStopped)
                        return;

                    factorManager.ReplaceCompositeByFactors(compositeFactor, resultList);   //add the result list to factorManager

                    if (usePeer2Peer)
                        peerToPeer.SyncFactorManager(factorManager);
                }
            }
            catch (NotSievableException)
            {
                GuiLogMessage("This number is not sievable by msieve (maybe input too long?)", NotificationLevel.Error);
            }
        }

        private void showFactorInformations(BigInteger compositeFactor)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                String compRep;
                if (compositeFactor.ToString().Length < 50)
                    compRep = compositeFactor.ToString();
                else
                    compRep = compositeFactor.ToString().Substring(0, 48) + "...";
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
            Queue relationPackageQueue = (Queue)parameters[3];
            int threadNR = (int)parameters[4];

            try
            {
                MethodInfo collectRelations = msieve.GetMethod("collectRelations");
                MethodInfo getRelationPackage = msieve.GetMethod("getRelationPackage");
                MethodInfo getAmountOfRelationsInRelationPackage = msieve.GetMethod("getAmountOfRelationsInRelationPackage");

                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                while (running)
                {
                    try
                    {
                        //Calculate the performance of this thread:
                        DateTime beginning = DateTime.Now;
                        collectRelations.Invoke(null, new object[] { clone, update, core_sieve_fcn });
                        IntPtr relationPackage = (IntPtr)getRelationPackage.Invoke(null, new object[] { clone });

                        int amountOfFullRelations = (int)getAmountOfRelationsInRelationPackage.Invoke(null, new object[] { relationPackage });
                        relationsPerMS[threadNR] = amountOfFullRelations / (DateTime.Now - beginning).TotalMilliseconds;

                        if (usePeer2Peer)
                            peerToPeer.SetOurPerformance(relationsPerMS);

                        relationPackageQueue.Enqueue(relationPackage);
                        newRelationPackageEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Error using msieve." + ex.Message, NotificationLevel.Error);
                        threadcount = 0;
                        return;
                    }
                }

                conf_listMutex.WaitOne();
                if (conf_list != null)
                    conf_list[threadNR] = null;
                MethodInfo freeSieveConf = msieve.GetMethod("freeSieveConf");
                freeSieveConf.Invoke(null, new object[] { clone });
                conf_listMutex.ReleaseMutex();
            }
            finally
            {
                threadcount--;
            }
        }       

        /// <summary>
        /// Stop all running threads
        /// </summary>
        private void stopThreads()
        {
            if (settings.UsePeer2Peer)
                peerToPeer.StopLoadStoreThread();

            if (conf_list != null)
            {
                MethodInfo stop = msieve.GetMethod("stop");
                MethodInfo getObjFromConf = msieve.GetMethod("getObjFromConf");

                conf_listMutex.WaitOne();
                foreach (Object conf in conf_list)
                    if (conf != null)
                        stop.Invoke(null, new object[] { getObjFromConf.Invoke(null, new object[] { (IntPtr)conf }) });
                
                running = false;

                conf_list = null;
                conf_listMutex.ReleaseMutex();

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
            if (usePeer2Peer)
                peerToPeerStatusUpdater.UpdateStatus(value/max);
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
                quadraticSieveQuickWatchPresentation.SelectFirstComposite();
            }, null);
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
            msieveDLL = Msieve.GetMsieveDLL();
            msieve = msieveDLL.GetType("Msieve.msieve");

            //init msieve with callbacks:
            MethodInfo initMsieve = msieve.GetMethod("initMsieve");
            Object callback_struct = Activator.CreateInstance(msieveDLL.GetType("Msieve.callback_struct"));            
            FieldInfo prepareSievingField = msieveDLL.GetType("Msieve.callback_struct").GetField("prepareSieving");
            FieldInfo putTrivialFactorlistField = msieveDLL.GetType("Msieve.callback_struct").GetField("putTrivialFactorlist");            
            Delegate prepareSievingDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.prepareSievingDelegate"), this, "prepareSieving");
            Delegate putTrivialFactorlistDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.putTrivialFactorlistDelegate"), this, "putTrivialFactorlist");            
            prepareSievingField.SetValue(callback_struct, prepareSievingDel);
            putTrivialFactorlistField.SetValue(callback_struct, putTrivialFactorlistDel);
            initMsieve.Invoke(null, new object[1] { callback_struct });
        }

        private void peerToPeer_P2PWarning(string warning)
        {
            GuiLogMessage(warning, NotificationLevel.Warning);
        }

        #endregion

        #region internal

        internal PeerToPeer PeerToPeer
        {
            get { return peerToPeer; }
        }

        internal bool Running
        {
            get { return running; }
        }

        /// <summary>
        /// Logs a message to the CrypTool gui
        /// </summary>
        /// <param name="p">p</param>
        /// <param name="notificationLevel">notificationLevel</param>
        internal void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

    }
}
