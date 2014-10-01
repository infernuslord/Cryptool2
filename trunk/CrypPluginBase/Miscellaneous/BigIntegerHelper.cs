/*
   Copyright 2010 Sven Rech (svenrech at cryptool dot org), University of Duisburg-Essen

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

/*
 * This class provides some additional functionality for the BigInteger class.
 * The parser stuff is written by Sven Rech and Nils Kopal.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;

namespace Cryptool.PluginBase.Miscellaneous
{
    public static class BigIntegerHelper
    {
        #region internal stuff of expression parser

        private struct TOKEN
        {
            public enum Ttype { MULTIPLY, DIVIDE, PLUS, MINUS, POW, BRACKETOPEN, BRACKETCLOSE, INTEGER };
            public Ttype ttype;
            public BigInteger integer;
        }

        private static Stack<TOKEN> Scan(string expr)
        {
            TOKEN t = new TOKEN();
            int startIndex = 0;
            if (expr == "")
                return new Stack<TOKEN>();
            switch (expr[0])
            {
                case ' ':
                    return Scan(expr.Substring(1));
                case '(':
                    t.ttype = TOKEN.Ttype.BRACKETOPEN;
                    startIndex = 1;
                    break;
                case ')':
                    t.ttype = TOKEN.Ttype.BRACKETCLOSE;
                    startIndex = 1;
                    break;
                case '+':
                    t.ttype = TOKEN.Ttype.PLUS;
                    startIndex = 1;
                    break;
                case '-':
                    t.ttype = TOKEN.Ttype.MINUS;
                    startIndex = 1;
                    break;
                case '*':
                    t.ttype = TOKEN.Ttype.MULTIPLY;
                    startIndex = 1;
                    break;
                case '/':
                    t.ttype = TOKEN.Ttype.DIVIDE;
                    startIndex = 1;
                    break;
                case '^':
                    t.ttype = TOKEN.Ttype.POW;
                    startIndex = 1;
                    break;
                case 'h':
                case 'H':
                case '#':
                case 'X':
                case 'x':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'E':
                case 'e':
                case 'F':
                case 'f':
                    if (expr[0] == '#' || expr[0] == 'H' || expr[0] == 'h' || expr[0] == 'X' || expr[0] == 'x')
                    {
                        int length = 1;
                        for (; length < expr.Length; length++)
                            if (!(expr[length] >= '0' && expr[length] <= '9') &&
                                expr[length] != 'A' && expr[length] != 'a' && 
                                expr[length] != 'B' && expr[length] != 'b' && 
                                expr[length] != 'C' && expr[length] != 'c' && 
                                expr[length] != 'D' && expr[length] != 'd' && 
                                expr[length] != 'E' && expr[length] != 'e' && 
                                expr[length] != 'F' && expr[length] != 'f')
                                break;
                        t.integer =  BigInteger.Parse(expr.Substring(1, length-1), NumberStyles.AllowHexSpecifier);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = length;
                    }
                    else
                    {                        
                        int length = 1;
                        for (; length < expr.Length; length++)
                            if (!(expr[length] >= '0' && expr[length] <= '9'))
                                break;
                        t.integer = BigInteger.Parse(expr.Substring(0, length));
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = length;
                    }
                    break;
                default:
                    throw new Exception("Expression parsing failed at character " + expr[0]);
            }
            Stack<TOKEN> st = Scan(expr.Substring(startIndex));
            st.Push(t);
            return st;
        }

        private enum Priority { ALL, POW, MULTDIV, ADDSUB };

        private static BigInteger Parse(Stack<TOKEN> stack, Priority priority, bool endbracket)
        {
            if (stack.Count == 0)
                throw new Exception("Expression Parsing Error.");
            int minus = 1;
            BigInteger v = 0;
            TOKEN t = stack.Pop();  //get -, +, integer or bracket

            if (t.ttype == TOKEN.Ttype.MINUS)
            {
                minus = -1;
                t = stack.Pop();    //get integer or bracket
            }
            else if (t.ttype == TOKEN.Ttype.PLUS)
            {
                minus = 1;
                t = stack.Pop();    //get integer or bracket
            }

            if (t.ttype == TOKEN.Ttype.INTEGER)
            {
                v = minus * t.integer;
            }
            else if (t.ttype == TOKEN.Ttype.BRACKETOPEN)
            {
                v = minus * Parse(stack, Priority.ALL, true);
                stack.Pop();    //pop the closing bracket
            }

            while (stack.Count != 0)
            {
                switch (stack.Peek().ttype) //next operator
                {
                    case TOKEN.Ttype.PLUS:
                        if (priority == Priority.MULTDIV || priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v + Parse(stack, Priority.ADDSUB, endbracket);
                        break;
                    case TOKEN.Ttype.MINUS:
                        if (priority == Priority.MULTDIV || priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v - Parse(stack, Priority.ADDSUB, endbracket);
                        break;
                    case TOKEN.Ttype.MULTIPLY:
                        if (priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v * Parse(stack, Priority.MULTDIV, endbracket);
                        break;
                    case TOKEN.Ttype.DIVIDE:
                        if (priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v / Parse(stack, Priority.MULTDIV, endbracket);
                        break;
                    case TOKEN.Ttype.POW:
                        stack.Pop();
                        v = BigIntegerHelper.Pow(v, Parse(stack, Priority.POW, endbracket));
                        break;
                    case TOKEN.Ttype.BRACKETCLOSE:
                        if (endbracket)
                            return v;
                        else
                            throw new Exception("Expression Parsing Error (closing bracket misplaced).");
                    default:
                        throw new Exception("Expression Parsing Error.");
                }
            }
            if (endbracket)
                throw new Exception("Expression Parsing Error (closing bracket missing).");

            return v;
        }

        #endregion

        /*         
         * Parses a math expression (example: (2+2)^(17-5) ) 
         * and returns a BigInteger based on this expression
         * 
         * throws an exception when expression is not valid or the Number gets too big
         */
        public static BigInteger ParseExpression(string expr)
        {
            Stack<TOKEN> stack = Scan(expr);
            BigInteger i = Parse(stack, Priority.ALL, false);
            return i;
        }

        public static BigInteger Pow(BigInteger b, BigInteger e)
        {
            if (b < 0) return (e % 2 == 0) ? Pow(-b, e) : -Pow(-b, e);
            if (e < 0) return (b > 1) ? 0 : (BigInteger)1 / Pow(b, -e);
            if (b <= 1) return b;

            return BigInteger.Pow(b, (int)e);
        }
        
        /// <summary>
        /// Returns the modulo inverse of input.
        /// Throws ArithmeticException if the inverse does not exist (i.e. gcd(this, modulus) != 1) or the modulus is smaller than 2.
        /// </summary>
        public static BigInteger ModInverse(BigInteger input, BigInteger modulus)
        {
            if (modulus < 2)
                throw (new ArithmeticException(String.Format("Modulus must be >= 2, is {0}.", modulus)));
            
            BigInteger x, y;
            BigInteger g = ExtEuclid(((input % modulus) + modulus) % modulus, modulus, out x, out y);

            if (g != 1)
                throw (new ArithmeticException(String.Format("{0} has no inverse modulo {1}.", input, modulus)));

            return ((x % modulus) + modulus) % modulus;
        }

        /// <summary>
        /// Extended Euclidean Algorithm.
        /// Returns the GCD of a and b and finds integers x and y that satisfy x*a + y*b = gcd(a,b)
        /// </summary>
        public static BigInteger ExtEuclid(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            BigInteger xx, t, q, r;

            BigInteger aa = BigInteger.Abs(a);
            BigInteger bb = BigInteger.Abs(b);
            xx = 0; x = 1;

            while (bb > 0)
            {
                q = BigInteger.DivRem(aa, bb, out r);
                aa = bb; bb = r;
                t = x - xx * q; x = xx; xx = t;
            }

            x *= a.Sign;
            y = (b == 0) ? 0 : (aa - x * a) / b;

            return aa;
        }

        /// <summary>
        /// Greatest Common Divisor
        /// Returns the GCD of a and b
        /// </summary>
        public static BigInteger GCD(this BigInteger a, BigInteger b)
        {
            return BigInteger.GreatestCommonDivisor(a, b);
        }

        /// <summary>
        /// Least Common Multiple
        /// Returns the LCM of a and b
        /// </summary>
        public static BigInteger LCM(this BigInteger a, BigInteger b)
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(a, b);
            return (gcd != 0) ? ((a * b) / gcd) : 0;
        }

        public static BigInteger SetBit(BigInteger b, int i)
        {
            if( i>=0 ) b |= (((BigInteger)1) << i);
            return b;
        }

        /// <summary>
        /// Returns a random prime with 'bits' bits and the MSB set.
        /// You need this if you want to create primes with a given bitlength, just
        /// calling RandomPrimeBits would not guarantee the bitlength of the prime.
        /// </summary>
        public static BigInteger RandomPrimeMSBSet(int bits)
        {
            if (bits <= 1) throw new ArithmeticException("No primes with this bitcount");

            BigInteger limit = ((BigInteger)1) << bits;

            while (true)
            {
                var p = NextProbablePrime(SetBit(RandomIntBits(bits - 1), bits - 1));
                if (p < limit) return p;
            }
        }

        /// <summary>
        /// Returns a random integer with 'bits' bits and the MSB set.
        /// </summary>
        public static BigInteger RandomIntMSBSet(int bits)
        {
            return SetBit( RandomIntBits(bits - 1), bits - 1 );
        }

        /// <summary>
        /// Returns a random prime less than limit
        /// </summary>
        public static BigInteger RandomPrimeLimit(this BigInteger limit)
        {
            if (limit <= 2) throw new ArithmeticException("No primes below this limit");

            while( true ) {
                var p = NextProbablePrime(RandomIntLimit(limit));
                if( p < limit ) return p;
            }
        }

        /// <summary>
        /// Returns a random prime less than 2^bits
        /// </summary>
        public static BigInteger RandomPrimeBits(int bits)
        {
            if (bits < 0) throw new ArithmeticException("Enter a positive bitcount");
            return RandomPrimeLimit( ((BigInteger)1) << bits );
        }

        /// <summary>
        /// Returns a random integer less than limit
        /// </summary>
        public static BigInteger RandomIntLimit(this BigInteger limit)
        {
            if (limit <= 0) throw new ArithmeticException("Enter a positive limit");

            byte[] buffer = limit.ToByteArray();
            int n = buffer.Length;
            byte msb = buffer[n - 1];
            int mask = 0;

            while (mask < msb)
                mask = (mask << 1) + 1;

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            while (true)
            {
                rng.GetBytes(buffer);
                buffer[n - 1] &= (byte)mask;
                var p = new BigInteger(buffer);
                if (p < limit) return p;
            }
        }

        /// <summary>
        /// Returns a random integer less than 2^bits
        /// </summary>
        public static BigInteger RandomIntBits(int bits)
        {
            if (bits < 0) throw new ArithmeticException("Enter a positive bitcount");
            return RandomIntLimit( ((BigInteger)1) << bits );
        }

        #region primesBelow2000
        // primes smaller than 2000 to test the generated prime number (taken from BigInteger class written by Chew Keong TAN)

        private static readonly int[] primesBelow2000 = {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
            101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,
	        211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293,
	        307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,
	        401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499,
	        503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599,
	        601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691,
	        701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797,
	        809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887,
	        907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,
	        1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097,
	        1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193,
	        1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297,
	        1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399,
	        1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499,
	        1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597,
	        1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699,
	        1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789,
	        1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,
	        1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999 };

        #endregion

        public static BigInteger NextProbablePrime( this BigInteger n )
        {
            if (n < 0) throw new ArithmeticException("NextProbablePrime cannot be called on value < 0");
            if (n <= 2) return 2;
            if (n.IsEven) n++;
            if (n == 3) return 3;
            BigInteger r = n % 6;
            if (r == 3) n += 2;
            if (r == 1) { if (IsProbablePrime(n)) return n; else n += 4; }
            
            // at this point n mod 6 = 5

            while (true)
            {
                if (IsProbablePrime(n)) return n;
                n += 2;
                if (IsProbablePrime(n)) return n;
                n += 4;
            }

        }

        public static BigInteger PreviousProbablePrime(this BigInteger n)
        {
            if (n < 2) throw new ArithmeticException("PreviousProbablePrime cannot be called on value < 2");
            if (n == 2) return 2;
            if (n.IsEven) n--;
            if (n == 3) return 3;
            BigInteger r = n % 6;
            if (r == 3) n -= 2;
            if (r == 5) { if (IsProbablePrime(n)) return n; else n -= 4; }

            // at this point n mod 6 = 1

            while (true)
            {
                if (IsProbablePrime(n)) return n;
                n -= 2;
                if (IsProbablePrime(n)) return n;
                n -= 4;
            }

        }

        /*
         * This code is heavily inspired by the code from the BigInteger class written by Chew Keong TAN
         */
        public static bool IsProbablePrime(this BigInteger thisVal)
        {
            thisVal = BigInteger.Abs(thisVal);

            //test small numbers
            if (thisVal == 0 || thisVal == 1)
                return false;
            if (thisVal == 2 || thisVal == 3)
                return true;            

            if (thisVal.IsEven)     // even numbers
                return false;

            // test for divisibility by primes < 2000
            for (int p = 0; p < primesBelow2000.Length; p++)
            {
                BigInteger divisor = primesBelow2000[p];

                if (divisor >= thisVal)
                    break;

                BigInteger resultNum = thisVal % divisor;
                if (resultNum == 0)                
                    return false;                
            }

            // Perform BASE 2 Rabin-Miller Test

            // calculate values of s and t
            int s = 0;

            BigInteger t = thisVal - 1;
            while ((t & 0x01) == 0)     //TODO: This could be implemented more efficient
            {
                t = t >> 1;
                s++;
            }

            BigInteger a = 2;

            // b = a^t mod p
            BigInteger b = BigInteger.ModPow(a, t, thisVal);
            bool result = false;

            if (b == 1)         // a^t mod p = 1
                result = true;

            BigInteger p_sub1 = thisVal - 1;
            for (int j = 0; result == false && j < s; j++)
            {
                if (b == p_sub1)         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                {
                    result = true;
                    break;
                }

                b = (b * b) % thisVal;
            }

            /*  TODO: Implement this:
            // if number is strong pseudoprime to base 2, then do a strong lucas test
            if (result)
                result = LucasStrongTestHelper(thisVal);
            */

            return result;
        }


        /// <summary>
        /// Because it is often necessary to convert a reversed byte array to a positive BigInteger, without having a the highest significant bit 
        /// set to zero for indicating the positiveness, this method can be used.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static BigInteger FromPositiveReversedByteArray(byte[] p)
        {
            byte[] b = new byte[p.Length+1];      //b has one more byte than p
            for (int i = 0; i < p.Length; i++)
                b[i] = p[p.Length-i-1];
            return new BigInteger(b);
        }

        /// <summary>
        /// Returns the number of bits that are needed to represent the integer argument.
        /// </summary>
        public static int BitCount(this BigInteger b)
        {
            if (b < 0) b = -b;

            byte[] buffer = b.ToByteArray();

            // ignore leading zero bytes
            int i = buffer.Length - 1;
            while( i > 0 && buffer[i] == 0 ) i--;
            
            // ignore leading zero bits
            byte mask = 0x80;
            int j = 8;
            while( j>0 && (buffer[i] & mask) == 0 )
            {
                j--;
                mask >>= 1;
            }

            return 8 * i + j;
        }

        public static BigInteger Factorial(this BigInteger n)
        {
            if (n < 0) throw new ArithmeticException("The factorial of a negative number is not defined");

            BigInteger result = 1;
            BigInteger counter = n;

            while (counter > 1)
            {
                result *= counter;
                counter--;
            }

            return result;
        }

        /// <summary>
        /// Calculates the number of unordered subsets with r objects of a set with n objects.
        /// </summary>
        public static BigInteger nCr(this BigInteger n, BigInteger r)
        {
            if ( n < r || r < 0 ) return 0;

            BigInteger result = 1;
            BigInteger ri = 1;
            BigInteger ni = n;

            while (ri <= r)
            {
                result *= ni;
                result /= ri;
                ni--;
                ri++;
            }

            return result;
        }

        /// <summary>
        /// Calculates the number of ordered subsets with r objects of a set with n objects.
        /// </summary>
        public static BigInteger nPr(this BigInteger n, BigInteger r)
        {
            if (n < r || r < 0) return 0;

            BigInteger result = 1;
            BigInteger ri = 1;
            BigInteger ni = n;

            while (ri <= r)
            {
                result *= ni;
                ni--;
                ri++;
            }

            return result;
        }

        /// <summary>
        /// Returns the minimum of a set of BigIntegers
        /// </summary>
        public static BigInteger Min(BigInteger n, params BigInteger[] values)
        {
            BigInteger min = n;

            for (int i = 0; i < values.Length; i++)
                if (min > values[i]) min = values[i];

            return min;
        }

        /// <summary>
        /// Returns the maximum of a set of BigIntegers
        /// </summary>
        public static BigInteger Max(BigInteger n, params BigInteger[] values)
        {
            BigInteger max = n;

            for (int i = 0; i < values.Length; i++)
                if (max < values[i]) max = values[i];

            return max;
        }

        /// <summary>
        /// Calculates the square root of a BigInteger.
        /// If the argument is not a perfect square, this function returns the floor of the square root,
        /// i.e. the biggest number that is smaller than the actual square root of the argument.
        /// </summary>
        
        // Compute the square root of n using Heron's method (which is Newton's method applied to x^2-n)

        public static BigInteger Sqrt(this BigInteger n)
        {
            if( n<0 )
                throw (new ArithmeticException("Square root of negative number does not exist!"));

            BigInteger x = n >> (n.BitCount() / 2);     // select starting value
            BigInteger lastx;

            while (true)
            {
                lastx = x;
                x = (n / x + x) >> 1;
                int i = x.CompareTo(lastx);
                if (i == 0) return x;
                if (i < 0)
                {
                    if (lastx - x == 1 && (x * x < n) && (lastx * lastx) > n) return x;
                }
                else
                {
                    if (x - lastx == 1 && (lastx * lastx) < n && (x * x) > n) return lastx;
                }
            }
        }

        public static Dictionary<BigInteger, long> Factorize(this BigInteger n)
        {
            bool isFactorized;
            return n.Factorize(n, out isFactorized);
        }

        public static Dictionary<BigInteger, long> Factorize(this BigInteger n, BigInteger limit, out bool isFactorized)
        {
            Dictionary<BigInteger, long> factors = new Dictionary<BigInteger, long>();
            BigInteger value = (n < 0) ? -n : n;

            isFactorized = false;

            if (value == 1)
            {
                isFactorized = true;
                return factors;
            }

            if (value.IsProbablePrime())
            {
                factors[value] = 1;
                isFactorized = true;
                return factors;
            }

            for (BigInteger factor = 2; ; factor = (factor + 1).NextProbablePrime())
            {
                if (factor * factor > value)
                {
                    factors[value] = 1;
                    isFactorized = true;
                    break;
                }

                if (factor > limit)
                {
                    factors[value] = 1;
                    isFactorized = false;
                    break;
                }

                if (value % factor == 0)
                {
                    factors[factor] = 0;

                    do
                    {
                        value /= factor;
                        factors[factor]++;
                    }
                    while (value % factor == 0);

                    if (value == 1)
                    {
                        isFactorized = true;
                        break;
                    }

                    if (value.IsProbablePrime())
                    {
                        factors[value] = 1;
                        isFactorized = true;
                        break;
                    }
                }
            }

            return factors;
        }

        public static BigInteger Refactor(Dictionary<BigInteger, long> factors)
        {
            BigInteger result = 1;

            foreach (var s in factors.Keys)
                result *= BigInteger.Pow(s,(int)factors[s]);

            return result;
        }

        public static List<BigInteger> Divisors(this BigInteger n)
        {
            return Divisors(n.Factorize());
        }

        public static List<BigInteger> Divisors(Dictionary<BigInteger, long> factors)
        {
            Dictionary<BigInteger, long> f = new Dictionary<BigInteger, long>();
            List<BigInteger> keys = new List<BigInteger>();
            foreach (var key in factors.Keys)
            {
                keys.Add(key);
                f[key] = 0;
            }

            List<BigInteger> result = new List<BigInteger>();

            int i;
            do
            {
                result.Add(Refactor(f));
                for (i = keys.Count - 1; i >= 0; i--)
                {
                    f[keys[i]]++;
                    if (f[keys[i]] <= factors[keys[i]]) break;
                }
                for (int j = i + 1; j < keys.Count; j++) f[keys[j]] = 0;
            }
            while (i >= 0);

            return result;
        }

        public static BigInteger Phi(this BigInteger n)
        {
            if (n == 0) return 0;
            return Phi(n.Factorize());
        }

        public static BigInteger Phi(Dictionary<BigInteger, long> factors)
        {
            BigInteger phi = 1;

            foreach (var f in factors.Keys)
            {
                if(f>0)
                    if (factors[f] > 0)
                        phi *= (f-1) * BigInteger.Pow(f,(int)factors[f] - 1);
            }

            return phi;
        }
    }
}