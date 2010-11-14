/*                              
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
using KeySearcher.P2P;
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
        /// used for creating the TopList
        /// </summary>
        private Queue valuequeue;
        private double value_threshold;
        /// <summary>
        /// the thread with the most keys left
        /// </summary>
        private int maxThread;
        private readonly Mutex maxThreadMutex = new Mutex();

        public bool IsKeySearcherRunning;
        private KeyQualityHelper keyQualityHelper;
        private readonly P2PQuickWatchPresentation p2PQuickWatchPresentation;
        private readonly LocalQuickWatchPresentation localQuickWatchPresentation;

        private OpenCLManager oclManager = null;

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

        public KeySearcher()
        {
            IsKeySearcherRunning = false;
            if (OpenCL.NumberOfPlatforms > 0)
            {
                oclManager = new OpenCLManager();
                oclManager.AttemptUseBinaries = false;
                oclManager.AttemptUseSource = true;
                oclManager.RequireImageSupport = false;
                oclManager.BuildOptions = "";
                oclManager.CreateDefaultContext(0, DeviceType.ALL);
            }

            settings = new KeySearcherSettings(this, oclManager);
            QuickWatchPresentation = new QuickWatch();
            localQuickWatchPresentation = ((QuickWatch) QuickWatchPresentation).LocalQuickWatchPresentation;
            p2PQuickWatchPresentation = ((QuickWatch)QuickWatchPresentation).P2PQuickWatchPresentation;
            p2PQuickWatchPresentation.UpdateSettings(this, settings);

            settings.PropertyChanged += SettingsPropertyChanged;

            localBruteForceStopwatch = new Stopwatch();
        }

        void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            p2PQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                             new Action(UpdateIsP2PEnabledSetting));
        }

        void UpdateIsP2PEnabledSetting()
        {
            ((QuickWatch)QuickWatchPresentation).IsP2PEnabled = settings.UsePeerToPeer;
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
        }

        // because Encryption PlugIns were changed radical, the new StartPoint is here - Arnie 2010.01.12
        public virtual void Execute()
        {
            IsKeySearcherRunning = true;

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
            object[] parameters = (object[])param;
            KeyPattern.KeyPattern[] patterns = (KeyPattern.KeyPattern[])parameters[0];
            int threadid = (int)parameters[1];
            BigInteger[] doneKeysArray = (BigInteger[])parameters[2];
            BigInteger[] keycounterArray = (BigInteger[])parameters[3];
            BigInteger[] keysLeft = (BigInteger[])parameters[4];
            IControlEncryption sender = (IControlEncryption)parameters[5];
            int bytesToUse = (int)parameters[6];
            Stack threadStack = (Stack)parameters[7];
            bool useOpenCL = (bool)parameters[8];

            KeySearcherOpenCLCode keySearcherOpenCLCode = null;
            if (useOpenCL)
                keySearcherOpenCLCode = new KeySearcherOpenCLCode(encryptedData, sender, CostMaster, 256 * 64);

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

                        if (!useOpenCL)         //CPU
                        {
                            finish = BruteforceCPU(keyTranslator, sender, bytesToUse);
                        }
                        else                    //OpenCL
                        {
                            try
                            {
                                finish = BruteforceOpenCL(keySearcherOpenCLCode, keyTranslator, sender, bytesToUse);
                            }
                            catch (Exception)
                            {
                                useOpenCL = false;
                                continue;
                            }
                        }
                        
                        int progress = keyTranslator.GetProgress();

                        doneKeysArray[threadid] += progress;
                        keycounterArray[threadid] += progress;
                        keysLeft[threadid] -= progress;

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
            }
        }

        private unsafe bool BruteforceOpenCL(KeySearcherOpenCLCode keySearcherOpenCLCode, IKeyTranslator keyTranslator, IControlEncryption sender, int bytesToUse)
        {
            float[] costArray = new float[keySearcherOpenCLCode.GetBruteforceBlock()];
            try
            {
                Kernel bruteforceKernel = keySearcherOpenCLCode.GetBruteforceKernel(oclManager, keyTranslator);
                int deviceIndex = settings.OpenCLDevice;
                
                Mem costs = oclManager.Context.CreateBuffer(MemFlags.READ_ONLY, costArray.Length * 4);
                IntPtr[] globalWorkSize = { (IntPtr)keySearcherOpenCLCode.GetBruteforceBlock() };

                Mem userKey;
                var key = keyTranslator.GetKey();
                fixed (byte* ukp = key)
                    userKey = oclManager.Context.CreateBuffer(MemFlags.USE_HOST_PTR, key.Length, new IntPtr((void*)ukp));

                bruteforceKernel.SetArg(0, userKey);
                bruteforceKernel.SetArg(1, costs);

                oclManager.CQ[deviceIndex].EnqueueNDRangeKernel(bruteforceKernel, 1, null, globalWorkSize, null);
                oclManager.CQ[deviceIndex].EnqueueBarrier();

                Event e;
                fixed (float* costa = costArray)
                    oclManager.CQ[deviceIndex].EnqueueReadBuffer(costs, true, 0, costArray.Length * 4, new IntPtr((void*)costa), 0, null, out e);

                e.Wait();
                costs.Dispose();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                GuiLogMessage("Bruteforcing with OpenCL failed! Using CPU instead.", NotificationLevel.Error);
                throw new Exception("Bruteforcing with OpenCL failed!");
            }

            //Check results:
            var op = this.costMaster.getRelationOperator();
            for (int i = 0; i < costArray.Length; i++)
            {
                float cost = costArray[i];
                if (((op == RelationOperator.LargerThen) && (cost > value_threshold))
                    || (op == RelationOperator.LessThen) && (cost < value_threshold))
                {
                    ValueKey valueKey = new ValueKey {value = cost, key = keyTranslator.GetKeyRepresentation(i)};
                    valueKey.keya = keyTranslator.GetKeyFromRepresentation(valueKey.key);
                    valueKey.decryption = sender.Decrypt(this.encryptedData, valueKey.keya, InitVector, bytesToUse);
                    valuequeue.Enqueue(valueKey);
                }
            }

            return !keyTranslator.NextOpenCLBatch();
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

            if (settings.UsePeerToPeer)
            {
                BruteForceWithPeerToPeerSystem();
                return null;
            }

            return BruteForceWithLocalSystem(pattern);
        }

        private void BruteForceWithPeerToPeerSystem()
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
        }

        internal LinkedList<ValueKey> BruteForceWithLocalSystem(KeyPattern.KeyPattern pattern, bool redirectResultsToStatisticsGenerator = false)
        {
            if (!redirectResultsToStatisticsGenerator)
            {
                localQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(SetStartDate));
                localBruteForceStopwatch.Start();
            }

            BigInteger size = pattern.size();
            KeyPattern.KeyPattern[] patterns = splitPatternForThreads(pattern);
            if (patterns == null || patterns.Length == 0)
            {
                GuiLogMessage("No ressources to BruteForce available. Check the KeySearcher settings!", NotificationLevel.Error);
                throw new Exception("No ressources to BruteForce available. Check the KeySearcher settings!");
            }

            BigInteger[] doneKeysA = new BigInteger[patterns.Length];
            BigInteger[] keycounters = new BigInteger[patterns.Length];
            BigInteger[] keysleft = new BigInteger[patterns.Length];
            Stack threadStack = Stack.Synchronized(new Stack());
            StartThreads(sender, bytesToUse, patterns, doneKeysA, keycounters, keysleft, threadStack);

            DateTime lastTime = DateTime.Now;

            //update message:
            while (!stop)
            {
                Thread.Sleep(1000);

                updateToplist();

                #region calculate global counters from local counters
                BigInteger keycounter = 0;
                BigInteger doneKeys = 0;
                foreach (BigInteger dk in doneKeysA)
                    doneKeys += dk;
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

                long keysPerSecond = (long)((long)doneKeys/(DateTime.Now - lastTime).TotalSeconds);
                lastTime = DateTime.Now;
                if (redirectResultsToStatisticsGenerator)
                {
                    distributedBruteForceManager.StatisticsGenerator.ShowProgress(costList, size, keycounter, keysPerSecond);
                }
                else
                {
                    showProgress(costList, size, keycounter, keysPerSecond);                    
                }
                

                #region set doneKeys to 0
                doneKeys = 0;
                for (int i = 0; i < doneKeysA.Length; i++)
                    doneKeysA[i] = 0;
                #endregion

                if (keycounter >= size)
                    break;
            }//end while

            showProgress(costList, 1, 1, 1);

            //wake up all sleeping threads, so they can stop:
            while (threadStack.Count != 0)
                ((ThreadStackElement)threadStack.Pop()).ev.Set();

            if (!stop && !redirectResultsToStatisticsGenerator)
                ProgressChanged(1, 1);

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



        private void SetStartDate()
        {
            localQuickWatchPresentation.startTime.Content = DateTime.Now.ToString("g", Thread.CurrentThread.CurrentCulture); ;
        }

        internal void showProgress(LinkedList<ValueKey> costList, BigInteger size, BigInteger keycounter, long keysPerSecond)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            LinkedListNode<ValueKey> linkedListNode;
            ProgressChanged((double)keycounter / (double) size, 1.0);

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
                    localQuickWatchPresentation.keysPerSecond.Content = "" + keysPerSecond;
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
                        entry.Value = "" + Math.Round(linkedListNode.Value.value,3);
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

        internal void IntegrateNewResults(LinkedList<ValueKey> updatedCostList)
        {
            foreach (var valueKey in updatedCostList)
            {
                if (keyQualityHelper.IsBetter(valueKey.value, value_threshold))
                {
                    valuequeue.Enqueue(valueKey);
                }
            }

            updateToplist();
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

        private void StartThreads(IControlEncryption sender, int bytesToUse, KeyPattern.KeyPattern[] patterns, BigInteger[] doneKeysA, BigInteger[] keycounters, BigInteger[] keysleft, Stack threadStack)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                WaitCallback worker = new WaitCallback(KeySearcherJob);
                doneKeysA[i] = new BigInteger();
                keycounters[i] = new BigInteger();
                bool useOpenCL = false;

                if (settings.UseOpenCL && (i == patterns.Length - 1))     //Last thread is the OpenCL thread
                    useOpenCL = true;

                ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, keycounters, keysleft, sender, bytesToUse, threadStack, useOpenCL });
            }
        }

        private KeyPattern.KeyPattern[] splitPatternForThreads(KeyPattern.KeyPattern pattern)
        {
            int threads = settings.CoresUsed;
            if (settings.UseOpenCL)
                threads++;

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
}