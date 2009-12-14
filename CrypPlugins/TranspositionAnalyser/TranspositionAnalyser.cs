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


namespace TranspositionAnalyser
{

    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "Transposition Analyser", "Bruteforces the columnar transposition.", "TranspositionAnalyser/Description/TADescr.xaml" , "TranspositionAnalyser/Images/icon.png")]
    public class TranspositionAnalyser : IAnalysisMisc
    {
        private enum ReadInMode { byRow = 0, byColumn = 1 };
        private enum PermutationMode { byRow = 0, byColumn = 1 };
        private enum ReadOutMode { byRow = 0, byColumn = 1 };


        TranspositionAnalyserSettings settings;

        /// <summary>
        /// Constructor
        /// </summary>
        public TranspositionAnalyser()
        {
            settings = new TranspositionAnalyserSettings();
        }

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                value.OnStatusChanged += onStatusChanged;
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }

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

        }

        public void Dispose()
        {

        }

        private void onStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
            {
                this.process((IControlEncryption)sender);
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void process(IControlEncryption sender)
        {
            Output = costfunction_bruteforce(sender);
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
                String best_text = "";
                byte[] best_bytes = null;
                ArrayList list = null;


                //Just for fractional-calculation:
                PermutationGenerator per = new PermutationGenerator(2);


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
                                int[] key = per.getNext();
                                byte[] b = new byte[key.Length];
                                for (int j = 0; j < b.Length; j++)
                                {
                                    b[j] = Convert.ToByte(key[j]);
                                }
                                byte[] dec = sender.Decrypt(b, b.Length);
                                if (dec != null)
                                {
                                    double val = costMaster.calculateCost(dec);
                                    if(val.Equals(new Double()))
                                    {
                                     return new byte[0];   
                                    }
                                    if (costMaster.getRelationOperator() == RelationOperator.LessThen)
                                    {
                                        if (val == best)
                                        {
                                            if (list == null)
                                            {
                                                list = new ArrayList();
                                                list.Add(best_text);
                                            }
                                            list.Add(System.Text.Encoding.ASCII.GetString(dec));
                                        }

                                        else if (val < best)
                                        {
                                            list = null;
                                            best = val;
                                            best_text = System.Text.Encoding.ASCII.GetString(dec);
                                            best_bytes = dec;
                                        }
                                    }
                                    else
                                    {
                                        if (val == best)
                                        {
                                            if (list == null)
                                            {
                                                list = new ArrayList();
                                                list.Add(best_text);
                                            }
                                            list.Add(System.Text.Encoding.ASCII.GetString(dec));
                                        }

                                        else if (val > best)
                                        {
                                            list = null;
                                            best = val;
                                            best_text = System.Text.Encoding.ASCII.GetString(dec);
                                            best_bytes = dec;
                                        }
                                    }
                                }


                                sum++;
                                if (sum % 1000 == 0)
                                {
                                    ProgressChanged(sum, size);
                                }

                            }
                        }
                    }
                    if (list != null)
                    {
                        int i = 1;
                        foreach (string tmp in list)
                        {
                            GuiLogMessage("ENDE (" + i++ + ")" + best + ": " + tmp,NotificationLevel.Info);
                        }
                    }
                    else
                    {
                        GuiLogMessage("ENDE " + best + ": " + best_text, NotificationLevel.Info);
                    }
                    return best_bytes;
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

        #region KeyLengthGuesser

        public int getKeyLength(String crib, String cipher)
        {
            crib = crib.ToUpper();

            for (int i = 1; i < crib.Length; i++)
            {
                char[,] cipherM = cipherToMatrix(i, cipher);
                char[,] cribM = cribToMatrix(i, crib);
                int[] analysed = analyse(i, cipherM, cribM);

                for (int j = 0; j < analysed.Length; j++)
                {
                    Console.WriteLine(analysed[j]);
                    if (analysed[j] != 0)
                    {
                        if (j == analysed.Length - 1)
                        {
                            GuiLogMessage("Probably Keyword-Length: " + i, NotificationLevel.Info);
                            return i;
                        }
                    }
                    else break;
                }
                
            }
            return 0;
           
        }

        char[,] cribToMatrix(int i, String tmp)
        {
            int x = tmp.Length / i;
            if (tmp.Length % i != 0)
            {
                x++;
            }
            char[,] arr = new char[i, x];
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

        char[,] cipherToMatrix(int i, String tmp)
        {
            int length = tmp.Length / i;
            int off = 0;
            if (tmp.Length % i != 0)
            {
                length++;
                off = (i * length) - tmp.Length;
            }
            char[,] cipherMatrix = new char[length, i];
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

        int[] analyse(int i, char[,] cipherMatrix, char[,] cribMatrix)
        {
            int cipherMatrixLength = cipherMatrix.Length / i;
            int cribMatrixHeight = cribMatrix.Length / i;
            int[] abc = new int[i];
            ArrayList[] def = new ArrayList[i];
            for (int a = 0; a < i; a++)
            {
                def[a] = new ArrayList();
            }

            char newchar = new char();
            char emptychar = new char();
            int count = 0;
            for (int a = 0; a < i; a++)
            {
                if (!cribMatrix[a, cribMatrixHeight - 1].Equals(emptychar))
                {
                    count++;
                }
                else
                {
                    abc[a] = -1;
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
                                            abc[x]++;
                                            def[x].Add(b);
                                            Console.WriteLine(def[x].ToArray()[0]);
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
            return abc;
        }

        #endregion
    }
}
