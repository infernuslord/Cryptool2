using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Mischel.Collections;


namespace PlayFair
{
    class Program
    {
       
 
        static void Main(string[] args)
        {
            Matrix Key = new Matrix();
            Double[,] BigraphStat = new Double[25,25];

            // CreateBS(@"H:\BS\BS.txt", @"H:\BS\BS.xml");
            
            
            // Read Bigraph Statistic from xml file
            string BsPath = @"H:\BS\BSLog10.xml";
            System.Xml.Serialization.XmlSerializer ReadBS = new System.Xml.Serialization.XmlSerializer(typeof(Double[][]));
            System.Xml.XmlReader XmlReader = System.Xml.XmlReader.Create(BsPath);
            Double[][] BigraphStatDummy = (Double[][])ReadBS.Deserialize(XmlReader);
            XmlReader.Close();

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    BigraphStat[i,j] = BigraphStatDummy[i][j];
                }
            }

            // Read Key from txt file
            Key.ReadMatrixFile(@"H:\Key.txt");

            // Create Ciphertext from txt file
            string CipherText = PlayFair.Encrypt(Key, PlayFair.Format(System.IO.File.ReadAllText(@"H:\Plaintext.txt")));
                      
                        
            string CrackedCipherText = PlayFair.Attack(CipherText, BigraphStat);

                             
            Console.WriteLine("\nMatch: " + (CrackedCipherText == PlayFair.Format(System.IO.File.ReadAllText(@"H:\Plaintext.txt"))));
            Console.WriteLine("\nCracked Cipher:\n\n" + CrackedCipherText);            
            Console.In.ReadLine();
            
                        
        }

        // Create a Bigraph Statistic and write in both txt and xml file
        static void CreateBS(string PathTxt, string PathXml)
        {
            string[] StatTextPath = { @"H:\Texte\Text1.txt", @"H:\Texte\Text2.txt", @"H:\Texte\Text3.txt", @"H:\Texte\Text4.txt",
                                      @"H:\Texte\Text5.txt", @"H:\Texte\Text6.txt", @"H:\Texte\Text7.txt", @"H:\Texte\Text8.txt",
                                      @"H:\Texte\Text9.txt", @"H:\Texte\Text10.txt", @"H:\Texte\Text11.txt", @"H:\Texte\Text12.txt",
                                      @"H:\Texte\Text13.txt", @"H:\Texte\Text14.txt", @"H:\Texte\Text15.txt", @"H:\Texte\Text16.txt",
                                      @"H:\Texte\Text17.txt", @"H:\Texte\Text18.txt", @"H:\Texte\Text19.txt", @"H:\Texte\Text20.txt"};

            BigraphStatistic BS = new BigraphStatistic();
            int[,] BigraphStat = BS.Generate(StatTextPath);

            string[] Tab = new string[25];
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    Tab[i] += Convert.ToString(BigraphStat[i, j]) + " ";
                    Tab[i] += "\t";
                }
            }

            System.IO.File.WriteAllLines(PathTxt, Tab);

            int[][] BigraphStatDummy = new int[25][];

            for (int i = 0; i < 25; i++)
            {
                BigraphStatDummy[i] = new int[25];
                for (int j = 0; j < 25; j++)
                {
                    BigraphStatDummy[i][j] = BigraphStat[i, j];
                }
            }
                        
            System.Xml.Serialization.XmlSerializer WriteBS = new System.Xml.Serialization.XmlSerializer(typeof(Int32[][]));
            System.Xml.XmlWriter XmlWriter = System.Xml.XmlWriter.Create(PathXml);
            WriteBS.Serialize(XmlWriter, BigraphStatDummy);
            XmlWriter.Close();

            // Compute log-probabilities
            int sum = 0;
            foreach (int value in BigraphStat)
                sum += value;
           

            double[][] BigraphStatLog = new double[25][];

            for (int i = 0; i < 25; i++ )
            {
                BigraphStatLog[i] = new double[25];
                for (int j = 0; j < 25; j++)
                {                    
                    if (BigraphStat[i, j] > 0)
                    {
                        BigraphStatLog[i][j] = Math.Log((double)BigraphStat[i, j] / sum);                        
                    }
                    else
                        BigraphStatLog[i][j] = -100;
                }
            }

            string[] TabLog = new string[25];
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    TabLog[i] += Convert.ToString(BigraphStatLog[i][j]) + "\t";                    
                }
            }
            System.IO.File.WriteAllLines(@"H:\BS\BSLog100.txt", TabLog);

            System.Xml.Serialization.XmlSerializer WriteBSLog = new System.Xml.Serialization.XmlSerializer(typeof(Double[][]));
            System.Xml.XmlWriter XmlWriterLog = System.Xml.XmlWriter.Create(@"H:\BS\BSLog100.xml");
            WriteBSLog.Serialize(XmlWriterLog, BigraphStatLog);
            XmlWriterLog.Close();
           

        }
    }


    class Matrix
    {
        public char[,] Mat;
        public static char[] Alphabet = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K','L', 'M', 'N', 'O', 
                                  'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
        public Matrix()
        {
            Mat = new char[5,5];
        }

        public Matrix(char[,] matrix)
        {
            Mat = matrix;            
        }        

        public void ReadMatrixConsole()
        {
            string s;

            for (int i = 0; i < 5; i += 1)
            {
                s = Console.In.ReadLine();
                int pos = -1;

                for (int j = 0; j < 5; j += 1)
                {
                    pos = s.IndexOfAny(Alphabet, pos + 1);
                    Mat[i,j] = s[pos];
                }
            }
        }

        public void ReadMatrixFile(string Path)
        {
            string[] s = System.IO.File.ReadAllLines(Path);

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    Mat[i, j] = s[i][j];
        }

        public int[] GetPosition(char Char)
        {
            for (int i = 0; i < 5; i += 1)
            {
                for (int j = 0; j < 5; j += 1)
                {
                    if (Char == Mat[i,j])
                    {
                        return new int[]{i,j};
                    }
                }
            }
            return null;
        }

        public void WriteOnConsole()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {                    
                    Console.Out.Write(Mat[i, j] + " ");
                }
                Console.Out.WriteLine("");
            }
        }

        public static void WriteOnConsoleInt(int[] matrix)
        {            
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (matrix[i * 5 + j] + 'A' < 'J')                       
                        Console.Out.Write((char)(matrix[i * 5 + j] + 'A') + " ");
                    else
                        Console.Out.Write((char)(matrix[i * 5 + j] + 'B') + " "); 
                }
                Console.Out.WriteLine("");
            }
        }

        public static char[,] ConvertToChar(int[] matrix)
        {
            char[,] matrixchar = new char[5, 5];            
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (matrix[i * 5 + j] + 'A' < 'J')
                        matrixchar[i, j] = (char)(matrix[i * 5 + j] + 'A');
                    else
                        matrixchar[i, j] = (char)(matrix[i * 5 + j] + 'B');                    
                }                
            }
            return matrixchar;
        }

        public static int[] ConvertToInt(char[,] matrix)
        {
            int[] matrixint = new int[25];            
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (matrix[i, j] < 'J')
                        matrixint[i * 5 + j] = (int)(matrix[i, j] - 'A');
                    else
                        matrixint[i * 5 + j] = (int)(matrix[i, j] - 'B');
                }
            }
            return matrixint;
        }
        
    }


    class PlayFair
    {
        static double[,] BigraphStat = new double[25, 25];
        static double[,] CipherStat = new double[25, 25];   
        static int[, ,] DecodingTab = new int[600, 2, 2];
        static int DecodingTabLength;


        public PlayFair()
        {
        }

        public static string Encrypt(Matrix Key, string PlainText)
        {
            char Char1, Char2;
            int RowChar1, RowChar2, ColumnChar1, ColumnChar2;
            StringBuilder  CipherText = new StringBuilder(PlainText.Length / 2);

            for (int i = 0; i < PlainText.Length; i += 2)
            {
                Char1 = PlainText[i];
                Char2 = PlainText[i+1];

                int[] pos1 = Key.GetPosition(Char1);
                int[] pos2 = Key.GetPosition(Char2);
                
                RowChar1 = pos1[0];
                ColumnChar1 = pos1[1]; 
                RowChar2 = pos2[0];
                ColumnChar2 = pos2[1];

                if (RowChar1 == RowChar2)
                {
                    CipherText.Append(Key.Mat[RowChar1, (ColumnChar1 + 1) % 5]);
                    CipherText.Append(Key.Mat[RowChar2, (ColumnChar2 + 1) % 5]);
                }
                else if (ColumnChar1 == ColumnChar2)
                {
                    CipherText.Append(Key.Mat[(RowChar1 + 1) % 5, ColumnChar1]);
                    CipherText.Append(Key.Mat[(RowChar2 + 1) % 5, ColumnChar2]);
                }
                else
                {
                    CipherText.Append(Key.Mat[RowChar1, ColumnChar2]);
                    CipherText.Append(Key.Mat[RowChar2, ColumnChar1]);
                }

            }

            return CipherText.ToString();
        }

        public static string Decrypt(Matrix Key, string CipherText)
        {
            char Char1, Char2;
            int RowChar1, RowChar2, ColumnChar1, ColumnChar2;
            StringBuilder PlainText = new StringBuilder(CipherText.Length);

            for (int i = 0; i < CipherText.Length; i += 2)
            {
                Char1 = CipherText[i];
                Char2 = CipherText[i + 1];

                int[] pos1 = Key.GetPosition(Char1);
                int[] pos2 = Key.GetPosition(Char2);

                RowChar1 = pos1[0];
                ColumnChar1 = pos1[1];
                RowChar2 = pos2[0];
                ColumnChar2 = pos2[1];

                if (RowChar1 == RowChar2)
                {
                    PlainText.Append(Key.Mat[RowChar1, (ColumnChar1 + 4) % 5]);
                    PlainText.Append(Key.Mat[RowChar2, (ColumnChar2 + 4) % 5]);
                }
                else if (ColumnChar1 == ColumnChar2)
                {
                    PlainText.Append(Key.Mat[(RowChar1 + 4) % 5, ColumnChar1]);
                    PlainText.Append(Key.Mat[(RowChar2 + 4) % 5, ColumnChar2]);
                }
                else
                {
                    PlainText.Append(Key.Mat[RowChar1, ColumnChar2]);
                    PlainText.Append(Key.Mat[RowChar2, ColumnChar1]);
                }

            }

            return PlainText.ToString();
        }

        public static string Format(string UnformattedText)
        {
            string PreformattedText;
            StringBuilder FormattedText = new StringBuilder(UnformattedText.Length);
           
            
            PreformattedText = UnformattedText.ToUpper();

            for (int i = 0; i < PreformattedText.Length; i += 1)
            {
                if (PreformattedText[i] == 'J')
                    FormattedText.Append("I");
                else if (PreformattedText[i] > 64 && PreformattedText[i] < 91)
                {
                    FormattedText.Append(PreformattedText[i]);
                }
                else if (PreformattedText[i] == 'Ä')
                    FormattedText.Append("AE");
                else if (PreformattedText[i] == 'Ö')
                    FormattedText.Append("OE");
                else if (PreformattedText[i] == 'Ü')
                    FormattedText.Append("UE");
            }

            int Length = FormattedText.Length;
            for (int i = 0; i < Length - 1; i += 2)
            {
                if (FormattedText[i] == FormattedText[i + 1])
                {
                    FormattedText = FormattedText.Insert(i + 1, "X");
                    Length += 1;
                }
            }

            if ((FormattedText.Length % 2) == 1)
            {
                FormattedText.Append("X");
            }

            return FormattedText.ToString();
        }

        public static string Attack(string CipherText, Double[,] BS)
        {
            BigraphStat = BS;
            BigraphStatistic CS = new BigraphStatistic();
            CipherStat = CS.CalcLog(CipherText);
            int KeyHeapLength = 5000;
            int[] TestKey = new int[25];
            double Score2;
            int[] WorstKey = new int[25];
            int[] BestKey = new int[25];
            double WorstScore = 0;
            double BestScore;
            int Count = 0;


            CalcDecodingTab(5);


            //SortedDictionary<double, int[]> KeyHeap = new SortedDictionary<double, int[]>();            
            //SortedDictionary<double, int[]>.KeyCollection KeyColl = KeyHeap.Keys;
                        
            PriorityQueue<int[], double>[] KeyHeap = new PriorityQueue<int[], double>[2];
            KeyHeap[0] = new PriorityQueue<int[], double>();
            KeyHeap[1] = new PriorityQueue<int[], double>();


            Console.Write("Testing all partial keys of length 5 ");

            DateTime time1 = DateTime.Now;

            for (int i = 0; i < 25; i++)
            {
                Console.Write(".");
                TestKey[0] = i;
                for (int j = 0; j < 25; j++)                
                    if (j != i)
                    {
                        TestKey[1] = j;
                        for (int k = 0; k < 25; k++)
                            if (k != i && k != j)
                            {
                                TestKey[2] = k;
                                for (int l = 0; l < 25; l++)
                                    if (l != i && l != j && l != k)
                                    {
                                        TestKey[3] = l;
                                        for (int m = 0; m < 25; m++)
                                            if (m != i && m != j && m != k && m != l)
                                            {
                                                TestKey[4] = m;
                                                Score2 = EvaluateKey2(TestKey);
                                                if (Count > KeyHeapLength-1)
                                                {                                                    
                                                    if (Score2 < WorstScore)
                                                    {                                                        
                                                        try
                                                        {
                                                            //if (!KeyHeap.Contains(Score2))
                                                            {
                                                                KeyHeap[0].Dequeue();                                                               
                                                                KeyHeap[0].Enqueue((int[]) TestKey.Clone(), Score2);
                                                                WorstScore = KeyHeap[0].Peek().Priority;
                                                            }
                                                        }
                                                        catch (ArgumentException)
                                                        {
                                                            Console.Out.WriteLine("Wert bereits im Heap (> {0})", KeyHeapLength);
                                                        }                                                       
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        KeyHeap[0].Enqueue(TestKey, Score2);
                                                        WorstScore = Math.Max(WorstScore, Score2);
                                                        Count = KeyHeap[0].Count;
                                                    }
                                                    catch (ArgumentException)
                                                    {
                                                        Console.Out.WriteLine("Wert bereits im Heap (< {0})", KeyHeapLength);
                                                    }
                                                }                                                
                                            }
                                    }
                            }
                    }                
            }


            DateTime time2 = DateTime.Now;
            TimeSpan diff = time2 - time1;
            Console.Out.WriteLine("\n\ntime required: " + Convert.ToString(diff.TotalSeconds) + " seconds");            
                                          
                                    
            WorstKey = KeyHeap[0].Peek().Value;
            BestScore = double.MaxValue;
            foreach (PriorityQueueItem<int[], double> pqi in KeyHeap[0])
            {
                if (pqi.Priority < BestScore)
                {
                    BestScore = pqi.Priority;
                    BestKey = pqi.Value;
                }
            }
            
            Console.Out.WriteLine("\nBest Score: {0}", BestScore);
            Console.Out.WriteLine("Best Key:");
            Matrix.WriteOnConsoleInt(BestKey);


            Console.Out.WriteLine("\nWorst Score: {0}", WorstScore);
            Console.Out.WriteLine("Worst Key:");
            Matrix.WriteOnConsoleInt(WorstKey);

                                  
            Console.Out.WriteLine("\nAmount of keys in Heap: {0}", KeyHeap[0].Count);

            Console.Out.WriteLine("\nTesting next position of keys in heap...");

            time1 = DateTime.Now;
            
            for (int pos = 5; pos < 25; pos++)
            {
                KeyHeap[pos % 2].Clear();
                CalcDecodingTab(pos + 1);
                Count = 0;
                
                foreach (PriorityQueueItem<int[], double> pqi in KeyHeap[(pos + 1) % 2])
                {
                    bool[] letterinkey = new bool[25];
                    for (int i = 0; i < pos; i++)
                    {
                        letterinkey[pqi.Value[i]] = true;
                    }
                    for (int i = 0; i < 25; i++)
                    {
                        if (!letterinkey[i])
                        {
                            pqi.Value[pos] = i;
                            Score2 = EvaluateKey2(pqi.Value);
                            if (Count > KeyHeapLength-1)
                            {
                                if (Score2 < WorstScore)
                                {
                                    KeyHeap[pos % 2].Dequeue();
                                    KeyHeap[pos % 2].Enqueue((int[])pqi.Value.Clone(), Score2);
                                    WorstScore = KeyHeap[pos % 2].Peek().Priority;
                                }
                            }
                            else
                            {
                                KeyHeap[pos % 2].Enqueue(pqi.Value, Score2);
                                WorstScore = Math.Max(WorstScore, Score2);
                                Count = KeyHeap[pos % 2].Count;
                            }
                        }
                    }
                }
                Console.Out.WriteLine("Position {0} done.", pos);
            }


            time2 = DateTime.Now;
            diff = time2 - time1;
            Console.Out.WriteLine("\ntime required: " + Convert.ToString(diff.TotalSeconds) + " seconds");   


            BestScore = double.MaxValue;

            foreach (PriorityQueueItem<int[], double> pqi in KeyHeap[24 % 2])
            {
                if (pqi.Priority < BestScore)
                {
                    BestScore = pqi.Priority;
                    BestKey = (int[])pqi.Value.Clone();
                }
            }

            Console.Out.WriteLine("\nBest Score: {0}", BestScore);
            Console.Out.WriteLine("Best Key: ");
            Matrix.WriteOnConsoleInt(BestKey);

            int[] CorrectKey = new int[25];
            for (int i = 0; i < 25; i++)
                CorrectKey[i] = i;
            Console.Out.WriteLine("\nCorrect Key Score: {0}", EvaluateKey2(CorrectKey));
            Console.Out.WriteLine("Correct Key: ");
            Matrix.WriteOnConsoleInt(CorrectKey);


            Matrix BestKeyMatrix = new Matrix(Matrix.ConvertToChar(BestKey));
            
            return Decrypt(BestKeyMatrix, CipherText);
        }

        public static Double EvaluateKey(int[] Key)
        {
            double Mean = 0;
            int MeanLength = 0;
            double SumProbCipher = 0;
            int BigraphsInCipher = 0;
            double Score = 0;

            for (int i = 0; i < DecodingTabLength; i++)
            {
                if (BigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] > -9)
                {
                    Mean += BigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]];
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

        public static Double EvaluateKey2(int[] Key)
        {
            Double Score2 = 0;           

            for (int i = 0; i < DecodingTabLength; i++)
            {
                //if (BigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] > -11)
                Score2 += Math.Abs(BigraphStat[Key[DecodingTab[i, 1, 0]], Key[DecodingTab[i, 1, 1]]] - CipherStat[Key[DecodingTab[i, 0, 0]], Key[DecodingTab[i, 0, 1]]]);
            }                     

            return Score2;
        }

        public static int[, ,] CalcDecodingTab(int KeyLength)
        {            
            int index = 0;

            for (int i = 0; i < KeyLength - 1; i++)
            {
                for (int j = i + 1; j < KeyLength; j++)
                {
                    if ((i / 5) == (j / 5))     // i and j in same row
                    {
                        if ((i % 5) > 0)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i - 1;
                            DecodingTab[index, 1, 1] = j - 1;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index-1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index-1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index-1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index-1, 1, 0];
                            index++;
                        }
                        else if (i + 4 < KeyLength)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i + 4;
                            DecodingTab[index, 1, 1] = j - 1;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                    }

                    else if ((i % 5) == (j % 5))      // i and j in same column
                    {
                        if ((i / 5) > 0)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i - 5;
                            DecodingTab[index, 1, 1] = j - 5;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                        else if ((i + 20) < KeyLength)
                        {
                            DecodingTab[index, 0, 0] = i;
                            DecodingTab[index, 0, 1] = j;
                            DecodingTab[index, 1, 0] = i + 20;
                            DecodingTab[index, 1, 1] = j - 5;
                            index++;
                            DecodingTab[index, 0, 0] = DecodingTab[index - 1, 0, 1];
                            DecodingTab[index, 0, 1] = DecodingTab[index - 1, 0, 0];
                            DecodingTab[index, 1, 0] = DecodingTab[index - 1, 1, 1];
                            DecodingTab[index, 1, 1] = DecodingTab[index - 1, 1, 0];
                            index++;
                        }
                    }
                    else if ((i / 5) * 5 + (j % 5) < KeyLength && (j / 5) * 5 + (i % 5) < KeyLength)   // i and j in a square
                    {   
                        DecodingTab[index, 0, 0] = i;
                        DecodingTab[index, 0, 1] = j;
                        DecodingTab[index, 1, 0] = (i / 5) * 5 + (j % 5);
                        DecodingTab[index, 1, 1] = (j / 5) * 5 + (i % 5);
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

    }

}
