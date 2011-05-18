using System;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;



namespace TranspositionAnalyser
{

    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("TranspositionAnalyser.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "TranspositionAnalyser/Images/icon.png")]
    public class TranspositionAnalyser : IAnalysisMisc
    {
        private enum ReadInMode { byRow = 0, byColumn = 1 };
        private enum PermutationMode { byRow = 0, byColumn = 1 };
        private enum ReadOutMode { byRow = 0, byColumn = 1 };
        private byte[] crib;
        private byte[] input;
        private Queue valuequeue;
        LinkedList<ValueKey> list1;
        private TranspositionAnalyserQuickWatchPresentation myPresentation;
        private Random rd;
        private AutoResetEvent ars;

        TranspositionAnalyserSettings settings;
        #region Properties
        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public Byte[] Input
        {
            get
            {
                return this.input;
            }

            set
            {
                this.input = value;
                OnPropertyChange("Input");

            }
        }

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public Byte[] Crib
        {
            get
            {
                return this.crib;
            }

            set
            {
                this.crib = value;
                OnPropertyChange("Crib");
            }
        }



        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TranspositionAnalyser()
        {
            settings = new TranspositionAnalyserSettings();
            myPresentation = new TranspositionAnalyserQuickWatchPresentation();
            QuickWatchPresentation = myPresentation;
            myPresentation.doppelClick += new EventHandler(this.doppelClick);
            ars = new AutoResetEvent(false);
        }

        private void doppelClick(object sender, EventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            ResultEntry rse = lvi.Content as ResultEntry;
            Output = System.Text.Encoding.GetEncoding(1252).GetBytes(rse.Text);
        }

        private IControlTranspoEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip", "", false, false, QuickWatchFormat.None, null)]
        public IControlTranspoEncryption ControlMaster
        {

            get { return controlMaster; }
            set
            {
                // value.OnStatusChanged += onStatusChanged;
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }



        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", "", false, false, QuickWatchFormat.None, null)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
            }
        }

        private byte[] output;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "")]
        public byte[] Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
        }

        #region IPlugin Member

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

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

        public void Execute()
        {


            if (this.input != null)
            {
                if (this.ControlMaster != null && this.input != null)
                    this.process(this.ControlMaster);
                else
                {
                    GuiLogMessage("You have to connect the Transposition Plugin to the Transpostion Analyzer Control!", NotificationLevel.Warning);
                }
            }
            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {

        }

        public void Pause()
        {

        }

        private Boolean stop;
        public void Stop()
        {
            ars.Set();
            stop = true;
        }

        public void Initialize()
        {
            this.settings.UpdateTaskPaneVisibility(); 

        }

        public void Dispose()
        {

        }

        private void onStatusChanged(IControl sender, bool readyForExecution)
        {

        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        public void process(IControlTranspoEncryption sender)
        {
            if (input != null)
            {
                switch (this.settings.Analysis_method)
                {
                    case 0: Output = costfunction_bruteforce(sender); GuiLogMessage("Starting Brute-Force Analysis", NotificationLevel.Info); break;

                    case 1: GuiLogMessage("Starting Analysis with crib", NotificationLevel.Info); cribAnalysis(sender, this.crib, this.input); break;

                    case 2: GuiLogMessage("Starting genetic analysis", NotificationLevel.Info); geneticAnalysis(sender); break;
                }
            }
            else
            {
                GuiLogMessage("No Input!", NotificationLevel.Error);
            }


        }

        private void updateToplist(LinkedList<ValueKey> costList)
        {
            LinkedListNode<ValueKey> node;

            while (valuequeue.Count != 0)
            {
                ValueKey vk = (ValueKey)valuequeue.Dequeue();
                if (this.costMaster.GetRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = vk.decryption;
                                }
                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        }//end while
                    }//end if
                }
                else
                {
                    if (vk.value < costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value < node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = vk.decryption;
                                }

                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        }//end while
                    }//end if
                }
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));

            }
        }

        private void showProgress(DateTime startTime, long size, long sum)
        {
            LinkedListNode<ValueKey> linkedListNode;
            if (QuickWatchPresentation.IsVisible && !stop)
            {
                DateTime currentTime = DateTime.Now;

                TimeSpan elapsedtime = DateTime.Now.Subtract(startTime); ;
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);



                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60 * minutes + 60 * 60 * hours + 24 * 60 * 60 * days;
                if (allseconds == 0) allseconds = 1;

                if (allseconds == 0)
                    allseconds = 1;

                long keysPerSec = sum / allseconds;

                long keystodo = (size - sum);

                
                if (keysPerSec == 0)
                    keysPerSec = 1;

                long secstodo = keystodo / keysPerSec;

                //dummy Time 
                DateTime endTime = new DateTime(1970, 1, 1);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                }
                catch
                {

                }


                ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).startTime.Content = "" + startTime;
                    ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).keysPerSecond.Content = "" + keysPerSec;


                    if (endTime != (new DateTime(1970, 1, 1)))
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Content = "" + endTime.Subtract(DateTime.Now);
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).elapsedTime.Content = "" + elapsedspan;
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Content = "" + endTime;
                    }
                    else
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Content = "incalculable";

                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Content = "in a galaxy far, far away...";
                    }
                    if (list1 != null)
                    {
                        linkedListNode = list1.First;
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).entries.Clear();
                        int i = 0;
                        while (linkedListNode != null)
                        {
                            i++;
                            ResultEntry entry = new ResultEntry();
                            entry.Ranking = i.ToString();


                            String dec = System.Text.Encoding.ASCII.GetString(linkedListNode.Value.decryption);
                            if (dec.Length > 2500) // Short strings need not to be cut off
                            {
                                dec = dec.Substring(0, 2500);
                            }
                            entry.Text = dec;
                            entry.Key = linkedListNode.Value.key;
                            entry.Value = Math.Round(linkedListNode.Value.value, 2) + "";


                            ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).entries.Add(entry);

                            linkedListNode = linkedListNode.Next;
                        }

                    }
                }
                , null);

            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region bruteforce

        private int[] getBruteforceSettings()
        {
            int[] set;
            int sum = 0;
            if (settings.ColumnColumnColumn) sum++;
            if (settings.ColumnColumnRow) sum++;
            if (settings.RowColumnColumn) sum++;
            if (settings.RowColumnRow) sum++;

            if (sum > 0)
            {
                set = new int[sum];
                int count = 0;
                if (settings.ColumnColumnColumn)
                {
                    set[count] = 0;
                    count++;
                }
                if (settings.ColumnColumnRow)
                {
                    set[count] = 1;
                    count++;
                }
                if (settings.RowColumnColumn)
                {
                    set[count] = 2;
                    count++;
                }

                if (settings.RowColumnRow)
                {
                    set[count] = 3;
                    count++;
                }
                return set;
            }
            else
            {
                return null;
            }

        }

        private byte[] costfunction_bruteforce(IControlTranspoEncryption sender)
        {
            valuequeue = Queue.Synchronized(new Queue());
            int[] set = getBruteforceSettings();
            stop = false;
            if (sender != null && costMaster != null && set != null)
            {
                GuiLogMessage("start", NotificationLevel.Info);
                double best = Double.MinValue;

                if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                {
                    best = Double.MaxValue;
                }

                list1 = getDummyLinkedList(best);
                String best_text = "";
                ArrayList list = null;


                //Just for fractional-calculation:
                PermutationGenerator per = new PermutationGenerator(2);
                DateTime starttime = DateTime.Now;
                DateTime lastUpdate = DateTime.Now;

                int max = 0;
                max = settings.MaxLength;
                //GuiLogMessage("Max: " + max, NotificationLevel.Info);
                if (max > 1 && max < 21)
                {
                    long size = 0;
                    for (int i = 2; i <= max; i++)
                    {
                        size = size + per.getFactorial(i);
                    }
                    size = size * set.Length;
                    long sum = 0;
                    for (int i = 1; i <= max; i++)
                    {
                        // for every selected bruteforce mode:
                        for (int s = 0; s < set.Length; s++)
                        {
                            switch (set[s])
                            {
                                case (0):
                                    controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                                    controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                    controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                                    break;
                                case (1):
                                    controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                                    controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                    controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                                    break;
                                case (2):
                                    controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                                    controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                    controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                                    break;
                                case (3):
                                    controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                                    controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                    controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                                    break;
                            }

                            per = new PermutationGenerator(i);

                            while (per.hasMore() && !stop)
                            {
                                best = list1.Last.Value.value;
                                int[] key = per.getNext();
                                byte[] b = new byte[key.Length];
                                for (int j = 0; j < b.Length; j++)
                                {
                                    b[j] = Convert.ToByte(key[j]);
                                }
                                byte[] dec = sender.Decrypt(input, b, null);
                                if (dec != null)
                                {
                                    double val = costMaster.CalculateCost(dec);
                                    if (val.Equals(new Double()))
                                    {
                                        return new byte[0];
                                    }
                                    if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                                    {
                                        if (val <= best)
                                        {
                                            ValueKey valkey = new ValueKey();
                                            String keyStr = "";
                                            foreach (int xyz in key)
                                            {
                                                keyStr += xyz + ", ";
                                            }
                                            valkey.decryption = dec;
                                            valkey.key = keyStr;
                                            valkey.value = val;
                                            valuequeue.Enqueue(valkey);
                                        }
                                    }
                                    else
                                    {
                                        if (val >= best)
                                        {
                                            ValueKey valkey = new ValueKey();
                                            String keyStr = "";
                                            foreach (int xyz in key)
                                            {
                                                keyStr += xyz;
                                            }
                                            valkey.decryption = dec;
                                            valkey.key = keyStr;
                                            valkey.value = val;
                                            valuequeue.Enqueue(valkey);
                                        }
                                    }
                                }

                                sum++;
                                if (DateTime.Now >= lastUpdate.AddMilliseconds(1000))
                                {
                                    updateToplist(list1);
                                    showProgress(starttime, size, sum);
                                    ProgressChanged(sum, size);
                                    lastUpdate = DateTime.Now;
                                }
                            }
                        }
                    }
                    if (list != null)
                    {
                        int i = 1;
                        foreach (string tmp in list)
                        {
                            GuiLogMessage("ENDE (" + i++ + ")" + best + ": " + tmp, NotificationLevel.Info);
                        }
                    }
                    else
                    {
                        GuiLogMessage("ENDE " + best + ": " + best_text, NotificationLevel.Info);
                    }
                    return list1.First.Value.decryption;
                }
                else
                {
                    GuiLogMessage("Error: Check transposition bruteforce length. Max length is 20!", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                GuiLogMessage("Error: No costfunction applied.", NotificationLevel.Error);
                return null;
            }
        }

        #endregion

        private LinkedList<ValueKey> getDummyLinkedList(double best)
        {
            ValueKey valueKey = new ValueKey();
            valueKey.value = best;
            valueKey.key = "dummykey";
            valueKey.decryption = new byte[0];
            LinkedList<ValueKey> list = new LinkedList<ValueKey>();
            LinkedListNode<ValueKey> node = list.AddFirst(valueKey);
            for (int i = 0; i < 9; i++)
            {
                node = list.AddAfter(node, valueKey);
            }
            return list;
        }

        #region cribAnalysis

        private ArrayList bestlist;
        private ArrayList valList;
        private long sumBinKeys;
        private int countBinKeys;
        private int binKeysPerSec;
        private int[] keysLastTenSecs;
        private int poskeysLastTenSecs;
        private int searchPosition;
        private DateTime starttime;
        private DateTime lastUpdate;

        private void cribAnalysis(IControlTranspoEncryption sender, byte[] crib, byte[] cipher)
        {
            stop = false;
            valList = new ArrayList();
            bestlist = new ArrayList();
            valuequeue = Queue.Synchronized(new Queue());
            starttime = DateTime.Now;
            lastUpdate = DateTime.Now;

            int maxKeylength = settings.CribSearchKeylength;

            if (crib == null)
            {
                GuiLogMessage("crib == null", NotificationLevel.Error);
                return;
            }

            if (cipher == null)
            {
                GuiLogMessage("cipher == null", NotificationLevel.Error);
                return;
            }

            if (crib.Length < 2)
            {
                GuiLogMessage("Crib is too short.", NotificationLevel.Error);
                return;
            }

            if (maxKeylength < 1)
            {
                GuiLogMessage("Keylength must be greater than 1", NotificationLevel.Error);
                return;
            }

            if (maxKeylength > crib.Length)
            {
                GuiLogMessage("Crib must be longer than maximum keylength", NotificationLevel.Error);
                return;
            }



            for (int keylength = 2; keylength <= maxKeylength; keylength++)
            {
                sumBinKeys += binomial_iter(keylength, cipher.Length % keylength);
            }

            GuiLogMessage("KEYS INSG: " + sumBinKeys,NotificationLevel.Debug);

            keysLastTenSecs = new int[10];
            poskeysLastTenSecs = 0;

            for (int keylength = 2; keylength <= maxKeylength && !stop; keylength++)
            {
                GuiLogMessage("Keylength: " + keylength, NotificationLevel.Debug);
                int[] binaryKey = getDefaultBinaryKey(cipher, keylength);
                int[] firstKey = (int[])binaryKey.Clone();

                do
                {
                    countBinKeys++;
                    binKeysPerSec++;
                    byte[,] cipherMatrix = cipherToMatrix(cipher, binaryKey);
                    byte[,] cribMatrix = cribToMatrix(crib, keylength);

                    if(possibleCribForCipher(cipherMatrix,cribMatrix,keylength))
                    {
                        ArrayList possibleList = analysis(sender, cipher, cipherMatrix, cribMatrix, keylength);

                        Boolean eq;
                        foreach (int[] k in possibleList)
                        {
                            eq = false;
                            foreach (int[] kbest in bestlist)
                            {
                                if (arrayEquals(k, kbest))
                                    eq = true;
                            }
                            if (!eq)
                            {
                                addToBestList(sender, k);
                            }
                        }
                    }

                    binaryKey = nextPossible(binaryKey, binaryKey.Sum());

                    if (DateTime.Now >= lastUpdate.AddMilliseconds(1000))
                    {
                        keysLastTenSecs[(poskeysLastTenSecs++ % 10)] = binKeysPerSec;

                        if (DateTime.Now < starttime.AddMilliseconds(12000))
                        {
                            showProgressCribAnalysis(starttime, sumBinKeys, countBinKeys, binKeysPerSec);
                        }
                        else
                        {
                            int keysPerSec = keysLastTenSecs.Sum() / 10;
                            showProgressCribAnalysis(starttime, sumBinKeys, countBinKeys, keysPerSec);
                        }

                        showBestKeysCribSearch();
                        binKeysPerSec = 0;
                        lastUpdate = DateTime.Now;
                        showProgress(starttime, sumBinKeys, countBinKeys);
                        ProgressChanged(countBinKeys, sumBinKeys);
                    }

                } while (!arrayEquals(firstKey, binaryKey)&&!stop);
            }

            showBestKeysCribSearch();
            showProgress(starttime, 1, 1);
            ProgressChanged(1, 1);
        }

        private void showBestKeysCribSearch()
        {
            valList = updateValueKeyArrayList(valList, 12);

            Double best = Double.MinValue;

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
            }

            foreach (ValueKey v in valList)
            {
                valuequeue.Enqueue(v);
            }

            list1 = getDummyLinkedList(best);
            updateToplist(list1);
        }

        private void addToBestList(IControlTranspoEncryption sender, int[] k)
        {
            int[] first = (int[])k.Clone();

            do
            {
                bestlist.Add((int[])k.Clone());

                int[] keyPlusOne = new int[k.Length];
                for (int i = 0; i < k.Length; i++)
                {
                    keyPlusOne[i] = k[i] + 1;
                }

                byte[] key = intArrayToByteArray(keyPlusOne);

                ValueKey tmpValue = new ValueKey();
                byte[] dec = sender.Decrypt(input, key, null);
                double val = costMaster.CalculateCost(dec);

                String keyStr = "";
                foreach (byte bb in key)
                {
                    keyStr += bb + ", ";
                }

                tmpValue.keyArray = key;
                tmpValue.decryption = dec;
                tmpValue.key = keyStr;
                tmpValue.value = val;
                valList.Add(tmpValue);

                k = shiftKey(k);

            } while (!arrayEquals(k, first));
        }

        private int[] shiftKey(int[] key)
        {
            int[] ret = new int[key.Length];
            ret[0] = key[key.Length - 1];
            for (int i = 1; i < ret.Length; i++)
            {
                ret[i] = key[i - 1];
            }

            return ret;
        }

        private ArrayList analysis(IControlTranspoEncryption sender, byte[] cipher, byte[,] cipherMatrix, byte[,] cribMatrix, int keylength)
        {
            ArrayList possibleList = new ArrayList();
            int[] key = new int[keylength];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = -1;
            }

            int keyPosition = 0;
            Boolean end = false;

            while (!end && !stop)
            {
                Boolean check = true;
                if (keyPosition == -1)
                {
                    end = true;
                    break;
                }

                if (key[keyPosition] == -1)
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        Boolean inUse = false;
                        for (int j = 0; j < keyPosition; j++)
                        {
                            if (i == key[j])
                                inUse = true;
                        }

                        if (!inUse)
                        {
                            key[keyPosition] = i;
                            break;
                        }
                    }
                }
                else
                {
                    Boolean incrementPosition = true;

                    if (keyPosition == 0 && searchPosition != -1)
                    {
                        byte[] cipherCol = getColumn(cipherMatrix, key[keyPosition], key.Length);
                        byte[] cribCol = getColumn(cribMatrix, keyPosition, key.Length);
                        int tmpSearchPosition = searchPosition;
                        searchPosition = -1;

                        if (containsAndCheckCribPosition(cipherCol, cribCol, tmpSearchPosition + 1))
                        {
                            keyPosition++;
                            check = false;
                            incrementPosition = false;
                        }
                    }

                    if (incrementPosition)
                    {
                        Boolean inUse = true;

                        while (inUse)
                        {
                            key[keyPosition] = key[keyPosition] + 1;
                            inUse = false;

                            for (int j = 0; j < keyPosition; j++)
                            {
                                if (key[keyPosition] == key[j])
                                    inUse = true;
                            }
                        }

                        if (key[keyPosition] >= key.Length)
                        {
                            key[keyPosition] = -1;
                            keyPosition--;
                            check = false;
                        }
                    }
                }

                if (keyPosition == 0 && key[0] == -1)
                {
                    break;
                }

                if (check)
                {
                    if (keyPosition >= 0 && keyPosition <= key.Length)
                    {
                        byte[] cipherCol = getColumn(cipherMatrix, key[keyPosition], key.Length);
                        byte[] cribCol = getColumn(cribMatrix, keyPosition, key.Length);

                        int startSearchAt = 0;
                        if (searchPosition != -1)
                        {
                            startSearchAt = searchPosition;
                        }

                        if (containsAndCheckCribPosition(cipherCol, cribCol, startSearchAt))
                            keyPosition++;

                        if (keyPosition == key.Length)
                        {
                            possibleList.Add(key.Clone());

                            keyPosition--;
                            key[keyPosition] = -1;
                            keyPosition--;
                        }

                        if (keyPosition == 0)
                        {
                            searchPosition = -1;
                        }
                    }
                }
            }
            return possibleList;
        }

        private byte[] getColumn(byte[,] input, int column, int keylength)
        {
            byte[] output = new byte[input.Length / keylength];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[column, i];
            }
            return output;
        }

        Boolean containsAndCheckCribPosition(byte[] one, byte[] two, int startSearchAt)
        {
            int max = one.Length - 1;

            if (searchPosition != -1)
            {
                max = startSearchAt + 2;
                if (max >= one.Length)
                    max = startSearchAt;
            }

            for (int i = startSearchAt; i <= max; i++)
            {
                if (one[i] == two[0])
                {
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length)
                        {
                            break;
                        }

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1)
                            {
                                searchPosition = i;
                            }
                            return true;
                        }

                        else
                        {
                            if (one[i + j] != two[j])
                            {
                                break;
                            }

                            if (j == two.Length - 1)

                                if (searchPosition == -1)
                                {
                                    searchPosition = i;
                                }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        Boolean contains(byte[] one, byte[] two)
        {
            for (int i = 0; i < one.Length; i++)
            {
                if (one[i] == two[0])
                {
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length)
                        {
                            break;
                        }

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1)
                            {
                                searchPosition = i;
                            }
                            return true;
                        }

                        else
                        {
                            if (one[i + j] != two[j])
                            {
                                break;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Boolean arrayEquals(int[] a, int[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private int[] nextPossible(int[] input, int numberOfOnes)
        {
            Boolean found = false;
            while (!found)
            {
                input = addBinOne(input);
                if (count(input, 1) == numberOfOnes)
                    found = true;
            }
            return input;
        }

        private int count(int[] array, int countThis)
        {
            int c = 0;
            foreach (int i in array)
            {
                if (i == countThis)
                    c++;
            }
            return c;
        }

        private int[] addBinOne(int[] input)
        {
            int i = input.Length - 1;
            while (i >= 0 && input[i] == 1)
            {
                input[i] = 0;
                i--;
            }
            if (i >= 0)
                input[i] = 1;
            return input;
        }

        private long binomial_iter(int n, int k)
        {
            long produkt = 1;
            if (k > n / 2)
                k = n - k;
            for (int i = 1; i <= k; ++i)
            {
                produkt = produkt * n-- / i;
            }
            return produkt;
        }

        private int[] getDefaultBinaryKey(byte[] cipher, int keylength)
        {
            int offset = cipher.Length % keylength;
            int[] binaryKey = new int[keylength];

            for (int i = 0; i < keylength; i++)
            {
                if (i + offset < keylength)
                {
                    binaryKey[i] = 0;
                }
                else
                {
                    binaryKey[i] = 1;
                }
            }
            if (binaryKey.Sum() == 0)
            {
                for (int i = 0; i < keylength; i++)
                {
                    binaryKey[i] = 1;
                }
            }

            return binaryKey;
        }

        private byte[,] cipherToMatrix(byte[] cipher, int[] key)
        {
            int height = cipher.Length / key.Length;
            if (cipher.Length % key.Length != 0)
            {
                height++;
            }

            byte[,] cipherMatrix = new byte[key.Length, height];
            int pos = 0;

            for (int a = 0; a < key.Length; a++)
            {
                for (int b = 0; b < height; b++)
                {
                    if ((b == height - 1) && (key[a] != 1))
                    {
                        break;
                    }
                    else
                    {
                        cipherMatrix[a, b] = cipher[pos++];
                    }
                }
            }
            return cipherMatrix;
        }

        private byte[,] cribToMatrix(byte[] crib, int keylength)
        {
            int height = crib.Length / keylength;
            if (crib.Length % keylength != 0)
            {
                height++;
            }

            byte[,] cribMatrix = new byte[keylength, height];
            int pos = 0;

            for (int b = 0; b < height; b++)
            {
                for (int a = 0; a < keylength; a++)
                {
                    if (pos < crib.Length)
                        cribMatrix[a, b] = crib[pos++];
                }
            }
            return cribMatrix;
        }

        private Boolean possibleCribForCipher(byte[,] cipher, byte[,] crib, int keylength)
        {
            Boolean found;


            for(int i=0; i<keylength; i++)
            {
                byte[] cribCol = getColumn(crib,i,keylength);
                found = false;

                for(int j=0; j< keylength; j++)
                {
                    byte[] cipherCol = getColumn(cipher, j, keylength);

                    if (contains(cipherCol, cribCol))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        private void showProgressCribAnalysis(DateTime startTime, long size, long sum, long keysPerSec)
        {
            LinkedListNode<ValueKey> linkedListNode;
            if (QuickWatchPresentation.IsVisible && !stop)
            {
                DateTime currentTime = DateTime.Now;

                TimeSpan elapsedtime = DateTime.Now.Subtract(startTime); ;
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);



                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60 * minutes + 60 * 60 * hours + 24 * 60 * 60 * days;
                if (allseconds == 0) allseconds = 1;

                long keystodo = (size - sum);

                long secstodo = keystodo / keysPerSec;

                //dummy Time 
                DateTime endTime = new DateTime(1970, 1, 1);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                }
                catch
                {

                }


                ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).startTime.Content = "" + startTime;
                    ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).keysPerSecond.Content = "" + keysPerSec;


                    if (endTime != (new DateTime(1970, 1, 1)))
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Content = "" + endTime.Subtract(DateTime.Now);
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).elapsedTime.Content = "" + elapsedspan;
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Content = "" + endTime;
                    }
                    else
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Content = "incalculable";

                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Content = "in a galaxy far, far away...";
                    }
                    if (list1 != null)
                    {
                        linkedListNode = list1.First;
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).entries.Clear();
                        int i = 0;
                        while (linkedListNode != null)
                        {
                            i++;
                            ResultEntry entry = new ResultEntry();
                            entry.Ranking = i.ToString();


                            String dec = System.Text.Encoding.ASCII.GetString(linkedListNode.Value.decryption);
                            if (dec.Length > 2500) // Short strings need not to be cut off
                            {
                                dec = dec.Substring(0, 2500);
                            }
                            entry.Text = dec;
                            entry.Key = linkedListNode.Value.key;
                            entry.Value = Math.Round(linkedListNode.Value.value, 2) + "";


                            ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).entries.Add(entry);

                            linkedListNode = linkedListNode.Next;
                        }

                    }
                }


                , null);

            }
        }

        private byte[] intArrayToByteArray(int[] input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Convert.ToByte(input[i]);
            }

            return output;
        }

        #endregion




        #region genetic analysis

        private void geneticAnalysis(IControlTranspoEncryption sender)
        {
            stop = false;

            valuequeue = Queue.Synchronized(new Queue());

            int size = settings.Iterations;
            int keylength = settings.KeySize;
            int repeatings = settings.Repeatings;

            if (size < 2 || keylength < 2 || repeatings < 1)
            {
                GuiLogMessage("Check keylength and iterations", NotificationLevel.Error);
                return;
            }

            if (sender == null || costMaster == null || input == null)
            {
                if (sender == null)
                {
                    GuiLogMessage("sender == null", NotificationLevel.Error);
                }
                if (costMaster == null)
                {
                    GuiLogMessage("costMaster == null", NotificationLevel.Error);
                }
                if (input == null)
                {
                    GuiLogMessage("input == null", NotificationLevel.Error);
                }
                return;
            }
            DateTime startTime = DateTime.Now;
            DateTime lastUpdate = DateTime.Now;

            ArrayList bestOf = null;

            for (int it = 0; it < repeatings; it++)
            {
                ArrayList valList = new ArrayList();

                for (int i = 0; i < 12; i++)
                {
                    byte[] rndkey = randomArray(keylength);
                    byte[] dec = sender.Decrypt(input, rndkey, null);
                    double val = costMaster.CalculateCost(dec);

                    String keyStr = "";
                    foreach (byte tmp in rndkey)
                    {
                        keyStr += tmp + ", ";
                    }


                    ValueKey v = new ValueKey();
                    v.decryption = dec;
                    v.key = keyStr;
                    v.keyArray = rndkey;
                    v.value = val;
                    valList.Add(v);
                }

                valuequeue = Queue.Synchronized(new Queue());

                int iteration = 0;
                while (iteration < size && !stop)
                {
                    valList = updateValueKeyArrayList(valList, 12);


                    //valListe sortieren
                    ArrayList tmpList = new ArrayList(12);

                    double best = Double.MinValue;
                    int bestpos = -1;
                    for (int a = 0; a < 12; a++)
                    {
                        best = Double.MinValue;
                        bestpos = -1;

                        for (int b = 0; b < valList.Count; b++)
                        {
                            ValueKey v = (ValueKey)valList[b];

                            if (best == Double.MinValue)
                            {
                                best = v.value;
                                bestpos = b;
                            }

                            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                            {
                                if (v.value < best)
                                {
                                    best = v.value;
                                    bestpos = b;
                                }
                            }
                            else
                            {
                                if (v.value > best)
                                {
                                    best = v.value;
                                    bestpos = b;
                                }
                            }
                        }
                        tmpList.Add(valList[bestpos]);
                        valList.RemoveAt(bestpos);

                    }

                    valList = tmpList;


                    // Kinder der besten Keys erstellen
                    int rndInt = 0;

                    int listSize = valList.Count;
                    for (int a = 0; a < 6; a++)
                    {
                        if (a % 2 == 0)
                        {
                            rndInt = (rd.Next(0, int.MaxValue)) % (keylength);
                            while (rndInt == 0)
                            {
                                rndInt = (rd.Next(0, int.MaxValue)) % (keylength);
                            }
                        }

                        ValueKey parent1 = (ValueKey)valList[a];
                        byte[] child = new byte[parent1.keyArray.Length];
                        for (int b = 0; b < rndInt; b++)
                        {
                            child[b] = parent1.keyArray[b];
                        }

                        int pos = rndInt;
                        if (a % 2 == 0)
                        {
                            ValueKey parent2 = (ValueKey)valList[a + 1];
                            for (int b = 0; b < parent2.keyArray.Length; b++)
                            {
                                for (int c = rndInt; c < parent1.keyArray.Length; c++)
                                {
                                    if (parent1.keyArray[c] == parent2.keyArray[b])
                                    {
                                        child[pos] = parent1.keyArray[c];
                                        pos++;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ValueKey parent2 = (ValueKey)valList[a - 1];
                            for (int b = 0; b < parent2.keyArray.Length; b++)
                            {
                                for (int c = rndInt; c < parent1.keyArray.Length; c++)
                                {
                                    if (parent1.keyArray[c] == parent2.keyArray[b])
                                    {
                                        child[pos] = parent1.keyArray[c];
                                        pos++;
                                        break;
                                    }
                                }
                            }
                        }
                        int apos = (rd.Next(0, int.MaxValue)) % keylength;
                        int bpos = (rd.Next(0, int.MaxValue)) % keylength;
                        while (apos == bpos)
                        {
                            apos = (rd.Next(0, int.MaxValue)) % keylength;
                            bpos = (rd.Next(0, int.MaxValue)) % keylength;
                        }
                        byte tmp = child[apos];
                        child[apos] = child[bpos];
                        child[bpos] = tmp;

                        Boolean eq = false;
                        foreach (ValueKey v in valList)
                        {

                            if (arrayEquals(v.keyArray, child))
                            {
                                //GuiLogMessage("ZWEI GLEICHE", NotificationLevel.Debug);
                                ValueKey tmpValue = new ValueKey();
                                tmpValue.keyArray = randomArray(keylength);
                                byte[] dec = sender.Decrypt(input, tmpValue.keyArray, null);
                                double val = costMaster.CalculateCost(dec);

                                String keyStr = "";
                                foreach (byte bb in child)
                                {
                                    keyStr += bb + ", ";
                                }

                                tmpValue.decryption = dec;
                                tmpValue.key = keyStr;
                                tmpValue.value = val;
                                valList.Add(tmpValue);
                                eq = true;
                                break;
                            }
                        }
                        if (!eq && bestOf != null)
                        {
                            foreach (ValueKey v in bestOf)
                            {

                                if (arrayEquals(v.keyArray, child))
                                {
                                    //GuiLogMessage("ZWEI GLEICHE", NotificationLevel.Debug);
                                    ValueKey tmpValue = new ValueKey();
                                    tmpValue.keyArray = randomArray(keylength);
                                    byte[] dec = sender.Decrypt(input, tmpValue.keyArray, null);
                                    double val = costMaster.CalculateCost(dec);

                                    String keyStr = "";
                                    foreach (byte bb in child)
                                    {
                                        keyStr += bb + ", ";
                                    }

                                    tmpValue.decryption = dec;
                                    tmpValue.key = keyStr;
                                    tmpValue.value = val;
                                    valList.Add(tmpValue);
                                    eq = true;
                                    break;
                                }
                            }
                        }
                        if (!eq)
                        {
                            ValueKey tmpValue = new ValueKey();
                            byte[] dec = sender.Decrypt(input, child, null);
                            double val = costMaster.CalculateCost(dec);

                            String keyStr = "";
                            foreach (byte bb in child)
                            {
                                keyStr += bb + ", ";
                            }

                            tmpValue.keyArray = child;
                            tmpValue.decryption = dec;
                            tmpValue.key = keyStr;
                            tmpValue.value = val;
                            valList.Add(tmpValue);
                        }
                    }

                    if (DateTime.Now >= lastUpdate.AddMilliseconds(1000))
                    {
                        best = Double.MinValue;

                        if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                        {
                            best = Double.MaxValue;
                        }

                        list1 = getDummyLinkedList(best);

                        if (bestOf != null)
                        {
                            foreach (ValueKey v in bestOf)
                            {
                                valuequeue.Enqueue(v);
                            }
                        }

                        foreach (ValueKey v in valList)
                        {
                            valuequeue.Enqueue(v);
                        }

                        updateToplist(list1);
                        showProgress(startTime, size * repeatings, it * size + iteration);
                        ProgressChanged(it * size + iteration, size * repeatings);
                        lastUpdate = DateTime.Now;
                    }
                    iteration++;
                }
                foreach (ValueKey v in valList)
                {
                    if (bestOf == null)
                        bestOf = new ArrayList();
                    bestOf.Add(v);
                }
                bestOf = updateValueKeyArrayList(bestOf, 12);
            }
        }

        #endregion


        private ArrayList updateValueKeyArrayList(ArrayList list, int rest)
        {
            //Dummy ValueKey erstellen:
            ValueKey best = new ValueKey();
            ArrayList ret = new ArrayList();

            // Schlechtesten x Keys löschen
            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                for (int a = 0; a < rest; a++)
                {
                    best.value = int.MaxValue;
                    int pos = -1;
                    for (int b = 0; b < list.Count; b++)
                    {
                        ValueKey v = (ValueKey)list[b];
                        if (v.value < best.value)
                        {
                            best = v;
                            pos = b;
                        }
                    }
                    if (pos != -1)
                    {
                        ret.Add(list[pos]);
                        list.RemoveAt(pos);
                    }
                }
            }
            //costmMaster Relation Operator == Larger Than
            else
            {
                for (int a = 0; a < rest; a++)
                {
                    best.value = int.MinValue;
                    int pos = -1;
                    for (int b = 0; b < list.Count; b++)
                    {
                        ValueKey v = (ValueKey)list[b];
                        if (v.value > best.value)
                        {
                            best = v;
                            pos = b;
                        }
                    }
                    if (pos != -1)
                    {
                        ret.Add(list[pos]);
                        list.RemoveAt(pos);
                    }
                }
            }
            return ret;
        }

        private byte[] randomArray(int length)
        {
            int[] src = new int[length];
            for (int i = 0; i < length; i++)
            {
                src[i] = i + 1;
            }
            if (src == null)
            {
                return null;
            }

            int[] tmp = new int[src.Length];

            int num = src.Length;
            int index;

            if (rd == null) rd = new Random(System.DateTime.Now.Millisecond);

            for (int i = 0; i < src.Length; i++)
            {
                index = (rd.Next(0, int.MaxValue)) % num;
                tmp[i] = src[index];
                src[index] = src[num - 1];
                num--;
            }

            byte[] output = new byte[length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Convert.ToByte(tmp[i]);
            }

            return output;
        }

        private Boolean arrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }


    }

    public struct ValueKey
    {
        public byte[] keyArray;
        public double value;
        public String key;
        public byte[] decryption;
    };
    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

    }
}
