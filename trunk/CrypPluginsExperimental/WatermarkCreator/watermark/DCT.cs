using System;

namespace net.watermark
{

	// 程式名稱：DCT.java
	// 程式功能：DCT 類別,含ForwardDCT與InverseDCT兩種方法class DCT
	// 執行範例：java embed
    // Ported to C# by Nils Rehwald

	public class DCT
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			c = RectangularArrays.ReturnRectangularDoubleArray(this.n, this.n);
			ct = RectangularArrays.ReturnRectangularDoubleArray(this.n, this.n);
		}

		private readonly int n;

		public double[][] c;

		public double[][] ct;

		public DCT() : this(8)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		public DCT(int N)
		{
            this.n = N;
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}			
			int i;
			int j;
			double pi = Math.Atan(1.0) * 4.0;
			for (j = 0; j < N; j++)
			{
				this.c[0][j] = 1.0 / Math.Sqrt(N);
				this.ct[j][0] = this.c[0][j];
			}
			for (i = 1; i < N; i++)
			{
				for (j = 0; j < N; j++)
				{
					this.c[i][j] = Math.Sqrt(2.0 / N) * Math.Cos(pi * (2 * j + 1) * i / (2.0 * N));
					this.ct[j][i] = this.c[i][j];
				}
			}
		}

		internal virtual void ForwardDCT(int[][] input, int[][] output)
		{

			double[][] temp = RectangularArrays.ReturnRectangularDoubleArray(this.n, this.n);
			double temp1;
			int i, j, k;
			for (i = 0; i < this.n; i++)
			{
				for (j = 0; j < this.n; j++)
				{
					temp[i][j] = 0.0;
					for (k = 0; k < this.n; k++)
					{
						temp[i][j] += (input[i][k] - 128) * this.ct[k][j];
					}
				}
			}

			for (i = 0; i < this.n; i++)
			{
				for (j = 0; j < this.n; j++)
				{
					temp1 = 0.0;
					for (k = 0; k < this.n; k++)
					{
						temp1 += this.c[i][k] * temp[k][j];
					}
					output[i][j] = (int) Math.Round(temp1);
				}
			}
		}

		internal virtual void InverseDCT(int[][] input, int[][] output)
		{

			double[][] temp = RectangularArrays.ReturnRectangularDoubleArray(this.n, this.n);
			double temp1;
			int i, j, k;

			for (i = 0; i < this.n; i++)
			{
				for (j = 0; j < this.n; j++)
				{
					temp[i][j] = 0.0;
					for (k = 0; k < this.n; k++)
					{
						temp[i][j] += input[i][k] * this.c[k][j];
					}
				}
			}

			for (i = 0; i < this.n; i++)
			{
				for (j = 0; j < this.n; j++)
				{
					temp1 = 0.0;
					for (k = 0; k < this.n; k++)
					{
						temp1 += this.ct[i][k] * temp[k][j];
					}
					temp1 += 128.0;
					if (temp1 < 0)
					{
						output[i][j] = 0;
					}
					else if (temp1 > 255)
					{
						output[i][j] = 255;
					}
					else
					{
						output[i][j] = (int) Math.Round(temp1);
					}
				}
			}
		}
	}

}