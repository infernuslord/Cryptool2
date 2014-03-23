using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using System.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Dictionary
    {
        #region Variables

        Dictionary<byte[], List<byte[]>> dic = new Dictionary<byte[], List<byte[]>>(new ByteArrayComparer());
        private Boolean stopFlag;

        #endregion

        #region Constructor

        public Dictionary(String filename)
        {
            BinaryReader binReader = null;
            try
            {
                FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read);
                binReader = new BinaryReader(fs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (binReader != null)
            {
                while (binReader.BaseStream.Position != binReader.BaseStream.Length)
                {
                    if (this.stopFlag == true)
                    {
                        break;
                    }

                    // Read length of pattern
                    int len_pattern = binReader.ReadInt32();
                    // Read pattern
                    byte[] pattern = binReader.ReadBytes(len_pattern);
                    // Read number of words with the same pattern
                    int number = binReader.ReadInt32();
                    // Read words for the pattern
                    List<byte[]> words = new List<byte[]>();
                    for (int i = 0; i < number; i++)
                    {
                        int len = binReader.ReadInt32();
                        words.Add(binReader.ReadBytes(len));
                    }
                    // Add pattern and words to dictionary
                    this.dic.Add(pattern, words);
                }
            }
        }

        #endregion

        #region Properties

        public Boolean StopFlag
        {
            get {return this.stopFlag;}
            set { this.stopFlag = value; }
        }

        #endregion

        #region Methods

        public List<byte[]> GetWordsFromPattern(byte[] pattern)
        {
            if (this.dic.ContainsKey(pattern))
            {
                return dic[pattern];
            }
            else
            {
                return new List<byte[]>();
            }
        }

        private double calcFit(byte[] candidate, Frequencies freq)
        {
            double fitness = 0.0;

            if (candidate.Length == 1)
            {
                int count = 0;
                for (int i = 0; i < freq.Prob4Gram.Length; i++)
                {
                    for (int j = 0; j < freq.Prob4Gram[i].Length; j++)
                    {
                        for (int t = 0; t < freq.Prob4Gram[i][j].Length; t++)
                        {
                            fitness += freq.Prob4Gram[candidate[0]][i][j][t];
                            count++;
                        }
                    }
                }
                fitness = fitness / count;
            }
            else if (candidate.Length == 2)
            {
                int count = 0;
                for (int i = 0; i < freq.Prob4Gram.Length; i++)
                {
                    for (int j = 0; j < freq.Prob4Gram[i].Length; j++)
                    {
                        fitness += freq.Prob4Gram[candidate[0]][candidate[1]][i][j];
                        count++;
                    }
                }
                fitness = fitness / count;
            }
            else if (candidate.Length == 3)
            {
                int count = 0;
                for (int i = 0; i < freq.Prob4Gram.Length; i++)
                {
                    fitness += freq.Prob4Gram[candidate[0]][candidate[1]][candidate[2]][i];
                    count++;
                }
                fitness = fitness / count;
            }
            else
            {
                int l1 = candidate[0];
                int l2 = candidate[1];
                int l3 = candidate[2];
                int l4 = candidate[3];

                int counter = 0;
                for (int i = 4; i < candidate.Length; i++)
                {
                    counter++;
                    fitness += freq.Prob4Gram[l1][l2][l3][l4];

                    l1 = l2;
                    l2 = l3;
                    l3 = l4;
                    l4 = candidate[i];
                }
                fitness = fitness / counter;
            }

            return fitness;
        }

        public void WriteWordsToConsole(Alphabet alpha)
        {
            foreach (KeyValuePair<byte[],List<byte[]>> pair in this.dic)
            {
                foreach (byte[] word in pair.Value)
                {
                    string w = "";
                    for (int i = 0; i < word.Length; i++)
                    {
                        w += alpha.GetLetterFromPosition((int)word[i]);
                    }
                    Console.WriteLine(w);
                }
            }
        }

        #endregion

        private class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] a, byte[] b)
            {
                if (a == null || b == null)
                {
                    return a == b;
                }
                else
                {
                    return a.SequenceEqual(b);
                }
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }
    }
}
