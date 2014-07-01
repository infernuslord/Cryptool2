using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using System.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Dictionary
    {
        #region Variables
        
        private Boolean stopFlag;
        private readonly Node root = new Node();

        #endregion

        #region Constructor

        public Dictionary(String filename)
        {
            
            using (var ms = new MemoryStream())
            {
                using (var fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open,FileAccess.Read))
                {
                    using (var zs = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        var buffer = new byte[1024];
                        while (true)
                        {
                            var length = zs.Read(buffer, 0, 1024);
                            if (length == 0)
                            {
                                break;
                            }
                            ms.Write(buffer, 0, length);
                        }
                    }
                }
                ms.Position = 0;
                using (var binReader = new BinaryReader(ms))
                {
                    while (ms.Position != ms.Length)
                    {
                        if (stopFlag)
                        {
                            break;
                        }

                        // Read length of pattern
                        var lenPattern = binReader.ReadInt32();
                        // Read pattern
                        var pattern = binReader.ReadBytes(lenPattern);
                        // Read number of words with the same pattern
                        var number = binReader.ReadInt32();
                        // Read words for the pattern
                        var words = new List<byte[]>();
                        for (var i = 0; i < number; i++)
                        {
                            var len = binReader.ReadInt32();
                            words.Add(binReader.ReadBytes(len));
                        }
                        // Add pattern and words to dictionary
                        Add(pattern, words);
                    }
                }
            }
        }

        private void Add(byte[] pattern, List<byte[]> words)
        {
            var actualNode = root;
            for (var i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] > 25)
                {
                    throw new Exception("Symbol > 25 not possible in dictionary");
                }
                if (actualNode.Nodes[pattern[i]] == null)
                {
                    actualNode.Nodes[pattern[i]] = new Node();
                }
                actualNode = actualNode.Nodes[pattern[i]];
            }
            if (actualNode.Words != null)
            {
                throw new Exception("Already words for this pattern stored in dictionary!");
            }
            actualNode.Words = words;
        }

        #endregion

        #region Properties

        public Boolean StopFlag
        {
            get {return stopFlag;}
            set { stopFlag = value; }
        }

        #endregion    
    
        public List<byte[]> GetWordsFromPattern(byte[] pattern)
        {
            var actualNode = root;
            for (var i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] > 25)
                {
                    throw new Exception("Symbol > 25 not possible in dictionary");
                }
                actualNode = actualNode.Nodes[pattern[i]];
                if (actualNode == null)
                {
                    return new List<byte[]>();
                }
            }
            return (actualNode.Words ?? new List<byte[]>());
        }
    }

    public class Node
    {
        public Node[] Nodes = new Node[26];
        public List<byte[]> Words { get; set; }
    }
}
