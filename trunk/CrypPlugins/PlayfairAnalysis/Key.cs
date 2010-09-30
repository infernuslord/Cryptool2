using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.PlayfairAnalysis
{
    class Key
    {
        int matrixSize;
        public char[,] Mat;
        public static char[] AlphabetSmall = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K','L', 'M', 'N', 'O', 
                                  'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
        public static char[] AlphabetLarge = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K','L', 'M', 'N', 'O', 
                                  'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5',
                                  '6', '7', '8', '9'};

        public Key(int matrixSize)
        {
            this.matrixSize = matrixSize;
            Mat = new char[matrixSize, matrixSize];
        }

        public Key(char[,] matrix)
        {            
            this.matrixSize = (int)Math.Sqrt(matrix.Length);
            Mat = matrix;
        }        

        public void ReadMatrixConsole()
        {
            string s;

            for (int i = 0; i < matrixSize; i += 1)
            {
                s = Console.In.ReadLine();
                int pos = -1;

                switch(matrixSize)
                {
                    case 5:
                        for (int j = 0; j < matrixSize; j += 1)
                        {
                            pos = s.IndexOfAny(AlphabetSmall, pos + 1);
                            Mat[i,j] = s[pos];
                        }
                    break;
                    case 6:
                        for (int j = 0; j < matrixSize; j += 1)
                        {
                            pos = s.IndexOfAny(AlphabetLarge, pos + 1);
                            Mat[i,j] = s[pos];
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void ReadMatrixFile(string Path)
        {
            string[] s = System.IO.File.ReadAllLines(Path);

            for (int i = 0; i < matrixSize; i++)
                for (int j = 0; j < matrixSize; j++)
                    Mat[i, j] = s[i][j];
        }

        public int[] GetPosition(char Char)
        {
            for (int i = 0; i < matrixSize; i += 1)
            {
                for (int j = 0; j < matrixSize; j += 1)
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
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {                    
                    Console.Out.Write(Mat[i, j] + " ");
                }
                Console.Out.WriteLine("");
            }
        }

        public static void WriteOnConsoleInt(int[] matrix)
        {
            int matrixSize = (int)Math.Sqrt(matrix.Length);

            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    switch (matrixSize)
                    {
                        case 5:
                            if (matrix[i * 5 + j] + 'A' < 'J')
                                Console.Out.Write((char)(matrix[i * 5 + j] + 'A') + " ");
                            else
                                Console.Out.Write((char)(matrix[i * 5 + j] + 'B') + " ");
                            break;
                        case 6:
                            if ((matrix[i * 6 + j] + 'A' >= 'A') && (matrix[i * 6 + j] + 'A' <= 'Z'))
                                Console.Out.Write((char)(matrix[i * 6 + j] + 'A') + " ");
                            else
                                Console.Out.Write((char)(matrix[i * 6 + j] + '0' - 26) + " ");
                            break;
                        default:
                            if (matrix[i * 5 + j] + 'A' < 'J')
                                Console.Out.Write((char)(matrix[i * 5 + j] + 'A') + " ");
                            else
                                Console.Out.Write((char)(matrix[i * 5 + j] + 'B') + " ");
                            break;
                    }
                }
                Console.Out.WriteLine("");
            }
        }

        public static char[,] ConvertToChar(int[] matrix)
        {
            int matrixSize = (int)Math.Sqrt(matrix.Length);
            char[,] matrixchar = new char[matrixSize, matrixSize];  
          
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    switch (matrixSize)
                    {
                        case 5:
                            if (matrix[i * 5 + j] + 'A' < 'J')
                                matrixchar[i, j] = (char)(matrix[i * 5 + j] + 'A');
                            else
                                matrixchar[i, j] = (char)(matrix[i * 5 + j] + 'B');
                            break;

                        case 6:
                            if ((matrix[i * 6 + j] + 'A' >= 'A') && (matrix[i * 6 + j] + 'A' <= 'Z'))
                                matrixchar[i, j] = (char)(matrix[i * 6 + j] + 'A');
                            else
                                matrixchar[i, j] = (char)(matrix[i * 6 + j] + '0' - 26);
                            break;

                        default:
                            break;
                    }
                }                
            }
            return matrixchar;
        }

        public static int[] ConvertToInt(char[,] matrix)
        {
            int matrixSize = (int)Math.Sqrt(matrix.Length);
            int[] matrixint = new int[(int)Math.Pow(matrixSize, 2)];
            
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    switch (matrixSize)
                    {
                        case 5:
                            if (matrix[i, j] < 'J')
                                matrixint[i * 5 + j] = (int)(matrix[i, j] - 'A');
                            else
                                matrixint[i * 5 + j] = (int)(matrix[i, j] - 'B');
                            break;

                        case 6:
                            if ((matrix[i, j] + 'A' >= 'A') && (matrix[i, j] + 'A' <= 'Z'))
                                matrixint[i * 6 + j] = (int)(matrix[i, j] - 'A');
                            else
                                matrixint[i * 6 + j] = (int)(matrix[i, j] - '0' + 26);
                            break;
                        
                        default:
                            break;
                    }
                }
            }
            return matrixint;
        }

    }
}
