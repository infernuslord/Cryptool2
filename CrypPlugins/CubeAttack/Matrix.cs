using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.CubeAttack
{
    public class Matrix
    {
        // Class attributes
        private int rows, cols;
        private int[,] element;

        // Constructor
        public Matrix(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            element = new int[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    element[i, j] = 0;
        }

        // Properties
        public int Rows
        {
            get { return rows; }
        }

        public int Cols
        {
            get { return cols; }
        }

        public int this[int row, int col]
        {
            get { return GetElement(row, col); }
            set { SetElement(row, col, value); }
        }

        public int GetElement(int row, int col)
        {
            if (row < 0 || row > rows - 1 || col < 0 || col > Cols - 1)
                throw new MatrixVectorException("Invalid index specified");
            return element[row, col];
        }

        public void SetElement(int row, int col, int value)
        {
            if (row < 0 || row > Rows - 1 || col < 0 || col > Cols - 1)
                throw new MatrixVectorException("Invalid index specified");
            element[row, col] = value;
        }

        // Transpose the matrix
        public Matrix Transpose()
        {
            Matrix transposeMatrix = new Matrix(this.Cols, this.Rows);
            for (int i = 0; i < transposeMatrix.Rows; i++)
                for (int j = 0; j < transposeMatrix.Cols; j++)
                    transposeMatrix[i, j] = this[j, i];
            return transposeMatrix;
        }

        // Returns the minor of a Matrix element[Row,Col] 
        public static Matrix Minor(Matrix matrix, int row, int col)
        {
            Matrix minor = new Matrix(matrix.Rows - 1, matrix.Cols - 1);
            int m = 0, n = 0;
            for (int i = 0; i < matrix.Rows; i++)
            {
                if (i == row)
                    continue;
                n = 0;
                for (int j = 0; j < matrix.Cols; j++)
                {
                    if (j == col)
                        continue;
                    minor[m, n] = matrix[i, j];
                    n++;
                }
                m++;
            }
            return minor;
        }

        // Returns the determinent of the matrix
        public int Determinent()
        {
            return Determinent(this);
        }

        private int Determinent(Matrix matrix)
        {
            int det = 0;
            if (matrix.Rows == 1)
                return matrix[0, 0];
            for (int j = 0; j < matrix.Cols; j++)
                det += (matrix[0, j] * Determinent(Matrix.Minor(matrix, 0, j)) * (int)System.Math.Pow(-1, 0 + j));
            return det;
        }

        // Returns the adjoint of the matrix
        public Matrix Adjoint()
        {
            Matrix adjointMatrix = new Matrix(this.Rows, this.Cols);
            for (int i = 0; i < this.Rows; i++)
                for (int j = 0; j < this.Cols; j++)
                    adjointMatrix[i, j] = (int)Math.Pow(-1, i + j) * (Minor(this, i, j).Determinent());
            adjointMatrix = adjointMatrix.Transpose();
            return adjointMatrix;
        }

        // Returns the inverse of a square matrix by adjoint method
        public Matrix Inverse()
        {
            if (this.Determinent() == 0)
                throw new MatrixVectorException("Matrix is non-regular !");
            Matrix m = (this.Adjoint() / this.Determinent());
            for (int i = 0; i < this.Rows; i++)
                for (int j = 0; j < this.Cols; j++)
                    if (m[i, j] < 0)
                        m[i, j] = -1 * m[i, j];
            return m;
        }

        private static Matrix Multiply(Matrix matrix, int iNo)
        {
            Matrix result = new Matrix(matrix.Rows, matrix.Cols);
            for (int i = 0; i < matrix.Rows; i++)
                for (int j = 0; j < matrix.Cols; j++)
                    result[i, j] = matrix[i, j] * iNo;
            return result;
        }

        public Matrix AddRow(List<int> superpoly)
        {
            Matrix m = new Matrix(this.Rows + 1, this.Cols);
            for (int i = 0; i < this.Rows; i++)
                for (int j = 0; j < this.Cols; j++)
                    m[i, j] = this[i, j];
            for (int j = 0; j < this.Cols; j++)
                m[m.Rows - 1, j] = superpoly[j];
            return m;
        }

        public Matrix DeleteLastRow()
        {
            Matrix m = new Matrix(this.Rows - 1, this.Cols);
            for (int i = 0; i < this.Rows - 1; i++)
                for (int j = 0; j < this.Cols; j++)
                    m[i, j] = this[i, j];
            return m;
        }

        public Matrix DeleteFirstColumn()
        {
            Matrix m = new Matrix(this.Rows, this.Cols - 1);
            for (int i = 0; i < m.Rows; i++)
                for (int j = 1; j <= m.Cols; j++)
                    m[i, j - 1] = this[i, j];
            return m;
        }

        public static Matrix operator /(Matrix matrix, int iNo)
        { return Matrix.Multiply(matrix, iNo); }
    }

    public class MatrixVectorException : Exception
    {
        public MatrixVectorException()
            : base()
        { }

        public MatrixVectorException(string Message)
            : base(Message)
        { }

        public MatrixVectorException(string Message, Exception InnerException)
            : base(Message, InnerException)
        { }
    } 
}
