using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sigaba
{
    class Rotor
    {
        public int Position { get; set; }

        public char[] Subalpha { get; set; }
        public char[] SubalphaRev { get; set; }
        public char[] SubalphaHu { get; set; }
        public char[] SubalphaRevHu { get; set; }

        public byte[,] RotSubMat { get; set; }
        public byte[,] RotSubMatRev { get; set; }
        public byte[,] RotSubMatBack { get; set; }
        public byte[,] RotSubMatRevBack { get; set; }


        public Boolean Reverse;

        public void IncrementPosition()
        {
            if (!Reverse)
            {
                Position = (RotSubMat.GetLength(0) + Position - 1) % RotSubMat.GetLength(0);
            }
            else
            {
                Position = (Position + 1) % RotSubMat.GetLength(0);
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
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatBack = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatRev = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatRevBack = new byte[subalpha.Count(), subalpha.Count()];

            for (int i = 0; i < subalpha.Count(); i++)
            {
                for (int j = 0; j < subalpha.Count(); j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % subalpha.Count()])) - i + subalpha.Count()) % subalpha.Count());
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % subalpha.Count())))) - i + subalpha.Count()) % subalpha.Count());
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + subalpha.Count()) % subalpha.Count()))))) + subalpha.Count()) % subalpha.Count());
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + subalpha.Count()) % subalpha.Count()]) + subalpha.Count()) % subalpha.Count());
                }
            }
        }

        public Rotor(char[] subalpha, int position, Boolean reverse)
        {
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatBack = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatRev = new byte[subalpha.Count(), subalpha.Count()];
            RotSubMatRevBack = new byte[subalpha.Count(), subalpha.Count()];

            for (int i = 0; i < subalpha.Count(); i++)
            {
                for (int j = 0; j < subalpha.Count(); j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % subalpha.Count()] - 65)) - i + subalpha.Count()) % subalpha.Count());
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % subalpha.Count()) + 65))) - i + subalpha.Count()) % subalpha.Count());
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + subalpha.Count()) % subalpha.Count()) + 65)))) + subalpha.Count()) % subalpha.Count());
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + subalpha.Count()) % subalpha.Count()] - 65) + subalpha.Count()) % subalpha.Count());
                }
            }
        }
    }
}
