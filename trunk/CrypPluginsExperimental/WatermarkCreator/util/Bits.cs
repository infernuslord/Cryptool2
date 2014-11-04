using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright 2012 by Christoph Gaffga licensed under the Apache License, Version 2.0 (the "License"); you may not use
 * this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language governing permissions and limitations under the
 * License.
 */

namespace net.util
{


	using GenericGF = com.google.zxing.common.reedsolomon.GenericGF;
	using ReedSolomonDecoder = com.google.zxing.common.reedsolomon.ReedSolomonDecoder;
	using ReedSolomonEncoder = com.google.zxing.common.reedsolomon.ReedSolomonEncoder;
	using ReedSolomonException = com.google.zxing.common.reedsolomon.ReedSolomonException;

	/// <summary>
	/// Some helper to work with an array of bits.
	/// 
	/// @author Christoph Gaffga
    /// @author Ported to C# by Nils Rehwald
	/// </summary>
	public class Bits
	{

		/// <summary>
		/// Unzip the bits </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Bits bitsGZIPDecode(final Bits bits) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public static Bits bitsGZIPDecode(Bits bits)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.ByteArrayInputStream byteIn = new java.io.ByteArrayInputStream(bits.getData());
            //ByteArrayInputStream byteIn = new ByteArrayInputStream(bits.Data);
            sbyte[] sdata = bits.Data; //TODO: Possible Error
            byte[] bdata = (byte[])(Array)sdata;
            System.IO.MemoryStream byteIn = new System.IO.MemoryStream(bdata);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.zip.GZIPInputStream zipIn = new java.util.zip.GZIPInputStream(byteIn);
            System.IO.Compression.GZipStream zipIn = new System.IO.Compression.GZipStream(byteIn, System.IO.Compression.CompressionMode.Compress);
			//GZIPInputStream zipIn = new GZIPInputStream(byteIn);
			int b;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Bits result = new Bits();
			Bits result = new Bits();
            //while ((b = zipIn.read()) >= 0)
			while ((b = zipIn.ReadByte()) >= 0)
			{
				result.addValue(b, 8);
			}
			return result;
		}

