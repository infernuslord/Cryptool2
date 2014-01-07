using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    public class KeyCandidate : IEquatable<KeyCandidate>
    {
        int[] key;
        double fitness;
        String plaintext;

        public int[] Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        public double Fitness
        {
            get { return this.fitness; }
            set { this.fitness = value; }
        }

        public String Plaintext
        {
            get { return this.plaintext; }
            set { ; }
        }

        public KeyCandidate(int[] key, double fitness, String plaintext)
        {
            this.key = key;
            this.fitness = fitness;
            this.plaintext = plaintext;
        }

        public bool Equals(KeyCandidate keyCandidate)
        {
            if (this.plaintext.Equals(keyCandidate.plaintext))
            {
                return true;
            }

            return false;

           /* if (keyCandidate == null)
            {
                return false;
            }

            if (this.key.Length != keyCandidate.key.Length)
            {
                return false;
            }

            for (int i = 0; i < this.key.Length; i++)
            {
                if (this.key[i] != keyCandidate.key[i])
                {
                    return false;
                }
            }

            return true;*/
        }
    }

    class KeyCandidateComparer : IComparer<KeyCandidate>
    {
        public int Compare(KeyCandidate a, KeyCandidate b)
        {
            return -a.Fitness.CompareTo(b.Fitness);
        }
    }
}
