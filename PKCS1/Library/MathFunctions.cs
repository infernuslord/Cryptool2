using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;

using Emil.GMP;
using PKCS1.BigNum;

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

            return compareByteArray(ref array1, ref array2, length);
        }

        static public bool compareByteArray(ref byte[] array1, ref byte[] array2, int length)
        {
            for (int i = length - 1; i > 0; i--)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// Computes an approximate cube root of a number,
        /// by using the Newton approximation for next guess.
        /// </summary>
        /// <param name="x">The number to compute the cube root from.</param>
        /// <returns></returns>
        static public BigInteger cuberoot3(BigInteger BigIntRad)
        {
            BigNumDec y;               // Guess
            BigNumDec d;               // Last difference of y3 and x
            BigNumDec l;               // The limit for optimal guess

            // Check for simple cases:
            if (BigIntRad.Equals(BigInteger.Zero))
                return BigInteger.Zero;
            else if (BigIntRad.Equals(BigInteger.One))
                return BigInteger.One;
            else if (BigIntRad.Equals(BigInteger.ValueOf(-1)))
                return BigInteger.ValueOf(-1);
            else
            {
                BigNumDec x = new BigNumDec(BigIntRad.ToString());

                //l = Math.Abs(x * 1E-14);                // Set the limit appropriately
                BigNumDec E = new BigNumDec("0.00000000000000000000000000000000000000000000000000000000000000000001");
                l = BigNumDec.Multiply(x, E);
                
                // the multiplication with x (its magnitude) should
                // ensure no infinite loops, at the cost
                // of some precision on high numbers.

                // Make initial guess:                
                //double g = Math.Abs(x);     // Do this guess on a positive number
                BigNumDec g = x;
                //if (g < BigInteger.One)
                //if (g.CompareTo(new BigNumDec(1)) < 1)
                if( g < new BigNumDec(1))
                    y = x;
                //else if (g < 10)
                //else if (g.CompareTo(new BigNumDec(10)) == -1)
                else if( g < 10)
                    //y = x / 3;
                    y = BigNumDec.Divide(x,new BigNumDec(3));
                else if (g < 20)
                    //y = x / 6;
                    y = BigNumDec.Divide(x,new BigNumDec(6));
                else if (g < 50)
                    //y = x / 10;
                    y = BigNumDec.Divide(x,new BigNumDec(10));
                else if (g < 100)
                    //y = x / 20;
                    y = BigNumDec.Divide(x,new BigNumDec(20));
                else if (g < 1000)
                    //y = x / 50;
                    y = BigNumDec.Divide(x,new BigNumDec(50));
                else if (g < 5000)
                    //y = x / 100;
                    y = BigNumDec.Divide(x, new BigNumDec(100));
                //else if (g < 10000)
                //else if (g.CompareTo(new BigNumDec(10000)) == -1)
                else if( g < 10000)
                    //y = x / 500;
                    y = BigNumDec.Divide(x,new BigNumDec(500));
                else if (g < 50000)
                    //y = x / 1000;
                    y = BigNumDec.Divide(x, new BigNumDec(1000));
                else if (g < 100000)
                    //y = x / 50000;
                    y = BigNumDec.Divide(x, new BigNumDec(50000));
                else
                    //y = x / 100000;
                    y = BigNumDec.Divide(x,new BigNumDec(100000));

                // Improve guess immediately:
                //y = ((x / (y * y)) + 2 * y) / 3;            // Newton's approx. for new guess
                y = BigNumDec.Divide( BigNumDec.Add( BigNumDec.Divide(x, BigNumDec.Multiply(y,y)) , BigNumDec.Multiply(new BigNumDec(2),y) ), new BigNumDec(3));
                //d = Math.Abs(y * y * y - x);                // Calculate difference
                d = BigNumDec.Multiply( BigNumDec.Multiply(y,y), y);
                d = BigNumDec.Subtract( d, x);
                d.Absolute();
                #region
                while (l < d)
                {
                    //y = ((x / (y * y)) + 2 * y) / 3;        // Newton's approx. for new guess
                    y = BigNumDec.Divide(BigNumDec.Add(BigNumDec.Divide(x, BigNumDec.Multiply(y, y)), BigNumDec.Multiply(new BigNumDec(2), y)), new BigNumDec(3));
                    //d = Math.Abs(y * y * y - x);                // Calculate difference
                    d = BigNumDec.Multiply(BigNumDec.Multiply(y, y), y);
                    d = BigNumDec.Subtract(d, x);
                    d.Absolute();
                }
                #endregion

                string test = y.ToString();

                return BigInteger.Three;
            }
        }

        static public BigInteger cuberoot4(BigInteger BigIntRad, int prec)
        {
            //BigInteger x;                    // ZZ: Langzahl-Integer
            myFloat a = new myFloat();
            myFloat xi = new myFloat();
            myFloat x3 = new myFloat();
            myFloat two = new myFloat(); // RR: Gleitkommazahlen beliebiger Präzision
            myFloat three = new myFloat(); // RR: Gleitkommazahlen beliebiger Präzision

            myFloat.setPrec(prec);

            //x = BigIntRad;

            BigInteger BigInt2 = BigInteger.Two;
            myFloat.to_Float(ref two, ref BigInt2);

            BigInteger BigInt3 = BigInteger.Three;
            myFloat.to_Float(ref three, ref BigInt3);

            // 1. Startwert für die Approximation berechnen (mit double)
            //appr_cr_x = exp( 1.0/3.0 * log(x) );  
            

            // 2. Startwert (xi) und Ausgangswert (a=x) in Gleitkommazahl mit hoher Präzision überführen
            //a  = to_RR(x);
            myFloat.to_Float(ref a, ref BigIntRad);

            myFloat tmp = new myFloat();
            BigInteger tmp2 = BigInteger.ValueOf(BigIntRad.BitLength);
            myFloat.to_Float(ref tmp,ref tmp2);
            //xi = to_RR(appr_cr_x);
            //xi = new myFloat(appr_cr_x);
            //myFloat.div(ref xi, ref a,ref tmp);
            BigInteger start = BigIntRad.ShiftRight(BigIntRad.BitLength * 2 / 3);
            myFloat.to_Float(ref xi, ref start);


            // 3. Halley's konvergierende Folge (x[i+1] = xi*(xi^3 + 2*a)/(2*xi^3 + a) --> x^(1/3)) mit 200 Iterationen -- *nicht optimiert*
            //two = to_RR(2.0);
            //two = new myFloat(2.0);

            for ( int i = 0; i<200; i++ )
            {
                //x3 = xi*xi*xi;
                myFloat.mul(ref x3, ref xi, ref xi);
                myFloat.mul(ref x3, ref x3, ref xi);
                //xi = (xi*(x3 + two * a)) / ( two * x3 + a );
                
                //xi = xi*( (x3 + two * a) / ( two * x3 +a ) );
                myFloat twoA = new myFloat();
                myFloat.mul(ref twoA, ref two, ref a);

                myFloat left = new myFloat();
                myFloat.add(ref left, ref x3, ref twoA);
                

                myFloat twoX3 = new myFloat();
                myFloat.mul(ref twoX3, ref two, ref x3);

                myFloat right = new myFloat();
                myFloat.add(ref right, ref twoX3, ref a);

                myFloat division = new myFloat();
                myFloat.div(ref division, ref left, ref right);

                myFloat.mul(ref xi, ref xi, ref division);
            }

            return myFloat.to_ZZ(ref xi);
        }
    }
}