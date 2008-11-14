using System;
using System.Globalization;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;

namespace Org.BouncyCastle.Security
{
    public class SecureRandom
		: Random
    {
		private static SecureRandom master;
		private static SecureRandom Master
		{
			get
			{
				if (master == null)
				{
//					master = GetInstance("SHA256PRNG");
					IRandomGenerator gen = new DigestRandomGenerator(new Sha256Digest());
					gen = new ReversedWindowGenerator(gen, 32);
					master = new SecureRandom(gen);

					master.SetSeed(DateTime.Now.Ticks);
					master.SetSeed(new ThreadedSeedGenerator().GenerateSeed(24, true));
				}

				return master;
			}
		}

		public static SecureRandom GetInstance(
			string algorithm)
		{
			// TODO Compared to JDK, we don't auto-seed if the client forgets - problem?

			// TODO Support all digests more generally, by stripping PRNG and calling DigestUtilities?
			IDigest digest = null;
			switch (algorithm.ToUpper(CultureInfo.InvariantCulture))
			{
				case "SHA1PRNG":
					digest = new Sha1Digest();
					break;
				case "SHA256PRNG":
					digest = new Sha256Digest();
					break;
			}

			if (digest != null)
			{
				return new SecureRandom(new DigestRandomGenerator(digest));
			}

			throw new ArgumentException("Unrecognised PRNG algorithm: " + algorithm, "algorithm");
		}

		public static byte[] GetSeed(
			int length)
		{
			SecureRandom master = Master;
			master.SetSeed(DateTime.Now.Ticks);
			return master.GenerateSeed(length);
		}

		protected IRandomGenerator generator;

		public SecureRandom()
			: base(0)
        {
			this.generator = new DigestRandomGenerator(new Sha1Digest());
			SetSeed(GetSeed(8));
		}

		public SecureRandom(
			byte[] inSeed)
			: base(0)
        {
			this.generator = new DigestRandomGenerator(new Sha1Digest());
			SetSeed(inSeed);
        }

		protected SecureRandom(
			IRandomGenerator generator)
			: base(0)
		{
			this.generator = generator;
		}

		public virtual byte[] GenerateSeed(
			int length)
		{
			// TODO Add some more seed material here?
			byte[] rv = new byte[length];
			NextBytes(rv);
			return rv;
		}

		public virtual void SetSeed(
			byte[] inSeed)
        {
			generator.AddSeedMaterial(inSeed);
        }

        public virtual void SetSeed(
			long seed)
        {
			generator.AddSeedMaterial(seed);
		}

		public override int Next()
		{
			for (;;)
			{
				int i = NextInt() & int.MaxValue;

				if (i != int.MaxValue)
					return i;
			}
		}

		public override int Next(
			int maxValue)
		{
			if (maxValue < 2)
			{
				if (maxValue < 0)
					throw new ArgumentOutOfRangeException("maxValue < 0");

				return 0;
			}

			// Test whether maxValue is a power of 2
			if ((maxValue & -maxValue) == maxValue)
			{
				int val = NextInt() & int.MaxValue;
				long lr = ((long) maxValue * (long) val) >> 31;
				return (int) lr;
			}

			int bits, result;
			do
			{
				bits = NextInt() & int.MaxValue;
				result = bits % maxValue;
			}
			while (bits - result + (maxValue - 1) < 0); // Ignore results near overflow

			return result;
		}

		public override int Next(
			int	minValue,
			int	maxValue)
		{
			if (maxValue <= minValue)
			{
				if (maxValue == minValue)
					return minValue;

				throw new ArgumentException("maxValue cannot be less than minValue");
			}

			int diff = maxValue - minValue;
			if (diff > 0)
				return minValue + Next(diff);

			for (;;)
			{
				int i = NextInt();

				if (i >= minValue && i < maxValue)
					return i;
			}
		}

		public override void NextBytes(
			byte[] buffer)
        {
			generator.NextBytes(buffer);
        }

		public virtual void NextBytes(
			byte[]	buffer,
			int		start,
			int		length)
		{
			generator.NextBytes(buffer, start, length);
		}

		private static double DoubleScale = System.Math.Pow(2.0, 64.0);

		public override double NextDouble()
		{
			return Convert.ToDouble((ulong) NextLong()) / DoubleScale;
		}

		public virtual int NextInt()
        {
			byte[] intBytes = new byte[4];
            NextBytes(intBytes);

			int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result = (result << 8) + (intBytes[i] & 0xff);
            }

			return result;
        }

		public virtual long NextLong()
		{
			return ((long)(uint) NextInt() << 32) | (long)(uint) NextInt();
		}
    }
}
