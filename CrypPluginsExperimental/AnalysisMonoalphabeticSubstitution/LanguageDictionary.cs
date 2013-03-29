using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class LanguageDictionary
    {
        private List<string> dic = new List<string>();        

        public LanguageDictionary(string dic, char separator) 
        {
            string[] words = dic.Split(separator);

            for (int i = 0; i < words.Length; i++)
            {
                this.dic.Add(words[i]);
            }
        }
    }
}
