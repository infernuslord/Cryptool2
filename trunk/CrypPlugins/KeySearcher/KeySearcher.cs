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

namespace KeySearcher
{
    public class KeyPattern
    {
        private class Wildcard
        {
            private char[] values = new char[256];
            private int length;
            private int counter;
            
            public Wildcard(string valuePattern)
            {
                counter = 0;
                length = 0;
                int i = 1;
                while (valuePattern[i] != ']')
                {
                    if (valuePattern[i + 1] == '-')
                    {
                        for (char c = valuePattern[i]; c <= valuePattern[i + 2]; c++)
                            values[length++] = c;
                        i += 2;
                    }
                    else
                        values[length++] = valuePattern[i];
                    i++;
                }
            }

            public Wildcard(Wildcard wc)
            {
                length = wc.length;
                counter = wc.counter;
                for (int i = 0; i < 256; i++)
                    values[i] = wc.values[i];
            }

            private Wildcard()
            {
            }

            public Wildcard[] split()
            {
                int length = this.length - this.counter;
                Wildcard[] wcs = new Wildcard[2];
                wcs[0] = new Wildcard();
                wcs[0].counter = 0;
                wcs[0].length = length / 2;
                wcs[1] = new Wildcard();
                wcs[1].counter = 0;
                wcs[1].length = length - wcs[0].length;
                for (int i = 0; i < wcs[0].length; i++)
                    wcs[0].values[i] = values[this.counter + i];
                for (int i = 0; i < wcs[1].length; i++)
                    wcs[1].values[i] = values[i + this.counter + wcs[0].length];
                return wcs;
            }

            public char getChar()
            {
                return values[counter];
            }

            public bool succ()
            {
                counter++;
                if (counter >= length)
                {
                    counter = 0;
                    return true;
                }
                return false;
            }

            public int size()
            {
                return length;
            }


            public int count()
            {
                return counter;
            }

            public void resetCounter()
            {
                counter = 0;
            }
        }

        private string pattern;
        private string key;        
        private ArrayList wildcardList;

        public KeyPattern(string pattern)
        {
            this.pattern = pattern;
        }

        public KeyPattern[] split()
        {
            KeyPattern[] patterns = new KeyPattern[2];
            for (int i = 0; i < 2; i++)
            {
                patterns[i] = new KeyPattern(pattern);
                patterns[i].key = key;
                patterns[i].wildcardList = new ArrayList();
            }
            bool s = false;
            for (int i = 0; i < wildcardList.Count; i++)
            {
                Wildcard wc = ((Wildcard)wildcardList[i]);
                if (!s && (wc.size() - wc.count()) > 1)
                {
                    Wildcard[] wcs = wc.split();
                    patterns[0].wildcardList.Add(wcs[0]);
                    patterns[1].wildcardList.Add(wcs[1]);
                    s = true;
                }
                else
                {
                    patterns[0].wildcardList.Add(new Wildcard(wc));
                    Wildcard copy = new Wildcard(wc);
                    if (s)
                        copy.resetCounter();
                    patterns[1].wildcardList.Add(copy);
                }
            }
            return patterns;
        }

