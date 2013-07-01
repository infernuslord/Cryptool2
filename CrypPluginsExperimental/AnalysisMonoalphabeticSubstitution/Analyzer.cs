using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Analyzer
    {
        #region Variables

        // Genetic algorithm parameters
        private const int population_size = 300;
        private const int mutateProbability = 100;
        private const int maxGenerations = 40;
        private const double changeBorder = 0.001;
        private int repetitions = 1;
        private const int maxTextLength = 500;


        // Working Variables
        private static Random rnd = new Random();
        private List<int[]> banlist = new List<int[]>();

        // Input Property Variables
        private Alphabet plaintext_alphabet = null;
        private Alphabet ciphertext_alphabet = null;
        private Text ciphertext = null;
        private LanguageDictionary language_dictionary = null;
        private Frequencies language_frequencies = null;
        private Boolean standardAlphabet = false;
        private string word_separator = " ";
        private bool auto_word_separator = false;
        
        // Output Property Variables
        private Text plaintext = null;
        private int[] best_key = null;

        // Delegate
        private PluginProgress PluginProgress;

        // Class for a population
        private class Population
        {
            public int[][] keys;
            public double[] fitness;
            public int[] prob;
            public double dev;
            public double fit_lastbestkey;
        }

        #endregion

        #region Constructor

        public Analyzer()
        {

        }

        #endregion 

        #region Input Properties

        public Alphabet Plaintext_Alphabet
        {
            get { return this.plaintext_alphabet; }
            set { this.plaintext_alphabet = value; }
        }

        public Alphabet Ciphertext_Alphabet
        {
            get { return this.ciphertext_alphabet; }
            set {this.ciphertext_alphabet = value;}
        }

        public Text Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
        }

        public Frequencies Language_Frequencies
        {
            get { return this.language_frequencies; }
            set { this.language_frequencies = value; }
        }

        public LanguageDictionary Language_Dictionary
        {
            get { return this.language_dictionary; }
            set { this.language_dictionary = value; }
        }

        public Boolean StandardAlphabet
        {
            get { return this.standardAlphabet; }
            set { this.standardAlphabet = value; }
        }

        public String WordSeparator
        {
            get { return this.word_separator; }
            set { this.word_separator = value; }
        }

        public Boolean AutoWordSeparator
        {
            get { return this.auto_word_separator; }
            set { this.auto_word_separator = value; }
        }

        #endregion

        #region Output Properties
        
        public Text Plaintext
        {
            get { return this.plaintext; }
            private set { }
        }

        public int[] Key
        {
            get { return this.best_key; }
            private set { }
        }

        #endregion

        #region Main Methods

        public void Analyze()
        {
            // Adjust analyzer parameters to ciphertext length
            AdjustAnalyzerParameters(this.ciphertext.Length);

            // Initialization of repetition data structures
            int[][] bestkeys = new int[this.repetitions][];
            double[] bestkeys_fit = new double[this.repetitions];

            // Execute analysis
            int statusBarProgress = 0;
            for (int curRep = 0; curRep < this.repetitions; curRep++)
            {
                Population population = new Population();
                SetUpEnvironment(population, Analyzer.population_size);

                // Use dictionnary attack only during the first repition
                if (curRep == 0)
                {
                    CreateInitialGeneration(population, this.ciphertext, this.ciphertext_alphabet, this.plaintext_alphabet, this.language_dictionary);
                }
                else
                {
                    CreateInitialGeneration(population, this.ciphertext, this.ciphertext_alphabet, this.plaintext_alphabet, null);
                }

                double change = population.dev;
                int curGen = 1;
                Population nextGen = population;

                while ((change > Analyzer.changeBorder) && (curGen < Analyzer.maxGenerations))
                {
                    nextGen = CreateNextGeneration(nextGen, this.ciphertext, this.ciphertext_alphabet, false);
                    change = nextGen.dev;
                    curGen++;
                    statusBarProgress++;
                    PluginProgress(statusBarProgress, this.repetitions*Analyzer.maxGenerations);
                }

                nextGen = CreateNextGeneration(nextGen, this.ciphertext, this.ciphertext_alphabet, true);
            
                this.plaintext = DecryptCiphertext(nextGen.keys[0], this.ciphertext, this.ciphertext_alphabet);
                
                bestkeys[curRep] = nextGen.keys[0];
                bestkeys_fit[curRep] = nextGen.fitness[0];
            }


            double best_fit = bestkeys_fit[0];
            int best_fit_index = 0;

            for (int t = 1; t < this.repetitions; t++)
            {
                if (bestkeys_fit[t] > best_fit)
                {
                    best_fit = bestkeys_fit[t];
                    best_fit_index = t;
                }
            }

            this.plaintext = DecryptCiphertext(bestkeys[best_fit_index], this.ciphertext, this.ciphertext_alphabet);
        }

        private void SetUpEnvironment(Population pop, int size)
        {
            // Initialize data structures
            pop.keys = new int[size][];
            pop.fitness = new double[size];
       
            // Create probability array to choose crossover keys
            int s = 0;
            int[] quantElements = new int[size];
            for (int i = 0; i < size; i++)
            {
                s += i;
                quantElements[i] = size - i;
            }
            s += size;
            pop.prob = new int[s];
            int index = 0;
            for (int i = 0; i < quantElements.Length; i++)
            {
                for (int j = 0; j < quantElements[i]; j++)
                {
                    pop.prob[index] = i;
                    index++;
                }
            }
        }

        private void CreateInitialGeneration(Population pop, Text ciphertext, Alphabet cipher_alpha, Alphabet plain_alpha, LanguageDictionary ldic)
        {
            // Create initial population keys
            int[] newkey;
            int keylength = cipher_alpha.Length;

            // Create one key of the population with the help of the language dictionary
            int startIndex;
            if (ldic == null)
            {
                startIndex = 0;
            }
            else
            {
                newkey = this.CreateInitialKeyLangDic(ciphertext, cipher_alpha, plain_alpha, ldic);
                pop.keys[0] = newkey;
                this.banlist.Add(newkey);
                startIndex = 1;
            }
            
            // Create the other population keys at random
            for (int i = startIndex; i < pop.keys.Length; i++)
            {
                newkey = this.CreateInitialKeyRandom(keylength);
                while (this.banlist.Contains(newkey)){
                    newkey = this.CreateInitialKeyRandom(keylength);
                }
                this.banlist.Add(newkey);
                pop.keys[i] = newkey;
            }

            // Calculate fitness of population keys
            for (int i = 0; i < pop.keys.Length; i++)
            {
                pop.fitness[i] = CalculateFitness(DecryptCiphertext(pop.keys[i], ciphertext, cipher_alpha));
            }

            // Sort keys according to their fitness
            int[] helper1;
            double helper2;

            for (int i = 0; i < pop.keys.Length; i++)
            {
                for (int j = 0; j < pop.keys.Length; j++)
                {
                    if (pop.fitness[i] > pop.fitness[j])
                    {
                        helper1 = pop.keys[i];
                        pop.keys[i] = pop.keys[j];
                        pop.keys[j] = helper1;
                        helper2 = pop.fitness[i];
                        pop.fitness[i] = pop.fitness[j];
                        pop.fitness[j] = helper2;
                    }
                }
            }

            // Calculate change in development
            pop.fit_lastbestkey = pop.fitness[0];
            pop.dev = Math.Abs(pop.fitness[0]);
        }

        private Population CreateNextGeneration(Population pop, Text ciphertext, Alphabet cipher_alpha, bool last)
        {
            Population next = new Population();
            

            next.prob = pop.prob;
            next.fitness = new double[pop.keys.Length];
            next.keys = new int[pop.keys.Length][];

            // Create population_size x children through crossover and mutate children
            int p1;
            int p2;
            int i1;
            int i2;
            int size = pop.prob.Length;
            int helper3;

            for (int i = 0; i < next.keys.Length; i++)
            {
                i1 = Analyzer.rnd.Next(size);
                i2 = Analyzer.rnd.Next(size);
                p1 = pop.prob[i1];
                p2 = pop.prob[i2];
    
                next.keys[i] = CombineKeys(pop.keys[p1], pop.fitness[p1], pop.keys[p2], pop.fitness[p2], this.ciphertext, this.ciphertext_alphabet);

                if (!last)
                {
                    for (int j = 0; j < next.keys[i].Length; j++)
                    {
                        if (Analyzer.rnd.Next(Analyzer.mutateProbability) == 0)
                        {
                            p1 = Analyzer.rnd.Next(next.keys[i].Length);
                            p2 = Analyzer.rnd.Next(next.keys[i].Length);
                            helper3 = next.keys[i][p1];
                            next.keys[i][p1] = next.keys[i][p2];
                            next.keys[i][p2] = helper3;
                        }
                    }
                }
            }

            // Calculate fitness of population
            for (int i = 0; i < next.keys.Length; i++)
            {
                next.fitness[i] = CalculateFitness(DecryptCiphertext(next.keys[i],ciphertext, cipher_alpha));
            }

            // Sort keys according to their fitness
            int[] helper1;
            double helper2;

            for (int i = 0; i < next.keys.Length; i++)
            {
                for (int j = 0; j < next.keys.Length; j++)
                {
                    if (next.fitness[i] > next.fitness[j])
                    {
                        helper1 = next.keys[i];
                        next.keys[i] = next.keys[j];
                        next.keys[j] = helper1;
                        helper2 = next.fitness[i];
                        next.fitness[i] = next.fitness[j];
                        next.fitness[j] = helper2;
                    }
                }
            }

            // Calculate change in development
            next.dev = Math.Abs(Math.Abs(next.fitness[0]) - Math.Abs(pop.fit_lastbestkey));
            next.fit_lastbestkey = next.fitness[0];

            return next;
        }

        private void AdjustAnalyzerParameters(int textlength)
        {
            // Change parameters according to the ciphertext length
            if (textlength >= 200)
            {
                this.repetitions = 1;
            }
            else if ((textlength >= 100) && (textlength < 200))
            {
                this.repetitions = 2;
            }
            else if ((textlength >= 90) && (textlength < 100))
            {
                this.repetitions = 10;
            }
            else if ((textlength >= 70) && (textlength < 80))
            {
                this.repetitions = 20;
            }
            else if ((textlength >= 60) && (textlength < 70))
            {
                this.repetitions = 30;
            }
            else if ((textlength >= 50) && (textlength < 60))
            {
                this.repetitions = 40;
            }
            else if ((textlength >= 0) && (textlength < 50))
            {
                this.repetitions = 50;
            }
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Create Random Initial Key
        /// </summary>
        private int[] CreateInitialKeyRandom(int keylength)
        {
            Boolean vorhanden = false;
            int[] res = new int[keylength];

            for (int i = 0; i < res.Length; i++)
            {
                int value;

                do
                {
                    vorhanden = false;
                    value = rnd.Next(res.Length);

                    for (int j = 0; j < i; j++)
                    {
                        if (res[j] == value)
                        {
                            vorhanden = true;
                            break;
                        }
                    }

                } while (vorhanden == true);

                res[i] = value;
            }
            return res;
        }

        /// <summary>
        /// Create initial key with the help of the language dictionary.
        /// </summary>
        private int[] CreateInitialKeyLangDic(Text ciphertext, Alphabet ciphertext_alphabet, Alphabet plaintext_alphabet, LanguageDictionary ldic)
        {
            string sep;
            int[] key = new int[ciphertext_alphabet.Length];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = -1;
            }
            List<string> text = new List<string>();

            // Determine word separator
            if (this.auto_word_separator == true)
            {
                sep = DetermineWordSeparator(ciphertext, ciphertext_alphabet);
            }
            else
            {
                sep = this.word_separator;
            }

            // Parse text
            Dictionary<string, int> word_count = new Dictionary<string, int>();
            string t = ciphertext.ToString(ciphertext_alphabet);
            string[] tar = t.Split(sep.ToCharArray());

            // Clean word because at the end of each word could be a sign that does not belong to the word
            for (int i = 0; i < tar.Length; i++)
            {
                string without = "";
                string cur = tar[i];

                for (int j = 0; j < cur.Length; j++)
                {
                    if (ciphertext_alphabet.GetPositionOfLetter(cur[j].ToString()) >= 0)
                    {
                        without += cur[j].ToString();
                    }
                }
                tar[i] = without;
            }
            
            // Leave last word because it could not be complete
            for (int i = 0; i < tar.Length-1; i++) 
            {
                if (word_count.ContainsKey(tar[i]) == false)
                {
                    word_count.Add(tar[i], tar[i].Length);
                }
            }
            
            foreach (KeyValuePair<string, int> pair in word_count.OrderBy(Key => Key.Value))
            {
                text.Add(pair.Key);
            }

            // Match words
            Stack<int[]> keystack = new Stack<int[]>();
            List<int[]> bestkeys = new List<int[]>();
            Stack<int[]> matchstack = new Stack<int[]>();
            List<bool[]> word_status = new List<bool[]>();
            for (int i = 0; i < text.Count; i++)
            {
                word_status.Add(new bool[ldic.GetNumberOfWords(text[i].Length)]);
                for (int j = 0; j < word_status[i].Length; j++)
                {
                    word_status[i][j] = true;
                }
            }
            keystack.Push(key);
            int[] curkey;
            int[] nextkey;
            bool matchfound;

            for (int i = 0; i < text.Count; i++)
            {
                matchfound = false;
                int len = text[i].Length;
                int maxnr = ldic.GetNumberOfWords(len);

                curkey = keystack.Peek();

                int start = 0;
                while ((start < word_status[i].Length) && (word_status[i][start]==false))
                {
                    start++;
                }
                if (start == maxnr)
                {
                    break;
                }

                for (int y=0;y<maxnr;y++)
                {
                    if (word_status[i][y]==true)
                    {
                        nextkey = KeyCreatedByMatch(curkey,ciphertext_alphabet, text[i], plaintext_alphabet, ldic.GetWord(len,y));
                        word_status[i][y] = false;
                        matchstack.Push(new int[] { i, y });
                        if (nextkey!=null)
                        {
                            curkey = nextkey;
                            keystack.Push(curkey);
                            matchfound = true;
                            break;
                        }
                    }
                }

                if (matchfound == false)
                {
                    // reset status of dic words of cur length and longer
                    for (int u = 0; u < word_status.Count; u++)
                    {
                        for (int o = 0; o < word_status[u].Length; o++)
                        {
                            word_status[u][o] = true;
                        }
                    }
                    for (int y = 0; y < maxnr; y++)
                    {
                        matchstack.Pop();
                    }
                    for (int y = 0; y < matchstack.Count; y++)
                    {
                        int[] match = matchstack.ElementAt(y);
                        word_status[match[0]][match[1]]=false;
                    }
                    // unmatch current word and go to one ciphertext word earlier and start matching again
                    if (i>0)
                    {
                      i = i -2;
                      bestkeys.Add(keystack.Pop());
                    } 
                }
            }

            // Make up incomplete key
            int[] k = keystack.Peek();
            if (KeyHasNoInformation(k) == true)
            {
                for (int i = 0; i < bestkeys.Count; i++)
                {
                    MakeComplete(bestkeys[i]);
                }
                double[] bestkeys_val = new double[bestkeys.Count];
                for (int i = 0; i < bestkeys.Count; i++)
                {
                    bestkeys_val[i] = CalculateFitness(DecryptCiphertext(bestkeys[i], ciphertext, ciphertext_alphabet));
                }
                double best = bestkeys_val[0];
                int best_index = 0;
                for (int i = 1; i < bestkeys.Count; i++)
                {
                    if (bestkeys_val[i] > best)
                    {
                        best_index = i;
                        best = bestkeys_val[i];
                    }
                }
                return bestkeys[best_index];
            }
            else
            {         
                MakeComplete(k);
                return k;
            }     
        }

        private string DetermineWordSeparator(Text ciphertext, Alphabet ciphertext_alphabet)
        {
            string sep = "";

            Dictionary<string, int> charcount = new Dictionary<string, int>();
            for (int i = 0; i < ciphertext.Length; i++)
            {
                string letter;
                int lint = ciphertext.GetLetterAt(i);
                if (lint >= 0)
                {
                    letter = ciphertext_alphabet.GetLetterFromPosition(lint);
                }
                else
                {
                    letter = ciphertext.GetLetterNotInAlphabetAt(lint);
                }

                if (charcount.ContainsKey(letter) == false)
                {
                    charcount.Add(letter, 1);
                }
                else
                {
                    charcount[letter]++;
                }
            }

            int sep_count = 0;
            foreach (KeyValuePair<string, int> pair in charcount)
            {
                if (pair.Value > sep_count)
                {
                    sep_count = pair.Value;
                    sep = pair.Key;
                }
            }

            return sep;
        }

        private void MakeComplete(int[] key)
        {
            List<int> let = new List<int>();
            for (int i = 0; i < key.Length; i++)
            {
                let.Add(i);
            }
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != -1)
                {
                    let.Remove(key[i]);
                }
            }
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == -1)
                {
                    key[i] = let[0];
                    let.RemoveAt(0);
                }
            }
        }

        private bool KeyHasNoInformation(int[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != -1)
                {
                    return false;
                }
            }
            return true;
        }

        private int[] KeyCreatedByMatch(int[] key, Alphabet cipher_alpha, string ct_word, Alphabet plain_alpha, string dic_word)
        {
            int[] newkey = new int[key.Length];
            for (int i = 0; i < newkey.Length; i++)
            {
                newkey[i] = key[i];
            }
            int ct_index;
            int pt_index;
            for (int i = 0; i < ct_word.Length; i++)
            {
                ct_index = cipher_alpha.GetPositionOfLetter(ct_word.Substring(i, 1));
                pt_index = plain_alpha.GetPositionOfLetter(dic_word.Substring(i, 1));
                if ((!newkey.Contains(pt_index)) && (newkey[ct_index] == -1))
                {
                    newkey[ct_index] = pt_index;
                }
                
                if ((newkey[ct_index] != pt_index) && (newkey[ct_index] != -1))
                {
                    return null;
                }
            }

            return newkey; 
        }

        /// <summary>
        /// Calculate quality of key
        /// </summary>
        private double CalculateFitness(Text plaintext)
        {
            double res = 0;

            for (int i = 0; i < this.plaintext_alphabet.Length; i++)
            {
                res+= this.language_frequencies.GetLogProb5gram(plaintext.GetLetterAt(0), plaintext.GetLetterAt(1), plaintext.GetLetterAt(2), plaintext.GetLetterAt(3), i);
            }

            int pos0 = -1;
            int pos1 = 0;
            int pos2 = 1;
            int pos3 = 2;

            for (int i = 4; i < plaintext.Length; i++)
            {
                pos0++; pos1++; pos2++; pos3++;
                while ((i < plaintext.Length) && (plaintext.GetLetterAt(i)<0))
                {
                    i++;
                }
                while ((pos0 < plaintext.Length) && (plaintext.GetLetterAt(pos0) < 0))
                {
                    pos0++;
                }
                while ((pos1 < plaintext.Length) && (plaintext.GetLetterAt(pos1) < 0))
                {
                    pos1++;
                }
                while ((pos2 < plaintext.Length) && (plaintext.GetLetterAt(pos2) < 0))
                {
                    pos2++;
                }
                while ((pos3 < plaintext.Length) && (plaintext.GetLetterAt(pos3) < 0))
                {
                    pos3++;
                }

                if (i < plaintext.Length)
                {
                    res += this.language_frequencies.GetLogProb5gram(plaintext.GetLetterAt(pos0), plaintext.GetLetterAt(pos1), plaintext.GetLetterAt(pos2), plaintext.GetLetterAt(pos3), plaintext.GetLetterAt(i));
                }
            }
            return res;
        }

        /// <summary>
        /// Decrypt the ciphertext
        /// </summary>
        /// <param name="k"></param>
        private Text DecryptCiphertext(int[] key, Text ciphertext, Alphabet ciphertext_alphabet)
        {
            int index = -1;
            Text plaintext = ciphertext.CopyTo();

            for (int i=0;i<ciphertext.Length;i++)
            {
                index = ciphertext.GetLetterAt(i);
                if (index >= 0)
                {
                    plaintext.ChangeLetterAt(i, key[index]);
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Combine two keys to make a better key
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="fit_p1"></param>
        /// <param name="p2"></param>
        /// <param name="fit_p2"></param>
        /// <returns></returns>
        private int[] CombineKeys(int[] p1, double fit_p1, int[] p2, double fit_p2, Text ciphertext, Alphabet ciphertext_alphabet)
        {
            int[] res = new int[this.ciphertext_alphabet.Length];
            int[] less_fit;
            double fitness;
            double new_fitness;
            Text plaintext;

            if (fit_p1 > fit_p2)
            {
                p1.CopyTo(res,0);
                less_fit = p2;
                fitness = fit_p1;
            }
            else
            {
                p2.CopyTo(res,0);
                less_fit = p1;
                fitness = fit_p2;
            }

            int index = -1;
            for (int i = 0; i < res.Length; i++)
            {
                if (res[i] != less_fit[i])
                {
                    for (int j = 0; j < res.Length; j++)
                    {
                        if (res[j] == less_fit[i])
                        {
                            index = j;
                            break;
                        }
                    }
                    int helper = res[i];
                    res[i] = res[index];
                    res[index] = helper;
                    plaintext = DecryptCiphertext(res, ciphertext, ciphertext_alphabet);
                    new_fitness = CalculateFitness(plaintext);
                    if (fitness > new_fitness)
                    {
                        helper = res[i];
                        res[i] = res[index];
                        res[index] = helper;
                    }

                }
            }

            return res;
        }

        public void SetPluginProgressCallback(PluginProgress method)
        {
            this.PluginProgress = new PluginProgress(method);
        }
        
        #endregion
    }
}