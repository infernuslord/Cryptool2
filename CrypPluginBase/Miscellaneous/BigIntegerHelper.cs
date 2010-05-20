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
         * Returns the modulo inverse of this.  Throws ArithmeticException if
         * the inverse does not exist.  (i.e. gcd(this, modulus) != 1)
         * 
         * This method is taken from the BigInteger class of
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

        
        public static bool isProbablePrime(BigInteger p)
        {
            return true;    //TODO: Implement this!!
            throw new NotImplementedException();
        }
    }
}
