using System;

namespace net.watermark
{

	// 程式名稱：Qt.java
	// 程式功能： Qt 類別,含WaterQt(量化)與WaterDeQt(反量化)兩種方法
    // Ported to C# by Nils Rehwald

	internal class Qt
	{
		internal static int N = 4;

		public double[][] qTable = new double[][]
		{
			new double[] {20, 30, 30, 35},
			new double[] {30, 30, 35, 45},
			new double[] {30, 35, 45, 50},
			new double[] {35, 45, 50, 60}
		};

		public double[][] filter = new double[][]
		{
			new double[] {0.2, 0.6, 0.6, 1},
			new double[] {0.6, 0.6, 1, 1.1},
			new double[] {0.6, 1, 1.1, 1.2},
			new double[] {1, 1.1, 1.2, 1.3}
		};

		internal Qt()
		{
		}

		/// <summary>
		/// Quantization </summary>
		internal virtual void WaterDeQt(int[][] input, int[][] output)
		{
			for (int i = 0; i < N; i++)
			{
				for (int j = 0; j < N; j++)
				{
					output[i][j] = (int)(input[i][j] * (this.qTable[i][j] * this.filter[i][j]));
				}
			}
		}

		/// <summary>
		/// De Quantization </summary>
		internal virtual void WaterQt(int[][] input, int[][] output)
		{
			for (int i = 0; i < N; i++)
			{
				for (int j = 0; j < N; j++)
				{
					output[i][j] = (int) Math.Round(input[i][j] / (this.qTable[i][j] * this.filter[i][j]));
				}
			}
		}

	}

}