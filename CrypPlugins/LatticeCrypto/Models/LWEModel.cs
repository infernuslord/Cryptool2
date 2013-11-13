using System;
using LatticeCrypto.Utilities;

namespace LatticeCrypto.Models
{
    public class LWEModel
    {
        public int n;
        public int m;
        public int q;
        public MatrixND s;
        public MatrixND A;
        public MatrixND b;
        public MatrixND e;
        public MatrixND r;
        public MatrixND u;
        public double alpha;
        public double std;

        public LWEModel ()
        {}

        public LWEModel (int n, int q, bool isSquare)
        {
            this.n = n;
            m = isSquare ? n : (int)Math.Round(1.1*n*Math.Log(q));
            this.q = q;
            Random random = new Random();
            
            s = new MatrixND(n, 1);
            for (int i = 0; i < n; i++)
                s[i, 0] = random.Next(q);
            
            A = new MatrixND(m,n);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    A[i, j] = random.Next(q);

            alpha = 1/(Math.Sqrt(n)*Math.Pow(Math.Log(n), 2));
            std = (alpha * q) / Math.Sqrt(2 * Math.PI);

            e = new MatrixND(m, 1);
            for (int i = 0; i < m; i++)
                e[i, 0] = (int)Math.Round(random.NextGaussian(0, std));

            b = (A*s + e) % q;
        }

        public void GenerateNewRandomVector()
        {
            Random random = new Random();
            r = new MatrixND(1, m);
            for (int i = 0; i < m; i++)
                r[0, i] = random.Next(2);
            u = r * A;
        }

        public EncryptLWETupel Encrypt(int bit)
        {
            double c = (r*b)[0, 0] + bit * Math.Floor((double)q/2);
            return new EncryptLWETupel(((int) c) % q, u % q);
        }

        public int Decrypt (EncryptLWETupel enc)
        {
            double result = (enc.c - (enc.u*s)[0,0]) % q;
            if (result < 0)
                result += q;

            double disZero = Math.Min(Math.Abs(q - result), result);
            double disOne = Math.Abs((Math.Floor((double) q/2) - result) % q);
            return disOne < disZero ? 1 : 0;
        }
    }

    public class EncryptLWETupel
    {
        public int c;
        public MatrixND u;

        public EncryptLWETupel(int c, MatrixND u)
        {
            this.c = c;
            this.u = u;
        }
    }
}
