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

namespace Cryptool.PluginBase.Miscellaneous
{
    public class BigIntegerHelper
    {
        #region internal stuff of expression parser

        private struct TOKEN
        {
            public enum Ttype { MULTIPLY, DIVIDE, PLUS, MINUS, POW, BRACKETOPEN, BRACKETCLOSE, INTEGER };
            public Ttype ttype;
            public BigInteger integer;
        }

        private static Stack<TOKEN> scan(string expr)
        {
            TOKEN t = new TOKEN();
            int startIndex = 0;
            if (expr == "")
                return new Stack<TOKEN>();
            switch (expr[0])
            {
                case ' ':
                    return scan(expr.Substring(1));
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
                    int length = 1;
                    for (; length < expr.Length; length++)
                        if (!(expr[length] >= '0' && expr[length] <= '9'))
                            break;
                    t.integer = BigInteger.Parse(expr.Substring(0, length));
                    t.ttype = TOKEN.Ttype.INTEGER;
                    startIndex = length;
                    break;
                default:
                    throw new Exception("Expression parsing failed at character " + expr[0]);
            }
            Stack<TOKEN> st = scan(expr.Substring(startIndex));
            st.Push(t);
            return st;
        }

        private enum Priority { ALL, POW, MULTDIV, ADDSUB };

        private static BigInteger parse(Stack<TOKEN> stack, Priority priority, bool endbracket)
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
                v = minus * parse(stack, Priority.ALL, true);
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
                        v = v + parse(stack, Priority.ADDSUB, endbracket);
                        break;
                    case TOKEN.Ttype.MINUS:
                        if (priority == Priority.MULTDIV || priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v - parse(stack, Priority.ADDSUB, endbracket);
                        break;
                    case TOKEN.Ttype.MULTIPLY:
                        if (priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v * parse(stack, Priority.MULTDIV, endbracket);
                        break;
                    case TOKEN.Ttype.DIVIDE:
                        if (priority == Priority.POW)
                            return v;
                        stack.Pop();
                        v = v / parse(stack, Priority.MULTDIV, endbracket);
                        break;
                    case TOKEN.Ttype.POW:
                        stack.Pop();
                        v = BigInteger.Pow(v, (int)parse(stack, Priority.POW, endbracket));
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
        public static BigInteger parseExpression(string expr)
        {
            Stack<TOKEN> stack = scan(expr);
            BigInteger i = parse(stack, Priority.ALL, false);
            return i;
        }

        /*
         * Returns the modulo inverse of "input".  Throws ArithmeticException if
         * the inverse does not exist.  (i.e. gcd(this, modulus) != 1)
         * 
         * This method is taken from the BigInteger class written by
         * Chew Keong TAN (source: http://www.codeproject.com/KB/cs/biginteger.aspx)
         * (but modified by us)
         */

        public static BigInteger ModInverse(BigInteger input, BigInteger modulus)
        {
            BigInteger[] p = { 0, 1 };
            BigInteger[] q = new BigInteger[2];    // quotients
            BigInteger[] r = { 0, 0 };             // remainders

            int step = 0;

            BigInteger a = modulus;
            BigInteger b = input;

            while (b != 0)
            {
                BigInteger quotient;
                BigInteger remainder;

                if (step > 1)
                {
                    BigInteger pval = (p[0] - (p[1] * q[0])) % modulus;
                    p[0] = p[1];
                    p[1] = pval;
                }

                quotient = BigInteger.DivRem(a, b, out remainder);

                q[0] = q[1];
                r[0] = r[1];
                q[1] = quotient; r[1] = remainder;

                a = b;
                b = remainder;

                step++;
            }

            if ((r[0] != 1 && r[0] != 0))
                throw (new ArithmeticException("No inverse!"));

            BigInteger result = ((p[0] - (p[1] * q[0])) % modulus);

            while (result < 0)
                result += modulus;  // get the least positive modulus

            return result;
        }

        #region primesBelow2000
        // primes smaller than 2000 to test the generated prime number (taken from BigInteger class written by Chew Keong TAN)

        public static readonly int[] primesBelow2000 = {
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

        /*
         * This code is heavily inspired by the code from the BigInteger class written by Chew Keong TAN
         */
        public static bool isProbablePrime(BigInteger thisVal)
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

    }
}
