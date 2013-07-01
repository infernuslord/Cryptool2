using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using System.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class LanguageDictionary
    {
        private List<string[]> dic = new List<string[]>();
        private List<bool[]> word_status = new List<bool[]>();

        #region Constructor

        public LanguageDictionary(string filename, char separator) 
        {
            using (TextReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename)))
            {
                string line;
                while ((line = reader.ReadLine()) != null){
                    dic.Add(line.Split(separator));
                }
            }

            for (int i = 0; i < this.dic.Count; i++)
            {
                this.word_status.Add(new bool[this.dic[i].Length]);
                for (int j = 0; j < this.word_status[i].Length; j++)
                {
                    this.word_status[i][j] = true;
                }
            }

        }
        #endregion

        #region Methods

        public int GetNumberOfWords(int length)
        {
            if (length <= 0)
            {
                return -1;
            }
            return this.dic[length - 1].Length;
        }

        public string GetWord(int length, int order)
        {
            if ((length <= 0) || (order < 0))
            {
                return "";
            }
            return this.dic[length-1][order];
        }

        public bool GetWordStatus(int length, int order)
        {
            return this.word_status[length - 1][order];
        }

        public void SetWordStatusFalse(int length, int order)
        {
            this.word_status[length - 1][order] = false;
        }

        public void ResetWordStatus(int length)
        {
            for (int i = 0; i < this.word_status[length - 1].Length; i++)
            {
                this.word_status[length - 1][i] = false;
            }
        }

        public void ResetAllWordStatus()
        {
            for (int i = 0; i < this.word_status.Count; i++)
            {
                for (int j = 0; j < this.word_status[i].Length; j++)
                {
                    this.word_status[i][j] = true;
                }
            }
        }

        public int GetMaxLength()
        {
            return this.dic.Count;
        }

        #endregion
    }
}
