//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://www.cryptool.org/svn/CrypTool2/trunk/CrypPlugins/PKCS5/SSCpkcs5.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 121                                                                            $://
// $Author:: junker                                                                           $://
// $Date:: 2008-12-05 10:55:45 +0100 (Fr, 05 Dez 2008)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using System.Security.Cryptography;
using System.Runtime.InteropServices;

using Whirlpool;

#if DEBUG
using System.Diagnostics;
#endif

namespace System.Security.Cryptography
{
	public class HMACWhirlpool : System.Security.Cryptography.HMAC
	{

		WhirlpoolHash whirlHash = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="HMACWhirlpool"/> class.
		/// </summary>
		public  HMACWhirlpool() : base()
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HMACWhirlpool"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public HMACWhirlpool(byte[] key) : base()
		{
			Initialize();
			HashCore(key,0, key.Length*8);

			base.HashValue = whirlHash.Hash;
			base.State = 1;
		}

		/// <summary>
		/// Initializes an instance of the default 
		/// implementation of <see cref="T:System.Security.Cryptography.HMAC"/>.
		/// </summary>
		public override void Initialize()
		{
			whirlHash = new WhirlpoolHash();

			//base.Initialize();
			base.State = 0;
			base.HashSizeValue = 512;
			base.HashName = "Whirlpool";
		}


		/// <summary>
		/// Releases the unmanaged resources used by the 
		/// <see cref="T:System.Security.Cryptography.HMAC"/> class when a key change 
		/// is legitimate and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.</param>
		public new void Dispose(bool disposing)
		{
			if (disposing) whirlHash = null;
			base.Dispose(disposing);
		}

		/// <summary>
		/// When overridden in a derived class, 
		/// gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		/// <value></value>
		/// <returns>true if multiple blocks can be transformed; otherwise, false.
		/// </returns>
		public override bool CanTransformMultipleBlocks
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// When overridden in a derived class, 
		/// routes data written to the object into the 
		/// default <see cref="T:System.Security.Cryptography.HMAC"/> hash algorithm 
		/// for computing the hash value.
		/// </summary>
		/// <param name="rgb">The input data.</param>
		/// <param name="ib">The offset into the byte array from which to begin using data.</param>
		/// <param name="cb">The number of bytes in the array to use as data.</param>
		protected override void HashCore(byte[] rgb, int ib, int cb)
		{
			whirlHash.Add(rgb, (ulong)cb);
			base.State = 1;
		}

		/// <summary>
		/// When overridden in a derived class, 
		/// finalizes the hash computation after the last data is processed by 
		/// the cryptographic stream object.
		/// </summary>
		/// <returns>The computed hash code in a byte array.</returns>
		protected override byte[] HashFinal()
		{
			whirlHash.Finish();
			base.State = 0;
			return whirlHash.Hash;
		}

		/// <summary>
		/// Gets or sets the key to use in the hash algorithm.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The key to use in the hash algorithm.
		/// </returns>
		/// <exception cref="T:System.Security.Cryptography.CryptographicException">
		/// An attempt is made to change the
		/// <see cref="P:System.Security.Cryptography.HMAC.Key"/> property after hashing has begun.
		/// </exception>
		public override byte[] Key
		{
			get
			{
				return base.Key;
			}
			set
			{
				base.Key = value;
			}
		}

		/// <summary>
		/// Computes the hash value for the specified byte array.
		/// </summary>
		/// <param name="buffer">The input to compute the hash code for.</param>
		/// <returns>The computed hash code.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer"/> is null.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// The object has already been disposed.
		/// </exception>
		public new byte[] ComputeHash(byte[] buffer)
		{
			HashCore(base.Key, 0, base.Key.Length * 8);
			if (null != buffer && buffer.Length > 0)
				HashCore(buffer, 0, buffer.Length * 8);
			HashFinal();
			return Hash;
		}

	}
}
