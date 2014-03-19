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

        public void CreateNewDictionary(Frequencies freq)
        {
            try
            {
                /*
                String lines = System.IO.File.ReadAllText(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "german.dic"));

                String[] l = lines.Split(new char[]{'\n'});
                
                // String to cipherbytes        
                Alphabet germ = new Alphabet("abcdefghijklmnopqrstuvwxyz", 1);
                List<Byte[]> words = new List<Byte[]>();
                for (int i = 0; i < l.Length; i++)
                {
                    List<Byte> w = new List<Byte>();
                    string wstr = l[i].ToLower();
                    for (int j = 0; j < l[i].Length; j++)
                    {
                        int letter = germ.GetPositionOfLetter(wstr.Substring(j, 1));
                        if (letter >= 0)
                        {
                            w.Add((byte)letter);
                        }
                    }
                    words.Add(w.ToArray());
                }

                // Create pattern from words
                Dictionary<byte[], List<byte[]>> dic = new Dictionary<byte[], List<byte[]>>(new ByteArrayComparer());
                foreach (byte[] word in words)
                {
                    byte[] pattern = Word.makePattern(word);
                    if (dic.ContainsKey(pattern))
                    {
                        List<byte[]> list = dic[pattern];
                        list.Add(word);
                    }
                    else
                    {
                        List<byte[]> list = new List<byte[]>();
                        list.Add(word);
                        dic.Add(pattern, list);
                    }
                }

                Console.WriteLine("Anzahl Pattern: " + dic.Count);

                int pattern_counter = 0;
                foreach (KeyValuePair<byte[],List<Byte[]>> pair in dic)
                {
                    pattern_counter++;
                    // Calculate fitness
                    double[] fit = new double[pair.Value.Count];
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        fit[i] = calcFit(pair.Value[i], freq);
                    }
                    // Sort words according to fitness
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        for (int j = 0; j < pair.Value.Count; j++)
                        {
                            if (fit[j] > fit[i])
                            {
                                double helper = fit[i];
                                fit[i] = fit[j];
                                fit[j] = helper;

                                byte[] helper1 = pair.Value[i];
                                pair.Value[i] = pair.Value[j];
                                pair.Value[j] = helper1;
                            }
                        }
                    }

                }*/

                // load large dic
                Dictionary<byte[], List<byte[]>> dic_large = new Dictionary<byte[], List<byte[]>>(new ByteArrayComparer());
                BinaryReader binReader = null;
                try
                {
                    FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "de-large.dic"), FileMode.Open, FileAccess.Read);
                    binReader = new BinaryReader(fs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                //int counter = 0;
                if (binReader != null)
                {
                    while (binReader.BaseStream.Position != binReader.BaseStream.Length)
                    {
                        Console.WriteLine(binReader.BaseStream.Position + " " + binReader.BaseStream.Length);
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
                            //counter++;
                        }
                        // Add pattern and words to dictionary
                        dic_large.Add(pattern, words);
                        //if (counter >= 1000000)
                        //{
                        //    break;
                        //}
                    }
                }
                // Order all words
                List<Byte[]> word_list = new List<byte[]>();
                foreach (KeyValuePair<Byte[],List<Byte[]>> pair in dic_large)
                {
                    foreach (Byte[] word in pair.Value)
                    {
                        word_list.Add(word);
                    }
                }
                double[] fit = new double[word_list.Count];
                for (int i = 0; i < word_list.Count; i++)
                {
                    fit[i] = calcFit(word_list[i], freq);
                }
                // Sort words according to fitness
                for (int i = 0; i < word_list.Count; i++)
                {
                    for (int j = i + 1; j < word_list.Count; j++)
                    {
                        if (fit[j] > fit[i])
                        {
                            double helper = fit[i];
                            fit[i] = fit[j];
                            fit[j] = helper;

                            byte[] helper1 = word_list[i];
                            word_list[i] = word_list[j];
                            word_list[j] = helper1;
                        }
                    }
                }

                // Choose words for small dic ~ 60.000
                Dictionary<byte[], List<Byte[]>> small_dic = new Dictionary<byte[], List<byte[]>>(new ByteArrayComparer());
                for (int i=0; i < 60000; i++)
                {
                    byte[] pattern = Word.makePattern(word_list[i]);
                    if (small_dic.ContainsKey(pattern))
                    {
                        List<Byte[]> list = small_dic[pattern];
                        list.Add(word_list[i]);
                    }
                    else
                    {
                        List<Byte[]> liste = new List<Byte[]>();
                        liste.Add(word_list[i]);
                        small_dic.Add(pattern, liste);
                    }
                }

                // Choose words for big dic ~ 200.000
                Dictionary<byte[], List<Byte[]>> mid_dic = new Dictionary<byte[], List<byte[]>>(new ByteArrayComparer());
                for (int i = 0; i < 200000; i++)
                {
                    byte[] pattern = Word.makePattern(word_list[i]);
                    if (mid_dic.ContainsKey(pattern))
                    {
                        List<Byte[]> list = mid_dic[pattern];
                        list.Add(word_list[i]);
                    }
                    else
                    {
                        List<Byte[]> liste = new List<Byte[]>();
                        liste.Add(word_list[i]);
                        mid_dic.Add(pattern, liste);
                    }
                }

                // Write small dictionary
                FileStream fs1 = File.Create(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "de-small.dic"), 2048, FileOptions.None);
                BinaryWriter bw = new BinaryWriter(fs1);
                try
                {
                    foreach (KeyValuePair<byte[], List<byte[]>> pair in small_dic)
                    {
                        bw.Write(pair.Key.Length);
                        bw.Write(pair.Key);
                        bw.Write(pair.Value.Count);
                        foreach (byte[] word in pair.Value)
                        {
                            bw.Write(word.Length);
                            bw.Write(word);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error");
                    bw.Close();
                    fs1.Close();
                }
                bw.Close();
                fs1.Close();

                // Write mid dictionary
                fs1 = File.Create(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "de-mid.dic"), 2048, FileOptions.None);
                bw = new BinaryWriter(fs1);
                try
                {
                    foreach (KeyValuePair<byte[], List<byte[]>> pair in mid_dic)
                    {
                        bw.Write(pair.Key.Length);
                        bw.Write(pair.Key);
                        bw.Write(pair.Value.Count);
                        foreach (byte[] word in pair.Value)
                        {
                            bw.Write(word.Length);
                            bw.Write(word);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error");
                    bw.Close();
                    fs1.Close();
                }
                bw.Close();
                fs1.Close();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
