using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System.Text.RegularExpressions;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using System.IO;

namespace IDPAnalyser
{
    [Author("George Lasry, Armin Krauß", "krauss@cryptool.org", "CrypTool", "http://www.uni-due.de")]
    [PluginInfo("IDPAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "IDPAnalyser/DetailedDescription/doc.xml", "IDPAnalyser/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class IDPAnalyser : ICrypComponent
    {
        private byte[] input;
        private string[] keywords;

        private HighscoreList TOPLIST;
        ValueKeyComparer comparer;

        private Random rd = new Random(System.DateTime.Now.Millisecond);
        private AutoResetEvent ars;

        private IDPAnalyserSettings settings;
        private IDPAnalyserQuickWatchPresentation myPresentation;

        int[] Key1MinColEnd, Key1MaxColEnd;

        #region Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
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

        [PropertyInfo(Direction.InputData, "KeywordsCaption", "KeywordsTooltip", false)]
        public String[] Keywords
        {
            get
            {
                return this.keywords;
            }

            set
            {
                this.keywords = value;
                OnPropertyChange("Keywords");
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public IDPAnalyser()
        {
            settings = new IDPAnalyserSettings();
            myPresentation = new IDPAnalyserQuickWatchPresentation();
            Presentation = myPresentation;
            myPresentation.doppelClick += new EventHandler(this.doppelClick);
            ars = new AutoResetEvent(false);

            for (int i = 0; i < 26; i++)
                for (int j = 0; j < 26; j++)
                {
                    Bigrams.FlatList2[(i + 'A') * 256 + (j + 'A')] = Bigrams.FlatList_EN[i, j];
                    Bigrams.FlatList2[(i + 'a') * 256 + (j + 'a')] = Bigrams.FlatList_EN[i, j];
                }
        }

        private void doppelClick(object sender, EventArgs e)
        {
            try
            {
                ListViewItem lvi = sender as ListViewItem;
                ResultEntry rse = lvi.Content as ResultEntry;
                Output = System.Text.Encoding.GetEncoding(1252).GetBytes(rse.Text);
            }
            catch (Exception ex)
            {
            }
        }

        //private IControlCost costMaster;
        //[PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        //public IControlCost CostMaster
        //{
        //    get { return costMaster; }
        //    set
        //    {
        //        costMaster = value;
        //    }
        //}

        private byte[] output;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
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

        //bool finished = false;
        //[PropertyInfo(Direction.OutputData, "FinishedCaption", "FinishedTooltip")]
        //public bool Finished
        //{
        //    get
        //    {
        //        return this.finished;
        //    }
        //    set
        //    {
        //        this.finished = value;
        //        OnPropertyChanged("Finished");
        //    }
        //}

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
            get;
            private set;
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            //Finished = false;

            if (this.input == null)
            {
                GuiLogMessage("No input!", NotificationLevel.Error);
                return;
            }

            //if (this.costMaster == null)
            //{
            //    GuiLogMessage("You have to connect the Cost Function component to the Transpostion Analyzer control!", NotificationLevel.Error);
            //    return;
            //}

            //comparer = new ValueKeyComparer(costMaster.GetRelationOperator() != RelationOperator.LargerThen);
            comparer = new ValueKeyComparer(false);
            TOPLIST = new HighscoreList(comparer, 10);

            myPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { myPresentation.entries.Clear(); }, null);

            switch (this.settings.Analysis_method)
            {
                case 0: GuiLogMessage("Starting Dictionary Attack", NotificationLevel.Info); DictionaryAttack(); break;
                case 1: GuiLogMessage("Starting Hill Climbing Analysis", NotificationLevel.Info); HillClimbingAnalysis(); break;
            }

            //Finished = true;
            Output = TOPLIST[0].plaintext;
            //OnPropertyChanged("Output");

            ProgressChanged(1, 1);
        }

        public void PostExecution()
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

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        private void showProgress(DateTime startTime, ulong totalKeys, ulong doneKeys)
        {
            if (!Presentation.IsVisible || stop) return;

            long ticksPerSecond = 10000000;

            TimeSpan elapsedtime = DateTime.Now.Subtract(startTime);
            double totalSeconds = elapsedtime.TotalSeconds;
            if (totalSeconds == 0) totalSeconds = 0.001;
            elapsedtime = new TimeSpan(elapsedtime.Ticks - (elapsedtime.Ticks % ticksPerSecond));   // truncate to seconds

            TimeSpan timeleft = new TimeSpan();
            DateTime endTime = new DateTime();
            double secstodo;

            double keysPerSec = doneKeys / totalSeconds;
            if (keysPerSec > 0)
            {
                if (totalKeys < doneKeys) totalKeys = doneKeys;
                secstodo = (totalKeys - doneKeys) / keysPerSec;
                timeleft = new TimeSpan((long)secstodo * ticksPerSecond);
                endTime = DateTime.Now.AddSeconds(secstodo);
                endTime = new DateTime(endTime.Ticks - (endTime.Ticks % ticksPerSecond));   // truncate to seconds
            }

            ((IDPAnalyserQuickWatchPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                myPresentation.startTime.Content = "" + startTime;
                myPresentation.keysPerSecond.Content = String.Format(culture, "{0:##,#}", (ulong)keysPerSec);

                if (keysPerSec > 0)
                {
                    myPresentation.timeLeft.Content = "" + timeleft;
                    myPresentation.elapsedTime.Content = "" + elapsedtime;
                    myPresentation.endTime.Content = "" + endTime;
                }
                else
                {
                    myPresentation.timeLeft.Content = "incalculable";
                    myPresentation.endTime.Content = "in a galaxy far, far away...";
                }

                myPresentation.entries.Clear();

                for (int i = 0; i < TOPLIST.Count; i++)
                {
                    ValueKey v = TOPLIST[i];
                    ResultEntry entry = new ResultEntry();

                    entry.Ranking = (i + 1).ToString();
                    entry.Value = String.Format("{0:0.00000}", v.score);
                    entry.KeyArray = v.key;
                    entry.KeyPhrase = v.keyphrase;
                    entry.Key = "[" + String.Join(",", v.key) + "]";
                    entry.Text = Encoding.GetEncoding(1252).GetString(v.plaintext);

                    myPresentation.entries.Add(entry);
                }
            }
            , null);
        }

        private void UpdatePresentationList(ulong totalKeys, ulong doneKeys, DateTime starttime)
        {
            showProgress(starttime, totalKeys, doneKeys);
            ProgressChanged(doneKeys, totalKeys);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region DictionaryAttack

        public static void getKeyFromKeyword(String phrase, byte[] key, int keylen)
        {
            for (int i = 0; i < keylen; i++)
                key[i] = 0xff;

            for (int i = 0; i < keylen; i++)
            {
                int minJ = -1;

                for (int j = 0; j < keylen; j++)
                {
                    if (key[j] != 0xff)
                        continue;
                    if ((minJ == -1) || (phrase[j] < phrase[minJ]))
                        minJ = j;
                }

                key[minJ] = (byte)i;
            }
        }
        
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";   // used for converting the numeric key to a keyword

        // Convert the numeric key to a keyword based upon the alphabet string
        string getKeywordFromKey(byte[] key)
        {
            string keyword = "";
            foreach (var i in key) keyword += alphabet[i];
            return keyword;
        }

        private void DictionaryAttack()
        {
            if (this.Keywords == null || this.Keywords.Length == 0)
            {
                GuiLogMessage("Check dictionary", NotificationLevel.Error);
                return;
            }

            if (settings.Key1Min < 2)
            {
                GuiLogMessage("The minimum size for key 1 is 2.", NotificationLevel.Error);
                return;
            }

            if (settings.Key1Max < settings.Key1Min)
            {
                GuiLogMessage("The maximum size for key 1 must be bigger than the minimum size.", NotificationLevel.Error);
                return;
            }

            if (settings.Key2Min < 2)
            {
                GuiLogMessage("The minimum size for key 2 is 2.", NotificationLevel.Error);
                return;
            }

            if (settings.Key2Max < settings.Key2Min)
            {
                GuiLogMessage("The maximum size for key 2 must be bigger than the minimum size.", NotificationLevel.Error);
                return;
            }

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList ROUNDLIST = new HighscoreList(comparer, 10);

            ValueKey vk = new ValueKey();

            ulong totalKeys = 0;
            foreach (var keyword in this.Keywords)
                if (keyword.Length >= settings.Key2Min && keyword.Length <= settings.Key2Max)
                    totalKeys++;
            totalKeys *= (ulong)(settings.Key1Max - settings.Key1Min + 1);
            ulong doneKeys = 0;

            stop = false;

            byte[] mybuffer = new byte[this.input.Length];

            for (int key1size = settings.Key1Min; key1size <= settings.Key1Max; key1size++)
            {
                computeKey1MinMaxColEnding(this.input.Length, key1size);

                for (int key2size = settings.Key2Min; key2size <= settings.Key2Max; key2size++)
                {
                    byte[] key2 = new byte[key2size];

                    foreach (var keyword in this.Keywords)
                    {
                        if (stop) break;

                        if (keyword.Length != key2size) continue;
                        getKeyFromKeyword(keyword, key2, key2size);

                        //decrypt(vk, key);
                        vk.key = key2;
                        vk.keyphrase = keyword;
                        decrypt2(vk.key, vk.key.Length, this.input, this.input.Length, mybuffer);
                        vk.plaintext = mybuffer;
                        vk.score = evalIDPKey2(vk.plaintext, key1size);

                        //if (TOPLIST.isBetter(vk))
                        //{
                        //    byte[] tmp = new byte[vk.plaintext.Length];
                        //    Array.Copy(vk.plaintext, tmp, vk.plaintext.Length);
                        //    Output = tmp;
                        //}

                        TOPLIST.Add(vk);

                        doneKeys++;

                        if (DateTime.Now >= nextUpdate)
                        {
                            UpdatePresentationList(totalKeys, doneKeys, startTime);
                            nextUpdate = DateTime.Now.AddMilliseconds(1000);
                        }
                    }

                    UpdatePresentationList(totalKeys, doneKeys, startTime);
                }
            }
        }

        #endregion

        #region Hill climbing

        private void HillClimbingAnalysis()
        {
            if (settings.Iterations < 2)
            {
                GuiLogMessage("Check iterations.", NotificationLevel.Error);
                return;
            }

            if (settings.Key1Size < 2)
            {
                GuiLogMessage("The minimum size for key 1 is 2.", NotificationLevel.Error);
                return;
            }

            if (settings.Key2Size < 2)
            {
                GuiLogMessage("The minimum size for key 2 is 2.", NotificationLevel.Error);
                return;
            }

            computeKey1MinMaxColEnding(this.input.Length, settings.Key1Size);
            byte[] mybuffer = new byte[this.input.Length];

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList ROUNDLIST = new HighscoreList(comparer, 10);

            ValueKey vk = new ValueKey();

            ulong totalKeys = (ulong)settings.Repeatings * (ulong)settings.Iterations;
            ulong doneKeys = 0;

            stop = false;

            for (int repeating = 0; repeating < settings.Repeatings; repeating++)
            {
                if (stop) break;

                ROUNDLIST.Clear();

                byte[] key = randomArray(settings.Key2Size);
                byte[] oldkey = new byte[settings.Key2Size];

                for (int iteration = 0; iteration < settings.Iterations; iteration++)
                {
                    if (stop) break;

                    Array.Copy(key, oldkey, key.Length);

                    int r = rd.Next(100);
                    if (r < 50)
                    {
                        for (int i = 0; i < rd.Next(10); i++)
                            swap(key, rd.Next(key.Length), rd.Next(key.Length));
                    }
                    else if (r < 70)
                    {
                        for (int i = 0; i < rd.Next(3); i++)
                        {
                            int l = rd.Next(key.Length - 1) + 1;
                            int f = rd.Next(key.Length);
                            int t = (f + l + rd.Next(key.Length - l)) % key.Length;
                            blockswap(key, f, t, l);
                        }
                    }
                    else if (r < 90)
                    {
                        int l = 1 + rd.Next(key.Length - 1);
                        int f = rd.Next(key.Length);
                        int t = (f + 1 + rd.Next(key.Length - 1)) % key.Length;
                        blockshift(key, f, t, l);
                    }
                    else
                    {
                        pivot(key, rd.Next(key.Length - 1) + 1);
                    }

                    vk.key = key;
                    decrypt2(vk.key, vk.key.Length, this.input, this.input.Length, mybuffer);
                    vk.plaintext = mybuffer;
                    vk.score = evalIDPKey2(vk.plaintext, settings.Key1Size);
                    vk.keyphrase = getKeywordFromKey(vk.key);

                    if (ROUNDLIST.Add(vk))
                    {
                        if (TOPLIST.isBetter(vk))
                        {
                            TOPLIST.Add(vk);
                            //Output = vk.plaintext;
                        }
                    }
                    else
                        Array.Copy(oldkey, key, key.Length);

                    doneKeys++;

                    if (DateTime.Now >= nextUpdate)
                    {
                        TOPLIST.Merge(ROUNDLIST);
                        UpdatePresentationList(totalKeys, doneKeys, startTime);
                        nextUpdate = DateTime.Now.AddMilliseconds(1000);
                    }
                }
            }

            TOPLIST.Merge(ROUNDLIST);
            UpdatePresentationList(totalKeys, doneKeys, startTime);
        }

        void computeKey1MinMaxColEnding(int ciphertextLength, int keylength)
        {
            Key1MinColEnd = new int[keylength];
            Key1MaxColEnd = new int[keylength];

            int fullRows = ciphertextLength / keylength;
            int numberOfLongColumns = ciphertextLength % keylength;

            for (int i = 0; i < keylength; i++)
            {
                Key1MinColEnd[i] = fullRows * (i + 1) - 1;
                if (i < numberOfLongColumns)
                    Key1MaxColEnd[i] = fullRows * (i + 1) + i;
                else
                    Key1MaxColEnd[i] = Key1MinColEnd[i] + numberOfLongColumns;
            }

            for (int i = 0; i < keylength; i++)
            {
                int index = keylength - 1 - i;
                Key1MaxColEnd[index] = Math.Min(Key1MaxColEnd[index], ciphertextLength - 1 - fullRows * i);
                if (i < numberOfLongColumns)
                    Key1MinColEnd[index] = Math.Max(Key1MinColEnd[index], ciphertextLength - 1 - fullRows * i - i);
                else
                    Key1MinColEnd[index] = Math.Max(Key1MinColEnd[index], Key1MaxColEnd[index] - numberOfLongColumns);
            }
        }

        // The core algorithm for IDP (index of Digraphic Potential)
        public long evalIDPKey2(byte[] ciphertext, int keylen)
        {
            long[,] p1p2Best = new long[keylen, keylen];

            // CCT: All columns always start at multiples of nrows. No need to sweep for different
            // ending positions
            if ((ciphertext.Length % keylen) == 0)
            {
                int fullRows = ciphertext.Length / keylen;

                for (int c1 = 0; c1 < keylen; c1++)
                {
                    for (int c2 = 0; c2 < keylen; c2++)
                    {
                        if (c1 == c2)
                            continue;

                        int p1 = Key1MinColEnd[c1];
                        int p2 = Key1MinColEnd[c2];

                        long sum = 0;

                        for (int l = 0; l < fullRows; l++)
                            sum += Bigrams.FlatList2[(ciphertext[p1 - l] << 8) + ciphertext[p2 - l]];

                        p1p2Best[c1, c2] = sum / fullRows;
                    }
                }
            }
            else if ((ciphertext.Length % keylen) != 0)
            {
                // ICT - we sweep all possible C1-C2-C3 combinations as well
                // as all posible ending positions (P1, P2, P3).

                int fullRows = ciphertext.Length / keylen;
                long[] base_ = new long[fullRows + keylen];

                for (int c1 = 0; c1 < keylen; c1++)
                {
                    int minP1 = Key1MinColEnd[c1];
                    int maxP1 = Key1MaxColEnd[c1];

                    for (int c2 = 0; c2 < keylen; c2++)
                    {
                        if (c1 == c2)
                            continue;

                        int minP2 = Key1MinColEnd[c2];
                        int maxP2 = Key1MaxColEnd[c2];

                        int offset1 = maxP1 - minP1;
                        int offset2 = maxP2 - minP2;

                        int start1 = minP1 - fullRows + 1;
                        int start2 = minP2 - fullRows + 1;
                        long best = 0;

                        for (int offset = 0; offset <= offset2; offset++)
                        {
                            long sum = 0;

                            int p1 = start1;
                            int p2 = start2 + offset;

                            for (int i = 0; i < fullRows; i++)
                            {
                                long val = base_[i] = Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
                                sum += val;
                            }

                            if (best < sum) best = sum;

                            int iMinusFullRows = 0;
                            while ((p1 <= maxP1) && (p2 <= maxP2))
                            {
                                sum -= base_[iMinusFullRows++];
                                sum += Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
                                if (best < sum) best = sum;
                            }
                        }

                        // we test only once with offset = 0;
                        for (int offset = 1; offset <= offset1; offset++)
                        {
                            long sum = 0;

                            int p1 = start1 + offset;
                            int p2 = start2;

                            for (int i = 0; i < fullRows; i++)
                            {
                                long val = base_[i] = Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
                                sum += val;
                            }

                            if (best < sum) best = sum;

                            int iMinusFullRows = 0;
                            while ((p1 <= maxP1) && (p2 <= maxP2))
                            {
                                sum -= base_[iMinusFullRows++];
                                sum += Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
                                if (best < sum) best = sum;
                            }
                        }

                        p1p2Best[c1, c2] = best / fullRows;
                    }
                }
            }

            return getMatrixScore(p1p2Best);
        }

        private static long getMatrixScore(long[,] matrix)
        {
            int dimension = matrix.GetLength(0);

            int[] left = new int[dimension];
            int[] right = new int[dimension];

            for (int i = 0; i < dimension; i++)
                left[i] = right[i] = -1;

            long sum = 0;

            for (int i = 1; i <= dimension; i++)
            {
                long best = 0;
                int bestP1 = -1;
                int bestP2 = -1;

                for (int p1 = 0; p1 < dimension; p1++)
                {
                    if (right[p1] != -1)
                        continue;

                    bool[] inP1LeftCycle = new bool[dimension];

                    if (i != dimension)
                    {
                        int curr = p1;
                        while (left[curr] != -1)
                        {
                            curr = left[curr];
                            inP1LeftCycle[curr] = true;
                        }
                    }

                    for (int p2 = 0; p2 < dimension; p2++)
                    {
                        if (left[p2] != -1)
                            continue;
                        if (inP1LeftCycle[p2])
                            continue;
                        if (p1 == p2)
                            continue;
                        if (best < matrix[p1, p2])
                        {
                            best = matrix[p1, p2];
                            bestP1 = p1;
                            bestP2 = p2;
                        }
                    }
                }

                sum += best;
                if (bestP1 == -1)
                    Console.Write("-1\n");
                else
                {
                    left[bestP2] = bestP1;
                    right[bestP1] = bestP2;
                }
            }

            return sum / (long)dimension;
        }

        public static void decrypt2(byte[] key, int keylen, byte[] ciphertext, int ciphertextLength, byte[] plaintext)
        {
            int[] invkey = new int[keylen];

            for (int i = 0; i < keylen; i++)
                invkey[key[i]] = i;

            int c = 0;
            for (int trcol = 0; trcol < keylen; trcol++)
                for (int p = invkey[trcol]; p < ciphertextLength; p += keylen)
                    plaintext[p] = ciphertext[c++];
        }

        #endregion

        private void swap(byte[] arr, int i, int j)
        {
            byte tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }

        private void blockswap(byte[] arr, int f, int t, int l)
        {
            for (int i = 0; i < l; i++)
                swap(arr, (f + i) % arr.Length, (t + i) % arr.Length);
        }

        private void pivot(byte[] arr, int p)
        {
            byte[] tmp = new byte[arr.Length];
            Array.Copy(arr, tmp, arr.Length);

            Array.Copy(tmp, p, arr, 0, arr.Length - p);
            Array.Copy(tmp, 0, arr, arr.Length - p, p);
        }

        private void blockshift(byte[] arr, int f, int t, int l)
        {
            byte[] tmp = new byte[arr.Length];
            Array.Copy(arr, tmp, arr.Length);

            int t0 = (t - f + arr.Length) % arr.Length;
            int n = (t0 + l) % arr.Length;

            for (int i = 0; i < n; i++)
            {
                int ff = (f + i) % arr.Length;
                int tt = (((t0 + i) % n) + f) % arr.Length;
                arr[tt] = tmp[ff];
            }
        }

        private byte[] randomArray(int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++) result[i] = (byte)i;
            for (int i = 0; i < length; i++) swap(result, rd.Next(length), rd.Next(length));
            return result;
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

        //private void decrypt(ValueKey vk, byte[] key)
        //{
        //    vk.key = key;
        //    vk.plaintext = this.controlMaster.Decrypt(this.input, vk.key);
        //    vk.score = this.costMaster.CalculateCost(vk.plaintext);

        //    vk.key = key;
        //    decrypt2(vk.key, vk.key.Length, ciphertext, ciphertext.Length, mybuffer);
        //    vk.plaintext = mybuffer;
        //    vk.score = evalIDPKey2(vk.plaintext, keylen1);
        //}

        //private ValueKey createKey(byte[] key)
        //{
        //    ValueKey result = new ValueKey();
        //    decrypt(result, (byte[])key.Clone());
        //    return result;
        //}
    }

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string KeyPhrase { get; set; }
        public byte[] KeyArray { get; set; }
        public string Text { get; set; }
    }
}