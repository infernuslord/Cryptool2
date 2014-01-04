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
