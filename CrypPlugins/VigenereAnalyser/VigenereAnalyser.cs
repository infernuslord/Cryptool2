using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using System.Windows.Documents;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.VigenereAnalyser
{
    [Author("Danail Vazov", "vazov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false,
    "Vigenere Analyser",
    "Analyses a plain text encrypted using the Vigenere cipher. Output is the keyword of the Vigenere cipher", "",
    "VigenereAnalyser/icon.png")]

    public class VigenereAnalyser:IStatistic
    {
        public int shiftKey = 0;
        private double[] elf;
        private VAPresentation vaPresentation;
        private char[] validchars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; //Sets alphabet of valid characters
        private string keywordOutput;
        private string frequencyOutput="";
        private string stringOutput = "";
        private string stringInput;
        private double friedmanInput;
        private int [] kasiskiInput;
        private string frequencyStats;
        private string[] vigToCaes;
        private int v=0;
        private int probableKeylength = 0;
        private char FrequentChar = 'e';
        public List <int> keys;
        public List<string> fStats=new List<string>();
        //private bool inputChange = false;
        public class Stats
        {
                public char letter;
                public int absoluteFrequency;
                public double relativeFrequency;
                public Stats(char letter, int absoluteFrequency, double relativeFrequency)
                {
                  this.letter = letter;
                  this.absoluteFrequency=absoluteFrequency;
                  this.relativeFrequency = relativeFrequency;
                }
            
            }
        

        #region Private methods
        
        private void ShowStatusBarMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion

        #region Custom methods

        private List<int> CaesarAnalysis(string text)
        {
            var Dic = new Dictionary<char, int>();

            if (!string.IsNullOrEmpty(text))
            {
                foreach (var s in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] tmpArr = s.Split(new[] { ':' });
                        if (tmpArr.Length > 1)
                        {

                            char c = tmpArr[0][0];
                            int Count;
                            int.TryParse(tmpArr[1], out Count);
                            if (!Dic.ContainsKey(c))
                                Dic.Add(c, 0);
                            Dic[c] += Count;
                        }
                    }
                }

                var items = (from k in Dic.Keys
                             orderby Dic[k] descending
                             select k);

                keys = new List<int>();
                foreach (var c in items)
                {
                    int tmp = c - FrequentChar;
                    int temp = 26 + tmp;
                    if (tmp < 0)
                        keys.Add(temp);
                    if (tmp > 0)
                        keys.Add(tmp);
                    if (tmp == 0)
                        keys.Add(tmp);


                }
                return keys;
            }
            return new List<int>();
        }
        private int chiSquare(string text)
        {   
            text=text.Replace(Environment.NewLine, ":");
            char [] delimiter = {':'};
            string[] splitStats = text.Split(delimiter);
            List<Stats> freqStats = new List<Stats>();
            for (int i = 0; i <splitStats.Length - 1; i = i + 3)
            {
                freqStats.Add(new Stats(System.Convert.ToChar(splitStats[i]), System.Convert.ToInt32(splitStats[i + 1]), System.Convert.ToDouble(splitStats[i + 2]))) ;
            }
            int textLength=0;
            List<int> observedFrequencies = new List<int>();
            freqStats.ForEach(delegate(Stats s)
            {
                textLength += s.absoluteFrequency;
            });
            double[] expectedFrequencies = elf;
            if (freqStats.Count != elf.Length)
            {
                char []check =new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
                int l = 0;
                for (int t = 0; t <= check.Length - 1;t++ )
                {
                       
                        if (l <= freqStats.Count - 1)
                        {
                            Stats r = freqStats.ElementAt(l);
                            if (check[t] == r.letter)
                            {
                                observedFrequencies.Add(r.absoluteFrequency);
                                l++;
                            }
                            else 
                            {
                                observedFrequencies.Add(0);
                            }
                        }
                        else
                        {
                            observedFrequencies.Add(0);
                        }
                }
            }
            List<double> correctedFrequencies = new List<double>();
            foreach (double d in expectedFrequencies)
            {   
                correctedFrequencies.Add((d / 100) * textLength);
            }
            double [] eFrequencies = correctedFrequencies.ToArray();
            //int[] oFrequencies = observedFrequencies.ToArray();
            double [] chiStats = new double[26];
            foreach (int f in observedFrequencies)
            {
                double chi = 0;
                double chiS = 0;
                int g = observedFrequencies.IndexOf(f);
                for (int j = 0; j <= 25; j++)
                {
                    int n = g + j;
                    if (n == 25||n>25)
                    {
                        n = n - 25;
                    }
                    chi = observedFrequencies[n];
                    chiS = Math.Pow(chi, 2);
                    chiS = chiS / eFrequencies[g];
                    chiStats[j] += chiS;
                }
            }
            
            foreach (double k in chiStats)
            {   
                shiftKey=Array.IndexOf(chiStats , k);
                for (int l = Array.IndexOf(chiStats, k) + 1; l < chiStats.Length; l++)
                {
                    if (chiStats[l]<chiStats[shiftKey] )
                    {
                        shiftKey = l;
                    }
                }
               
            }
            return shiftKey;


        }
        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "Double precission floating point value.", "Keylength as proposed by the Friedman Test.", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public double FriedmanInput
        {
            get { return friedmanInput; }
            set
            {
                if (value != friedmanInput)
                {
                    friedmanInput = value;
                    OnPropertyChanged("FriedmanInput");
                    
                }
            }
        }
        [PropertyInfo(Direction.Input, "Text Input.", "Cipher text encrypted with the Vigenere cipher.", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringInput
        {
            get
            {
                return stringInput;
            }
            set { stringInput = value; OnPropertyChanged("StringInput"); }
        }
        [PropertyInfo(Direction.Input, "Integer Array.", "The Array cointains keylengths as proposed by the Kasiski Test.", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public int[] KasiskiInput
        {
            get { return kasiskiInput; }
            set
            {
                if (value != kasiskiInput)
                {
                    kasiskiInput = value;
                    OnPropertyChanged("KasiskiInput");
                    
                }
            }
        }
        [PropertyInfo(Direction.Output, "Integer Array.", "Keyword represented as an integer Array.", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string KeywordOutput
        {
            get { return keywordOutput; }
            set
            {
                if (value != keywordOutput)
                {
                    keywordOutput = value;
                    OnPropertyChanged("KeywordOutput");

                }
            }
        }
        [PropertyInfo(Direction.Input, "String", "Please only connect to the text output of the Frequency Test.", "", true, true, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
        public string FrequencyStats
        {
            get { return frequencyStats; }
            set
            {
                if (value != frequencyStats)
                {
                    frequencyStats = value;
                    OnPropertyChanged("FrequencyStats");
                    //inputChange = true;
                    v++;
                }
            }
        }
        
        [PropertyInfo(Direction.Output, "String", "The cipher text divided into columns. Number of columns (strings) equals keylength.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string FrequencyOutput
        {
            get { return frequencyOutput; }
            set
            {
                frequencyOutput = value;
                OnPropertyChanged("FrequencyOutput");
                

            }
        }
        [PropertyInfo(Direction.Output, "Text output", " Keyword of the cipher which was used to encrypt the input text. ", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get { return stringOutput; }
            set
            {
                stringOutput = value;
                OnPropertyChanged("StringOutput");
               

            }
        }
        
        #endregion


        #region IPlugin Members
        private VigenereAnalyserSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (VigenereAnalyserSettings)value; }

        }
        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation
        {
            get { return Presentation; }
        }
        public VigenereAnalyser() 
        {
            settings = new VigenereAnalyserSettings();
            vaPresentation = new VAPresentation();

            Presentation = vaPresentation;
            vaPresentation.textBoxInputText.TextChanged +=textBoxInputText_TextChanged;
        }
        void textBoxInputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.NotifyUpdate();
            settings.HasChanges = true;
            
        }
        public void NotifyUpdate()
        {
            settings.Text = vaPresentation.textBoxInputText.Text;
        }
        public void Initialize()
        {
            if (vaPresentation.textBoxInputText != null)
                vaPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    vaPresentation.textBoxInputText.Text = settings.Text;
                }, null);
        }
        public void Dispose()
        {
            settings.Text = (string)vaPresentation.textBoxInputText.Dispatcher.Invoke(
        DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
        {
            return vaPresentation.textBoxInputText.Text;
        }, null);
        }
        public void PreExecution()
        {
            keywordOutput = null;
            fStats.Clear();
            frequencyStats = null;
            frequencyOutput = null;
            vigToCaes = null;
            probableKeylength = 0;
            keys = null;
            

        }
        public void Execute()
        {
           if (kasiskiInput != null)
           {
                //double [] elf;
                switch (settings.ELF)
                {
                   case 1: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                   case 2: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                   case 3: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                   case 4: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                   case 5: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                   default: elf = new double[26] { 8.167, 1.492, 2.782, 4.253, 12.702, 2.228, 2.015, 6.094, 6.966, 0.153, 0.772, 4.025, 2.406, 6.749, 7.507, 1.929, 0.095, 5.987, 6.327, 9.056, 2.758, 0.978, 2.360, 0.150, 1.974, 0.074 }; break;
                }
                if (vigToCaes==null)
                {   
                    double friedmanKey = friedmanInput;
                    int[] kasiskiFactors = kasiskiInput;
                    string workString = stringInput;
                    string workstring2 = "";
                    //int probableKeylength=0;
                    //Convert the cipher text into a format suitable for analysing i.e. remove all non-plaintext characters.
                    
                    string strValidChars = new string(validchars);
                    StringBuilder workstring1 = new StringBuilder();
                    char[] workStringArray = workString.ToCharArray();
                    foreach (char c in workStringArray)
                    {
                        if (strValidChars.IndexOf(c) >= 0) //If a char from the workString is valid, it is  appended to a newly built workstring1
                        {
                            workstring1.Append(c);
                        }
                    }

                    workstring2 = workstring1.ToString(); // Now copy workstring1 to workstring2. Needed because workstring1 can not be altered once its built
                    workstring2 = workstring2.ToLower();
                    string cipherText = workstring2;
                    //Start analysing the keylenghts proposed by the Friedman and Kasiski tests, and find the most propbable keylength.
                    int[] primes = new int[] { 5  ,    7  ,   11   ,  13   ,  17  ,   19 ,    23  ,   29 ,
                31   ,  37  ,   41 ,    43  ,   47  ,   53}; //Basic Array of prime numbers...replace with a primes generator or some sieve algorithm???
                    int biggestSoFar = 0;
                    int biggestSoFarIndex = 0;
                    int z = 0;
                    //Now we initialize an empty jagged array in which plausable keylengths with their respective probabilities will be stored
                    int[][] proposedKeylengths = 
                {
                 new int[] {0,0,0,0,0,0,0,0,0,0,0},
                 new int[] {0,0,0,0,0,0,0,0,0,0,0}
    
                };
                    //Fill up the array
                    for (int j = kasiskiFactors.Length - 1; j >= 0; j--)
                    {
                        if (kasiskiFactors[j] > biggestSoFar)
                        {
                            biggestSoFar = kasiskiFactors[j];
                            biggestSoFarIndex = j;
                            proposedKeylengths[0][z] = j;
                            proposedKeylengths[1][z] = kasiskiFactors[j];
                            z++;

                        }
                    }
                    //The resulting array contains some plausible keylengths and some not so plausible keylengths. 
                    //The variable "biggestSoFarIndex" contains the most common factor, hence most plausible keylength. Problem - biggestSoFarIndex is 2...

                    //After the most common factor is found check if this factor is a prime number. If yes - job done, this is indeed the key length.
                    foreach (int s in primes)
                    {
                        if (s == biggestSoFarIndex)
                        {
                            probableKeylength = biggestSoFarIndex;
                            goto Massages;
                        }
                    }
                    //In case the most common factor is not prime...well tough luck. We'll have to make some assumptions...
                    //First of all let's sort out unprobable keylengths.
                    double check1 = 0.55 * biggestSoFar;
                    int check = Convert.ToInt32(check1);
                    for (int r = 0; r <= proposedKeylengths[0].Length - 1; r++)
                    {
                        if (proposedKeylengths[1][r] < check)
                        {
                            proposedKeylengths[0][r] = 0;
                            proposedKeylengths[1][r] = 0;
                        }

                    }
                    //The unprobalbe keylengths are now replaced with zeroes.
                    //Get rid of the zeroes...
                    ArrayList factors = new ArrayList();
                    ArrayList count = new ArrayList();
                    for (int d = 0; d <= proposedKeylengths[0].Length - 1; d++)
                    {
                        if (proposedKeylengths[0][d] != 0)
                        {
                            factors.Add(proposedKeylengths[0][d]);
                            count.Add(proposedKeylengths[1][d]);
                        }
                    }
                    //The dynamic arrays "factors" and "count" now contain only the most prorbale keylengths and their respective probability.
                    //For ease of access convert the dynamic arrays in to normal ones
                    int[] factors1 = (int[])factors.ToArray(typeof(int));
                    int[] count1 = (int[])count.ToArray(typeof(int));
                    int a = factors1.Length;
                    //Now check the difference in probability between the most common and most uncommon factors
                    double smallestCount = count1[0]; //c# does not implicitly convert between int and double, hence two new variables are needed
                    double biggestCount = count1[a - 1];
                    double controlValue = smallestCount / biggestCount;
                    //Now can make some assumptions...
                    if (a > 3)
                    {
                        if (factors1[0] % factors1[a - 1] == 0 && factors1[0] % factors1[a - 2] == 0 && factors1[0] % factors1[a - 3] == 0 && controlValue > 0.65)
                        {
                            probableKeylength = factors1[0];
                        }
                        else { probableKeylength = factors1[1]; }
                    }
                    if (a == 3)
                    {
                        if (factors1[0] % factors1[a - 1] == 0 && factors1[0] % factors1[a - 2] == 0 && controlValue > 0.75)
                        {
                            probableKeylength = factors1[0];
                        }
                        if (factors1[0] % factors1[a - 2] == 0 && factors1[0] % factors1[a - 1] != 0 && controlValue > 0.6)
                        {
                            probableKeylength = factors1[0];
                        }

                    }
                    if (a == 2)
                    {
                        if (factors1[0] % factors1[a - 1] == 0 && controlValue > 0.75)
                        {
                            probableKeylength = factors1[0];
                        }

                    }
                    if (a == 1)
                    {
                        probableKeylength = factors1[0];
                    }
                    //Now that we've made some rudimentary decission making, let's check if it has payed off...
                Massages:
                    if (Math.Abs(probableKeylength - friedmanKey) < 1)
                    {
                        ShowStatusBarMessage("Analysed proposed keylengths. The derived keylength is the correct value." + "" + "Derived keylength is:" + probableKeylength.ToString(), NotificationLevel.Info);
                    }
                    if (Math.Abs(probableKeylength - friedmanKey) > 1 && Math.Abs(probableKeylength - friedmanKey) < 2)
                    {
                        ShowStatusBarMessage("Analysed proposed keylengths. The derived keylength is probably the correct value" + "" + "Derived keylength is:" + probableKeylength.ToString(), NotificationLevel.Info);
                    }
                    if (Math.Abs(probableKeylength - friedmanKey) > 2 && Math.Abs(probableKeylength - friedmanKey) < 3)
                    {
                        ShowStatusBarMessage("Analysed proposed keylengths. The derived keylength may not be the correct value." + "" + "Derived keylength is:" + probableKeylength.ToString(), NotificationLevel.Info);
                    }
                    if (Math.Abs(probableKeylength - friedmanKey) > 3)
                    {
                        ShowStatusBarMessage("Analysed proposed keylengths. Friedman or Kasiski test provided a value that was wrong. A manual analysis may be needed to confirm the derived keylength." + "" + "Derived keylength is:" + probableKeylength.ToString(), NotificationLevel.Info);
                    }
                    //Now we have a good idea of the keylength used to encrypt the ciphertext recived on the stringInput.
                    //Let's start with the analysis of the Vigenere cipher proper.
                    //First we need to divide the cipher text into columns. The number of columns must be equal to the probableKeylength.
                    //Create an array of strings. Just a container for the columns. 
                    char[] cipherTextChars = cipherText.ToCharArray();
                    int l = 0;
                    //Now we fill up the vigenereToCeasar array with strings.
                    string[] vigenereToCaesar = new string[probableKeylength];
                    for (int b = 0; b <= probableKeylength - 1; b++)
                    {
                        StringBuilder tempstring = new StringBuilder();
                        for (l = b; l <= cipherTextChars.Length - 1; l = l + probableKeylength)
                        {
                            tempstring.Append(cipherTextChars[l]);
                        }
                        vigenereToCaesar[b] = tempstring.ToString();
                        tempstring = null;
                    }
                    //After the outer loop is executed probableKeylength-times every element of vigenereToCaesar contains a string.
                    //Each of those strings should be alligned to the same keyletter of the Vigenere key which was used to encrypt the analysed text.
                    //Hence each string is encrypted using the Caeser cipher, and a Frequency Test for each string should give us a good idea of what the shift key is for the respective string.
                    //Furthermore the vigenerToCaesar is allready sorted in such a way that the index of each element coresponds to the position of its respective shift key in the whole key.
                    vigToCaes = vigenereToCaesar;
                    factors1 = null;
                    
                }
                if (vigToCaes != null)
                {
                    if (v <= probableKeylength - 1)
                    {
                        frequencyOutput = vigToCaes[v];
                        OnPropertyChanged("FrequencyOutput");
                    }
                    if (frequencyStats != null && frequencyStats != string.Empty&&!fStats.Contains(frequencyStats))
                    {
                        fStats.Add(frequencyStats);
                    }
                }
                if (v == probableKeylength)
                {
                    List<List<int>> keyList= new List<List<int>>();
                    List<int> chiList = new List<int>();
                    foreach (string c in fStats)
                    {
                        if (c != null)
                        {
                            CaesarAnalysis(c);
                            chiSquare(c);
                            keyList.Add(keys);
                            chiList.Add(shiftKey);
                        }
                    }
                    int[] probableKeyword = new int[probableKeylength];
                    for (int f = 0; f <= probableKeylength - 1; f++)
                    {
                        int [] tempKey = keyList.ElementAt(f).ToArray();
                        probableKeyword[f]=tempKey[0];
                        tempKey = null;
                    }
                    keyList = null;
                    StringBuilder keywordstring = new StringBuilder();
                    foreach(int r in probableKeyword)
                    {
                        keywordstring.Append(validchars[r]);
                    }
                    keywordOutput = keywordstring.ToString();
                    OnPropertyChanged("KeywordOutput");
                }
            }
        }

       

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void PostExecution()
        {   
            frequencyStats = null;
            Dispose();
            kasiskiInput = null;
            v = 0;
        }
        public void Stop()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
