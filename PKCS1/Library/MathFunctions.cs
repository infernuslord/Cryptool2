using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Emil.GMP;

namespace PKCS1.Library
{
    class MathFunctions
    {
        // nur zum Test; nutzt Emil.GMP lib
        static public BigInteger cuberoot2(BigInteger radicant)
        {
            BigInt test = new BigInt(radicant.ToString());
            BigInt returnBigInt = test.Root(3);
            return new BigInteger(returnBigInt.ToString());
        }

        // Heron Algorithmus
        static public BigInteger cuberoot(BigInteger radicant)
        {
            int i = 0;
            BigInteger biStart = BigInteger.ValueOf(1000);
            BigInteger biFix2 = BigInteger.Two;
            BigInteger biFix3 = BigInteger.Three;
            BigInteger biFromBevor = BigInteger.Zero;

            while (!biStart.Equals(biFromBevor))
            {
                biFromBevor = biStart;
                // (2 * biStart + (x/ biStart^2)) / 3
                biStart = biFix2.Multiply(biStart).Add(radicant.Divide(biStart.Pow(2))).Divide(biFix3);                
                i++;
            }
            return biStart; 
        }

        static public bool compareBigInt(BigInteger value1, BigInteger value2, int length)
        {
            byte[] array1 = value1.ToByteArray();
            byte[] array2 = value2.ToByteArray();

            return compareByteArray(array1, array2, length);
        }

        static public bool compareByteArray(byte[] array1, byte[] array2, int length)
        {
            for(int i = length-1; i>0; i--)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        
            /*
            for (int i = 0; i < length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
             */
        }
    }

    /*
     /// <summary>
     /// Computes an approximate cube root of a number,
     /// by using the Newton approximation for next guess.
     /// </summary>
     /// <param name="x">The number to compute the cube root from.</param>
     /// <returns></returns>
     public static BigInteger Cbrt(BigInteger x)
     {
         BigInteger y;               // Guess
         BigInteger d;               // Last difference of y3 and x
         BigInteger l;               // The limit for optimal guess

         // Check for simple cases:
         #region
         if (x.Equals( BigInteger.Zero))
             return BigInteger.Zero;
         else if (x.Equals(BigInteger.One))
             return BigInteger.One;
         else if (x.Equals(BigInteger.ValueOf(-1)))
             return BigInteger.ValueOf(-1);
         else
         #endregion
         {

             //l = Math.Abs(x * 1E-14);                // Set the limit appropriately
             BigInteger E = new BigInteger("0,00000000001");
             l = x.Multiply(E).Abs();
             // the multiplication with x (its magnitude) should
             // ensure no infinite loops, at the cost
             // of some precision on high numbers.

             // Make initial guess:
             #region
             //double g = Math.Abs(x);     // Do this guess on a positive number
             BigInteger g = x.Abs();
             if (g < BigInteger.One)
                 y = x;
             else if (g < 10)
                 y = x / 3;
             else if (g < 20)
                 y = x / 6;
             else if (g < 50)
                 y = x / 10;
             else if (g < 100)
                 y = x / 20;
             else if (g < 1000)
                 y = x / 50;
             else if (g < 5000)
                 y = x / 100;
             else if (g < 10000)
                 y = x / 500;
             else if (g < 50000)
                 y = x / 1000;
             else if (g < 100000)
                 y = x / 50000;
             else
                 y = x / 100000;
             #endregion

             // Improve guess immediately:
             y = ((x / (y * y)) + 2 * y) / 3;            // Newton's approx. for new guess
             d = Math.Abs(y * y * y - x);                // Calculate difference
             #region
             while (l < d)
             {
                 y = ((x / (y * y)) + 2 * y) / 3;        // Newton's approx. for new guess
                 d = Math.Abs(y * y * y - x);            // Calculate difference
             }
             #endregion
             return y;
         }
     }*/
}
