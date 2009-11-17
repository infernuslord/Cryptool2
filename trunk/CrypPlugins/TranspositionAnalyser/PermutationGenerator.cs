using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranspositionAnalyser
{
    class PermutationGenerator
    {
        private int[] a;
        private long numLeft;
        private long total;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">n < 20 </param>
        public PermutationGenerator(int n)
        {
            a = new int[n];
            total = getFactorial(n);
            reset();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void reset()
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = i + 1;
            }
            numLeft = total;
        }

        /// <summary>
        /// Return number of permutations not yet generated
        /// </summary>
        public long getNumLeft()
        {
            return numLeft;
        }

        /// <summary>
        /// Return total number of permutations
        /// </summary>
        public long getTotal()
        {
            return total;
        }

        /// <summary>
        /// Are there more permutations?
        /// </summary>
        public bool hasMore()
        {
            long nu = 0;
            return !(numLeft.Equals(nu));
        }

        public long getFactorial(int n)
        {
            long fact = 1;
            for (int i = n; i > 1; i--)
            {
                fact = fact * i;
            }
            return fact;
        }


        public int[] getNext()
        {

            if (numLeft.Equals(total))
            {
                numLeft = numLeft - 1;
                return a;
            }

            int temp;

            // Find largest index j with a[j] < a[j+1]

            int j = a.Length - 2;
            while (a[j] > a[j + 1])
            {
                j--;
            }

            // Find index k such that a[k] is smallest integer
            // greater than a[j] to the right of a[j]

            int k = a.Length - 1;
            while (a[j] > a[k])
            {
                k--;
            }

            // Interchange a[j] and a[k]

            temp = a[k];
            a[k] = a[j];
            a[j] = temp;

            // Put tail end of permutation after jth position in increasing order

            int r = a.Length - 1;
            int s = j + 1;

            while (r > s)
            {
                temp = a[s];
                a[s] = a[r];
                a[r] = temp;
                r--;
                s++;
            }

            numLeft = numLeft - 1;
            return a;

        }

    }
    
}
