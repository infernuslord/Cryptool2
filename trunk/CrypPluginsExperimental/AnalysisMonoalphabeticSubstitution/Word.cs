using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Word
    {
        #region Variables

        String text;
        byte[] byteValue;
        byte[] pattern;
        Candidate[] candidates;
        bool enabled = true;

        #endregion

        # region Constructor

        public Word()
        {

        }

        public Word(String text, int wordnum, byte[] byteValue)
        {
	        this.text = text;
	        this.byteValue = byteValue;

	        this.pattern = makePattern(byteValue);

            List<Byte> f = new List<Byte>();
            for (int i = 0; i < byteValue.Length; i++) 
            {
	            if (!f.Contains(byteValue[i]))
                {
		            f.Add(byteValue[i]);
                }
	        }
        }

        #endregion
       
        #region Properties

        public String Text
        {
            get { return this.text; }
            set { ; }
        }

        public Byte[] ByteValue
        {
            get { return this.byteValue; }
            set { ; }
        }

        public Candidate[] Candidates
        {
            get { return this.candidates; }
            set { this.candidates = value; }
        }

        public Byte[] Pattern
        {
            get { return this.pattern; }
            set { ; }
        }

        public Boolean Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        #endregion

        #region Helper Methods

        public static byte[] makePattern(byte[] word)
        {
	        byte[] pattern = new byte[word.Length];

	        byte next = 1;

	        byte[] map = new byte[256];

	        for (int i = 0; i < word.Length; i++)
	        {
		        int c = word[i]&0xff;

		        if (map[c]==0) 
                {
		            map[c] = next;
		            next++;
		        }

		        pattern[i] = (byte) ( map[c] - 1);
	        }

	        return pattern;
        }

        public static int compareArrays(byte[] a, byte[] b)
        {
	        if (a.Length != b.Length)
            {
	            return a.Length - b.Length;
            }
	        
            for (int i = 0; i < a.Length; i++)
	        {
		        if (a[i]!=b[i])
                {
		            return a[i]-b[i];
                }
	        }
	        return 0;
        }

        #endregion
    }

    class WordComparer : IComparer<Word>
    {
        public int Compare(Word a, Word b)
        {
            if (b == null)
            {
                return -1;
            }
            return Word.compareArrays(a.ByteValue, b.ByteValue);
        }
    }

    class PatternComparer : IComparer<Word>
    {
        public int Compare(Word a, Word b)
        {
            int i = Word.compareArrays(a.Pattern, b.Pattern);
            if (i != 0)
            {
                return i;
            }
            return a.Text.CompareTo(b.Text);
        }
    }

    class TextComparer : IComparer<Word>
    {
        public int Compare(Word a, Word b)
        {
            return a.Text.CompareTo(b.Text);
        }
    }

    class Candidate
    {
        #region Variables

        double fitness;
        byte[] byteValue;

        #endregion

        #region Properties

        public Double Fitness
        {
            get { return this.fitness; }
            set { ; }
        }

        public Byte[] ByteValue
        {
            get { return this.byteValue; }
            set { ; }
        }

        #endregion

        #region Constructor

        public Candidate(byte[] byteValue, double fitness)
        {
            this.byteValue = byteValue;
            this.fitness = fitness;
        }

        #endregion
    }
    
}