		/// <summary>
		/// Zip the bits </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static Bits bitsGZIPEncode(final Bits bits)
		public static Bits bitsGZIPEncode(Bits bits)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.ByteArrayOutputStream byteOut = new java.io.ByteArrayOutputStream();
                System.IO.MemoryStream byteOut = new System.IO.MemoryStream();
				//ByteArrayOutputStream byteOut = new ByteArrayOutputStream();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.zip.GZIPOutputStream zipOut = new java.util.zip.GZIPOutputStream(byteOut);
				//GZIPOutputStream zipOut = new GZIPOutputStream(byteOut);
                System.IO.Compression.GZipStream zipOut = new System.IO.Compression.GZipStream(byteOut, System.IO.Compression.CompressionMode.Decompress);
                byte[] tempBits = (byte[])(Array)bits.Data;
                zipOut.Write(tempBits,0,bits.Data.Length);
				zipOut.Close();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Bits result = new Bits();
				Bits result = new Bits();
                byte[] bdata = byteOut.ToArray();
                sbyte[] sdata = (sbyte[])(Array)bdata;
                result.addData(sdata);
				//result.addData(byteOut.toByteArray());
				return result;
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
			catch (System.IO.IOException e)
			{
                Console.WriteLine(e.GetType().Name);
				//throw new Exception(e);
                return null; //Bessere Idee? TODO
			}
		}

		/// <summary>
		/// Decode using Reed-Solomon error correction (with n bytes at the end of bits). </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Bits bitsReedSolomonDecode(final Bits bits, final int n) throws com.google.zxing.common.reedsolomon.ReedSolomonException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public static Bits bitsReedSolomonDecode(Bits bits, int n)
		{
			int[] data = (new Bits(bits.getBits(0, bits.size() - n * 8))).Bytes;
            Array.Copy(data, 0, data, 0, data.Length + n);
            //data = Arrays.copyOf(data, data.Length + n);
			for (int i = 0; i < n; i++)
			{
				data[data.Length - n + i] = (int) bits.getValue(bits.size() - n * 8 + i * 8, 8);
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.google.zxing.common.reedsolomon.ReedSolomonDecoder dec = new com.google.zxing.common.reedsolomon.ReedSolomonDecoder(com.google.zxing.common.reedsolomon.GenericGF.QR_CODE_FIELD_256);
			ReedSolomonDecoder dec = new ReedSolomonDecoder(GenericGF.QR_CODE_FIELD_256);
			dec.decode(data, n);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Bits result = new Bits();
			Bits result = new Bits();
            Array.Copy(data, 0, data, 0, data.Length - n);
            result.addBytes(data);
            //result.addBytes(Arrays.copyOf(data, data.Length - n));
			return result;
		}

		/// <summary>
		/// Encode using Reed-Solomon error correction (with n bytes added to the end of bits). </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static Bits bitsReedSolomonEncode(final Bits bits, final int n)
		public static Bits bitsReedSolomonEncode(Bits bits, int n)
		{
//ORIGINAL LINE: final int[] data = java.util.Arrays.copyOf(bits.getBytes(), (bits.size() + 7) / 8 + n);
            //copyOf(int[] original,int newLength)
            //Array.copy(Array sourceArray, Array destinationArray, int length)
            int tmpSize = (bits.size() + 7) / 8 + n;
            int[] data = new int[tmpSize]; 
		    if (tmpSize > bits.Bytes.Length)
		    {
		        tmpSize = bits.Bytes.Length;
		    }
            Array.Copy(bits.Bytes, 0, data, 0, tmpSize);
			//int[] data = Arrays.copyOf(bits.Bytes, (bits.size() + 7) / 8 + n);

            ReedSolomonEncoder enc = new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256);
			enc.encode(data, n);

			Bits result = new Bits(bits);
			for (int i = data.Length - n; i < data.Length; i++)
			{
				result.addValue(data[i], 8);
			}
			return result;
		}

		/// <summary>
		/// Some test I run, just while debugging. </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void main(final String[] args)
		public static void Main(string[] args)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Bits bits = new Bits();
			Bits bits = new Bits();
			bits.addBit(true);
			bits.addBit(false);
			bits.addBit(true);
			Console.WriteLine(bits);
			Console.WriteLine(bits.getValue(0, 3));
			bits.addValue(6, 3);
			Console.WriteLine(bits);
			Console.WriteLine(bits.getValue(3, 3));
			sbyte[] data = bits.Data;
			foreach (sbyte element in data)
			{
				Console.Write(element + " ");
			}
			Console.WriteLine();
			Console.WriteLine("------");
			bits.reset();
			bits.addData(data);
			Console.WriteLine(bits);
			data[0] = unchecked((sbyte) 0xFF);
			bits.addData(data);
			Console.WriteLine(bits);
			data = bits.Data;
			foreach (sbyte element in data)
			{
				Console.Write(element + " ");
			}
			Console.WriteLine();
			Console.WriteLine("------");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] bytes = bits.getBytes();
			int[] bytes = bits.Bytes;
			foreach (int b in bytes)
			{
				Console.Write(b + " ");
			}
			Console.WriteLine();
			Console.WriteLine("------");

			Console.WriteLine(bitsGZIPEncode(bits));
			try
			{
				Console.WriteLine(bitsGZIPDecode(bitsGZIPEncode(bits)));
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
			catch (System.IO.IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			Console.WriteLine("------");

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Bits bitsRS = bitsReedSolomonEncode(bits, 2);
			Bits bitsRS = bitsReedSolomonEncode(bits, 2);
			Console.WriteLine(bitsRS);
			try
			{
				Console.WriteLine(bitsReedSolomonDecode(bitsRS, 2));
				bitsRS.setBit(10, false);
				Console.WriteLine(bitsRS);
				Console.WriteLine(bitsReedSolomonDecode(bitsRS, 2));
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final com.google.zxing.common.reedsolomon.ReedSolomonException e)
			catch (ReedSolomonException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}

		/// <summary>
		/// Internal array with bits. </summary>
		private readonly IList<bool?> bits;

		/// <summary>
		/// The read-counter for pop-methods. </summary>
		private int readPosition = 0;

		public Bits()
		{
			this.bits = new List<bool?>();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public Bits(final Bits bits)
		public Bits(Bits bits)
		{
			this.bits = new List<bool?>(bits.bits);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public Bits(final java.util.Collection<Boolean> bits)
		public Bits(ICollection<bool?> bits)
		{
			this.bits = new List<bool?>(bits);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addBit(final boolean bit)
		public virtual void addBit(bool bit)
		{
			this.bits.Add(bit);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addBits(final boolean[] bits)
		public virtual void addBits(bool[] bits)
		{
			foreach (bool bit in bits)
			{
				addBit(bit);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addBits(final java.util.Collection<Boolean> bits)
		public virtual void addBits(ICollection<bool?> bits)
		{
			foreach (Boolean bit in bits)
			{
				addBit(bit);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addBytes(final int[] bytes)
		public virtual void addBytes(int[] bytes)
		{
			addBytes(bytes, bytes.Length);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addBytes(final int[] bytes, final int len)
		public virtual void addBytes(int[] bytes, int len)
		{
			for (int i = 0; i < len; i++)
			{
				int bit = 0x01;
				for (int j = 0; j < 8; j++)
				{
					addBit((bytes[i] & bit) > 0);
					bit <<= 1;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addData(final byte[] data)
		public virtual void addData(sbyte[] data)
		{
			addData(data, data.Length);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addData(final byte[] data, final int len)
		public virtual void addData(sbyte[] data, int len)
		{
			for (int i = 0; i < len; i++)
			{
				int bit = 0x01;
				for (int j = 0; j < 8; j++)
				{
					addBit((data[i] & bit) > 0);
					bit <<= 1;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addValue(final long bits, final int len)
		public virtual void addValue(long bits, int len)
		{
			long bit = 0x01;
			for (int i = 0; i < len; i++)
			{
				addBit((bits & bit) > 0);
				bit <<= 1;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public boolean getBit(final int index)
		public virtual bool getBit(int index)
		{
            return (bool)this.bits[index];
		}

		public virtual IList<bool?> getBits()
		{
			return this.bits;
		}

		/// <summary>
		/// Return a sublist of the bits.
		/// </summary>
		/// <param name="fromIndex"> Start position, inclusive. </param>
		/// <param name="toIndex"> End position, exclusive. </param>
		/// <returns> Bits for the specified range. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public java.util.List<Boolean> getBits(final int fromIndex, final int toIndex)
		public virtual IList<bool?> getBits(int fromIndex, int toIndex)
		{
			//return this.bits.subList(fromIndex, toIndex)
            IList<bool?> returns = this.bits;
            for (int i = 0; i < fromIndex; i++)
            {
                returns.RemoveAt(i);
            }
            for (int i = toIndex; i < returns.Count; i++ )
            {
                returns.RemoveAt(i);
            }
            return returns; //TODO: Moeglicher Fehler
		}

		public virtual int[] Bytes
		{
			get
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int[] bytes = new int[(this.bits.size() + 7) / 8];
				int[] bytes = new int[(this.bits.Count + 7) / 8];
				for (int i = 0; i < bytes.Length; i++)
				{
					int bit = 0x01;
					for (int j = 0; j < 8 && i * 8 + j < this.bits.Count; j++)
					{
						if ((bool)this.bits[i * 8 + j])
						{
							bytes[i] |= bit;
						}
						bit <<= 1;
					}
				}
				return bytes;
			}
		}

		public virtual sbyte[] Data
		{
			get
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final byte[] data = new byte[(this.bits.size() + 7) / 8];
				sbyte[] data = new sbyte[(this.bits.Count + 7) / 8];
				for (int i = 0; i < data.Length; i++)
				{
					int bit = 0x01;
					for (int j = 0; j < 8 && i * 8 + j < this.bits.Count; j++)
					{
						if ((bool)this.bits[i * 8 + j])
						{
							data[i] |= (sbyte)bit;
						}
						bit <<= 1;
					}
				}
				return data;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public long getValue(final int index, final int len)
		public virtual long getValue(int index, int len)
		{
			long result = 0;
			long bit = 0x01;
			for (int i = index; i < index + len; i++)
			{
				if ((bool)this.bits[i])
				{
					result |= bit;
				}
				bit <<= 1;
			}
			return result;
		}

		public virtual bool hasNext()
		{
			return this.readPosition < this.bits.Count;
		}



		public virtual bool hasNext(int len)
		{
			return this.readPosition + len < this.bits.Count + 1;
		}

		public virtual bool popBit()
		{
			return getBit(this.readPosition++);
		}


		public virtual IList<bool?> popBits(int len)
		{
			this.readPosition += len;
			return getBits(this.readPosition - len, this.readPosition);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public long popValue(final int len)
		public virtual long popValue(int len)
		{
			this.readPosition += len;
			return getValue(this.readPosition - len, len);
		}

		public virtual void reset()
		{
			this.bits.Clear();
			this.readPosition = 0;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void setBit(final int index, final boolean bit)
		public virtual void setBit(int index, bool bit)
		{
			this.bits[index] = bit;
		}

		public virtual int size()
		{
			return this.bits.Count;
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder buf = new StringBuilder(this.bits.size());
			StringBuilder buf = new StringBuilder(this.bits.Count);
			for (int i = 0; i < this.bits.Count; i++)
			{
				buf.Append((bool)this.bits[i] ? '1' : '0');
			}
			return buf.ToString();
		}

	}

}