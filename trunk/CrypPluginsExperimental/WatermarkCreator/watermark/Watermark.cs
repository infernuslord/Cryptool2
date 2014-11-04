using System;
using System.Text;

/*
 * Copyright 2012 by Christoph Gaffga licensed under the Apache License, Version 2.0 (the "License"); you may not use
 * this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language governing permissions and limitations under the
 * License.
 */

namespace net.watermark
{


	using Bits = net.util.Bits;

	using ReedSolomonException = com.google.zxing.common.reedsolomon.ReedSolomonException;

	/// <summary>
	/// Implementation of a watermarking also. See https://code.google.com/p/dct-watermark/
	/// 
	/// @author Christoph Gaffga
    /// @author Ported to C# by Nils Rehwald
	/// </summary>
	public class Watermark
	{

		/// <summary>
		/// Valid characters and their order in our 6-bit charset. </summary>
		public static readonly string VALID_CHARS = " abcdefghijklmnopqrstuvwxyz0123456789.-,:/()?!\"'#*+_%$&=<>[];@§\n";

		/// <summary>
		/// Just for debugging. It reads a file called <tt>lena.jpg</tt> and embeds a watermark. Writes it to
		/// <tt>lena2.jpg</tt>, reads it again, and extracts the watermark.
		/// </summary>

        
		public static void Main(string[] args)
		{
			debug = true;
			try
			{
				string message = "¡This is a TEST!";

				Watermark watermark = new Watermark(8, 20, 0.6);

				// read source image...
                System.Drawing.Bitmap image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile("lena.jpg"); 
                //BufferedImage image = ImageIO.read(new File("lena.jpg"));

				Console.WriteLine("Image width:  " + image.Width);
				Console.WriteLine("Image height: " + image.Height);
				Console.WriteLine("Message: " + message);
				Console.WriteLine("Max bits total:   " + watermark.maxBitsTotal);
				Console.WriteLine("Max bits message: " + watermark.maxBitsData);
				Console.WriteLine("Max text len:     " + watermark.maxTextLen);

				// embedding...
				watermark.embed(image, message);

				// save the new image as JPEG, and load it again...
                image.Save("lena2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile("lena2.jpg"); 
                //ImageIO.write(image, "jpeg", new File("lena2.jpg"));
				//image = ImageIO.read(new File("lena2.jpg"));

				// extraction...
				message = watermark.extractText(image);
				Console.WriteLine("Extracted Message: " + message);
			}

			catch (Exception e)
			{
				Console.WriteLine(e.GetType().Name);
			}
		}

		public static void writeRaw(string filename, int[][] data)
		{
			//FileOutputStream fos = new FileOutputStream(filename);
            System.IO.FileStream fos = new System.IO.FileStream(filename,System.IO.FileMode.OpenOrCreate);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream os = new java.io.BufferedOutputStream(fos, 1024);
            System.IO.BufferedStream os = new System.IO.BufferedStream(fos, 1024);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.DataOutputStream dos = new java.io.DataOutputStream(os);
            //Stream
            System.IO.Stream dos = new System.IO.MemoryStream();
			//DataOutputStream dos = new DataOutputStream(os);
			foreach (int[] element in data)
			{
				foreach (int element2 in element)
				{
					//dos.writeByte(element2);
                    dos.WriteByte((byte)element2); //TODO: Ob das so gut ist...
				}
			}
			dos.Close();
		}

		/// <summary>
		/// The width and height of our quantization box in pixels (n-times n pixel per bit). </summary>
		internal int bitBoxSize = 10;

		/// <summary>
		/// Number of bytes used for Reed-Solomon error correction. No error correction if zero. </summary>
		internal int byteLenErrorCorrection = 6;

		/// <summary>
		/// Number of bits that could be stored, total, including error correction bits. </summary>
		internal int maxBitsTotal;

		/// <summary>
		/// Number of bits for data (excluding error correction). </summary>
		internal int maxBitsData;

		/// <summary>
		/// Maximal length in of characters for text messages. </summary>
		internal int maxTextLen;

		/// <summary>
		/// Opacity of the marks when added to the image. </summary>
		internal double opacity = 1.0; // 1.0 is strongest watermark

		/// <summary>
		/// Seed for randomization of the watermark. </summary>
		private long randomizeWatermarkSeed = 19;

		/// <summary>
		/// Seed for randomization of the embedding. </summary>
		private long randomizeEmbeddingSeed = 24;

		/// <summary>
		/// Enable some debugging output. </summary>
		public static bool debug = false;

		public Watermark()
		{
			calculateSizes();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public Watermark(final int boxSize, final int errorCorrectionBytes, final double opacity)
		public Watermark(int boxSize, int errorCorrectionBytes, double opacity)
		{
			this.bitBoxSize = boxSize;
			this.byteLenErrorCorrection = errorCorrectionBytes;
			this.opacity = opacity;
			calculateSizes();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public Watermark(final int boxSize, final int errorCorrectionBytes, final double opacity, final long seed1, final long seed2)
		public Watermark(int boxSize, int errorCorrectionBytes, double opacity, long seed1, long seed2)
		{
			this.bitBoxSize = boxSize;
			this.byteLenErrorCorrection = errorCorrectionBytes;
			this.opacity = opacity;
			this.randomizeEmbeddingSeed = seed1;
			this.randomizeWatermarkSeed = seed2;
			calculateSizes();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public Watermark(final long seed1, final long seed2)
		public Watermark(long seed1, long seed2)
		{
			this.randomizeEmbeddingSeed = seed1;
			this.randomizeWatermarkSeed = seed2;
			calculateSizes();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private String bits2String(final net.util.Bits bits)
		private string bits2String(Bits bits)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder buf = new StringBuilder();
			StringBuilder buf = new StringBuilder();
			{
				for (int i = 0; i < this.maxTextLen; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int c = (int) bits.getValue(i * 6, 6);
					int c = (int) bits.getValue(i * 6, 6);
					buf.Append(VALID_CHARS[c]);
				}
			}
			return buf.ToString();
		}

		private void calculateSizes()
		{
			this.maxBitsTotal = 128 / this.bitBoxSize * (128 / this.bitBoxSize);
			this.maxBitsData = this.maxBitsTotal - this.byteLenErrorCorrection * 8;
			this.maxTextLen = this.maxBitsData / 6;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void embed(final java.awt.image.BufferedImage image, final net.util.Bits data)
		public virtual void embed(System.Drawing.Bitmap image, Bits data)
		{
			Bits bits;
			// make the size fit...
			if (data.size() > this.maxBitsData)
			{
				bits = new Bits(data.getBits(0, this.maxBitsData));
			}
			else
			{
				bits = new Bits(data);
				while (bits.size() < this.maxBitsData)
				{
					bits.addBit(false);
				}
			}

			// add error correction...
			if (this.byteLenErrorCorrection > 0)
			{
				bits = Bits.bitsReedSolomonEncode(bits, this.byteLenErrorCorrection);
			}

			// create watermark image...
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[][] watermarkBitmap = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] watermarkBitmap = new int[128][128];
			int[][] watermarkBitmap = RectangularArrays.ReturnRectangularIntArray(128, 128);
			for (int y = 0; y < 128 / this.bitBoxSize * this.bitBoxSize; y++)
			{
				for (int x = 0; x < 128 / this.bitBoxSize * this.bitBoxSize; x++)
				{
					if (bits.size() > x / this.bitBoxSize + y / this.bitBoxSize * (128 / this.bitBoxSize))
					{
						watermarkBitmap[y][x] = bits.getBit(x / this.bitBoxSize + y / this.bitBoxSize * (128 / this.bitBoxSize)) ? 255 : 0;
					}
				}
			}

			if (debug)
			{
				try
				{
					writeRaw("water1.raw", watermarkBitmap);
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (System.IO.IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			// embedding...
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[][] grey = embed(image, watermarkBitmap);
			int[][] grey = embed(image, watermarkBitmap);

			// added computed data to original image...
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.awt.Color color = new java.awt.Color(image.getRGB(x, y));
                    System.Drawing.Color color = image.GetPixel(x, y);
					//System.Drawing.Color color = new System.Drawing.Color(image.getRGB(x, y));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] hsb = java.awt.Color.RGBtoHSB(color.getRed(), color.getGreen(), color.getBlue(), null);
					float[] hsb = RGBtoHSB(color.R, color.G, color.B);
					// adjust brightness of the pixel...
					hsb[2] = (float)(hsb[2] * (1.0 - this.opacity) + grey[y][x] * this.opacity / 255.0);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.awt.Color colorNew = new java.awt.Color(java.awt.Color.HSBtoRGB(hsb[0], hsb[1], hsb[2]));
                    System.Drawing.Color colorNew = HSBtoRGB(hsb[0], hsb[1], hsb[2]);
					image.SetPixel(x, y, colorNew);

				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private int[][] embed(final java.awt.image.BufferedImage src, final int[][] water1)
		private int[][] embed(System.Drawing.Bitmap src, int[][] water1)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = (src.getWidth() + 7) / 8 * 8;
			int width = (src.Width + 7) / 8 * 8;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = (src.getHeight() + 7) / 8 * 8;
			int height = (src.Height + 7) / 8 * 8;

			// original image process
			const int N = 8;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buff1[][] = new int[height][width];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] buff1 = new int[height][width]; // Original image
			int[][] buff1 = RectangularArrays.ReturnRectangularIntArray(height, width); // Original image
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buff2[][] = new int[height][width];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] buff2 = new int[height][width]; // DCT Original image coefficients
			int[][] buff2 = RectangularArrays.ReturnRectangularIntArray(height, width); // DCT Original image coefficients
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buff3[][] = new int[height][width];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] buff3 = new int[height][width]; // IDCT Original image coefficients
			int[][] buff3 = RectangularArrays.ReturnRectangularIntArray(height, width); // IDCT Original image coefficients

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b1[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b1 = new int[N][N]; // DCT input
			int[][] b1 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT input
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b2[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b2 = new int[N][N]; // DCT output
			int[][] b2 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT output

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b3[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b3 = new int[N][N]; // IDCT input
			int[][] b3 = RectangularArrays.ReturnRectangularIntArray(N, N); // IDCT input
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b4[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b4 = new int[N][N]; // IDCT output
			int[][] b4 = RectangularArrays.ReturnRectangularIntArray(N, N); // IDCT output

			// watermark image process
			const int W = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int water2[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] water2 = new int[128][128]; // random watermark image
			int[][] water2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // random watermark image
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int water3[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] water3 = new int[128][128]; // DCT watermark image coefficients
			int[][] water3 = RectangularArrays.ReturnRectangularIntArray(128, 128); // DCT watermark image coefficients

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w1[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w1 = new int[W][W]; // DCT input
			int[][] w1 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT input
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w2[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w2 = new int[W][W]; // DCT output
			int[][] w2 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT output
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w3[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w3 = new int[W][W]; // quantization output
			int[][] w3 = RectangularArrays.ReturnRectangularIntArray(W, W); // quantization output
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int mfbuff1[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] mfbuff1 = new int[128][128]; // embed coefficients
			int[][] mfbuff1 = RectangularArrays.ReturnRectangularIntArray(128, 128); // embed coefficients
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int mfbuff2[] = new int[width * height];
			int[] mfbuff2 = new int[width * height]; // 2 to 1

			// random process...
			int a, b, c;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tmp[] = new int[128 * 128];
			int[] tmp = new int[128 * 128];

			// random embed...
			int c1;
			int cc = 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tmp1[] = new int[128 * 128];
			int[] tmp1 = new int[128 * 128];

			// divide 8x8 block...
			int k = 0, l = 0;

			// init buf1 from src image...
			for (int y = 0; y < src.Height; y++)
			{
				for (int x = 0; x < src.Width; x++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.awt.Color color = new java.awt.Color(src.getRGB(x, y));
                    System.Drawing.Color color = new System.Drawing.Color();
                    color = src.GetPixel(x, y);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] hsb = java.awt.Color.RGBtoHSB(color.getRed(), color.getGreen(), color.getBlue(), null);
					float[] hsb = RGBtoHSB(color.R, color.G, color.B);
					// use brightness of the pixel...
					buff1[y][x] = (int)(hsb[2] * 255.0);
				}
			}

			// 將 Original image 切成 8*8 的 block 作 DCT 轉換
			// System.out.println("Original image         ---> FDCT");
			for (int y = 0; y < height; y += N)
			{
				for (int x = 0; x < width; x += N)
				{
					for (int i = y; i < y + N; i++)
					{
						for (int j = x; j < x + N; j++)
						{
							b1[k][l] = buff1[i][j];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DCT o1 = new DCT();
					DCT o1 = new DCT(); // 宣告 DCT 物件
					o1.ForwardDCT(b1, b2); // 引用 DCT class 中,ForwardDCT的方法

					for (int p = y; p < y + N; p++)
					{
						for (int q = x; q < x + N; q++)
						{
							buff2[p][q] = b2[k][l];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;

				}
			}
			// System.out.println("                       OK!      ");

			// watermark image 作 random 處理
			// System.out.println("Watermark image        ---> Random");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Random r = new java.util.Random(this.randomizeWatermarkSeed);
			Random r = new Random((int)this.randomizeWatermarkSeed); // 設定亂數產生器的seed
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					while (true)
					{
						c = r.Next(128 * 128);
						if (tmp[c] == 0)
						{
							break;
						}
					}
					a = c / 128;
					b = c % 128;
					water2[i][j] = water1[a][b];
					tmp[c] = 1;
				}
			}
			// System.out.println("                       OK!      ");

			// 將 watermark image 切成 4x4 的 block 作 DCT 轉換與quantization
			k = 0;
			l = 0;
			// System.out.println("Watermark image        ---> FDCT & Quantization");
			for (int y = 0; y < 128; y += W)
			{
				for (int x = 0; x < 128; x += W)
				{
					for (int i = y; i < y + W; i++)
					{
						for (int j = x; j < x + W; j++)
						{
							w1[k][l] = water2[i][j];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;

					// 宣告 DCT2 物件
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DCT wm1 = new DCT(4);
					DCT wm1 = new DCT(4);

					// 引用DCT2 class 中,ForwardDCT的方法
					wm1.ForwardDCT(w1, w2);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Qt qw1 = new Qt();
					Qt qw1 = new Qt(); // 宣告 Qt 物件
					qw1.WaterQt(w2, w3); // 引用Qt class 中,WaterQt的方法

					for (int p = y; p < y + W; p++)
					{
						for (int q = x; q < x + W; q++)
						{
							water3[p][q] = w3[k][l];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;
				}
			}
			// System.out.println("                       OK!      ");

			// Embedding Watermark water3[128][128] -->buff2[512][512]
			// System.out.println("Watermarked image      ---> Embedding");

			// Random Embedding
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Random r1 = new java.util.Random(this.randomizeEmbeddingSeed);
			Random r1 = new Random((int)this.randomizeEmbeddingSeed); // 設定亂數產生器的seed
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					while (true)
					{
						c1 = r1.Next(128 * 128);
						if (tmp1[c1] == 0)
						{
							break;
						}
					}
					a = c1 / 128;
					b = c1 % 128;
					mfbuff1[i][j] = water3[a][b];
					tmp1[c1] = 1;
				}
			}

			// 二維 轉 一維
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ZigZag scan = new ZigZag();
			ZigZag scan = new ZigZag();
			scan.two2one(mfbuff1, mfbuff2);

			// WriteBack coefficients
			for (int i = 0; i < height; i += N)
			{
				for (int j = 0; j < width; j += N)
				{
					buff2[i + 1][j + 4] = mfbuff2[cc];
					cc++;
					buff2[i + 2][j + 3] = mfbuff2[cc];
					cc++;
					buff2[i + 3][j + 2] = mfbuff2[cc];
					cc++;
					buff2[i + 4][j + 1] = mfbuff2[cc];
					cc++;
				}
			}
			cc = 0;
			// System.out.println("                       OK!      ");

			// 將 Watermarked image 切成 8*8 的 block 作 IDCT 轉換
			// System.out.println("Watermarked image      ---> IDCT");
			k = 0;
			l = 0;
			for (int y = 0; y < height; y += N)
			{
				for (int x = 0; x < width; x += N)
				{
					for (int i = y; i < y + N; i++)
					{
						for (int j = x; j < x + N; j++)
						{
							b3[k][l] = buff2[i][j];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DCT o2 = new DCT();
					DCT o2 = new DCT(); // 宣告 DCT 物件
					o2.InverseDCT(b3, b4); // 引用DCT class 中,InverseDCT的方法

					for (int p = y; p < y + N; p++)
					{
						for (int q = x; q < x + N; q++)
						{
							buff3[p][q] = b4[k][l];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;
				}
			}
			// System.out.println("                       OK!      ");

			return buff3;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void embed(final java.awt.image.BufferedImage image, final String data)
		public virtual void embed(System.Drawing.Bitmap image, string data)
		{
			embed(image, string2Bits(data));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public net.util.Bits extractData(final java.awt.image.BufferedImage image) throws com.google.zxing.common.reedsolomon.ReedSolomonException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual Bits extractData(System.Drawing.Bitmap image)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[][] extracted = extractRaw(image);
			int[][] extracted = extractRaw(image);

			if (debug)
			{
				try
				{
					writeRaw("water2.raw", extracted);
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (System.IO.IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			// black/white the extracted result...
			for (int y = 0; y < 128 / this.bitBoxSize * this.bitBoxSize; y += this.bitBoxSize)
			{
				for (int x = 0; x < 128 / this.bitBoxSize * this.bitBoxSize; x += this.bitBoxSize)
				{
					int sum = 0;
					for (int y2 = y; y2 < y + this.bitBoxSize; y2++)
					{
						for (int x2 = x; x2 < x + this.bitBoxSize; x2++)
						{
							sum += extracted[y2][x2];
						}
					}
					sum = sum / (this.bitBoxSize * this.bitBoxSize);
					for (int y2 = y; y2 < y + this.bitBoxSize; y2++)
					{
						for (int x2 = x; x2 < x + this.bitBoxSize; x2++)
						{
							extracted[y2][x2] = sum > 127 ? 255 : 0;
						}
					}
				}
			}

			if (debug)
			{
				try
				{
					writeRaw("water3.raw", extracted);
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (System.IO.IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			// reconstruct bits...
			Bits bits = new Bits();
			for (int y = 0; y < 128 / this.bitBoxSize * this.bitBoxSize; y += this.bitBoxSize)
			{
				for (int x = 0; x < 128 / this.bitBoxSize * this.bitBoxSize; x += this.bitBoxSize)
				{
					bits.addBit(extracted[y][x] > 127);
				}
			}
			bits = new Bits(bits.getBits(0, this.maxBitsTotal));

			// if debugging, copy original before error correction...
			Bits bitsBeforeCorrection = null;
			if (debug)
			{
				bitsBeforeCorrection = new Bits(bits.getBits(0, this.maxBitsData));
			}

			// apply error correction...
			if (this.byteLenErrorCorrection > 0)
			{
				bits = Bits.bitsReedSolomonDecode(bits, this.byteLenErrorCorrection);
			}

			if (debug) // count errors (faulty bits)...
			{
				int errors = 0;
				for (int i = 0; i < bitsBeforeCorrection.size(); i++)
				{
					if (bitsBeforeCorrection.getBit(i) != bits.getBit(i))
					{
						errors++;
					}
				}
				Console.WriteLine("Error Correction:\n" + errors + " bits of " + bitsBeforeCorrection.size() + " are faulty");
			}

			return bits;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private int[][] extractRaw(final java.awt.image.BufferedImage src)
        private int[][] extractRaw(System.Drawing.Bitmap src)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = (src.getWidth() + 7) / 8 * 8;
			int width = (src.Width + 7) / 8 * 8;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = (src.getHeight() + 7) / 8 * 8;
			int height = (src.Height + 7) / 8 * 8;

			// original image
			const int N = 8;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buff1[][] = new int[height][width];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] buff1 = new int[height][width]; // watermarked image
			int[][] buff1 = RectangularArrays.ReturnRectangularIntArray(height, width); // watermarked image
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buff2[][] = new int[height][width];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] buff2 = new int[height][width]; // DCT watermarked image coefficients
			int[][] buff2 = RectangularArrays.ReturnRectangularIntArray(height, width); // DCT watermarked image coefficients
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b1[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b1 = new int[N][N]; // DCT input
			int[][] b1 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT input
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int b2[][] = new int[N][N];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] b2 = new int[N][N]; // DCT output
			int[][] b2 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT output

			// watermark
			const int W = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int water1[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] water1 = new int[128][128]; // extract watermark image
			int[][] water1 = RectangularArrays.ReturnRectangularIntArray(128, 128); // extract watermark image
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int water2[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] water2 = new int[128][128]; // DCT watermark image coefficients
			int[][] water2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // DCT watermark image coefficients
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int water3[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] water3 = new int[128][128]; // random watermark image
			int[][] water3 = RectangularArrays.ReturnRectangularIntArray(128, 128); // random watermark image

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w1[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w1 = new int[W][W]; // dequantization output
			int[][] w1 = RectangularArrays.ReturnRectangularIntArray(W, W); // dequantization output
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w2[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w2 = new int[W][W]; // DCT input
			int[][] w2 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT input
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int w3[][] = new int[W][W];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] w3 = new int[W][W]; // DCT output
			int[][] w3 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT output

			// random process
			int a, b, c, c1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tmp[] = new int[128 * 128];
			int[] tmp = new int[128 * 128];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tmp1[] = new int[128 * 128];
			int[] tmp1 = new int[128 * 128];
			int cc = 0;

			// middle frequency coefficients
			// final int mfbuff1[] = new int[128 * 128];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int mfbuff1[] = new int[width * height];
			int[] mfbuff1 = new int[width * height];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int mfbuff2[][] = new int[128][128];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] mfbuff2 = new int[128][128]; // 1 to 2
			int[][] mfbuff2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // 1 to 2

			// divide 8x8 block
			int k = 0, l = 0;

			// init buf1 from watermarked image src...
			for (int y = 0; y < src.Height; y++)
			{
				for (int x = 0; x < src.Width; x++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.awt.Color color = new java.awt.Color(src.getRGB(x, y));
					System.Drawing.Color color = new System.Drawing.Color();
                    color = src.GetPixel(x, y);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] hsb = java.awt.Color.RGBtoHSB(color.getRed(), color.getGreen(), color.getBlue(), null);
					float[] hsb = RGBtoHSB(color.R, color.G, color.B);
					// use brightness of the pixel...
					buff1[y][x] = (int)(hsb[2] * 255.0);
				}
			}

			// 將 watermark image 切成 8x8 的 block 作 DCT 轉換
			// System.out.println("Watermarked image         ---> FDCT");
			for (int y = 0; y < height; y += N)
			{
				for (int x = 0; x < width; x += N)
				{
					for (int i = y; i < y + N; i++)
					{
						for (int j = x; j < x + N; j++)
						{
							b1[k][l] = buff1[i][j];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DCT o1 = new DCT();
					DCT o1 = new DCT(); // 宣告 DCT 物件
					o1.ForwardDCT(b1, b2); // 引用DCT class 中,ForwardDCT的方法

					for (int p = y; p < y + N; p++)
					{
						for (int q = x; q < x + N; q++)
						{
							buff2[p][q] = b2[k][l];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;
				}
			}
			// System.out.println("                       OK!      ");

			// extract middle frequency coefficients...
			// System.out.println("watermark image       ---> Extracting");
			for (int i = 0; i < height; i += N)
			{
				for (int j = 0; j < width; j += N)
				{
					mfbuff1[cc] = buff2[i + 1][j + 4];
					cc++;
					mfbuff1[cc] = buff2[i + 2][j + 3];
					cc++;
					mfbuff1[cc] = buff2[i + 3][j + 2];
					cc++;
					mfbuff1[cc] = buff2[i + 4][j + 1];
					cc++;
				}
			}
			cc = 0;

			// 一維 轉 二維
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ZigZag scan = new ZigZag();
			ZigZag scan = new ZigZag(); // 宣告 zigZag 物件
			scan.one2two(mfbuff1, mfbuff2); // 引用zigZag class 中,one2two的方法

			// random extracting
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Random r1 = new java.util.Random(this.randomizeEmbeddingSeed);
			Random r1 = new Random((int)this.randomizeEmbeddingSeed);
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					while (true)
					{
						c1 = r1.Next(128 * 128);
						if (tmp1[c1] == 0)
						{
							break;
						}
					}
					a = c1 / 128;
					b = c1 % 128;
					water1[a][b] = mfbuff2[i][j];
					tmp1[c1] = 1;
				}
			}
			// System.out.println("                       OK!      ");

			k = 0;
			l = 0;
			// System.out.println("Watermark image       ---> Dequantization & IDCT");
			for (int y = 0; y < 128; y += W)
			{
				for (int x = 0; x < 128; x += W)
				{

					for (int i = y; i < y + W; i++)
					{
						for (int j = x; j < x + W; j++)
						{
							w1[k][l] = water1[i][j];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Qt qw2 = new Qt();
					Qt qw2 = new Qt(); // 宣告 Qt 物件
					qw2.WaterDeQt(w1, w2); // 引用Qt class 中,WaterDeQt的方法

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DCT wm2 = new DCT(4);
					DCT wm2 = new DCT(4); // 宣告 DCT2 物件
					wm2.InverseDCT(w2, w3); // 引用DCT2 class 中,InverseDCT的方法
					for (int p = y; p < y + W; p++)
					{
						for (int q = x; q < x + W; q++)
						{
							water2[p][q] = w3[k][l];
							l++;
						}
						l = 0;
						k++;
					}
					k = 0;
				}
			}
			// System.out.println("                       OK!      ");

			// System.out.println("Watermark image       ---> Re Random");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Random r = new java.util.Random(this.randomizeWatermarkSeed);
			Random r = new Random((int)this.randomizeWatermarkSeed); // 設定亂數產生器的seed
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					while (true)
					{
						c = r.Next(128 * 128);
						if (tmp[c] == 0)
						{
							break;
						}
					}
					a = c / 128;
					b = c % 128;
					water3[a][b] = water2[i][j];
					tmp[c] = 1;
				}
			}
			// System.out.println("                       OK!      ");

			return water3;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String extractText(final java.awt.image.BufferedImage image) throws com.google.zxing.common.reedsolomon.ReedSolomonException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual string extractText(System.Drawing.Bitmap image)
		{
			return bits2String(extractData(image)).Trim();
		}

		public virtual int BitBoxSize
		{
			get
			{
				return this.bitBoxSize;
			}
		}

		public virtual int ByteLenErrorCorrection
		{
			get
			{
				return this.byteLenErrorCorrection;
			}
		}

		public virtual int MaxBitsData
		{
			get
			{
				return this.maxBitsData;
			}
		}

		public virtual int MaxBitsTotal
		{
			get
			{
				return this.maxBitsTotal;
			}
		}

		public virtual int MaxTextLen
		{
			get
			{
				return this.maxTextLen;
			}
		}

		public virtual double Opacity
		{
			get
			{
				return this.opacity;
			}
		}

		public virtual long RandomizeEmbeddingSeed
		{
			get
			{
				return this.randomizeEmbeddingSeed;
			}
		}

		public virtual long RandomizeWatermarkSeed
		{
			get
			{
				return this.randomizeWatermarkSeed;
			}
		}

		private Bits string2Bits(string s)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final net.util.Bits bits = new net.util.Bits();
			Bits bits = new Bits();

			// remove invalid characters...
			s = s.ToLower();
			for (int i = 0; i < s.Length; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char c = s.charAt(i);
				char c = s[i];
				if (VALID_CHARS.IndexOf(c) < 0)
				{
					s = s.Substring(0, i) + s.Substring(i + 1);
					i--;
				}
			}

			// shorten if needed...
			if (s.Length > this.maxTextLen)
			{
				s = s.Substring(0, this.maxTextLen);
			}
			// padding if needed...
			while (s.Length < this.maxTextLen)
			{
				s += " ";
			}

			// create watermark bits...
			for (int j = 0; j < s.Length; j++)
			{
				bits.addValue(VALID_CHARS.IndexOf(s[j]), 6);
			}

			return bits;
		}

        public static System.Drawing.Color HSBtoRGB(float hue, float saturation, float brightness)
        {
            int r = 0, g = 0, b = 0;
            if (saturation == 0)
            {
                r = g = b = (int)(brightness * 255.0f + 0.5f);
            }
            else
            {
                float h = (hue - (float)Math.Floor(hue)) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = brightness * (1.0f - saturation);
                float q = brightness * (1.0f - saturation * f);
                float t = brightness * (1.0f - (saturation * (1.0f - f)));
                switch ((int)h)
                {
                    case 0:
                        r = (int)(brightness * 255.0f + 0.5f);
                        g = (int)(t * 255.0f + 0.5f);
                        b = (int)(p * 255.0f + 0.5f);
                        break;
                    case 1:
                        r = (int)(q * 255.0f + 0.5f);
                        g = (int)(brightness * 255.0f + 0.5f);
                        b = (int)(p * 255.0f + 0.5f);
                        break;
                    case 2:
                        r = (int)(p * 255.0f + 0.5f);
                        g = (int)(brightness * 255.0f + 0.5f);
                        b = (int)(t * 255.0f + 0.5f);
                        break;
                    case 3:
                        r = (int)(p * 255.0f + 0.5f);
                        g = (int)(q * 255.0f + 0.5f);
                        b = (int)(brightness * 255.0f + 0.5f);
                        break;
                    case 4:
                        r = (int)(t * 255.0f + 0.5f);
                        g = (int)(p * 255.0f + 0.5f);
                        b = (int)(brightness * 255.0f + 0.5f);
                        break;
                    case 5:
                        r = (int)(brightness * 255.0f + 0.5f);
                        g = (int)(p * 255.0f + 0.5f);
                        b = (int)(q * 255.0f + 0.5f);
                        break;
                }
            }
            return System.Drawing.Color.FromArgb(Convert.ToByte(255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }

        public static float[] RGBtoHSB(int red, int green, int blue)
        {
            float[] hsbarray = new float[3];

            float r = ((float)red / 255);
            float g = ((float)green / 255);
            float b = ((float)blue / 255);

            // Find strongest color
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            float h = 0;

            //Crazy calculations to find out hsb values from some sick formula. Source: http://www.codeproject.com/Articles/19045/Manipulating-colors-in-NET-Part
            if (max == r && g >= b)
            {
                h = 60 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == g)
            {
                h = 60 * (b - r) / (max - min) + 120;
            }
            else if (max == b)
            {
                h = 60 * (r - g) / (max - min) + 240;
            }
            else
            {
                Console.WriteLine("Some Wild error appeared while converting RGB to HSB. Fix it an get a Pokemon as reward");
            }

            float s = (max == 0) ? 0 : (1 - (min / max));

            hsbarray[0] = h;
            hsbarray[1] = s;
            hsbarray[2] = max;

            return hsbarray;
        }

	}

}