﻿/*                              
   Copyright 2009 Sven Rech, Nils Kopal, Uni Duisburg-Essen

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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using Cryptool.PluginBase.IO;
using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P;
using KeySearcher.P2P.Exceptions;
using KeySearcherPresentation;
using KeySearcherPresentation.Controls;
using OpenCLNet;

namespace KeySearcher
{
    [Author("Sven Rech, Nils Kopal, Raoul Falk, Dennis Nolte", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "KeySearcher", "Bruteforces a decryption algorithm.", "KeySearcher/DetailedDescription/Description.xaml", "KeySearcher/Images/icon.png")]
    public class KeySearcher : IAnalysisMisc
    {
        /// <summary>
        /// used for creating the UserStatistics
        /// </summary>
        private Dictionary<string, Dictionary<long, Information>> statistic;
        private bool initialized;
        /// <summary>
        /// used for creating the TopList
        /// </summary>
        private Queue valuequeue;
        private double value_threshold;
        /// <summary>
        /// the thread with the most keys left
        /// </summary>
        private int maxThread;
        private readonly Mutex maxThreadMutex = new Mutex();
        private ArrayList threadsStopEvents;

        public bool IsKeySearcherRunning;
        private KeyQualityHelper keyQualityHelper;
        private readonly P2PQuickWatchPresentation p2PQuickWatchPresentation;
        private readonly LocalQuickWatchPresentation localQuickWatchPresentation;

        private OpenCLManager oclManager = null;
        private Mutex openCLPresentationMutex = new Mutex();

        private readonly Stopwatch localBruteForceStopwatch;

        private KeyPattern.KeyPattern pattern;
        public KeyPattern.KeyPattern Pattern
        {
            get
            {
                return pattern;
            }
            set
            {
                pattern = value;
                if ((settings.Key == null) || ((settings.Key != null) && !pattern.testWildcardKey(settings.Key)))
                    settings.Key = pattern.giveInputPattern();
            }
        }

        internal bool stop;

        internal bool update;

        #region IControlEncryption + IControlCost + InputFields

        #region IControlEncryption Members

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                if (controlMaster != null)
                {
                    controlMaster.keyPatternChanged -= keyPatternChanged;
                }
                if (value != null)
                {
                    Pattern = new KeyPattern.KeyPattern(value.getKeyPattern());
                    value.keyPatternChanged += keyPatternChanged;
                    controlMaster = value;
                    OnPropertyChanged("ControlMaster");

                }
                else
                    controlMaster = null;
            }
        }

        #endregion

        #region IControlCost Members

        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "Cost Master", "Used for cost calculation", "", DisplayLevel.Beginner)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
                keyQualityHelper = new KeyQualityHelper(costMaster);
            }
        }

        #endregion

        /* BEGIN: following lines are from Arnie - 2010.01.12 */
        CryptoolStream csEncryptedData;
        [PropertyInfo(Direction.InputData, "CS Encrypted Data", "Encrypted data out of an Encryption PlugIn", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, "")]
        public virtual CryptoolStream CSEncryptedData
        {
            get { return this.csEncryptedData; }
            set
            {
                if (value != this.csEncryptedData)
                {
                    this.csEncryptedData = value;
                    this.encryptedData = GetByteFromCryptoolStream(value);
                    OnPropertyChanged("CSEncryptedData");
                }
            }
        }

        byte[] encryptedData;
        [PropertyInfo(Direction.InputData,"Encrypted Data","Encrypted data out of an Encryption PlugIn","",false,false,DisplayLevel.Beginner,QuickWatchFormat.Hex,"")]
        public virtual byte[] EncryptedData 
        {
            get { return this.encryptedData; }
            set
            {
                if (value != this.encryptedData)
                {
                    this.encryptedData = value;
                    OnPropertyChanged("EncryptedData");
                }
            }
        }

        /// <summary>
        /// When the Input-Slot changed, set this variable to true, so the new Stream will be transformed to byte[]
        /// </summary>
        private byte[] GetByteFromCryptoolStream(CryptoolStream cryptoolStream)
        {
            byte[] encryptedByteData = null;

            if (cryptoolStream != null)
            {
                CryptoolStream cs = new CryptoolStream();
                cs.OpenRead(cryptoolStream.FileName);
                encryptedByteData = new byte[cs.Length];
                if(cs.Length > Int32.MaxValue)
                    throw(new Exception("CryptoolStream length is longer than the Int32.MaxValue"));
                cs.Read(encryptedByteData, 0, (int)cs.Length);
            }
            return encryptedByteData;
        }

        byte[] initVector;
        [PropertyInfo(Direction.InputData, "Initialization Vector", "Initialization vector with which the data were encrypted", "", DisplayLevel.Beginner)]
        public virtual byte[] InitVector
        {
            get { return this.initVector; }
            set
            {
                if (value != this.initVector)
                {
                    this.initVector = value;
                    OnPropertyChanged("InitVector");
                }
            }
        }
        /* END: Lines above are from Arnie - 2010.01.12 */

        private ValueKey top1ValueKey;
        public virtual ValueKey Top1
        {
            set { top1ValueKey = value; OnPropertyChanged("Top1Message"); OnPropertyChanged("Top1Key"); }
        }

        [PropertyInfo(Direction.OutputData, "Top1 Message", "The best message found", "", DisplayLevel.Beginner)]
        public virtual byte[] Top1Message
        {
            get { return top1ValueKey.decryption; }
        }
        [PropertyInfo(Direction.OutputData, "Top1 Key", "The best key found", "", DisplayLevel.Beginner)]
        public virtual byte[] Top1Key
        {
            get
            {
                if (top1ValueKey.key != null)
                {
                    return top1ValueKey.keya;
                }
                else
                    return null;
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private KeySearcherSettings settings;
        private AutoResetEvent connectResetEvent;

        #region external client variables
        private CryptoolServer cryptoolServer;
        private KeySearcherOpenCLCode externalKeySearcherOpenCLCode;
        private IKeyTranslator externalKeyTranslator;
        private int externalKeysProcessed;
        private EndPoint externalClientConnected;
        private AutoResetEvent waitForExternalClientToFinish = new AutoResetEvent(false);
        private DateTime assignTime;
        #endregion

        public KeySearcher()
        {
            IsKeySearcherRunning = false;
            
            if (OpenCL.NumberOfPlatforms > 0)
            {
                oclManager = new OpenCLManager();
                oclManager.AttemptUseBinaries = false;
                oclManager.AttemptUseSource = true;
                oclManager.RequireImageSupport = false;
                var directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "KeySearcher");
                oclManager.BinaryPath = Path.Combine(directoryName, "openclbin");
                oclManager.BuildOptions = "";
                oclManager.CreateDefaultContext(0, DeviceType.ALL);
            }

            settings = new KeySearcherSettings(this, oclManager);
            
            QuickWatchPresentation = new QuickWatch();
            localQuickWatchPresentation = ((QuickWatch) QuickWatchPresentation).LocalQuickWatchPresentation;
            p2PQuickWatchPresentation = ((QuickWatch)QuickWatchPresentation).P2PQuickWatchPresentation;
            p2PQuickWatchPresentation.UpdateSettings(this, settings);

            settings.PropertyChanged += SettingsPropertyChanged;
            ((QuickWatch)QuickWatchPresentation).IsOpenCLEnabled = (settings.DeviceSettings.Count(x => x.useDevice) > 0);

            localBruteForceStopwatch = new Stopwatch();
        }

        void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            p2PQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             new Action(UpdateQuickwatchSettings));
        }

        void UpdateQuickwatchSettings()
        {
            ((QuickWatch)QuickWatchPresentation).IsP2PEnabled = settings.UsePeerToPeer;
            ((QuickWatch)QuickWatchPresentation).IsOpenCLEnabled = (settings.DeviceSettings.Count(x => x.useDevice) > 0);
            p2PQuickWatchPresentation.UpdateSettings(this, settings);
        }

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return QuickWatchPresentation; }
        }

        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            update = false;
        }

        // because Encryption PlugIns were changed radical, the new StartPoint is here - Arnie 2010.01.12
        public virtual void Execute()
        {
            IsKeySearcherRunning = true;
            localBruteForceStopwatch.Reset();

            //either byte[] CStream input or CryptoolStream Object input
            if (encryptedData != null || csEncryptedData != null) //to prevent execution on initialization
            {
                if (ControlMaster != null)
                    process(ControlMaster);
                else
                {
                    GuiLogMessage("You have to connect the KeySearcher with the Decryption Control!", NotificationLevel.Warning);
                }
            }
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            IsKeySearcherRunning = false;
            stop = true;
            waitForExternalClientToFinish.Set();
        }

        public void Initialize()
        {
            settings.Initialize();
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region whole KeySearcher functionality

        private class ThreadStackElement
        {
            public AutoResetEvent ev;
            public int threadid;
        }

        #region code for the worker threads

        private void KeySearcherJob(object param)
        {
            AutoResetEvent stopEvent = new AutoResetEvent(false);
            threadsStopEvents.Add(stopEvent);

            object[] parameters = (object[])param;
            KeyPattern.KeyPattern[] patterns = (KeyPattern.KeyPattern[])parameters[0];
            int threadid = (int)parameters[1];
            BigInteger[] doneKeysArray = (BigInteger[])parameters[2];
            BigInteger[] openCLDoneKeysArray = (BigInteger[])parameters[3];
            BigInteger[] keycounterArray = (BigInteger[])parameters[4];
            BigInteger[] keysLeft = (BigInteger[])parameters[5];
            IControlEncryption sender = (IControlEncryption)parameters[6];
            int bytesToUse = (int)parameters[7];
            Stack threadStack = (Stack)parameters[8];
            var openCLDeviceSettings = (KeySearcherSettings.OpenCLDeviceSettings)parameters[9];

            KeySearcherOpenCLCode keySearcherOpenCLCode = null;
            KeySearcherOpenCLSubbatchOptimizer keySearcherOpenCLSubbatchOptimizer = null;
            if (openCLDeviceSettings != null)
            {
                keySearcherOpenCLCode = new KeySearcherOpenCLCode(this, encryptedData, sender, CostMaster, 256 * 256 * 256 * 16);
                keySearcherOpenCLSubbatchOptimizer = new KeySearcherOpenCLSubbatchOptimizer(openCLDeviceSettings.mode, 
                        oclManager.CQ[openCLDeviceSettings.index].Device.MaxWorkItemSizes.Aggregate(1, (x, y) => (x * (int)y)) / 8);

                ((QuickWatch)QuickWatchPresentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    openCLPresentationMutex.WaitOne();
                    ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.AmountOfDevices++;
                    openCLPresentationMutex.ReleaseMutex();
                }, null);
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            }

            try
            {
                while (patterns[threadid] != null)
                {
                    BigInteger size = patterns[threadid].size();
                    keysLeft[threadid] = size;
                    
                    IKeyTranslator keyTranslator = ControlMaster.getKeyTranslator();
                    keyTranslator.SetKeys(patterns[threadid]);

                    bool finish = false;

                    do
                    {
                        //if we are the thread with most keys left, we have to share them:
                        keyTranslator = ShareKeys(patterns, threadid, keysLeft, keyTranslator, threadStack);

                        if (openCLDeviceSettings == null)         //CPU
                        {
                            finish = BruteforceCPU(keyTranslator, sender, bytesToUse);
                        }
                        else                    //OpenCL
                        {
                            try
                            {
                                finish = BruteforceOpenCL(keySearcherOpenCLCode, keySearcherOpenCLSubbatchOptimizer, keyTranslator, sender, bytesToUse, parameters);
                            }
                            catch (Exception)
                            {
                                openCLDeviceSettings.useDevice = false;
                                ((QuickWatch)QuickWatchPresentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    openCLPresentationMutex.WaitOne();
                                    ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.AmountOfDevices--;
                                    openCLPresentationMutex.ReleaseMutex();
                                }, null);
                                continue;
                            }
                        }
                        
                        int progress = keyTranslator.GetProgress();

                        if (openCLDeviceSettings == null)
                        {
                            doneKeysArray[threadid] += progress;
                            keycounterArray[threadid] += progress;
                            keysLeft[threadid] -= progress;
                        }

                    } while (!finish && !stop);

                    if (stop)
                        return;

                    //Let's wait until another thread is willing to share with us:
                    WaitForNewPattern(patterns, threadid, threadStack);
                }
            }
            finally
            {
                sender.Dispose();
                stopEvent.Set();
            }
        }

        private unsafe bool BruteforceOpenCL(KeySearcherOpenCLCode keySearcherOpenCLCode, KeySearcherOpenCLSubbatchOptimizer keySearcherOpenCLSubbatchOptimizer, IKeyTranslator keyTranslator, IControlEncryption sender, int bytesToUse, object[] parameters)
        {
            int threadid = (int)parameters[1];
            BigInteger[] doneKeysArray = (BigInteger[])parameters[2];
            BigInteger[] openCLDoneKeysArray = (BigInteger[])parameters[3];
            BigInteger[] keycounterArray = (BigInteger[])parameters[4];
            BigInteger[] keysLeft = (BigInteger[])parameters[5];
            var openCLDeviceSettings = (KeySearcherSettings.OpenCLDeviceSettings)parameters[9];
            try
            {
                Kernel bruteforceKernel = keySearcherOpenCLCode.GetBruteforceKernel(oclManager, keyTranslator);

                int deviceIndex = openCLDeviceSettings.index;
                
                Mem userKey;
                var key = keyTranslator.GetKey();
                fixed (byte* ukp = key)
                    userKey = oclManager.Context.CreateBuffer(MemFlags.USE_HOST_PTR, key.Length, new IntPtr((void*)ukp));

                int subbatches = keySearcherOpenCLSubbatchOptimizer.GetAmountOfSubbatches(keyTranslator);
                int subbatchSize = keyTranslator.GetOpenCLBatchSize() / subbatches;
                ((QuickWatch) QuickWatchPresentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                    {
                                                                        ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.workItems.Content = subbatchSize;
                                                                    }, null);
                //GuiLogMessage(string.Format("Now using {0} subbatches", subbatches), NotificationLevel.Info);
                
                float[] costArray = new float[subbatchSize];
                Mem costs = oclManager.Context.CreateBuffer(MemFlags.READ_WRITE, costArray.Length * 4);

                IntPtr[] globalWorkSize = { (IntPtr)subbatchSize, (IntPtr)1, (IntPtr)1 };

                keySearcherOpenCLSubbatchOptimizer.BeginMeasurement();

                try
                {
                    for (int i = 0; i < subbatches; i++)
                    {
                        bruteforceKernel.SetArg(0, userKey);
                        bruteforceKernel.SetArg(1, costs);
                        bruteforceKernel.SetArg(2, i * subbatchSize);
                        oclManager.CQ[deviceIndex].EnqueueNDRangeKernel(bruteforceKernel, 3, null, globalWorkSize, null);
                        oclManager.CQ[deviceIndex].EnqueueBarrier();

                        Event e;
                        fixed (float* costa = costArray)
                            oclManager.CQ[deviceIndex].EnqueueReadBuffer(costs, true, 0, costArray.Length * 4, new IntPtr((void*)costa), 0, null, out e);

                        e.Wait();

                        checkOpenCLResults(keyTranslator, costArray, sender, bytesToUse, i * subbatchSize);

                        doneKeysArray[threadid] += subbatchSize;
                        openCLDoneKeysArray[threadid] += subbatchSize;
                        keycounterArray[threadid] += subbatchSize;
                        keysLeft[threadid] -= subbatchSize;

                        if (stop)
                            return false;
                    }

                    keySearcherOpenCLSubbatchOptimizer.EndMeasurement();
                }
                finally
                {
                    costs.Dispose();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                GuiLogMessage("Bruteforcing with OpenCL failed! Using CPU instead.", NotificationLevel.Error);
                throw new Exception("Bruteforcing with OpenCL failed!");
            }

            return !keyTranslator.NextOpenCLBatch();
        }

        private void checkOpenCLResults(IKeyTranslator keyTranslator, float[] costArray, IControlEncryption sender, int bytesToUse, int add)
        {
            var op = this.costMaster.getRelationOperator();
            for (int i = 0; i < costArray.Length; i++)
            {
                float cost = costArray[i];
                if (((op == RelationOperator.LargerThen) && (cost > value_threshold))
                    || (op == RelationOperator.LessThen) && (cost < value_threshold))
                {
                    ValueKey valueKey = new ValueKey { value = cost, key = keyTranslator.GetKeyRepresentation(i + add) };
                    valueKey.keya = keyTranslator.GetKeyFromRepresentation(valueKey.key);
                    valueKey.decryption = sender.Decrypt(this.encryptedData, valueKey.keya, InitVector, bytesToUse);
                    valuequeue.Enqueue(valueKey);
                }
            }
        }

        private bool BruteforceCPU(IKeyTranslator keyTranslator, IControlEncryption sender, int bytesToUse)
        {
            bool finish = false;
            for (int count = 0; count < 256 * 256; count++)
            {
                byte[] keya = keyTranslator.GetKey();

                if (!decryptAndCalculate(sender, bytesToUse, keya, keyTranslator))
                    throw new Exception("Bruteforcing not possible!");

                finish = !keyTranslator.NextKey();
                if (finish)
                    break;
            }
            return finish;
        }

        private IKeyTranslator ShareKeys(KeyPattern.KeyPattern[] patterns, int threadid, BigInteger[] keysLeft, IKeyTranslator keyTranslator, Stack threadStack)
        {
            BigInteger size;
            if (maxThread == threadid && threadStack.Count != 0)
            {
                try
                {
                    maxThreadMutex.WaitOne();
                    if (maxThread == threadid && threadStack.Count != 0)
                    {
                        KeyPattern.KeyPattern[] split = patterns[threadid].split();
                        if (split != null)
                        {
                            patterns[threadid] = split[0];
                            keyTranslator = ControlMaster.getKeyTranslator();
                            keyTranslator.SetKeys(patterns[threadid]);

                            ThreadStackElement elem = (ThreadStackElement)threadStack.Pop();
                            patterns[elem.threadid] = split[1];
                            elem.ev.Set();    //wake the other thread up                                    
                            size = patterns[threadid].size();
                            keysLeft[threadid] = size;
                        }
                        maxThread = -1;
                    }
                }
                finally
                {
                    maxThreadMutex.ReleaseMutex();
                }
            }
            return keyTranslator;
        }

        private void WaitForNewPattern(KeyPattern.KeyPattern[] patterns, int threadid, Stack threadStack)
        {
            ThreadStackElement el = new ThreadStackElement();
            el.ev = new AutoResetEvent(false);
            el.threadid = threadid;
            patterns[threadid] = null;
            threadStack.Push(el);
            GuiLogMessage("Thread waiting for new keys.", NotificationLevel.Debug);
            el.ev.WaitOne();
            if (!stop)
            {
                GuiLogMessage("Thread waking up with new keys.", NotificationLevel.Debug);
            }
        }

        #region bruteforce methods

        private bool decryptAndCalculate(IControlEncryption sender, int bytesToUse, byte[] keya, IKeyTranslator keyTranslator)
        {
            ValueKey valueKey;

            try
            {
                if (this.encryptedData != null && this.encryptedData.Length > 0)
                {
                    valueKey.decryption = sender.Decrypt(this.encryptedData, keya, InitVector, bytesToUse);
                }
                else
                {
                    GuiLogMessage("Can't bruteforce empty input!", NotificationLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Decryption is not possible: " + ex.Message, NotificationLevel.Error);
                GuiLogMessage("Stack Trace: " + ex.StackTrace, NotificationLevel.Error);
                return false;
            }

            try
            {
                valueKey.value = CostMaster.calculateCost(valueKey.decryption);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Cost calculation is not possible: " + ex.Message, NotificationLevel.Error);
                return false;
            }

            if (this.costMaster.getRelationOperator() == RelationOperator.LargerThen)
            {
                if (valueKey.value > value_threshold)
                {
                    valueKey.key = keyTranslator.GetKeyRepresentation();
                    valueKey.keya = (byte[])keya.Clone();
                    valuequeue.Enqueue(valueKey);                    
                }
            }
            else
            {
                if (valueKey.value < value_threshold)
                {
                    valueKey.key = keyTranslator.GetKeyRepresentation();
                    valueKey.keya = (byte[])keya.Clone();                 
                    valuequeue.Enqueue(valueKey);
                }
            }
            return true;
        }

        #endregion

        #endregion

        public void process(IControlEncryption sender)
        {
            if (sender == null || costMaster == null)
                return;
            if (!Pattern.testWildcardKey(settings.Key))
            {
                GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                return;
            }
            Pattern.WildcardKey = settings.Key;
            this.sender = sender;

            bruteforcePattern(Pattern);
        }

        internal LinkedList<ValueKey> costList = new LinkedList<ValueKey>();
        private int bytesToUse;
        private IControlEncryption sender;
        private DateTime beginBruteforcing;
        private DistributedBruteForceManager distributedBruteForceManager;

        // main entry point to the KeySearcher
        private LinkedList<ValueKey> bruteforcePattern(KeyPattern.KeyPattern pattern)
        {
            beginBruteforcing = DateTime.Now;
            GuiLogMessage("Start bruteforcing pattern '" + pattern.getKey() + "'", NotificationLevel.Debug);
                        
            int maxInList = 10;
            costList = new LinkedList<ValueKey>();
            fillListWithDummies(maxInList, costList);
            valuequeue = Queue.Synchronized(new Queue());

            statistic = new Dictionary<string, Dictionary<long, Information>>();
            initialized = false;

            stop = false;
            if (!pattern.testWildcardKey(settings.Key))
            {
                GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                return null;
            }

            // bytesToUse = 0;

            try
            {
                bytesToUse = CostMaster.getBytesToUse();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Bytes used not valid: " + ex.Message, NotificationLevel.Error);
                return null;
            }

            Thread serverThread = null;
            try
            {
                if (settings.UseExternalClient)
                {
                    GuiLogMessage("Waiting for external client!", NotificationLevel.Info);
                    cryptoolServer = new CryptoolServer();
                    externalClientConnected = null;
                    cryptoolServer.Port = settings.Port;
                    cryptoolServer.OnJobCompleted += server_OnJobCompleted;
                    cryptoolServer.OnClientConnected += server_OnClientConnected;
                    cryptoolServer.OnClientDisconnected += cryptoolServer_OnClientDisconnected;
                    serverThread = new Thread(new ThreadStart(delegate
                                                                      {
                                                                          cryptoolServer.Run();
                                                                      }));
                    serverThread.Start();
                }

                if (settings.UsePeerToPeer)
                {
                    BruteForceWithPeerToPeerSystem();
                    return null;
                }

                return BruteForceWithLocalSystem(pattern);
            }
            finally
            {
                if (stop && serverThread != null)
                {
                    //stop server here!
                    serverThread.Interrupt();
                    cryptoolServer.OnJobCompleted -= server_OnJobCompleted;
                    cryptoolServer.OnClientConnected -= server_OnClientConnected;
                    cryptoolServer.OnClientDisconnected -= cryptoolServer_OnClientDisconnected;
                }
            }
        }

        private void BruteForceWithPeerToPeerSystem()
        {
            if (!update)
            {
                GuiLogMessage("Launching p2p based bruteforce logic...", NotificationLevel.Info);

                try
                {
                    distributedBruteForceManager = new DistributedBruteForceManager(this, pattern, settings,
                                                                                    keyQualityHelper,
                                                                                    p2PQuickWatchPresentation);
                    distributedBruteForceManager.Execute();
                }
                catch (NotConnectedException)
                {
                    GuiLogMessage("P2P not connected.", NotificationLevel.Error);
                }
                catch (KeySearcherStopException)
                {
                    update = true;
                    return;
                }
            }
            else
            {
                GuiLogMessage("Keysearcher Fullstop.Please Update your Version.", NotificationLevel.Error);
                Thread.Sleep(3000);
            }
        }

        internal LinkedList<ValueKey> BruteForceWithLocalSystem(KeyPattern.KeyPattern pattern, bool redirectResultsToStatisticsGenerator = false)
        {
            ((QuickWatch)QuickWatchPresentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                openCLPresentationMutex.WaitOne();
                ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.AmountOfDevices = 0;
                openCLPresentationMutex.ReleaseMutex();
            }, null);

            if (!redirectResultsToStatisticsGenerator)
            {
                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(SetStartDate));
                localBruteForceStopwatch.Start();
            }

            if (settings.UseExternalClient)
            {
                GuiLogMessage("Only using external client to bruteforce!", NotificationLevel.Info);
                lock (this)
                {
                    externalKeySearcherOpenCLCode = new KeySearcherOpenCLCode(this, encryptedData, sender, CostMaster,
                                                                              256*256*256*64);
                    externalKeysProcessed = 0;
                    externalKeyTranslator = ControlMaster.getKeyTranslator();
                    externalKeyTranslator.SetKeys(pattern);
                    if (externalClientConnected != null)
                        AssignJobToClient(externalClientConnected, externalKeySearcherOpenCLCode.CreateOpenCLBruteForceCode(externalKeyTranslator));
                }
                waitForExternalClientToFinish.Reset();
                waitForExternalClientToFinish.WaitOne();
            }
            else
            {
                BigInteger size = pattern.size();
                KeyPattern.KeyPattern[] patterns = splitPatternForThreads(pattern);
                if (patterns == null || patterns.Length == 0)
                {
                    GuiLogMessage("No ressources to BruteForce available. Check the KeySearcher settings!", NotificationLevel.Error);
                    throw new Exception("No ressources to BruteForce available. Check the KeySearcher settings!");
                }

                BigInteger[] doneKeysA = new BigInteger[patterns.Length];
                BigInteger[] openCLDoneKeysA = new BigInteger[patterns.Length];
                BigInteger[] keycounters = new BigInteger[patterns.Length];
                BigInteger[] keysleft = new BigInteger[patterns.Length];
                Stack threadStack = Stack.Synchronized(new Stack());
                threadsStopEvents = ArrayList.Synchronized(new ArrayList());
                StartThreads(sender, bytesToUse, patterns, doneKeysA, openCLDoneKeysA, keycounters, keysleft, threadStack);

                DateTime lastTime = DateTime.Now;

                //update message:
                while (!stop)
                {
                    Thread.Sleep(2000);

                    updateToplist();

                    #region calculate global counters from local counters
                    BigInteger keycounter = 0;
                    BigInteger doneKeys = 0;
                    BigInteger openCLdoneKeys = 0;
                    foreach (BigInteger dk in doneKeysA)
                        doneKeys += dk;
                    foreach (BigInteger dk in openCLDoneKeysA)
                        openCLdoneKeys += dk;
                    foreach (BigInteger kc in keycounters)
                        keycounter += kc;
                    #endregion

                    if (keycounter > size)
                        GuiLogMessage("There must be an error, because we bruteforced too much keys...", NotificationLevel.Error);

                    #region determination of the thread with most keys
                    if (size - keycounter > 1000)
                    {
                        try
                        {
                            maxThreadMutex.WaitOne();
                            BigInteger max = 0;
                            int id = -1;
                            for (int i = 0; i < patterns.Length; i++)
                                if (keysleft[i] != null && keysleft[i] > max)
                                {
                                    max = keysleft[i];
                                    id = i;
                                }
                            maxThread = id;
                        }
                        finally
                        {
                            maxThreadMutex.ReleaseMutex();
                        }
                    }
                    #endregion

                    long keysPerSecond = (long)((long)doneKeys / (DateTime.Now - lastTime).TotalSeconds);
                    long openCLKeysPerSecond = (long)((long)openCLdoneKeys / (DateTime.Now - lastTime).TotalSeconds);
                    lastTime = DateTime.Now;
                    if (redirectResultsToStatisticsGenerator)
                    {
                        distributedBruteForceManager.StatisticsGenerator.ShowProgress(costList, size, keycounter, keysPerSecond);
                    }
                    else
                    {
                        showProgress(costList, size, keycounter, keysPerSecond);
                    }

                    //show OpenCL keys/sec:
                    var ratio = (double)openCLdoneKeys / (double)doneKeys;
                    ((QuickWatch)QuickWatchPresentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.keysPerSecondOpenCL.Content = String.Format("{0:N}", openCLKeysPerSecond);
                        ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.keysPerSecondCPU.Content = String.Format("{0:N}", (keysPerSecond - openCLKeysPerSecond));
                        ((QuickWatch)QuickWatchPresentation).OpenCLPresentation.ratio.Content = String.Format("{0:P}", ratio);
                    }, null);


                    #region set doneKeys to 0
                    doneKeys = 0;
                    for (int i = 0; i < doneKeysA.Length; i++)
                        doneKeysA[i] = 0;
                    openCLdoneKeys = 0;
                    for (int i = 0; i < openCLDoneKeysA.Length; i++)
                        openCLDoneKeysA[i] = 0;
                    #endregion

                    if (keycounter >= size)
                        break;
                }//end while

                showProgress(costList, 1, 1, 1);

                //wake up all sleeping threads, so they can stop:
                while (threadStack.Count != 0)
                    ((ThreadStackElement)threadStack.Pop()).ev.Set();

                //wait until all threads finished:
                foreach (AutoResetEvent stopEvent in threadsStopEvents)
                {
                    stopEvent.WaitOne();
                }

                if (!stop && !redirectResultsToStatisticsGenerator)
                    ProgressChanged(1, 1);

            }

            /* BEGIN: For evaluation issues - added by Arnold 2010.03.17 */
            TimeSpan bruteforcingTime = DateTime.Now.Subtract(beginBruteforcing);
            StringBuilder sbBFTime = new StringBuilder();
            if (bruteforcingTime.Days > 0)
                sbBFTime.Append(bruteforcingTime.Days.ToString() + " days ");
            if (bruteforcingTime.Hours > 0)
            {
                if (bruteforcingTime.Hours <= 9)
                    sbBFTime.Append("0");
                sbBFTime.Append(bruteforcingTime.Hours.ToString() + ":");
            }
            if (bruteforcingTime.Minutes <= 9)
                sbBFTime.Append("0");
            sbBFTime.Append(bruteforcingTime.Minutes.ToString() + ":");
            if (bruteforcingTime.Seconds <= 9)
                sbBFTime.Append("0");
            sbBFTime.Append(bruteforcingTime.Seconds.ToString() + "-");
            if (bruteforcingTime.Milliseconds <= 9)
                sbBFTime.Append("00");
            if (bruteforcingTime.Milliseconds <= 99)
                sbBFTime.Append("0");
            sbBFTime.Append(bruteforcingTime.Milliseconds.ToString());

            GuiLogMessage("Ended bruteforcing pattern '" + pattern.getKey() + "'. Bruteforcing TimeSpan: " + sbBFTime.ToString(), NotificationLevel.Debug);
            /* END: For evaluation issues - added by Arnold 2010.03.17 */

            return costList;
        }

        #region External Client

        void cryptoolServer_OnClientDisconnected(EndPoint client)
        {
            GuiLogMessage("Client disconnected!", NotificationLevel.Info);
            externalClientConnected = null;
        }

        void server_OnClientConnected(System.Net.EndPoint client, string identification)
        {
            lock (this)
            {
                if (externalClientConnected == null)
                {
                    externalClientConnected = client;
                    GuiLogMessage(string.Format("Client {0} connected!", identification), NotificationLevel.Info);
                    AssignJobToClient(client, externalKeySearcherOpenCLCode.CreateOpenCLBruteForceCode(externalKeyTranslator));
                }
                else
                {
                    GuiLogMessage("Client tried to connect, but only one client allowed!", NotificationLevel.Info);
                }
            }
        }

        private void AssignJobToClient(EndPoint client, string src)
        {
            JobInput j = new JobInput();
            j.Guid = Guid.NewGuid().ToString();
            j.Src = src;
            var key = externalKeyTranslator.GetKey();
            j.Key = key;
            j.LargerThen = (costMaster.getRelationOperator() == RelationOperator.LargerThen);
            j.Size = externalKeyTranslator.GetOpenCLBatchSize();
            j.ResultSize = 10;
            GuiLogMessage(string.Format("Assigning new job with Guid {0} to client!", j.Guid), NotificationLevel.Info);
            cryptoolServer.SendJob(j, client);
            assignTime = DateTime.Now;
        }

        void server_OnJobCompleted(System.Net.EndPoint client, JobResult jr)
        {
            GuiLogMessage(string.Format("Client returned result of job with Guid {0}!", jr.Guid), NotificationLevel.Info);
            //check:
            var op = this.costMaster.getRelationOperator();
            foreach (var res in jr.ResultList)
            {
                float cost = res.Key;
                if (((op == RelationOperator.LargerThen) && (cost > value_threshold))
                    || (op == RelationOperator.LessThen) && (cost < value_threshold))
                {
                    ValueKey valueKey = new ValueKey { value = cost, key = externalKeyTranslator.GetKeyRepresentation(res.Value) };
                    valueKey.keya = externalKeyTranslator.GetKeyFromRepresentation(valueKey.key);
                    valueKey.decryption = sender.Decrypt(this.encryptedData, valueKey.keya, InitVector, bytesToUse);
                    valuequeue.Enqueue(valueKey);
                }
            }
            updateToplist();

            //progress:
            externalKeyTranslator.NextOpenCLBatch();
            int progress = externalKeyTranslator.GetProgress();
            externalKeysProcessed += progress;
            int keysPerSec = (int)(progress / (DateTime.Now - assignTime).TotalSeconds);
            showProgress(costList, pattern.size(), externalKeysProcessed, keysPerSec);

            if (externalKeysProcessed != pattern.size())
            {
                AssignJobToClient(client, null);
            }
            else
            {
                waitForExternalClientToFinish.Set();
            }
        }

        #endregion

        private void SetStartDate()
        {
            localQuickWatchPresentation.startTime.Content = DateTime.Now.ToString("g", Thread.CurrentThread.CurrentCulture); ;
        }

        internal void showProgress(LinkedList<ValueKey> costList, BigInteger size, BigInteger keycounter, long keysPerSecond)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            LinkedListNode<ValueKey> linkedListNode;
            ProgressChanged((double)keycounter / (double)size, 1.0);

            if (localQuickWatchPresentation.IsVisible && keysPerSecond != 0 && !stop)
            {
                double time = (Math.Pow(10, BigInteger.Log((size - keycounter), 10) - Math.Log10(keysPerSecond)));
                TimeSpan timeleft = new TimeSpan(-1);

                try
                {
                    if (time / (24 * 60 * 60) <= int.MaxValue)
                    {
                        int days = (int)(time / (24 * 60 * 60));
                        time = time - (days * 24 * 60 * 60);
                        int hours = (int)(time / (60 * 60));
                        time = time - (hours * 60 * 60);
                        int minutes = (int)(time / 60);
                        time = time - (minutes * 60);
                        int seconds = (int)time;

                        timeleft = new TimeSpan(days, hours, minutes, (int)seconds, 0);
                    }
                }
                catch
                {
                    //can not calculate time span
                }

                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    localQuickWatchPresentation.elapsedTime.Content = localBruteForceStopwatch.Elapsed;
                    localQuickWatchPresentation.keysPerSecond.Content = String.Format("{0:0,0}", keysPerSecond);
                    if (timeleft != new TimeSpan(-1))
                    {
                        localQuickWatchPresentation.timeLeft.Content = "" + timeleft;
                        try
                        {
                            localQuickWatchPresentation.endTime.Content = "" + DateTime.Now.Add(timeleft);
                        }
                        catch
                        {
                            localQuickWatchPresentation.endTime.Content = "in a galaxy far, far away...";
                        }
                    }
                    else
                    {
                        localQuickWatchPresentation.timeLeft.Content = "incalculable :-)";
                        localQuickWatchPresentation.endTime.Content = "in a galaxy far, far away...";
                    }

                    localQuickWatchPresentation.entries.Clear();
                    linkedListNode = costList.First;

                    int i = 0;
                    while (linkedListNode != null)
                    {
                        i++;

                        ResultEntry entry = new ResultEntry();
                        entry.Ranking = "" + i;
                        entry.Value = "" + Math.Round(linkedListNode.Value.value, 3);
                        entry.Key = linkedListNode.Value.key;
                        entry.Text = enc.GetString(linkedListNode.Value.decryption);

                        localQuickWatchPresentation.entries.Add(entry);
                        linkedListNode = linkedListNode.Next;
                    }
                }
                , null);
            }//end if
            else if (!stop && localQuickWatchPresentation.IsVisible)
            {

                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    localQuickWatchPresentation.entries.Clear();
                    linkedListNode = costList.First;
                    int i = 0;

                    while (linkedListNode != null)
                    {
                        i++;

                        ResultEntry entry = new ResultEntry();
                        entry.Ranking = "" + i;
                        entry.Value = "" + Math.Round(linkedListNode.Value.value, 3);
                        entry.Key = linkedListNode.Value.key;
                        entry.Text = enc.GetString(linkedListNode.Value.decryption);

                        localQuickWatchPresentation.entries.Add(entry);
                        linkedListNode = linkedListNode.Next;
                    }
                }
                , null);
            }
        }

        #region For TopList

        private void fillListWithDummies(int maxInList, LinkedList<ValueKey> costList)
        {
            ValueKey valueKey = new ValueKey();
            if (this.costMaster.getRelationOperator() == RelationOperator.LessThen)
                valueKey.value = double.MaxValue;
            else
                valueKey.value = double.MinValue;
            valueKey.key = "dummykey";
            valueKey.decryption = new byte[0];
            value_threshold = valueKey.value;
            LinkedListNode<ValueKey> node = costList.AddFirst(valueKey);
            for (int i = 1; i < maxInList; i++)
            {
                node = costList.AddAfter(node, valueKey);
            }
        }

        public void SetInitialized(bool ini)
        {
            this.initialized = ini;
        }

        internal void IntegrateNewResults(LinkedList<ValueKey> updatedCostList, Dictionary<string, Dictionary<long, Information>> updatedStatistics, string dataIdentifier)
        {
            foreach (var valueKey in updatedCostList)
            {
                if (keyQualityHelper.IsBetter(valueKey.value, value_threshold))
                {
                    valuequeue.Enqueue(valueKey);
                }
            }

            foreach (string avname in updatedStatistics.Keys)
            {
                //taking the dictionary in this avatarname
                Dictionary<long, Information> MaschCount = updatedStatistics[avname];

                //if the avatarname already exists in the statistics
                if (statistic.ContainsKey(avname))
                {
                    foreach (long id in MaschCount.Keys)
                    {
                        //get the statistic maschcount for this avatarname
                        Dictionary<long, Information> statMaschCount = statistic[avname];
                        //ID of the statistic (has to be always positive)
                        var mID = Math.Abs(id);
                        //if the id of the Maschine already exists for this avatarname
                        if (statMaschCount.ContainsKey(mID))
                        {
                            if (!initialized || ((MaschCount[id].Count == 1) && (MaschCount.Keys.Count == 1)))
                            {
                                statMaschCount[mID].Count = statMaschCount[mID].Count + MaschCount[id].Count;
                                statMaschCount[mID].Hostname = MaschCount[id].Hostname;
                                statMaschCount[mID].Date = MaschCount[id].Date;
                                statistic[avname] = statMaschCount;
                            }
                        }
                        else
                        {
                            //add a new id,information value for this avatarname
                            statistic[avname].Add(mID, MaschCount[id]);
                        }
                    }
                }
                else
                {
                    //add the maschinecount dictionary to this avatarname
                    statistic[avname] = MaschCount;
                }
            }
            WriteStatistics(dataIdentifier);
            updateToplist();
        }

        //Write the User Statistics to an external csv-document
        internal void WriteStatistics(String dataIdentifier)
        {
            using (StreamWriter sw = new StreamWriter(string.Format("{0}\\UserRanking{1}.csv", DirectoryHelper.DirectoryLocal, dataIdentifier)))
            {
                sw.WriteLine("Avatarname" + ";" + "MaschineID" + ";" + "Hostname" + ";"+ "Pattern Count" + ";" + "Last Update");
                foreach (string avatar in statistic.Keys)
                {
                    foreach(long mID in statistic[avatar].Keys)
                    {
                        sw.WriteLine(avatar + ";" + mID.ToString() + ";" + statistic[avatar][mID].Hostname + ";" + statistic[avatar][mID].Count + ";" + statistic[avatar][mID].Date);
                    }
                }
            }
        }

        internal void updateToplist()
        {
            LinkedListNode<ValueKey> node;
            while (valuequeue.Count != 0)
            {
                ValueKey vk = (ValueKey)valuequeue.Dequeue();

                //if (costList.Contains(vk)) continue;
                var result = costList.Where(valueKey => valueKey.key == vk.key);
                if (result.Count() > 0)
                {
                    continue;
                }

                if (this.costMaster.getRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
                                if (node == costList.First)
                                    Top1 = vk;
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                        }//end while
                    }//end if
                }
                else
                {
                    if (vk.value < costList.Last().value)
                    {
                        node = costList.First;
                        while (node != null)
                        {
                            if (vk.value < node.Value.value)
                            {
                                if (node == costList.First)
                                    Top1 = vk;
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                        }//end while
                    }//end if
                }
            }
        }

        #endregion

        private void StartThreads(IControlEncryption sender, int bytesToUse, KeyPattern.KeyPattern[] patterns, BigInteger[] doneKeysA, BigInteger[] openCLDoneKeysA, BigInteger[] keycounters, BigInteger[] keysleft, Stack threadStack)
        {
            //First start the opencl threads:
            int i = 0;
            foreach (var ds in settings.DeviceSettings)
            {
                if (ds.useDevice)
                {
                    WaitCallback worker = new WaitCallback(KeySearcherJob);
                    doneKeysA[i] = new BigInteger();
                    openCLDoneKeysA[i] = new BigInteger();
                    keycounters[i] = new BigInteger();

                    ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, openCLDoneKeysA, keycounters, keysleft, sender, bytesToUse, threadStack, ds });
                    i++;
                }
            }

            //Then the CPU threads:
            for (; i < patterns.Length; i++)
            {
                WaitCallback worker = new WaitCallback(KeySearcherJob);
                doneKeysA[i] = new BigInteger();
                openCLDoneKeysA[i] = new BigInteger();
                keycounters[i] = new BigInteger();

                ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, openCLDoneKeysA, keycounters, keysleft, sender, bytesToUse, threadStack, null });
            }
        }

        private KeyPattern.KeyPattern[] splitPatternForThreads(KeyPattern.KeyPattern pattern)
        {
            int threads = settings.CoresUsed;
            threads += settings.DeviceSettings.Count(x => x.useDevice); 

            if (threads < 1)
                return null;

            KeyPattern.KeyPattern[] patterns = new KeyPattern.KeyPattern[threads];
            if (threads > 1)
            {
                KeyPattern.KeyPattern[] patterns2 = pattern.split();
                if (patterns2 == null)
                {
                    patterns2 = new KeyPattern.KeyPattern[1];
                    patterns2[0] = pattern;
                    return patterns2;
                }
                patterns[0] = patterns2[0];
                patterns[1] = patterns2[1];
                int p = 1;
                threads -= 2;

                while (threads > 0)
                {
                    int maxPattern = -1;
                    BigInteger max = 0;
                    for (int i = 0; i <= p; i++)
                        if (patterns[i].size() > max)
                        {
                            max = patterns[i].size();
                            maxPattern = i;
                        }
                    KeyPattern.KeyPattern[] patterns3 = patterns[maxPattern].split();
                    if (patterns3 == null)
                    {
                        patterns3 = new KeyPattern.KeyPattern[p+1];
                        for (int i = 0; i <= p; i++)
                            patterns3[i] = patterns[i];
                        return patterns3;
                    }
                    patterns[maxPattern] = patterns3[0];
                    patterns[++p] = patterns3[1];
                    threads--;
                }
            }
            else
                patterns[0] = pattern;
            return patterns;
        }

        private void keyPatternChanged()
        {
            Pattern = new KeyPattern.KeyPattern(controlMaster.getKeyPattern());
        }

        // added by Arnie - 2009.12.07
        public delegate void BruteforcingEnded(LinkedList<ValueKey> top10List);
        /// <summary>
        /// This event gets thrown after Bruteforcing had ended. This is no evidence, that bruteforcing was successful.
        /// But when the returned List is filled, we have (at least a part) of the possible best keys
        /// </summary>
        public event BruteforcingEnded OnBruteforcingEnded;

        // added by Arnie -2009.12.02
        // for inheritance reasons
        public void BruteforcePattern(KeyPattern.KeyPattern pattern, byte[] encryptedData, byte[] initVector, IControlEncryption encryptControl, IControlCost costControl)
        {
            /* Begin: New stuff because of changing the IControl data flow - Arnie 2010.01.18 */
            this.encryptedData = encryptedData;
            this.initVector = initVector;
            /* End: New stuff because of changing the IControl data flow - Arnie 2010.01.18 */

            this.sender = encryptControl;
            LinkedList<ValueKey> lstRet = bruteforcePattern(pattern);
            if(OnBruteforcingEnded != null)
                OnBruteforcingEnded(lstRet);
        }

        #endregion

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));

            }
        }

        /// <summary>
        /// used for delivering the results from the worker threads to the main thread:
        /// </summary>
        public struct ValueKey
        {
            public double value;
            public String key;
            public byte[] decryption;
            public byte[] keya;
        };
    }

    /// <summary>
    /// Represents one entry in our result list
    /// </summary>
    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
    }
    /// <summary>
    /// Represents one entry in our statistic list
    /// </summary>
    public class Information
    {
        public int Count { get; set; }
        public string Hostname { get; set; }
        public DateTime Date { get; set; }
    } 
}