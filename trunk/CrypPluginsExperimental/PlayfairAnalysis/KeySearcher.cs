/*
   Copyright 2010 CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Mischel.Collections;

using System.Windows.Controls;
using Cryptool.PluginBase.IO;


namespace Cryptool.Plugins.PlayfairAnalysis
{
    class KeySearcher
    {
        int matrixSize = 5;
        double[,] bigraphStat;
        double[,] CipherStat;
        int[, ,] DecodingTab;
        int DecodingTabLength;
        string cipherText;
        string plainText;        
        int keyHeapSize;
        string alphabet;


        public string CipherText
        {
            get
            {                
                return cipherText;
            }
            set
            {
                cipherText = value;
            }
        }

        public string PlainText
        {
            get
            {
                return plainText;
            }
            set
            {
                plainText = value;
            }
        }

        public double[,] BigraphStat
        {
            get
            {
                return bigraphStat;
            }
            set
            {
                bigraphStat = value;
            }
        }

        public int KeyHeapSize
        {
            get
            {
                return keyHeapSize;
            }
            set
            {
                keyHeapSize = value;
            }
        }

        public int MatrixSize
        {
            get
            {
                return matrixSize;
            }
            set
            {
                matrixSize = value;
            }
        }

        public KeySearcher()
        {
            bigraphStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            CipherStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            DecodingTab = new int[(int)Math.Pow(matrixSize, 2) * ((int)Math.Pow(matrixSize, 2) - 1), 2, 2];
        }

        public KeySearcher(int matrixSize)
        {
            this.matrixSize = matrixSize;
            bigraphStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            CipherStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            DecodingTab = new int[(int)Math.Pow(matrixSize, 2) * ((int)Math.Pow(matrixSize, 2) - 1), 2, 2];  
        }

        public KeySearcher(int matrixSize, int keyHeapSize, double[,] bs, string alphabet, string cipherText)
        {
            this.matrixSize = matrixSize;
            this.keyHeapSize = keyHeapSize;            
            this.cipherText = cipherText;
            bigraphStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            CipherStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            DecodingTab = new int[(int)Math.Pow(matrixSize, 2) * ((int)Math.Pow(matrixSize, 2) - 1), 2, 2];
            this.bigraphStat = bs;
            this.alphabet = alphabet;
        }



        public string Encrypt(Key key, string PlainText)
        {
            char Char1, Char2;
            int RowChar1, RowChar2, ColumnChar1, ColumnChar2;
            StringBuilder CipherText = new StringBuilder(PlainText.Length / 2);

            for (int i = 0; i < PlainText.Length; i += 2)
            {
                Char1 = PlainText[i];
                Char2 = PlainText[i + 1];

                int[] pos1 = key.GetPosition(Char1);
                int[] pos2 = key.GetPosition(Char2);

                RowChar1 = pos1[0];
                ColumnChar1 = pos1[1];
                RowChar2 = pos2[0];
                ColumnChar2 = pos2[1];

                if (RowChar1 == RowChar2)
                {
                    CipherText.Append(key.Mat[RowChar1, (ColumnChar1 + 1) % matrixSize]);
                    CipherText.Append(key.Mat[RowChar2, (ColumnChar2 + 1) % matrixSize]);
                }
                else if (ColumnChar1 == ColumnChar2)
                {
                    CipherText.Append(key.Mat[(RowChar1 + 1) % matrixSize, ColumnChar1]);
                    CipherText.Append(key.Mat[(RowChar2 + 1) % matrixSize, ColumnChar2]);
                }
                else
                {
                    CipherText.Append(key.Mat[RowChar1, ColumnChar2]);
                    CipherText.Append(key.Mat[RowChar2, ColumnChar1]);
                }

            }

            return CipherText.ToString();
        }

        public string Decrypt(Key key, string CipherText)
        {
            char Char1, Char2;
            int RowChar1, RowChar2, ColumnChar1, ColumnChar2;
            StringBuilder PlainText = new StringBuilder(CipherText.Length);

            for (int i = 0; i < CipherText.Length; i += 2)
            {
                Char1 = CipherText[i];
                Char2 = CipherText[i + 1];

                int[] pos1 = key.GetPosition(Char1);
                int[] pos2 = key.GetPosition(Char2);

                RowChar1 = pos1[0];
                ColumnChar1 = pos1[1];
                RowChar2 = pos2[0];
                ColumnChar2 = pos2[1];

                if (RowChar1 == RowChar2)
                {
                    PlainText.Append(key.Mat[RowChar1, (ColumnChar1 + matrixSize - 1) % matrixSize]);
                    PlainText.Append(key.Mat[RowChar2, (ColumnChar2 + matrixSize - 1) % matrixSize]);
                }
                else if (ColumnChar1 == ColumnChar2)
                {
                    PlainText.Append(key.Mat[(RowChar1 + matrixSize - 1) % matrixSize, ColumnChar1]);
                    PlainText.Append(key.Mat[(RowChar2 + matrixSize - 1) % matrixSize, ColumnChar2]);
                }
                else
                {
                    PlainText.Append(key.Mat[RowChar1, ColumnChar2]);
                    PlainText.Append(key.Mat[RowChar2, ColumnChar1]);
                }

            }

            return PlainText.ToString();
        }
        
        public string Format(string UnformattedText)
        {
            string PreformattedText;
            StringBuilder FormattedText = new StringBuilder(UnformattedText.Length);
            
            PreformattedText = UnformattedText.ToUpper();

            switch (matrixSize)
            {
                case 5:
                    for (int i = 0; i < PreformattedText.Length; i += 1)
                    {
                        if (PreformattedText[i] == 'J')
                            FormattedText.Append("I");
                        else if (PreformattedText[i] >= 'A' && PreformattedText[i] <= 'Z')
                        {
                            FormattedText.Append(PreformattedText[i]);
                        }
                        else if (PreformattedText[i] == 'Ä')
                            FormattedText.Append("AE");
                        else if (PreformattedText[i] == 'Ö')
                            FormattedText.Append("OE");
                        else if (PreformattedText[i] == 'Ü')
                            FormattedText.Append("UE");
                        else if (PreformattedText[i] == 'ß')
                            FormattedText.Append("SS");
                    }
                    break;
                case 6:
                    for (int i = 0; i < PreformattedText.Length; i += 1)
                    {

                        if ((PreformattedText[i] >= 'A' && PreformattedText[i] <= 'Z') || (PreformattedText[i] >= '0' && PreformattedText[i] <= '9'))
                        {
                            FormattedText.Append(PreformattedText[i]);
                        }
                        else if (PreformattedText[i] == 'Ä')
                            FormattedText.Append("AE");
                        else if (PreformattedText[i] == 'Ö')
                            FormattedText.Append("OE");
                        else if (PreformattedText[i] == 'Ü')
                            FormattedText.Append("UE");
                        else if (PreformattedText[i] == 'ß')
                            FormattedText.Append("SS");
                    }
                    break;
            }

            int Length = FormattedText.Length;
            for (int i = 0; i < Length - 1; i += 2)
            {
                if (FormattedText[i] == FormattedText[i + 1])
                {
                    if (FormattedText[i] != 'X')
                    {
                        FormattedText = FormattedText.Insert(i + 1, "X");                        
                    }
                    else
                    {
                        FormattedText = FormattedText.Insert(i + 1, "Y");
                    }
                    Length += 1;
                }
            }

            if ((FormattedText.Length % 2) == 1)
            {
                if (FormattedText[FormattedText.Length - 1] != 'X')
                {
                    FormattedText.Append("X");
                }
                else
                {
                    FormattedText.Append("Y");
                }
            }

            return FormattedText.ToString();
        }

        public void Attack()
        {                        
            BigraphStatistic CS = new BigraphStatistic(matrixSize);
            CipherStat = CS.CalcLogStat(cipherText, alphabet);            
            int[] TestKey = new int[(int)Math.Pow(matrixSize, 2)];
            double Score2;
            int[] WorstKey = new int[(int)Math.Pow(matrixSize, 2)];
            int[] BestKey = new int[(int)Math.Pow(matrixSize, 2)];
            double WorstScore = 0;
            double BestScore;
            int Count = 0;

                        
            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("Calculating decoding tab..", NotificationLevel.Info);                
            }
            
            CalcDecodingTab(5);


            //SortedDictionary<double, int[]> KeyHeap = new SortedDictionary<double, int[]>();            
            //SortedDictionary<double, int[]>.KeyCollection KeyColl = KeyHeap.Keys;

            PriorityQueue<int[], double>[] keyHeap = new PriorityQueue<int[], double>[2];
            keyHeap[0] = new PriorityQueue<int[], double>();
            keyHeap[1] = new PriorityQueue<int[], double>();


            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("Testing all partial keys of length 5 ", NotificationLevel.Info);
            }
           

            DateTime time1 = DateTime.Now;

            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {

                if (ProgressChangedByKeySearcher != null)
                {
                    ProgressChangedByKeySearcher(i, (int)Math.Pow(matrixSize, 2) * 2);
                }
                
                TestKey[0] = i;
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                    if (j != i)
                    {
                        TestKey[1] = j;
                        for (int k = 0; k < (int)Math.Pow(matrixSize, 2); k++)
                            if (k != i && k != j)
                            {
                                TestKey[2] = k;
                                for (int l = 0; l < (int)Math.Pow(matrixSize, 2); l++)
                                    if (l != i && l != j && l != k)
                                    {
                                        TestKey[3] = l;
                                        for (int m = 0; m < (int)Math.Pow(matrixSize, 2); m++)
                                            if (m != i && m != j && m != k && m != l)
                                            {
                                                TestKey[4] = m;
                                                Score2 = EvaluateKey2(TestKey);
                                                if (Count > keyHeapSize - 1)
                                                {
                                                    if (Score2 < WorstScore)
                                                    {
                                                        try
                                                        {
                                                            //if (!KeyHeap.Contains(Score2))
                                                            {
                                                                keyHeap[0].Dequeue();
                                                                keyHeap[0].Enqueue((int[])TestKey.Clone(), Score2);
                                                                WorstScore = keyHeap[0].Peek().Priority;
                                                            }
                                                        }
                                                        catch (ArgumentException)
                                                        {
                                                            Console.Out.WriteLine("Wert bereits im Heap (> {0})", keyHeapSize);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        keyHeap[0].Enqueue(TestKey, Score2);
                                                        WorstScore = Math.Max(WorstScore, Score2);
                                                        Count = keyHeap[0].Count;
                                                    }
                                                    catch (ArgumentException)
                                                    {
                                                        Console.Out.WriteLine("Wert bereits im Heap (< {0})", keyHeapSize);
                                                    }
                                                }
                                            }
                                    }
                            }
                    }
            }


            DateTime time2 = DateTime.Now;
            TimeSpan diff = time2 - time1;

            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("\n\ntime required: " + Convert.ToString(diff.TotalSeconds) + " seconds", NotificationLevel.Info);
            }
            

            WorstKey = keyHeap[0].Peek().Value;
            BestScore = double.MaxValue;
            foreach (PriorityQueueItem<int[], double> pqi in keyHeap[0])
            {
                if (pqi.Priority < BestScore)
                {
                    BestScore = pqi.Priority;
                    BestKey = pqi.Value;
                }
            }

            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("\nBest Score: " +  Convert.ToString(BestScore), NotificationLevel.Info);
                LogMessageByKeySearcher("\nBest Key: ", NotificationLevel.Info);

                LogMessageByKeySearcher("\nWorst Score: " + Convert.ToString(WorstScore), NotificationLevel.Info);
                LogMessageByKeySearcher("\nWorst Key: ", NotificationLevel.Info);

                LogMessageByKeySearcher("\nAmount of keys in Heap: " + Convert.ToString(keyHeap[0].Count), NotificationLevel.Info);
                LogMessageByKeySearcher("\nTesting next position of keys in heap...", NotificationLevel.Info);
            }


            time1 = DateTime.Now;

            for (int pos = 5; pos < (int)Math.Pow(matrixSize, 2); pos++)
            {
                keyHeap[pos % 2].Clear();
                CalcDecodingTab(pos + 1);
                Count = 0;

                if (ProgressChangedByKeySearcher != null)
                {
                    ProgressChangedByKeySearcher((pos - 5) + ((int)Math.Pow(matrixSize, 2) - 5), ((int)Math.Pow(matrixSize, 2) - 5) * 2);
                }

                foreach (PriorityQueueItem<int[], double> pqi in keyHeap[(pos + 1) % 2])
                {
                    bool[] letterinkey = new bool[(int)Math.Pow(matrixSize, 2)];
                    for (int i = 0; i < pos; i++)
                    {
                        letterinkey[pqi.Value[i]] = true;
                    }
                    for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
                    {
                        if (!letterinkey[i])
                        {
                            pqi.Value[pos] = i;
                            Score2 = EvaluateKey2(pqi.Value);
                            if (Count > keyHeapSize - 1)
                            {
                                if (Score2 < WorstScore)
                                {
                                    keyHeap[pos & 1].Dequeue();
                                    keyHeap[pos & 1].Enqueue((int[])pqi.Value.Clone(), Score2);
                                    WorstScore = keyHeap[pos & 1].Peek().Priority;
                                }
                            }
                            else
                            {
                                keyHeap[pos & 1].Enqueue(pqi.Value, Score2);
                                WorstScore = Math.Max(WorstScore, Score2);
                                Count = keyHeap[pos & 1].Count;
                            }
                        }
                    }
                }
                if (LogMessageByKeySearcher != null)
                {
                    LogMessageByKeySearcher("Position " + Convert.ToString(pos+1) + " done.", NotificationLevel.Info);
                }
            }


            time2 = DateTime.Now;
            diff = time2 - time1;

            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("\ntime required: " + Convert.ToString(diff.TotalSeconds) + " seconds", NotificationLevel.Info);
            }
            

            BestScore = double.MaxValue;

            foreach (PriorityQueueItem<int[], double> pqi in keyHeap[((int)Math.Pow(matrixSize, 2) - 1) % 2])
            {
                if (pqi.Priority < BestScore)
                {
                    BestScore = pqi.Priority;
                    BestKey = (int[])pqi.Value.Clone();
                }
            }


            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("\nBest Score: " + Convert.ToString(BestScore), NotificationLevel.Info);
                LogMessageByKeySearcher("\nBest Key: ", NotificationLevel.Info);
            }


            int[] CorrectKey = new int[(int)Math.Pow(matrixSize, 2)];
            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
                CorrectKey[i] = i;

            if (LogMessageByKeySearcher != null)
            {
                LogMessageByKeySearcher("\nCorrect Key Score: " + Convert.ToString(EvaluateKey2(CorrectKey)), NotificationLevel.Info);
                LogMessageByKeySearcher("\nCorrect Key: ", NotificationLevel.Info);
            }
            
            
            Key BestKeyMatrix = new Key(Key.ConvertToChar(BestKey, alphabet));

            plainText =  Decrypt(BestKeyMatrix, cipherText);
            
        }

        public Double EvaluateKey(int[] Key)
        {
            double Mean = 0;
            int MeanLength = 0;
            double SumProbCipher = 0;
            int BigraphsInCipher = 0;
            double Score = 0;

            for (int i = 0; i < DecodingTabLength; i++)
            {
                if (bigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] > -9)
                {
                    Mean += bigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]];
                    MeanLength++;
                }
                if (CipherStat[Key[DecodingTab[i, 0, 0]], Key[DecodingTab[i, 0, 1]]] > -100)
                {
                    SumProbCipher += CipherStat[Key[DecodingTab[i, 0, 0]], Key[DecodingTab[i, 0, 1]]];
                    BigraphsInCipher++;
                }
            }

            //Console.Out.WriteLine("Summe der " + Convert.ToString(MeanLength) + " allg. Wahrsch. > -9 : " + Convert.ToString(Mean));
            Mean /= MeanLength;

            //Console.Out.WriteLine("Durchschnitt: " + Convert.ToString(Mean));

            //Console.Out.WriteLine("Summe der " + Convert.ToString(BigraphsInCipher) + " Wahrsch. > -100 im Ciphertext: " + Convert.ToString(SumProbCipher));

            Score = SumProbCipher - BigraphsInCipher * Mean;

            //Console.Out.WriteLine("Score: " + Convert.ToString(Score));

            return Score;
        }

        public Double EvaluateKey2(int[] Key)
        {
            Double Score2 = 0;
                        
            for (int i = 0; i < DecodingTabLength; i++)
            {
                //if (bigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] > -11)
                Score2 += Math.Abs(bigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] - CipherStat[Key[DecodingTab[i, 0, 0]], Key[DecodingTab[i, 0, 1]]]);
            }

            return Score2;
        }

        public int[, ,] CalcDecodingTab(int KeyLength)
        {
            int index = 0;

            for (int i = 0; i < KeyLength - 1; i++)
            {
                for (int j = i + 1; j < KeyLength; j++)
                {
                    if ((i / matrixSize) == (j / matrixSize))     // i and j in same row
                    {
                        if ((i % matrixSize) > 0)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i - 1;
                            DecodingTab[index, 1, 1] = j - 1;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                        else if (i + matrixSize - 1 < KeyLength)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i + matrixSize - 1;
                            DecodingTab[index, 1, 1] = j - 1;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                    }

                    else if ((i % matrixSize) == (j % matrixSize))      // i and j in same column
                    {
                        if ((i / matrixSize) > 0)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i - matrixSize;
                            DecodingTab[index, 1, 1] = j - matrixSize;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                        else if (i + (matrixSize * (matrixSize - 1)) < KeyLength)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i + (matrixSize * (matrixSize - 1));
                            DecodingTab[index, 1, 1] = j - matrixSize;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                    }
                    else if ((i / matrixSize) * matrixSize + (j % matrixSize) < KeyLength && (j / matrixSize) * matrixSize + (i % matrixSize) < KeyLength)   // i and j in a square
                    {
                        DecodingTab[index, 0, 0] = i;
                        DecodingTab[index, 0, 1] = j;
                        DecodingTab[index, 1, 0] = (i / matrixSize) * matrixSize + (j % matrixSize);
                        DecodingTab[index, 1, 1] = (j / matrixSize) * matrixSize + (i % matrixSize);
                        index++;
                        DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                        DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                        DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                        DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                        index++;
                    }

                }
            }

            DecodingTabLength = index;
            return DecodingTab;
        }

        #region Event Handling


        public delegate void LogMessageByKeySearcherEventHandler(string message, NotificationLevel logLevel);
        public event LogMessageByKeySearcherEventHandler LogMessageByKeySearcher;

        public delegate void ProgressChangedByKeySearcherEventHandler(double value, double max);
        public event ProgressChangedByKeySearcherEventHandler ProgressChangedByKeySearcher;

        
        #endregion

    }
}
