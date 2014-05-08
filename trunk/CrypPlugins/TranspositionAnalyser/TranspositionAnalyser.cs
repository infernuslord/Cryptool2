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

namespace TranspositionAnalyser
{

    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("TranspositionAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "TranspositionAnalyser/DetailedDescription/doc.xml", "TranspositionAnalyser/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class TranspositionAnalyser : ICrypComponent
    {
        private enum ReadInMode { byRow = 0, byColumn = 1 };
        private enum PermutationMode { byRow = 0, byColumn = 1 };
        private enum ReadOutMode { byRow = 0, byColumn = 1 };

        private byte[] crib;
        private byte[] input;
        private HighscoreList TOPLIST;
        ValueKeyComparer comparer;

        private Random rd = new Random(System.DateTime.Now.Millisecond);
        private AutoResetEvent ars;

        private TranspositionAnalyserSettings settings;
        private TranspositionAnalyserQuickWatchPresentation myPresentation;

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

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", false)]
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
            Presentation = myPresentation;
            myPresentation.doppelClick += new EventHandler(this.doppelClick);
            ars = new AutoResetEvent(false);
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

        private IControlTranspoEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip", false)]
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
        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
            }
        }

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
            if (this.input == null)
            {
                GuiLogMessage("No input!", NotificationLevel.Error);
                return;
            }

            if (this.ControlMaster == null)
            {
                GuiLogMessage("You have to connect the Transposition component to the Transpostion Analyzer control!", NotificationLevel.Error);
                return;
            }

            if (this.costMaster == null)
            {
                GuiLogMessage("You have to connect the Cost Function component to the Transpostion Analyzer control!", NotificationLevel.Error);
                return;
            }

            comparer = new ValueKeyComparer(costMaster.GetRelationOperator() != RelationOperator.LargerThen);
            TOPLIST = new HighscoreList(comparer, 10);

            myPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { myPresentation.entries.Clear(); }, null);

            switch (this.settings.Analysis_method)
            {
                case 0: GuiLogMessage("Starting Brute-Force Analysis", NotificationLevel.Info); BruteforceAnalysis(); break;
                case 1: GuiLogMessage("Starting Crib Analysis", NotificationLevel.Info); CribAnalysis(crib, input); break;
                case 2: GuiLogMessage("Starting Genetic Analysis", NotificationLevel.Info); GeneticAnalysis(); break;
                case 3: GuiLogMessage("Starting Hill Climbing Analysis", NotificationLevel.Info); HillClimbingAnalysis(); break;
            }

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

            ((TranspositionAnalyserQuickWatchPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
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

        #region bruteforce

        private int[] getBruteforceSettings()
        {
            List<int> set = new List<int>();
            if (settings.ColumnColumnColumn) set.Add(0);
            if (settings.ColumnColumnRow) set.Add(1);
            if (settings.RowColumnColumn) set.Add(2);
            if (settings.RowColumnRow) set.Add(3);
            return (set.Count > 0) ? set.ToArray() : null;
        }

        private void BruteforceAnalysis()
        {
            int[] set = getBruteforceSettings();

            if (set == null)
            {
                GuiLogMessage("Specify the type of transposition to examine.", NotificationLevel.Error);
                return;
            }

            if (settings.MaxLength < 2 || settings.MaxLength > 20)
            {
                GuiLogMessage("Check transposition bruteforce length. Min length is 2, max length is 20!", NotificationLevel.Error);
                return;
            }

            ValueKey vk = new ValueKey();

            //Just for fractional-calculation:
            PermutationGenerator per = new PermutationGenerator(2);

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            ulong totalKeys = 0;
            for (int i = 1; i <= settings.MaxLength; i++) totalKeys += (ulong)per.getFactorial(i);
            totalKeys *= (ulong)set.Length;

            ulong doneKeys = 0;

            stop = false;

            for (int keylength = 1; keylength <= settings.MaxLength; keylength++)
            {
                if (stop) break;

                // for every selected bruteforce mode:
                for (int s = 0; s < set.Length; s++)
                {
                    if (stop) break;

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

                    byte[] key = new byte[keylength];

                    per = new PermutationGenerator(keylength);

                    while (per.hasMore() && !stop)
                    {
                        int[] keyInt = per.getNext();

                        for (int i = 0; i < key.Length; i++)
                            key[i] = Convert.ToByte(keyInt[i]);

                        decrypt(vk, key);

                        if (TOPLIST.isBetter(vk))
                            Output = vk.plaintext;

                        TOPLIST.Add(vk);
                        doneKeys++;

                        if (DateTime.Now >= nextUpdate)
                        {
                            UpdatePresentationList(totalKeys, doneKeys, startTime);
                            nextUpdate = DateTime.Now.AddMilliseconds(1000);
                        }
                    }
                }
            }

            UpdatePresentationList(totalKeys, doneKeys, startTime);
        }

        #endregion

        #region cribAnalysis

        private ArrayList bestlist;
        private int searchPosition;

        private void CribAnalysis(byte[] crib, byte[] cipher)
        {
            bestlist = new ArrayList();

            DateTime starttime = DateTime.Now;
            DateTime nextUpdate = starttime.AddMilliseconds(100);

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

            ulong totalKeys = 0;
            for (int i = 2; i <= settings.CribSearchKeylength; i++) totalKeys += (ulong)binomial_iter(i, cipher.Length % i);

            ulong doneKeys = 0;

            stop = false;

            for (int keylength = 2; keylength <= maxKeylength; keylength++)
            {
                if (stop) break;

                GuiLogMessage("Keylength: " + keylength, NotificationLevel.Debug);

                int[] binaryKey = getDefaultBinaryKey(cipher, keylength);
                int[] firstKey = (int[])binaryKey.Clone();

                do
                {
                    byte[,] cipherMatrix = cipherToMatrix(cipher, binaryKey);
                    byte[,] cribMatrix = cribToMatrix(crib, keylength);

                    if (possibleCribForCipher(cipherMatrix, cribMatrix, keylength))
                    {
                        ArrayList possibleList = analysis(cipher, cipherMatrix, cribMatrix, keylength);
                        foreach (int[] k in possibleList)
                            if (!ContainsList(bestlist, k)) addToBestList(k);
                    }

                    binaryKey = nextPossible(binaryKey, binaryKey.Sum());

                    doneKeys++;

                    if (DateTime.Now >= nextUpdate)
                    {
                        UpdatePresentationList(totalKeys, doneKeys, starttime);
                        nextUpdate = DateTime.Now.AddMilliseconds(1000);
                    }

                } while (!arrayEquals(firstKey, binaryKey) && !stop);
            }

            UpdatePresentationList(totalKeys, doneKeys, starttime);
        }

        private bool ContainsList(ArrayList list, int[] search)
        {
            foreach (int[] k in list)
                if (arrayEquals(k, search)) return true;
            return false;
        }

        private void addToBestList(int[] k)
        {
            ValueKey vk = new ValueKey();

            int[] first = (int[])k.Clone();

            do
            {
                bestlist.Add((int[])k.Clone());

                int[] keyPlusOne = new int[k.Length];
                for (int i = 0; i < k.Length; i++)
                    keyPlusOne[i] = k[i] + 1;

                byte[] key = intArrayToByteArray(keyPlusOne);

                decrypt(vk, key);

                if (TOPLIST.isBetter(vk))
                    Output = vk.plaintext;

                TOPLIST.Add(vk);

                k = shiftKey(k);

            } while (!arrayEquals(k, first));
        }

        private int[] shiftKey(int[] key)
        {
            int[] ret = new int[key.Length];
            ret[0] = key[key.Length - 1];
            for (int i = 1; i < ret.Length; i++)
                ret[i] = key[i - 1];

            return ret;
        }

        private ArrayList analysis(byte[] cipher, byte[,] cipherMatrix, byte[,] cribMatrix, int keylength)
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
                output[i] = input[column, i];

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
                if (one[i] == two[0])
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length) break;

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1) searchPosition = i;
                        }
                        else
                        {
                            if (one[i + j] != two[j]) break;

                            if (j == two.Length - 1 && searchPosition == -1)
                                searchPosition = i;
                        }
                        return true;
                    }

            return false;
        }

        Boolean contains(byte[] one, byte[] two)
        {
            for (int i = 0; i < one.Length; i++)
                if (one[i] == two[0])
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length) break;

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1) searchPosition = i;
                        }
                        else
                        {
                            if (one[i + j] != two[j]) break;
                        }
                        return true;
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
            do input = addBinOne(input); while (count(input, 1) != numberOfOnes);
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

            if (k > n / 2) k = n - k;

            for (int i = 1; i <= k; ++i)
                produkt = produkt * n-- / i;

            return produkt;
        }

        private int[] getDefaultBinaryKey(byte[] cipher, int keylength)
        {
            int[] binaryKey = new int[keylength];
            int offset = (keylength - (cipher.Length % keylength)) % keylength;

            for (int i = 0; i < keylength; i++)
                binaryKey[i] = (i < offset) ? 0 : 1;

            return binaryKey;
        }

        private byte[,] cipherToMatrix(byte[] cipher, int[] key)
        {
            int height = (cipher.Length + key.Length - 1) / key.Length;
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
            int height = (crib.Length + keylength - 1) / keylength;
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

            for (int i = 0; i < keylength; i++)
            {
                byte[] cribCol = getColumn(crib, i, keylength);
                found = false;

                for (int j = 0; j < keylength; j++)
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

        private void GeneticAnalysis()
        {
            if (settings.Iterations < 2 || settings.KeySize < 2 || settings.Repeatings < 1)
            {
                GuiLogMessage("Check keylength and iterations", NotificationLevel.Error);
                return;
            }

            ValueKey vk = new ValueKey();

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList ROUNDLIST = new HighscoreList(comparer, 12);

            ulong totalKeys = (ulong)settings.Repeatings * (ulong)settings.Iterations * 6;
            ulong doneKeys = 0;

            stop = false;

            for (int repeating = 0; repeating < settings.Repeatings; repeating++)
            {
                if (stop) break;

                ROUNDLIST.Clear();

                for (int i = 0; i < ROUNDLIST.Capacity; i++)
                    ROUNDLIST.Add(createKey(randomArray(settings.KeySize)));

                for (int iteration = 0; iteration < settings.Iterations; iteration++)
                {
                    if (stop) break;

                    // Kinder der besten Keys erstellen
                    int rndInt = 0;

                    for (int a = 0; a < 6; a++)
                    {
                        if (a % 2 == 0)
                            rndInt = rd.Next(settings.KeySize - 1) + 1;

                        // combine DNA of two parents
                        ValueKey parent1 = ROUNDLIST[a];
                        ValueKey parent2 = ROUNDLIST[(a % 2 == 0) ? a + 1 : a - 1];

                        byte[] child = new byte[parent1.key.Length];
                        Array.Copy(parent1.key, child, rndInt);

                        int pos = rndInt;
                        for (int b = 0; b < parent2.key.Length; b++)
                        {
                            for (int c = rndInt; c < parent1.key.Length; c++)
                            {
                                if (parent1.key[c] == parent2.key[b])
                                {
                                    child[pos] = parent1.key[c];
                                    pos++;
                                    break;
                                }
                            }
                        }

                        // add a single mutation
                        int apos = rd.Next(settings.KeySize);
                        int bpos = (apos + rd.Next(1, settings.KeySize)) % settings.KeySize;
                        swap(child, apos, bpos);

                        decrypt(vk, child);

                        ROUNDLIST.Add(vk);

                        if (TOPLIST.isBetter(vk))
                        {
                            TOPLIST.Add(vk);
                            Output = vk.plaintext;
                        }

                        doneKeys++;
                    }

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

        #endregion

        #region Hill climbing

        private void HillClimbingAnalysis()
        {
            if (settings.Iterations < 2 || settings.KeySize < 2)
            {
                GuiLogMessage("Check keylength and iterations", NotificationLevel.Error);
                return;
            }

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

                byte[] key = randomArray(settings.KeySize);
                byte[] oldkey = new byte[settings.KeySize];

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

                    decrypt(vk, key);

                    if (ROUNDLIST.Add(vk))
                    {
                        if (TOPLIST.isBetter(vk))
                        {
                            TOPLIST.Add(vk);
                            Output = vk.plaintext;
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
            for (int i = 0; i < length; i++) result[i] = (byte)(i + 1);
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

        private void decrypt(ValueKey vk, byte[] key)
        {
            vk.key = key;
            vk.plaintext = this.controlMaster.Decrypt(this.input, vk.key);
            vk.score = this.costMaster.CalculateCost(vk.plaintext);
        }

        private ValueKey createKey(byte[] key)
        {
            ValueKey result = new ValueKey();
            decrypt(result, (byte[])key.Clone());
            return result;
        }
    }

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public byte[] KeyArray { get; set; }
        public string Text { get; set; }
    }
}