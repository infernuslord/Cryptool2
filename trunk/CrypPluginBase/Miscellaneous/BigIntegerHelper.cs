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

        public static BigInteger ModInverse(BigInteger Input1, BigInteger Mod)
        {
            throw new NotImplementedException();
        }

        public static bool isProbablePrime(BigInteger p)
        {
            throw new NotImplementedException();
        }
    }
}
