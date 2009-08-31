using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.CubeAttack
{
    public class Vector
    {
        // Class attributes
        private int length;
        private int[] element;

        // Constructor
        public Vector(int length)
        {
            this.length = length; 
            element = new int[length];
        }

        // Properties
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        public int this[int i]
        {
            get { return GetElement(i); }
            set { SetElement(i, value); }
        }

        public int GetElement(int i)
        {
            if (i < 0 || i > Length - 1)
                throw new MatrixVectorException("Invalid index specified");
            return element[i];
        }

        public void SetElement(int i, int value)
        {
            if (i < 0 || i > Length - 1)
                throw new MatrixVectorException("Invalid index specified");
            element[i] = value;
        }

        // Returns the product of a matrix and a vector
        public static Vector Multiply(Matrix matrix1, Vector vec)
        {
            if (matrix1.Cols != vec.Length)
                throw new MatrixVectorException("Operation not possible");
            Vector result = new Vector(vec.Length);
            for (int i = 0; i < result.Length; i++)
                for (int k = 0; k < matrix1.Cols; k++)
                    result[i] ^= matrix1[i, k] * vec[k];
            return result;
        }

        public static Vector operator *(Matrix matrix1, Vector vec)
        { return (Multiply(matrix1, vec)); }
    }
}
