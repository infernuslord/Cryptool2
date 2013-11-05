using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sigaba
{
   public class Rotor
    {
        public int Position { get; set; }

        public char[] Subalpha { get; set; }
        public char[] SubalphaRev { get; set; }
        public char[] SubalphaHu { get; set; }
        public char[] SubalphaRevHu { get; set; }

        public byte[,] RotSubMat { get; set; }
        private int _rotSubMatLength0 = -1;  
        
        public byte[,] RotSubMatRev { get; set; }
        public byte[,] RotSubMatBack { get; set; }
        public byte[,] RotSubMatRevBack { get; set; }
        

        public Boolean Reverse;

        public void IncrementPosition()
        {
            if (_rotSubMatLength0 == -1)
            {
                _rotSubMatLength0 = RotSubMat.GetLength(0);
            }

            if (!Reverse)
            {
                Position = (_rotSubMatLength0 + Position - 1) % _rotSubMatLength0;
            }
            else
            {
                Position = (Position + 1) % _rotSubMatLength0;
            }
        }

        public byte Ciph(int input)
        {
            if (!Reverse)
                return RotSubMat[Position, input];
            return RotSubMatRev[Position, input];
        }

        public byte DeCiph(int input)
        {
            if (!Reverse)
                return RotSubMatBack[Position, input];
            return RotSubMatRevBack[Position, input];
        }

        public Rotor(int[] subalpha, int position, Boolean reverse)
        {
            int subalphaCount = subalpha.Count();
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalphaCount, subalphaCount];
            RotSubMatBack = new byte[subalphaCount, subalphaCount];
            RotSubMatRev = new byte[subalphaCount, subalphaCount];
            RotSubMatRevBack = new byte[subalphaCount, subalphaCount];

            for (int i = 0; i < subalphaCount; i++)
            {
                for (int j = 0; j < subalphaCount; j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % subalphaCount])) - i + subalphaCount) % subalphaCount);
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % subalphaCount)))) - i + subalphaCount) % subalphaCount);
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + subalphaCount) % subalphaCount))))) + subalphaCount) % subalphaCount);
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + subalphaCount) % subalphaCount]) + subalphaCount) % subalphaCount);
                }
            }
        }

        public Rotor(char[] subalpha, int position, Boolean reverse)
        {
            int subalphaCount = subalpha.Count();
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalphaCount, subalphaCount];
            RotSubMatBack = new byte[subalphaCount, subalphaCount];
            RotSubMatRev = new byte[subalphaCount, subalphaCount];
            RotSubMatRevBack = new byte[subalphaCount, subalphaCount];

            for (int i = 0; i < subalphaCount; i++)
            {
                for (int j = 0; j < subalphaCount; j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % subalphaCount] - 65)) - i + subalphaCount) % subalphaCount);
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % subalphaCount) + 65))) - i + subalphaCount) % subalphaCount);
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + subalphaCount) % subalphaCount) + 65)))) + subalphaCount) % subalphaCount);
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + subalphaCount) % subalphaCount] - 65) + subalphaCount) % subalphaCount);
                }
            }
        }
    }
}
