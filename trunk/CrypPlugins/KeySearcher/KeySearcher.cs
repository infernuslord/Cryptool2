using System;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using Cryptool.PluginBase.Miscellaneous;
using System.IO;
using Cryptool.PluginBase.IO;

namespace KeySearcher
{    
    [Author("Sven Rech, Nils Kopal, Raoul Falk, Dennis Nolte", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "KeySearcher", "Bruteforces a decryption algorithm.", "KeySearcher/DetailedDescription/Description.xaml", "KeySearcher/Images/icon.png")]
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
        private Mutex maxThreadMutex = new Mutex();

        private KeyPattern pattern = null;
        public KeyPattern Pattern
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

        private bool stop;

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
                    Pattern = new KeyPattern(value.getKeyPattern());
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
                    cryptoolStreamChanged = true;
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
                    cryptoolStreamChanged = true;
                    OnPropertyChanged("EncryptedData");
                }
            }
        }

        /// <summary>
        /// When the Input-Slot changed, set this variable to true, so the new Stream will be transformed to byte[]
        /// </summary>
        private bool cryptoolStreamChanged = false;
        private byte[] encryptedByteData;
        private byte[] GetByteFromCryptoolStream(CryptoolStream cryptoolStream)
        {
            // only transform CryptoolStream to Byte[], if there is a new CryptoolStream
            // or decryptedByteData is Null
            if (cryptoolStreamChanged || encryptedByteData == null)
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

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private KeySearcherSettings settings;

        public KeySearcher()
        {
            settings = new KeySearcherSettings(this);
            QuickWatchPresentation = new KeySearcherQuickWatchPresentation();
            
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
            //either byte[] CStream input or CryptoolStream Object input
            if (this.EncryptedData != null || this.CSEncryptedData != null) //to prevent execution on initialization
            {
                if (this.ControlMaster != null)
                    this.process(this.ControlMaster);
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
            stop = true;
        }

        public void Initialize()
        {
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

        /* BEGIN functionality */

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
            KeyPattern[] patterns = (KeyPattern[])parameters[0];
            int threadid = (int)parameters[1];
            BigInteger[] doneKeysArray = (BigInteger[])parameters[2];
            BigInteger[] keycounterArray = (BigInteger[])parameters[3];
            BigInteger[] keysLeft = (BigInteger[])parameters[4];
            IControlEncryption sender = (IControlEncryption)parameters[5];
            int bytesToUse = (int)parameters[6];
            Stack threadStack = (Stack)parameters[7];

            KeyPattern pattern = patterns[threadid];

            bool useKeyblocks = true;

            try
            {
                while (pattern != null)
                {
                    BigInteger size = pattern.size();
                    keysLeft[threadid] = size;
                    int nextWildcard;

                    do
                    {
                        //if we are the thread with most keys left, we have to share them:
                        if (maxThread == threadid && threadStack.Count != 0)
                        {
                            maxThreadMutex.WaitOne();
                            if (maxThread == threadid && threadStack.Count != 0)
                            {
                                try
                                {
                                    KeyPattern[] split = pattern.split();
                                    patterns[threadid] = split[0];
                                    pattern = split[0];
                                    ThreadStackElement elem = (ThreadStackElement)threadStack.Pop();
                                    patterns[elem.threadid] = split[1];
                                    elem.ev.Set();    //wake the other thread up                                    
                                    size = pattern.size();
                                    keysLeft[threadid] = size;
                                }
                                catch (Exception e)
                                {
                                    //pattern can't be split? who cares :)
                                }
                                maxThread = -1;
                            }
                            maxThreadMutex.ReleaseMutex();
                        }


                        ValueKey valueKey = new ValueKey();
                        int blocksize = 0;
                        nextWildcard = -3;
                        try
                        {
                            string key = "";
                            if (useKeyblocks)
                                key = pattern.getKeyBlock(ref blocksize, ref nextWildcard);
                            if (key == null)
                                useKeyblocks = false;
                            if (!useKeyblocks)
                                key = pattern.getKey();
                            valueKey.key = key;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get next key: " + ex.Message, NotificationLevel.Error);
                            return;
                        }

                        int[] arrayPointers = null;
                        int[] arraySuccessors = null;
                        int[] arrayUppers = null;
                        byte[] keya = ControlMaster.getKeyFromString(valueKey.key, ref arrayPointers, ref arraySuccessors, ref arrayUppers);
                        if (keya == null)
                        {
                            useKeyblocks = false;
                            nextWildcard = -2;
                            continue;   //try again
                        }

                        if (arrayPointers == null)  //decrypt only one key
                        {
                            if (!decryptAndCalculate(sender, bytesToUse, ref valueKey, keya, 0, null))
                                return;
                            doneKeysArray[threadid]++;
                            keycounterArray[threadid]++;
                            keysLeft[threadid]--;
                        }
                        else  //decrypt several keys
                        {
                            int counter = 0;
                            if (!bruteforceBlock(sender, bytesToUse, ref valueKey, keya, arrayPointers, arraySuccessors, arrayUppers, 0, ref counter, pattern))
                                return;
                            doneKeysArray[threadid] += blocksize;
                            keycounterArray[threadid] += blocksize;
                            keysLeft[threadid] -= blocksize;
                        }
                    } while (pattern.nextKey(nextWildcard) && !stop);

                    if (stop)
                        return;

                    //Let's wait until another thread is willing to share with us:
                    pattern = null;
                    ThreadStackElement el = new ThreadStackElement();
                    el.ev = new AutoResetEvent(false);
                    el.threadid = threadid;
                    patterns[threadid] = null;
                    threadStack.Push(el);
                    GuiLogMessage("Thread waiting for new keys.", NotificationLevel.Debug);
                    el.ev.WaitOne();
                    GuiLogMessage("Thread waking up with new keys.", NotificationLevel.Debug);
                    pattern = patterns[threadid];
                }
            }
            finally
            {
                sender.Dispose();
            }
        }

        #region bruteforce methods

        private bool bruteforceBlock(IControlEncryption sender, int bytesToUse, ref ValueKey valueKey, byte[] keya, int[] arrayPointers,
            int[] arraySuccessors, int[] arrayUppers, int arrayPointer, ref int counter, KeyPattern pattern)
        {
            byte store = keya[arrayPointers[arrayPointer]];
            while (!stop)
            {
                if (arrayPointer + 1 < arrayPointers.Length && arrayPointers[arrayPointer + 1] != -1)
                {
                    if (!bruteforceBlock(sender, bytesToUse, ref valueKey, keya, arrayPointers, arraySuccessors, arrayUppers, arrayPointer + 1, ref counter, pattern))
                        return false;
                }
                else
                {
                    if (!decryptAndCalculate(sender, bytesToUse, ref valueKey, keya, counter, pattern))
                        return false;
                }

                if (keya[arrayPointers[arrayPointer]] + arraySuccessors[arrayPointer] <= arrayUppers[arrayPointer])
                {
                    keya[arrayPointers[arrayPointer]] += (byte)arraySuccessors[arrayPointer];
                    counter++;
                }
                else
                    break;
            }
            keya[arrayPointers[arrayPointer]] = store;
            if (stop)
                return false;
            return true;
        }

        private bool decryptAndCalculate(IControlEncryption sender, int bytesToUse, ref ValueKey valueKey, byte[] keya, int counter, KeyPattern pattern)
        {
            try
            {
                CryptoolStream cs = new CryptoolStream();
                if (this.CSEncryptedData == null)
                {
                    cs.OpenRead(this.EncryptedData);
                    valueKey.decryption = sender.Decrypt(this.EncryptedData, keya);
                }
                else
                {
                    cs.OpenRead(this.CSEncryptedData.FileName);
                    byte[] byteCS = new byte[cs.Length];
                    cs.Read(byteCS, 0, byteCS.Length);
                    //this.CSEncryptedData.Read(byteCS, 0, byteCS.Length);
                    valueKey.decryption = sender.Decrypt(byteCS, keya);
                }

                //valueKey.decryption = sender.Decrypt(keya, bytesToUse);
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
                    if (pattern != null)
                        valueKey.key = pattern.getKey(counter);
                    valuequeue.Enqueue(valueKey);
                }
            }
            else
            {
                if (valueKey.value < value_threshold)
                {
                    if (pattern != null)
                        valueKey.key = pattern.getKey(counter);
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
            bruteforcePattern(Pattern, sender);
        }

        // modified by Christian Arnold 2009.12.07 - return type LinkedList (top10List)
        // main entry point to the KeySearcher
        private LinkedList<ValueKey> bruteforcePattern(KeyPattern pattern, IControlEncryption sender)
        {
            int maxInList = 10;
            LinkedList<ValueKey> costList = new LinkedList<ValueKey>();
            fillListWithDummies(maxInList, costList);

            stop = false;
            if (!pattern.testWildcardKey(settings.Key))
            {
                GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                return null;
            }

            int bytesToUse = 0;

            try
            {
                bytesToUse = CostMaster.getBytesToUse();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Bytes used not valid: " + ex.Message, NotificationLevel.Error);
                return null;
            }

            BigInteger size = pattern.size();
            KeyPattern[] patterns = splitPatternForThreads(pattern);

            valuequeue = Queue.Synchronized(new Queue());

            BigInteger[] doneKeysA = new BigInteger[patterns.Length];
            BigInteger[] keycounters = new BigInteger[patterns.Length];
            BigInteger[] keysleft = new BigInteger[patterns.Length];
            Stack threadStack = Stack.Synchronized(new Stack());
            startThreads(sender, bytesToUse, patterns, doneKeysA, keycounters, keysleft, threadStack);

            //update message:
            while (!stop)
            {
                Thread.Sleep(1000);

                updateToplist(costList);

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
                    maxThreadMutex.ReleaseMutex();
                }
                #endregion

                showProgress(costList, size, keycounter, doneKeys);

                #region set doneKeys to 0
                doneKeys = 0;
                for (int i = 0; i < doneKeysA.Length; i++)
                    doneKeysA[i] = 0;
                #endregion

                if (keycounter >= size)
                    break;
            }//end while

            //wake up all sleeping threads, so they can stop:
            while (threadStack.Count != 0)
                ((ThreadStackElement)threadStack.Pop()).ev.Set();

            if (!stop)
                ProgressChanged(1, 1);

            return costList;
        }

        private void showProgress(LinkedList<ValueKey> costList, BigInteger size, BigInteger keycounter, BigInteger doneKeys)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            LinkedListNode<ValueKey> linkedListNode;
            ProgressChanged(Math.Pow(10, keycounter.log(10) - size.log(10)), 1.0);

            if (QuickWatchPresentation.IsVisible && doneKeys != 0 && !stop)
            {
                double time = (Math.Pow(10, (size - keycounter).log(10) - doneKeys.log(10)));
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

                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).keysPerSecond.Text = "" + doneKeys;
                    if (timeleft != new TimeSpan(-1))
                    {
                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).timeLeft.Text = "" + timeleft;
                        try
                        {
                            ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "" + DateTime.Now.Add(timeleft);
                        }
                        catch
                        {
                            ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "in a galaxy far, far away...";
                        }
                    }
                    else
                    {
                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).timeLeft.Text = "incalculable :-)";
                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "in a galaxy far, far away...";
                    }

                    ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).entries.Clear();
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

                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).entries.Add(entry);
                        linkedListNode = linkedListNode.Next;
                    }
                }
                , null);
            }//end if


            if (!stop && QuickWatchPresentation.IsVisible)
            {

                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).entries.Clear();
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

                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).entries.Add(entry);
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

        private void updateToplist(LinkedList<ValueKey> costList)
        {
            LinkedListNode<ValueKey> node;
            while (valuequeue.Count != 0)
            {
                ValueKey vk = (ValueKey)valuequeue.Dequeue();
                if (this.costMaster.getRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
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

        private void startThreads(IControlEncryption sender, int bytesToUse, KeyPattern[] patterns, BigInteger[] doneKeysA, BigInteger[] keycounters, BigInteger[] keysleft, Stack threadStack)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                WaitCallback worker = new WaitCallback(KeySearcherJob);
                doneKeysA[i] = new BigInteger();
                keycounters[i] = new BigInteger();
                //ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, keycounters, keysleft, sender.clone(), bytesToUse, threadStack });
                ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, keycounters, keysleft, sender, bytesToUse, threadStack });
            }
        }

        private KeyPattern[] splitPatternForThreads(KeyPattern pattern)
        {
            KeyPattern[] patterns = new KeyPattern[settings.CoresUsed + 1];
            if (settings.CoresUsed > 0)
            {
                KeyPattern[] patterns2 = pattern.split();
                patterns[0] = patterns2[0];
                patterns[1] = patterns2[1];
                int p = 1;
                int threads = settings.CoresUsed - 1;
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
                    KeyPattern[] patterns3 = patterns[maxPattern].split();
                    patterns[maxPattern] = patterns3[0];
                    patterns[++p] = patterns3[1];
                    threads--;
                }
            }
            else
                patterns[0] = Pattern;
            return patterns;
        }

        private void keyPatternChanged()
        {
            Pattern = new KeyPattern(controlMaster.getKeyPattern());
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
        public void BruteforcePattern(KeyPattern pattern, byte[] encryptedData, byte[] initVector, IControlEncryption encryptControl, IControlCost costControl)
        {
            /* Begin: New stuff because of changing the IControl data flow - Arnie 2010.01.18 */
            this.EncryptedData = encryptedData;
            this.InitVector = initVector;
            /* End: New stuff because of changing the IControl data flow - Arnie 2010.01.18 */
            
            LinkedList<ValueKey> lstRet = bruteforcePattern(pattern, encryptControl);
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