using System;
using System.Collections.Generic;
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
        private const int population_size = 100;
        private int[] crossProbability;
        private const int mutateProbability = 1000; // 100000

        // Working Variables
        private Frequencies plaintext_frequencies = null;
        private int[][] key;
        private double[] key_fitness;
        private static Random rnd = new Random();
        private double last_best_key_fit = 0;
 
        // Input Property Variables
        private Alphabet plaintext_alphabet = null;
        private Alphabet ciphertext_alphabet = null;
        private Text ciphertext = null;
        private LanguageDictionary language_dictionary = null;
        private Frequencies language_frequencies = null;
        
        // Output Property Variables
        private Text plaintext = null;
        private int[] best_key = null;

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

        /// <summary>
        /// Start analysis (Create first generation of keys and initialize data structures)
        /// </summary>
        public double StartAnalysis()
        {
            /*
             * Set up environment
             */
            // Initialize data structures
            this.best_key = new int[this.ciphertext_alphabet.Length];
            this.key_fitness = new double[Analyzer.population_size];
            this.key = new int[Analyzer.population_size][];
            for (int i = 0; i < this.key.Length; i++)
            {
                this.key[i] = new int[this.ciphertext_alphabet.Length];
            }
            this.plaintext = this.ciphertext.CopyTo();
            this.plaintext_frequencies = new Frequencies(this.plaintext_alphabet);
            // Create probability array to choose crossover keys
            int size = 0;
            int[] quantElements = new int[Analyzer.population_size];   
            for (int i=0;i<Analyzer.population_size;i++)
            {
                size += i;
                quantElements[i]=Analyzer.population_size - i;
            }
            size += Analyzer.population_size;
            this.crossProbability = new int[size];
            int index = 0;
            for (int i=0;i < quantElements.Length;i++)
            {
                for (int j=0;j < quantElements[i];j++)
                {
                    this.crossProbability[index] = i;
                    index++;
                }
            }
            // Create initial population keys
            for (int i = 0; i < this.key.Length; i++)
            {
                this.key[i] = this.CreateInitialKeyRandom();
            }
            
            // Calculate fitness of population keys
            for (int i = 0; i < this.key.Length; i++)
            {
                DecryptCiphertext(this.key[i],this.ciphertext,this.ciphertext_alphabet,this.plaintext);
                this.key_fitness[i] = CalculateFitness(this.plaintext);
            }

            // Return fitness of best key
            this.last_best_key_fit = this.key_fitness[GetIndexOfFittestKey(this.key_fitness)];
            return this.last_best_key_fit;
        }

        /// <summary>
        /// Next step of the cryptanalysis process (Create new generation of keys)
        /// </summary>
        public double NextStep()
        {
            int[][] newkeys = new int[Analyzer.population_size][];
            //// Crossover parents to generate children (Combination)
            // Sort keys according to their fitness
            for (int i = 0; i < this.key.Length; i++)
            {
                for (int j = 0; j < this.key.Length; j++)
                {
                    if (this.key_fitness[i] > this.key_fitness[j])
                    {
                        int[] helper = this.key[i];
                        this.key[i] = this.key[j];
                        this.key[j] = helper;
                        double h = this.key_fitness[i];
                        this.key_fitness[i] = this.key_fitness[j];
                        this.key_fitness[j] = h;
                    }
                }
            }
            // Create population_size x children through crossover and mutate children
            int p1;
            int p2;
            int i1;
            int i2;
            int size = this.crossProbability.Length;

            for (int i = 0; i < newkeys.Length; i++)
            {
                i1 = Analyzer.rnd.Next(size);
                i2 = Analyzer.rnd.Next(size);
                p1 = this.crossProbability[i1];
                p2 = this.crossProbability[i2];
                //while (this.crossProbability[p1] == this.crossProbability[p2])
                //{
                //    p2 = this.crossProbability[Analyzer.rnd.Next(this.crossProbability.Length)];
                //}
                newkeys[i] = CombineKeys(this.key[p1], this.key_fitness[p1], this.key[p2], this.key_fitness[p2], this.ciphertext, this.ciphertext_alphabet, this.plaintext);

                for (int j = 0; j < newkeys[i].Length; j++)
                {
                    if (Analyzer.rnd.Next(Analyzer.mutateProbability) == 0)
                    {
                        p1 = Analyzer.rnd.Next(newkeys[i].Length);
                        p2 = Analyzer.rnd.Next(newkeys[i].Length);
                        int helper = newkeys[i][p1];
                        newkeys[i][p1] = newkeys[i][p2];
                        newkeys[i][p2] = helper;
                    }
                }
            
            }

            this.key = newkeys;

            // Calculate fitness of population
            for (int i = 0; i < this.key.Length; i++)
            {
                DecryptCiphertext(this.key[i], this.ciphertext, this.ciphertext_alphabet, this.plaintext);
                this.key_fitness[i] = CalculateFitness(this.plaintext);
            }

            double cur_best_key_fit = this.key_fitness[GetIndexOfFittestKey(this.key_fitness)];
            double change = cur_best_key_fit - this.last_best_key_fit;
            this.last_best_key_fit = cur_best_key_fit;

            return change;
        }

        public void LastStep()
        {
            int index_best_key = 0;
            double best_key_fit = this.key_fitness[0];
            // Determine best key in population
            for (int i = 1; i < this.key.Length; i++)
            {
                if (this.key_fitness[i] < best_key_fit)
                {
                    best_key_fit = this.key_fitness[i];
                    index_best_key = i;
                }
            }
            // Decrypt ciphertext
            this.key[index_best_key].CopyTo(this.best_key, 0);
            DecryptCiphertext(this.key[index_best_key],this.ciphertext,this.ciphertext_alphabet,this.plaintext);
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Create Random Initial Key
        /// </summary>
        private int[] CreateInitialKeyRandom()
        {
            Boolean vorhanden = false;
            int[] res = new int[this.ciphertext_alphabet.Length];

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
        private void createInitialKeyLangDic()
        {



        }

        /// <summary>
        /// Create initial key with the help of the language frequencies.
        /// </summary>
        private void createInitialKeyLangFreq()
        {
          
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

            for (int i = 4; i < this.plaintext.Length; i++)
            {
                pos0++; pos1++; pos2++; pos3++;
                while ((i < this.plaintext.Length) && (this.plaintext.GetLetterAt(i)==-1))
                {
                    i++;
                }
                while ((pos0 < this.plaintext.Length) && (this.plaintext.GetLetterAt(pos0) == -1))
                {
                    pos0++;
                }
                while ((pos1 < this.plaintext.Length) && (this.plaintext.GetLetterAt(pos1) == -1))
                {
                    pos1++;
                }
                while ((pos2 < this.plaintext.Length) && (this.plaintext.GetLetterAt(pos2) == -1))
                {
                    pos2++;
                }
                while ((pos3 < this.plaintext.Length) && (this.plaintext.GetLetterAt(pos3) == -1))
                {
                    pos3++;
                }

                if (i < this.plaintext.Length)
                {
                    res += this.language_frequencies.GetLogProb5gram(plaintext.GetLetterAt(pos0), plaintext.GetLetterAt(pos1), plaintext.GetLetterAt(pos2), plaintext.GetLetterAt(pos3), plaintext.GetLetterAt(i));
                }
            }
            return res;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Decrypt the ciphertext
        /// </summary>
        /// <param name="k"></param>
        private void DecryptCiphertext(int[] key, Text ciphertext, Alphabet ciphertext_alphabet, Text plaintext)
        {
            int index = -1;

            for (int i=0;i<ciphertext.Length;i++)
            {
                index = ciphertext.GetLetterAt(i);
                if (index != -1)
                {
                    plaintext.ChangeLetterAt(i, key[index]);
                }
            }
        }

        /// <summary>
        /// Combine two keys to make a better key
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="fit_p1"></param>
        /// <param name="p2"></param>
        /// <param name="fit_p2"></param>
        /// <returns></returns>
        private int[] CombineKeys(int[] p1, double fit_p1, int[] p2, double fit_p2, Text ciphertext, Alphabet ciphertext_alphabet, Text plaintext)
        {
            int[] res = new int[this.ciphertext_alphabet.Length];
            int[] less_fit;
            double fitness;
            double new_fitness;

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
                    DecryptCiphertext(res, ciphertext, ciphertext_alphabet, plaintext);
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

        private int GetIndexOfFittestKey(double[] fitness)
        {
            int index_best_key = 0;
            double best_key_fit = this.key_fitness[0];
            // Determine best key in population
            for (int i = 1; i < this.key.Length; i++)
            {
                if (this.key_fitness[i] < best_key_fit)
                {
                    best_key_fit = this.key_fitness[i];
                    index_best_key = i;
                }
            }

            return index_best_key;
        }
        
        #endregion
    }
}