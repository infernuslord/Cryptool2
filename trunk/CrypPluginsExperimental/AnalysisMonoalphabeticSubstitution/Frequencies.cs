using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Frequencies
    {
        #region Private Variables

        private int[][][][][] freq5gram;
        private double[][][][][] prob5gram;
        private Alphabet alpha;
        private int ratio5gram = 0;

        #endregion

        #region Constructor

        public Frequencies(Alphabet alphabet)
        {
            this.alpha = alphabet;

            int size = alphabet.Length;
            this.freq5gram = new int[size][][][][];
            this.prob5gram = new double[size][][][][];
            for (int i = 0; i < size;i++ )
            {
                this.freq5gram[i] = new int[size][][][];
                this.prob5gram[i] = new double[size][][][];
                for (int j = 0; j < size; j++)
                {
                    this.freq5gram[i][j] = new int[size][][];
                    this.prob5gram[i][j] = new double[size][][];
                    for (int k = 0; k < size; k++)
                    {
                        this.freq5gram[i][j][k] = new int[size][];
                        this.prob5gram[i][j][k] = new double[size][];
                        for (int l = 0; l < size; l++)
                        {
                            this.freq5gram[i][j][k][l] = new int[size];
                            this.prob5gram[i][j][k][l] = new double[size];
                        }
                    }
                }
            }
        }

        #endregion

        #region Properties

        public int SizeFrequencies5gram
        {
            get { return this.freq5gram.Length; }
            private set { ; }
        }

        public int Ratio5gram
        {
            get { return this.ratio5gram; }
            private set { ; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get frequency
        /// </summary>
        public int GetFrequency5gram(int l0, int l1, int l2, int l3, int l4)
        {
            return this.freq5gram[l0][l1][l2][l3][l4];
        }

        /// <summary>
        /// Get probability
        /// </summary>
        public double GetLogProb5gram(int l0, int l1, int l2, int l3, int l4)
        {
            return this.prob5gram[l0][l1][l2][l3][l4];
        }

        /// <summary>
        /// Generate 2-gram and 5-gram frequencies of a text and store them 
        /// </summary>
        /// 
        public void updateFrequenciesProbabilities(String filename)
        {
            using (TextReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename)))
            {
                for (int i = 0; i < this.freq5gram.Length; i++)
                {
                    for (int j = 0; j < this.freq5gram.Length; j++)
                    {
                        for (int k = 0; k < this.freq5gram.Length; k++)
                        {
                            for (int l = 0; l < this.freq5gram.Length; l++)
                            {
                                for (int m = 0; m < this.freq5gram.Length; m++)
                                {
                                    string line = reader.ReadLine();
                                    double nr = double.Parse(line);
                                    this.prob5gram[i][j][k][l][m] = nr;
                                }
                            }
                        }
                    }
                }
            }
            
        }

        public void updateFrequenciesProbabilities(Text text)
        {
            // Set ratios and frequencies to zero
            this.ratio5gram = 0;
            for (int i = 0; i < this.freq5gram.Length; i++)
            {
                for (int j = 0; j < this.freq5gram.Length; j++)
                {
                    for (int k = 0; k < this.freq5gram.Length; k++)
                    {
                        for (int l = 0; l < this.freq5gram.Length; l++)
                        {
                            for (int m = 0; m < this.freq5gram.Length; m++)
                            {
                                this.freq5gram[i][j][k][l][m] = 0;
                            }
                        }
                    }
                }
            }

            // Extract frequencies
            int pos1, pos2, pos3, pos4;
            for (int pos0 = 0; pos0 < text.Length; pos0++)
            {
                while ((pos0 < text.Length) && (text.GetLetterAt(pos0) < 0))
                {
                    pos0++;
                }
                if (pos0 >= text.Length)
                {
                    break;
                }
                pos1 = pos0 + 1;
                while ((pos1 < text.Length) && (text.GetLetterAt(pos1) < 0))
                {
                    pos1++;
                }
                if (pos1 >= text.Length)
                {
                    continue;
                }

                pos2 = pos1 + 1;
                while ((pos2 < text.Length) && (text.GetLetterAt(pos2) < 0))
                {
                    pos2++;
                }
                if (pos2 >= text.Length)
                {
                    continue;
                }
                pos3 = pos2 + 1;
                while ((pos3 < text.Length) && (text.GetLetterAt(pos3) < 0))
                {
                    pos3++;
                }
                if (pos3 >= text.Length)
                {
                    continue;
                }
                pos4 = pos3 + 1;
                while ((pos4 < text.Length) && (text.GetLetterAt(pos4) < 0))
                {
                    pos4++;
                }
                if (pos4 >= text.Length)
                {
                    continue;
                }
                this.freq5gram[text.GetLetterAt(pos0)][text.GetLetterAt(pos1)][text.GetLetterAt(pos2)][text.GetLetterAt(pos3)][text.GetLetterAt(pos4)]++;
                this.ratio5gram++;
            }

            // Generate probabilities with Simple-Good-Turing algorithm
            Dictionary<int, int> table_rn = new Dictionary<int, int>();
            for (int i = 0; i < this.freq5gram.Length; i++)
            {
                for (int j = 0; j < this.freq5gram.Length; j++)
                {
                    for (int k = 0; k < this.freq5gram.Length; k++)
                    {
                        for (int l = 0; l < this.freq5gram.Length; l++)
                        {
                            for (int m = 0; m < this.freq5gram.Length; m++)
                            {
                                if (table_rn.ContainsKey(this.freq5gram[i][j][k][l][m]))
                                {
                                    table_rn[this.freq5gram[i][j][k][l][m]]++;
                                }
                                else
                                {
                                    table_rn.Add(this.freq5gram[i][j][k][l][m],1);
                                }
                            }
                        }
                    }
                }
            }
            int unseen = table_rn[0];
            table_rn.Remove(0);

            int N = this.ratio5gram;
            double N_1, a, b;
            
            int[] t_r = new int[table_rn.Count];
            int[] t_n = new int[table_rn.Count];
            double[] t_z = new double[table_rn.Count];
            double[] t_logr = new double[table_rn.Count];
            double[] t_logz = new double[table_rn.Count];
            double[] t_rstar = new double[table_rn.Count];
            double[] t_p = new double[table_rn.Count];

            // fill r and n

            List<int> keylist = table_rn.Keys.ToList<int>();
            keylist.Sort();
            for (int i = 0; i < keylist.Count; i++)
            {
                t_r[i] = keylist[i];
                t_n[i] = table_rn[keylist[i]];
            }

            double P0 = (double)t_n[getIndexOfField(t_r,1)]/N;
            // fill Z
            int var_i, var_k = 0;
            for (int index_j=0;index_j<t_r.Length;index_j++){
                if (index_j==0){
                    var_i = 0;
                    var_k = t_r[index_j + 1];
                } else if (index_j == t_r.Length - 1){
                    var_i = t_r[index_j - 1];
                    var_k = 2 * t_r[index_j] - var_i;
                } else {
                    var_i = t_r[index_j - 1];
                    var_k = t_r[index_j + 1];
                }
                t_z[index_j] = ((double)2*t_n[index_j])/(var_k - var_i);
            }
            // fill logr and logz
            for (int j=0;j<t_logr.Length;j++)
            {
                t_logr[j] = Math.Log(t_r[j]);
                t_logz[j] = Math.Log(t_z[j]);
            }
            // find a and b
            LinReg(t_logr,t_logz, out a, out b);

            // fill r*
            bool useY = false;
            for (int i = 0; i < t_r.Length; i++)
            {
                double y = ((double) (t_r[i] + 1)) * (Math.Exp(a* Math.Log(t_r[i] + 1) + b) / Math.Exp(a* Math.Log(t_r[i]) + b));
                
                // if r+1 not in t_r
                if (useY)
                {
                    t_rstar[i] = y;
                }
                else
                {
                    double x = (t_r[i] + 1) * ((double) t_n[getIndexOfField(t_r, (t_r[i] + 1))] / t_n[getIndexOfField(t_r, t_r[i])]);

                    double lside = Math.Abs(x - y);

                    double n_r1 = t_n[getIndexOfField(t_r, (t_r[i] + 1))];
                    double n_r = t_n[getIndexOfField(t_r, t_r[i])];
                    double rside = 1.96 * Math.Sqrt((t_r[i] + 1) * (t_r[i] + 1) * ((double)n_r1/(n_r*n_r)) * (1 + ((double)n_r1/n_r))); 
                            
                    if (lside > rside && !useY)
                    {
                        t_rstar[i] = x;
                    }
                    else
                    {
                        t_rstar[i] = y;
                        useY = true;
                    }
                }  
            }

            N_1 = 0;
            
            for (int i=0;i<t_rstar.Length;i++)
            {
                N_1 += t_n[i]*t_rstar[i];
            }
            for (int i = 0; i < t_p.Length; i++)
            {
                t_p[i] = (1 - P0) * (t_rstar[i] / N_1);
            }

            // fill prob array
            double prob_unseen = P0/unseen;
            for (int i = 0; i < this.freq5gram.Length; i++)
            {
                for (int j = 0; j < this.freq5gram.Length; j++)
                {
                    for (int k = 0; k < this.freq5gram.Length; k++)
                    {
                        for (int l = 0; l < this.freq5gram.Length; l++)
                        {
                            for (int m = 0; m < this.freq5gram.Length; m++)
                            {
                                if (this.freq5gram[i][j][k][l][m] == 0)
                                {
                                    this.prob5gram[i][j][k][l][m] = Math.Log(prob_unseen);
                                }
                                else
                                {
                                    int index = getIndexOfField(t_r, this.freq5gram[i][j][k][l][m]);
                                    this.prob5gram[i][j][k][l][m] = Math.Log(t_p[index]);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Linear regression
        /// </summary>
        public void LinReg(double[] xValues, double[] yValues, out double a, out double b)
        {
            double sumX = 0;
            double sumY = 0;
            double sumXX = 0;
            double sumYY = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = xValues.Length;

            for (int i = 0; i < xValues.Length; i++)
            {
                sumCodeviates += xValues[i] * yValues[i];
                sumX += xValues[i];
                sumY += yValues[i];
                sumXX += xValues[i] * xValues[i];
                sumYY += yValues[i] * yValues[i];
            }

            ssX = sumXX - ((sumX * sumX) / count);
            ssY = sumYY - ((sumY * sumY) / count);

            sCo = sumCodeviates - ((sumX * sumY) / count);

            a = sCo / ssX;
            b = (sumY / count) - ((sCo / ssX) * (sumX / count));
        }

        private int getIndexOfField(int[] ar, int value)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Get position of letters in frequency array
        /// </summary>
        private int GetFreqArrayPos(params string[] lets)
        {
            int res = 0;
            int fac = 0;

            res += this.alpha.GetPositionOfLetter(lets[4]);
            if (lets[3]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[3]); 
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (1)) * (fac);
            }
            if (lets[2]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[2]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (2)) * (fac);
            }
            if (lets[1]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[1]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (3)) * (fac);
            }
            if (lets[0]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[0]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (4)) * (fac);
            }
            
            return res;
        }

        /// <summary>
        /// Power
        /// </summary>
        private int Pow(int x, int e)
        {
            int res = x;

            if (e == 0)
            {
                return 1;
            }
            for (int i = 0; i < e-1; i++)
            {
                res *= x;
            }
            return res;
        }
        
        #endregion
    }
}