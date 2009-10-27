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

            public char getChar()
            {                
                return (char)values[counter];
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

            public int Size()
            {
                return length;
            }

        }

        private string pattern;
        private string key;        
        private ArrayList wildcardList;

        public KeyPattern(string pattern)
        {
            this.pattern = pattern;
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

        public double initKeyIteration(string key)
        {
            double counter = 1;
            this.key = key;
            int pcount = 0;
            wildcardList = new ArrayList();
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '*')
                {
                    Wildcard wc = new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount) + 1 - pcount));
                    wildcardList.Add(wc);
                    counter *= wc.Size();
                }

                if (pattern[pcount] == '[')
                    while (pattern[pcount] != ']')
                        pcount++;
                pcount++;
            }
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
                double size = Pattern.initKeyIteration(settings.Key);
                
                try
                {
                    bytesToUse = CostMaster.getBytesToUse();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Bytes to use not valid: " + ex.Message, NotificationLevel.Error);
                    return;
                }
                string key;
                double keycounter = 0;
                double doneKeys = 0;
                LinkedListNode<ValueKey> linkedListNode;

                DateTime lastTime = DateTime.Now;                
                do
                {
                    valueKey = new ValueKey();
                    try
                    {
                        valueKey.key = Pattern.getKey();
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

                        node = costList.First;
                        while (node != null)
                        {

                            if (valueKey.value > node.Value.value)
                            {
                                costList.AddBefore(node, valueKey);
                                break;
                            }
                            node = node.Next;
                        }
                    }
                    else
                    {
                        node = costList.First;

                        while (node != null)
                        {

                            if (valueKey.value < node.Value.value)
                            {
                                costList.AddBefore(node, valueKey);
                                break;
                            }
                            node = node.Next;
                        }
                    }
                    if (costList.Count > maxInList)
                    {
                        costList.RemoveLast();
                    }

                    keycounter++;                    
                    ProgressChanged(keycounter, size);

                    //Key per second calculation
                    doneKeys++;
                    TimeSpan duration = DateTime.Now - lastTime;
                    if (duration.Seconds >= 1)
                    {
                        lastTime = DateTime.Now;
                        double time = ((size - keycounter) / doneKeys);                        
                        TimeSpan timeleft = new TimeSpan(-1) ;

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
                                }catch{
                                    ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "in a galaxy far, far away...";
                                }
                            }
                            else
                            {
                                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).timeLeft.Text = "incalculable :-)";
                                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "in a galaxy far, far away...";
                            }
                            

                        }
                        , null);
                        doneKeys = 0;

                        if (QuickWatchPresentation.IsVisible)
                        {                                                        
                            
                            ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Clear();
                                linkedListNode = costList.First;
                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                int i=0;
                                while (linkedListNode != null)
                                {
                                    i++;
                                    ((KeySearcherQuickWatchPresentation)QuickWatchPresentation).listbox.Items.Add(i + ") " + Math.Round(linkedListNode.Value.value,4) + " = " + linkedListNode.Value.key + " : \"" + 
                                        enc.GetString(linkedListNode.Value.decryption).Replace("\n","").Replace("\r", "").Replace("\t", "") + "\"");
                                    linkedListNode = linkedListNode.Next;
                                }                                
                            }
                            , null);
                        }
                    }

                } while (Pattern.nextKey() && !stop);

                if (QuickWatchPresentation.IsVisible)
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

            }//end if
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