        public string giveWildcardKey()
        {
            string res = "";
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] != '[')
                    res += pattern[i];
                else
                {
                    res += '*';
                    while (pattern[i] != ']')
                        i++;
                }
                i++;
            }
            return res;
        }

        public bool testKey(string key)
        {
            int kcount = 0;
            int pcount = 0;
            while (kcount < key.Length && pcount < pattern.Length)
            {
                if (pattern[pcount] != '[')
                {
                    if (key[kcount] != '*' && pattern[pcount] != key[kcount])
                        return false;
                    kcount++;
                    pcount++;
                }
                else
                {
                    bool contains = false;
                    pcount++;
                    while (pattern[pcount] != ']')
                    {
                        if (key[kcount] != '*')
                        {
                            if (pattern[pcount + 1] == '-')
                            {
                                if (key[kcount] >= pattern[pcount] && key[kcount] <= pattern[pcount + 2])
                                    contains = true;
                                pcount += 2;
                            }
                            else
                                if (pattern[pcount] == key[kcount])
                                    contains = true;
                        }
                        pcount++;
                    }
                    if (!contains && !(key[kcount] == '*'))
                        return false;
                    kcount++;
                    pcount++;
                }                
            }
            if (pcount != pattern.Length || kcount != key.Length)
                return false;
            return true;
        }

        public long initKeyIteration(string key)
        {
            long counter = 1;
            this.key = key;
            int pcount = 0;
            wildcardList = new ArrayList();
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '*')
                {
                    Wildcard wc = new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount) + 1 - pcount));
                    wildcardList.Add(wc);
                    counter *= wc.size();
                }

                if (pattern[pcount] == '[')
                    while (pattern[pcount] != ']')
                        pcount++;
                pcount++;
            }
            return counter;
        }

        public long size()
        {
            if (wildcardList == null)
                return 0;
            long counter = 1;
            foreach (Wildcard wc in wildcardList)
                    counter *= wc.size();
            return counter;
        }

        public bool nextKey()
        {
            int wildcardCount = wildcardList.Count-1;
            bool overflow = ((Wildcard)wildcardList[wildcardCount]).succ();
            wildcardCount--;
            while (overflow && (wildcardCount >= 0))
                overflow = ((Wildcard)wildcardList[wildcardCount--]).succ();
            return !overflow;
        }

        public string getKey()
        {
            string res = "";
            int wildcardCount = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != '*')
                    res += key[i];
                else
                {
                    Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                    res += wc.getChar();
                }
            }
            return res;
        }
    }
    
    [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    //[Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "KeySearcher", "Bruteforces a decryption algorithm.", null, "KeySearcher/Images/icon.png")]
    public class KeySearcher : IAnalysisMisc
    {
        private Queue valuequeue;
        private double value_threshold;
        private int maxThread;  //the thread with the most keys left
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
                if ((settings.Key == null) ||((settings.Key != null) && !pattern.testKey(settings.Key)))
                    settings.Key = pattern.giveWildcardKey();
            }
        }

        private bool stop;

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
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
        }

        private class ThreadStackElement
        {
            public AutoResetEvent ev;
            public int threadid;
        }

        private void KeySearcherJob(object param)
        {
            object[] parameters = (object[])param;
            KeyPattern[] patterns = (KeyPattern[])parameters[0];
            int threadid = (int)parameters[1];
            Int64[] doneKeysArray = (Int64[])parameters[2];
            Int64[] keycounterArray = (Int64[])parameters[3];
            Int64[] keysLeft = (Int64[])parameters[4];
            IControlEncryption sender = (IControlEncryption)parameters[5];            
            int bytesToUse = (int)parameters[6];
            Stack threadStack = (Stack)parameters[7];

            KeyPattern pattern = patterns[threadid];

            try
            {
                while (pattern != null)
                {
                    long size = pattern.size();
                    keysLeft[threadid] = size;

                    do
                    {
                        ValueKey valueKey = new ValueKey();
                        try
                        {
                            valueKey.key = pattern.getKey();
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not get next Key: " + ex.Message, NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            valueKey.decryption = sender.Decrypt(ControlMaster.getKeyFromString(valueKey.key), bytesToUse);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Decryption is not possible: " + ex.Message, NotificationLevel.Error);
                            GuiLogMessage("Stack Trace: " + ex.StackTrace, NotificationLevel.Error);
                            return;
                        }

                        try
                        {
                            valueKey.value = CostMaster.calculateCost(valueKey.decryption);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Cost calculation is not possible: " + ex.Message, NotificationLevel.Error);
                            return;
                        }

                        if (this.costMaster.getRelationOperator() == RelationOperator.LargerThen)
                        {
                            if (valueKey.value > value_threshold)
                                valuequeue.Enqueue(valueKey);
                        }
                        else
                        {
                            if (valueKey.value < value_threshold)
                                valuequeue.Enqueue(valueKey);
                        }

                        doneKeysArray[threadid]++;
                        keycounterArray[threadid]++;
                        keysLeft[threadid]--;

                        //if we are the thread with most keys left, we have to share them:
                        if (maxThread == threadid && threadStack.Count != 0)
                        {
                            maxThreadMutex.WaitOne();
                            if (maxThread == threadid && threadStack.Count != 0)
                            {
                                KeyPattern[] split = pattern.split();
                                patterns[threadid] = split[0];
                                pattern = split[0];
                                ThreadStackElement elem = (ThreadStackElement)threadStack.Pop();
                                patterns[elem.threadid] = split[1];
                                elem.ev.Set();    //wake the other thread up
                                maxThread = -1;
                                size = pattern.size();
                                keysLeft[threadid] = size;
                            }
                            maxThreadMutex.ReleaseMutex();
                        }

                    } while (pattern.nextKey() && !stop);

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

        public void process(IControlEncryption sender)
        {
            if (sender != null && costMaster != null)
            {
                int maxInList = 10;
                LinkedList<ValueKey> costList = new LinkedList<ValueKey>();
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

                stop = false;
                if (!Pattern.testKey(settings.Key))
                {
                    GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                    return;
                }

                int bytesToUse = 0;

                try
                {
                    bytesToUse = CostMaster.getBytesToUse();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Bytes used not valid: " + ex.Message, NotificationLevel.Error);
                    return;
                }

                LinkedListNode<ValueKey> linkedListNode;

                KeyPattern[] patterns = new KeyPattern[settings.CoresUsed+1];
                long size = Pattern.initKeyIteration(settings.Key);
                
                if (settings.CoresUsed > 0)
                {
                    KeyPattern[] patterns2 = Pattern.split();                    
                    patterns[0] = patterns2[0];
                    patterns[1] = patterns2[1];
                    int p = 1;
                    int threads = settings.CoresUsed - 1;
                    while (threads > 0)
                    {
                        int maxPattern = -1;
                        long max = 0;
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

                valuequeue = Queue.Synchronized(new Queue());

                Int64[] doneKeysA = new Int64[patterns.Length];
                Int64[] keycounters = new Int64[patterns.Length];
                Int64[] keysleft = new Int64[patterns.Length];
                Stack threadStack = Stack.Synchronized(new Stack());
                for (int i = 0; i < patterns.Length; i++)
                {
                    WaitCallback worker = new WaitCallback(KeySearcherJob);
                    doneKeysA[i] = new Int64();
                    keycounters[i] = new Int64();
                    ThreadPool.QueueUserWorkItem(worker, new object[] { patterns, i, doneKeysA, keycounters, keysleft, sender.clone(), bytesToUse, threadStack });
                }
                
                //update message:
                while (!stop)
                {
                    Thread.Sleep(1000);

                    //update toplist:
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
                            node = costList.First;
                            if (vk.value < costList.Last().value)
                            {
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

                    long keycounter = 0;
                    long doneKeys = 0;
                    foreach (Int64 dk in doneKeysA)
                        doneKeys += dk;
                    foreach (Int64 kc in keycounters)
                        keycounter += kc;

                    if (keycounter > size)
                        GuiLogMessage("There must be an error, because we bruteforced too much keys...", NotificationLevel.Error);

                    //Let's determine which thread has the most keys to share:
                    if (size - keycounter > 1000)
                    {
                        maxThreadMutex.WaitOne();
                        long max = 0;
                        int id = -1;
                        for (int i = 0; i < patterns.Length; i++)
                            if (keysleft[i] > max)
                            {
                                max = keysleft[i];
                                id = i;
                            }
                        maxThread = id;
                        maxThreadMutex.ReleaseMutex();
                    }

                    ProgressChanged(keycounter, size);

                    if (QuickWatchPresentation.IsVisible && doneKeys != 0 && !stop)
                    {
                        double time = ((size - keycounter) / doneKeys);
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

                            ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Clear();
                            linkedListNode = costList.First;
                            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                            int i = 0;
                            while (linkedListNode != null)
                            {
                                i++;
                                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Add(i + ") " + Math.Round(linkedListNode.Value.value, 4) + " = " + linkedListNode.Value.key + " : \"" +
                                    enc.GetString(linkedListNode.Value.decryption).Replace("\n", "").Replace("\r", "").Replace("\t", "") + "\"");
                                linkedListNode = linkedListNode.Next;
                            }
                        }
                        , null);
                    }//end if
                    doneKeys = 0;
                    for (int i = 0; i < doneKeysA.Length; i++)
                        doneKeysA[i] = 0;

                    if (!stop && QuickWatchPresentation.IsVisible)
                    {

                        ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Clear();
                            linkedListNode = costList.First;
                            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                            int i = 0;
                            while (linkedListNode != null)
                            {
                                i++;
                                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Add(i + ") " + Math.Round(linkedListNode.Value.value, 4) + " = " + linkedListNode.Value.key + " : \"" +
                                    enc.GetString(linkedListNode.Value.decryption).Replace("\n", "").Replace("\r", "").Replace("\t", "") + "\"");
                                linkedListNode = linkedListNode.Next;
                            }
                        }
                        , null);
                    }

                    if (keycounter >= size)
                        break;
                }//end while

                //wake up all sleeping threads:
                while (threadStack.Count != 0)
                    ((ThreadStackElement)threadStack.Pop()).ev.Set();
            }//end if

            if (!stop)
                ProgressChanged(1, 1);
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

        private void keyPatternChanged()
        {
            Pattern = new KeyPattern(controlMaster.getKeyPattern());
        }

        private void onStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
            {
                this.process((IControlEncryption)sender);
            }
        }

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
                    controlMaster.OnStatusChanged -= onStatusChanged;
                }
                if (value != null)
                {
                    Pattern = new KeyPattern(value.getKeyPattern());
                    value.keyPatternChanged += keyPatternChanged;
                    value.OnStatusChanged += onStatusChanged;
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

        private struct ValueKey
        {
            public double value;
            public String key;
            public byte[] decryption;
        };
    }
}
