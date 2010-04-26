﻿using System;
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
    [PluginInfo(false, "Transposition Analyser", "Bruteforces the columnar transposition.", "TranspositionAnalyser/Description/TADescr.xaml", "TranspositionAnalyser/Images/icon.png")]
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

        TranspositionAnalyserSettings settings;
        #region Properties
        [PropertyInfo(Direction.InputData, "Input", "Input data for Analysis", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Crib", "Crib input", "Crib for Analysis", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
        }

        private void doppelClick(object sender, EventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            ResultEntry rse = lvi.Content as ResultEntry;
            Output = System.Text.Encoding.GetEncoding(1252).GetBytes(rse.Text);
        }

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        public IControlEncryption ControlMaster
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
        [PropertyInfo(Direction.ControlMaster, "Cost Master", "Used for cost calculation", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
            }
        }

        private byte[] output;
        [PropertyInfo(Direction.OutputData, "Output", "output", "", DisplayLevel.Beginner)]
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
            stop = true;
        }

        public void Initialize()
        {
            this.settings.Analysis_method = 0;
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

        public void process(IControlEncryption sender)
        {
            if (input != null)
            {
                switch (this.settings.Analysis_method)
                {
                    case 0: Output = costfunction_bruteforce(sender); GuiLogMessage("Starting Brute-Force Analysis", NotificationLevel.Info); break;
                    case 1: GuiLogMessage("Starting Analysis with crib", NotificationLevel.Info); cribAnalysis(this.crib, this.input); break;
                    case 2: GuiLogMessage("Starting genetic analysis", NotificationLevel.Info); geneticAnalysis(sender); break;
                }
            }
            else
            {
                GuiLogMessage("No Input!", NotificationLevel.Error);
            }


        }

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

        private byte[] costfunction_bruteforce(IControlEncryption sender)
        {
            valuequeue = Queue.Synchronized(new Queue());
            int[] set = getBruteforceSettings();
            stop = false;
            if (sender != null && costMaster != null && set != null)
            {
                GuiLogMessage("start", NotificationLevel.Info);
                double best = Double.MinValue;

                if (costMaster.getRelationOperator() == RelationOperator.LessThen)
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
                                    double val = costMaster.calculateCost(dec);
                                    if (val.Equals(new Double()))
                                    {
                                        return new byte[0];
                                    }
                                    if (costMaster.getRelationOperator() == RelationOperator.LessThen)
                                    {
                                        if (val <= best)
                                        {
                                            ValueKey valkey = new ValueKey();
                                            String keyStr = "";
                                            foreach (int xyz in key)
                                            {
                                                keyStr += xyz +", ";
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

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));

            }
        }

        #region cribAnalysis
        public void cribAnalysis(byte[] crib, byte[] cipher)
        {

            if (crib != null && crib != null)
            {
                foreach (int c in getKeyLength(crib, cipher))
                {
                    GuiLogMessage("Possible Key-Length: " + c, NotificationLevel.Info);
                }
            }
            else { GuiLogMessage("Missing crib or input!", NotificationLevel.Info); }
        }

        #endregion

        #region

        public ArrayList getKeyLength(byte[] crib, byte[] cipher)
        {

            ArrayList keylengths = new ArrayList();

            for (int i = 1; i < crib.Length; i++)
            {
                byte[,] cipherM = cipherToMatrix(i, cipher);
                byte[,] cribM = cribToMatrix(i, crib);
                int[] analysed = analyse(i, cipherM, cribM);

                for (int j = 0; j < analysed.Length; j++)
                {

                    if (analysed[j] != 0)
                    {
                        if (j == analysed.Length - 1)
                        {
                            keylengths.Add(i);
                        }
                    }
                    else break;
                }

            }
            return keylengths;

        }

        byte[,] cribToMatrix(int i, byte[] tmp)
        {
            int x = tmp.Length / i;
            if (tmp.Length % i != 0)
            {
                x++;
            }
            byte[,] arr = new byte[i, x];
            int count = 0;

            for (int a = 0; a < x; a++)
            {
                for (int b = 0; b < i; b++)
                {
                    if (count < tmp.Length)
                        arr[b, a] = tmp[count++];
                }
            }
            return arr;
        }

        byte[,] cipherToMatrix(int i, byte[] tmp)
        {
            int length = tmp.Length / i;
            int off = 0;
            if (tmp.Length % i != 0)
            {
                length++;
                off = (i * length) - tmp.Length;
            }
            byte[,] cipherMatrix = new byte[length, i];
            int pos = 0;

            for (int a = 0; a < i; a++)
            {
                for (int b = 0; b < length; b++)
                {
                    if (b == length - 1)
                    {
                        if (a < off)
                        {
                            break;
                        }
                    }
                    cipherMatrix[b, a] = tmp[pos];
                    pos++;
                }
            }
            return cipherMatrix;
        }

        int[] analyse(int i, byte[,] cipherMatrix, byte[,] cribMatrix)
        {
            int cipherMatrixLength = cipherMatrix.Length / i;
            int cribMatrixHeight = cribMatrix.Length / i;
            int[] poscount = new int[i];
            ArrayList[] def = new ArrayList[i];
            for (int a = 0; a < i; a++)
            {
                def[a] = new ArrayList();
            }

            byte newchar = new byte();
            byte emptychar = new byte();
            int count = 0;
            for (int a = 0; a < i; a++)
            {
                if (!cribMatrix[a, cribMatrixHeight - 1].Equals(emptychar))
                {
                    count++;
                }
                else
                {
                    poscount[a] = -1;
                }
            }

            for (int x = 0; x < count; x++)
            {
                for (int a = 0; a < i; a++)
                {
                    for (int b = 0; b < cipherMatrixLength; b++)
                    {
                        if (cribMatrix[x, 0].Equals(cipherMatrix[b, a]))
                        {
                            int tmpA = a;
                            int tmpB = b;

                            for (int y = 1; y < cribMatrixHeight; y++)
                            {
                                tmpB++;
                                if (tmpB == cipherMatrixLength - 1)
                                {
                                    if (cipherMatrix[tmpB, tmpA].Equals(newchar))
                                    {
                                        tmpB = 0;
                                        tmpA++;
                                    }
                                }

                                if ((tmpB) < cipherMatrixLength)
                                {
                                    if (cribMatrix[x, y].Equals(cipherMatrix[tmpB, tmpA]))
                                    {
                                        if (y.Equals(cribMatrixHeight - 1))
                                        {
                                            poscount[x]++;
                                            def[x].Add(b);

                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return poscount;
        }

        #endregion

        private void showProgress(DateTime startTime, long size, long sum)
        {
            LinkedListNode<ValueKey> linkedListNode;
            if (QuickWatchPresentation.IsVisible && !stop)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60 * minutes + 60 * 60 * hours + 24 * 60 * 60 * days;
                if (allseconds == 0) allseconds = 1;
                long keysPerSec = sum / allseconds;

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

                    ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).keysPerSecond.Text = "" + keysPerSec;

                    if (endTime != (new DateTime(1970, 1, 1)))
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Text = "" + endTime.Subtract(DateTime.Now);

                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "" + endTime;
                    }
                    else
                    {
                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).timeLeft.Text = "incalculable";

                        ((TranspositionAnalyserQuickWatchPresentation)QuickWatchPresentation).endTime.Text = "in a galaxy far, far away...";
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

        private void geneticAnalysis(IControlEncryption sender)
        {
            stop = false;

            valuequeue = Queue.Synchronized(new Queue());
            
            int size = settings.Iterations;
            int keylength = settings.KeySize;
            int repeatings = settings.Repeatings;

            if (size < 2 || keylength < 2 || repeatings <1)
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
                    double val = costMaster.calculateCost(dec);

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
                    //Dummy ValueKey erstellen:
                    ValueKey highest = new ValueKey();

                    // Schlechtesten 6 Keys löschen
                    if (costMaster.getRelationOperator() == RelationOperator.LessThen)
                    {
                        for (int a = 0; a < 6; a++)
                        {
                            highest.value = int.MinValue;
                            int pos = -1;
                            for (int b = 0; b < valList.Count; b++)
                            {
                                ValueKey v = (ValueKey)valList[b];
                                if (v.value > highest.value)
                                {
                                    highest = v;
                                    pos = b;
                                }
                            }
                            if (pos != -1)
                            {
                                valList.RemoveAt(pos);
                            }
                        }
                    }
                    //costmMaster Relation Operator == Larger Than
                    else
                    {
                        for (int a = 0; a < 6; a++)
                        {
                            highest.value = int.MaxValue;
                            int pos = -1;
                            for (int b = 0; b < valList.Count; b++)
                            {
                                ValueKey v = (ValueKey)valList[b];
                                if (v.value < highest.value)
                                {
                                    highest = v;
                                    pos = b;
                                }
                            }
                            if (pos != -1)
                            {
                                valList.RemoveAt(pos);
                            }
                        }
                    }

                    //valListe sortieren
                    ArrayList tmpList = new ArrayList(6);

                    double best = Double.MinValue;
                    int bestpos = -1;
                    for (int a = 0; a < 6; a++)
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

                            if (costMaster.getRelationOperator() == RelationOperator.LessThen)
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
                                double val = costMaster.calculateCost(dec);

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
                                    double val = costMaster.calculateCost(dec);

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
                            double val = costMaster.calculateCost(dec);

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

                        if (costMaster.getRelationOperator() == RelationOperator.LessThen)
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
                        ProgressChanged(it*size+ iteration, size*repeatings);
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
            }
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
