using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.Plugins.PlayfairAnalysis;
using System.Diagnostics;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.PlayfairAnalysis
{
    class BigraphStatistic
    {
        int NumberOfFiles;
        string[] Path;
        string[] Text;
        int TotalLength;
        KeySearcher keySearcher;
        int matrixSize;

        public BigraphStatistic(int matrixSize)
        {
            keySearcher = new KeySearcher(matrixSize);
            this.matrixSize = matrixSize;
        }

        void LoadFiles(int matrixSize)
        {
            byte[] UnformattedTextByte;
            string UnformattedText;
            Text = new string[NumberOfFiles];

            for (int i = 0; i < NumberOfFiles; i++)
            {
                UnformattedTextByte = System.IO.File.ReadAllBytes(Path[i]);
                UnformattedText = Encoding.Default.GetString(UnformattedTextByte);
                Text[i] = keySearcher.Format(UnformattedText);
            }
        }

        public int[,] Generate(string[] Path)
        {
            this.Path = Path;
            NumberOfFiles = Path.Length;
            int[,] Stat = new int[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            int Length;
            int Pos1, Pos2;

            LoadFiles(matrixSize);

            switch (matrixSize)
            {
                case 5:
                    for (int i = 0; i < NumberOfFiles; i++)
                    {
                        Length = Text[i].Length;
                        TotalLength += Length;

                        for (int j = 0; j < Length - 1; j += 2)
                        {
                            if (Text[i][j] < 'J')
                                Pos1 = Text[i][j] - 'A';
                            else
                                Pos1 = Text[i][j] - 'B';

                            if (Text[i][j + 1] < 'J')
                                Pos2 = Text[i][j + 1] - 'A';
                            else
                                Pos2 = Text[i][j + 1] - 'B';

                            Stat[Pos1, Pos2] += 1;
                        }
                    }
                    break;

                case 6:
                    for (int i = 0; i < NumberOfFiles; i++)
                    {
                        Length = Text[i].Length;
                        TotalLength += Length;

                        for (int j = 0; j < Length - 1; j += 2)
                        {
                            if (Text[i][j] >= 'A' && Text[i][j] <= 'Z')
                                Pos1 = Text[i][j] - 'A';
                            else
                                Pos1 = Text[i][j] - '0' + 26;

                            if (Text[i][j + 1] >= 'A' && Text[i][j + 1] <= 'Z')
                                Pos2 = Text[i][j + 1] - 'A';
                            else
                                Pos2 = Text[i][j + 1] - '0' + 26;
                            
                            Stat[Pos1, Pos2] += 1;
                        }
                    }

                    break;
            }

            return Stat;
        }

        public int[,] Calc(string Text)
        {
            int[,] Stat = new int[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            int Pos1, Pos2;

            switch (matrixSize)
            {
                case 5:
                    for (int i = 0; i < Text.Length - 1; i += 2)
                    {
                        if (Text[i] < 'J')
                            Pos1 = Text[i] - 'A';
                        else
                            Pos1 = Text[i] - 'B';

                        if (Text[i + 1] < 'J')
                            Pos2 = Text[i + 1] - 'A';
                        else
                            Pos2 = Text[i + 1] - 'B';

                        Stat[Pos1, Pos2] += 1;
                    }
                    break;

                case 6:
                    for (int i = 0; i < Text.Length - 1; i += 2)
                    {
                        if (Text[i] >= 'A' && Text[i] <= 'Z')
                            Pos1 = Text[i] - 'A';
                        else
                            Pos1 = Text[i] - '0' + 26;

                        if (Text[i + 1] >= 'A' && Text[i + 1] <= 'Z')
                            Pos2 = Text[i + 1] - 'A';
                        else
                            Pos2 = Text[i + 1] - '0' + 26;

                        Stat[Pos1, Pos2] += 1;
                    }
                    break;

                default:
                    break;
            }

            return Stat;
        }


        public double[,] CalcLog(string Text)
        {
            double[,] Stat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];
            int Pos1, Pos2;
            int sum = 0;

            switch (matrixSize)
            {
                case 5:
                    for (int i = 0; i < Text.Length - 1; i += 2)
                    {
                        if (Text[i] < 'J')
                            Pos1 = Text[i] - 'A';
                        else
                            Pos1 = Text[i] - 'B';

                        if (Text[i + 1] < 'J')
                            Pos2 = Text[i + 1] - 'A';
                        else
                            Pos2 = Text[i + 1] - 'B';

                        Stat[Pos1, Pos2]++;
                        sum++;
                    }
                    break;

                case 6:
                    for (int i = 0; i < Text.Length - 1; i += 2)
                    {
                        if (Text[i] >= 'A' && Text[i] <= 'Z')
                            Pos1 = Text[i] - 'A';
                        else
                            Pos1 = Text[i] - '0' + 26;

                        if (Text[i + 1] >= 'A' && Text[i + 1] <= 'Z')
                            Pos2 = Text[i + 1] - 'A';
                        else
                            Pos2 = Text[i + 1] - '0' + 26;

                        Stat[Pos1, Pos2]++;
                        sum++;
                    }
                    break;

                default:
                    break;
            }

            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    if (Stat[i, j] > 0)
                    {
                        Stat[i, j] = Math.Log(Stat[i, j] / sum);
                    }
                    else
                        Stat[i, j] = -10;
                }
            }
            
            

            return Stat;
        }

        // Create a Bigraph Statistic and write in both txt and xml file
        internal static void CreateBS(string Path, int matrixSize)
        {
            string[] StatTextPath = { @"H:\Texte\Text1.txt", @"H:\Texte\Text2.txt", @"H:\Texte\Text3.txt", @"H:\Texte\Text4.txt",
                                      @"H:\Texte\Text5.txt", @"H:\Texte\Text6.txt", @"H:\Texte\Text7.txt", @"H:\Texte\Text8.txt",
                                      @"H:\Texte\Text9.txt", @"H:\Texte\Text10.txt", @"H:\Texte\Text11.txt", @"H:\Texte\Text12.txt",
                                      @"H:\Texte\Text13.txt", @"H:\Texte\Text14.txt", @"H:\Texte\Text15.txt", @"H:\Texte\Text16.txt",
                                      @"H:\Texte\Text17.txt", @"H:\Texte\Text18.txt", @"H:\Texte\Text19.txt", @"H:\Texte\Text20.txt"};
            

            /*
            string[] StatTextPath = { @"H:\Texte eng\Text1.txt", @"H:\Texte eng\Text2.txt", @"H:\Texte eng\Text3.txt", @"H:\Texte eng\Text4.txt",
                                      @"H:\Texte eng\Text5.txt", @"H:\Texte eng\Text6.txt", @"H:\Texte eng\Text7.txt", @"H:\Texte eng\Text8.txt",
                                      @"H:\Texte eng\Text9.txt", @"H:\Texte eng\Text10.txt", @"H:\Texte eng\Text11.txt", @"H:\Texte eng\Text12.txt"};
            */

            BigraphStatistic BS = new BigraphStatistic(matrixSize);
            int[,] BigraphStat = BS.Generate(StatTextPath);

            string[] Tab = new string[(int)Math.Pow(matrixSize, 2)];
            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    Tab[i] += Convert.ToString(BigraphStat[i, j]) + " ";
                    Tab[i] += "\t";
                }
            }

            System.IO.File.WriteAllLines(Path + "BSde.txt", Tab);

            int[][] BigraphStatDummy = new int[(int)Math.Pow(matrixSize, 2)][];

            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                BigraphStatDummy[i] = new int[(int)Math.Pow(matrixSize, 2)];
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    BigraphStatDummy[i][j] = BigraphStat[i, j];
                }
            }

            System.Xml.Serialization.XmlSerializer WriteBS = new System.Xml.Serialization.XmlSerializer(typeof(Int32[][]));
            System.Xml.XmlWriter XmlWriter = System.Xml.XmlWriter.Create(Path + "BSde.xml");
            WriteBS.Serialize(XmlWriter, BigraphStatDummy);
            XmlWriter.Close();

            // Compute log-probabilities
            int sum = 0;
            foreach (int value in BigraphStat)
                sum += value;


            double[][] BigraphStatLog = new double[(int)Math.Pow(matrixSize, 2)][];

            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                BigraphStatLog[i] = new double[(int)Math.Pow(matrixSize, 2)];
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    if (BigraphStat[i, j] > 0)
                    {
                        BigraphStatLog[i][j] = Math.Log((double)BigraphStat[i, j] / sum);
                    }
                    else
                        BigraphStatLog[i][j] = -10;
                }
            }

            string[] TabLog = new string[(int)Math.Pow(matrixSize, 2)];
            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    TabLog[i] += Convert.ToString(BigraphStatLog[i][j]) + "\t";
                }
            }

            System.Xml.Serialization.XmlSerializer WriteBSLog = new System.Xml.Serialization.XmlSerializer(typeof(Double[][]));
            System.Xml.XmlWriter XmlWriterLog;

            switch (matrixSize)
            {
                case 5:
                    System.IO.File.WriteAllLines(Path + "BSLog10sde.txt", TabLog);
                    XmlWriterLog = System.Xml.XmlWriter.Create(Path + "BSLog10sde.xml");
                    break;
                case 6:
                    System.IO.File.WriteAllLines(Path + "BSLog10lde.txt", TabLog);
                    XmlWriterLog = System.Xml.XmlWriter.Create(Path + "BSLog10lde.xml");
                    break;
                default:
                    XmlWriterLog = System.Xml.XmlWriter.Create(Path + "BSLog10.xml");
                    break;
            }            
            
            WriteBSLog.Serialize(XmlWriterLog, BigraphStatLog);
            XmlWriterLog.Close();

        }               

    }
}