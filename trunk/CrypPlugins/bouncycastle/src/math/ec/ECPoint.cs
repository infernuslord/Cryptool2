using System;
using System.Collections;
using System.Diagnostics;

using Org.BouncyCastle.Asn1.X9;

namespace Org.BouncyCastle.Math.EC
{
	/**
	 * base class for points on elliptic curves.
	 */
	public abstract class ECPoint
	{
		internal readonly ECCurve			curve;
		internal readonly ECFieldElement	x, y;

		protected ECPoint(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y)
		{
			if (curve == null)
				throw new ArgumentNullException("curve");

			this.curve = curve;
			this.x = x;
			this.y = y;
		}

		public ECCurve Curve
		{
			get { return curve; }
		}

		public ECFieldElement X
		{
			get { return x; }
		}

		public ECFieldElement Y
		{
			get { return y; }
		}

		public bool IsInfinity
		{
			get { return x == null && y == null; }
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			ECPoint o = obj as ECPoint;

			if (o == null)
				return false;

			if (this.IsInfinity)
				return o.IsInfinity;

			return x.Equals(o.x) && y.Equals(o.y);
		}

		public override int GetHashCode()
		{
			if (this.IsInfinity)
				return 0;

			return x.GetHashCode() ^ y.GetHashCode();
		}

		public abstract byte[] GetEncoded();

		public abstract ECPoint Add(ECPoint b);
		public abstract ECPoint Subtract(ECPoint b);
		public abstract ECPoint Twice();
		public abstract ECPoint Multiply(BigInteger b);
	}

	public abstract class ECPointBase
		: ECPoint
	{
		protected internal readonly bool withCompression;

		protected internal ECPointBase(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y,
			bool			withCompression)
			: base(curve, x, y)
		{
			this.withCompression = withCompression;
		}

		protected internal abstract bool YTilde { get; }

		/**
		 * return the field element encoded with point compression. (S 4.3.6)
		 */
		public override byte[] GetEncoded()
		{
			if (this.IsInfinity)
				return new byte[1];

			// Note: some of the tests rely on calculating byte length from the field element
			// (since the test cases use mismatching fields for curve/elements)
			int byteLength = X9IntegerConverter.GetByteLength(x);
			byte[] X = X9IntegerConverter.IntegerToBytes(this.X.ToBigInteger(), byteLength);
			byte[] PO;

			if (withCompression)
			{
				PO = new byte[1 + X.Length];

				PO[0] = (byte)(YTilde ? 0x03 : 0x02);
			}
			else
			{
				byte[] Y = X9IntegerConverter.IntegerToBytes(this.Y.ToBigInteger(), byteLength);
				PO = new byte[1 + X.Length + Y.Length];

				PO[0] = 0x04;

				Y.CopyTo(PO, 1 + X.Length);
			}

			X.CopyTo(PO, 1);

			return PO;
		}

		// D.3.2 pg 101
		public override ECPoint Multiply(
			BigInteger b)
		{
			if (this.IsInfinity)
				return this;

			if (b.SignValue == 0)
				return this.curve.Infinity;

			// BigInteger e = k.mod(n); // n == order this
			BigInteger e = b;
			BigInteger h = e.Multiply(BigInteger.Three);

			ECPoint neg = this.curve.Infinity.Subtract(this);
			ECPoint R = this;

			for (int i = h.BitLength - 2; i > 0; i--)
			{
				R = R.Twice();

				bool hBit = h.TestBit(i);
				bool eBit = e.TestBit(i);

				if (hBit != eBit)
				{
					R = R.Add(hBit ? this : neg);
				}
			}

			return R;
		}
	}

	/**
	 * Elliptic curve points over Fp
	 */
	public class FpPoint
		: ECPointBase
	{
		/**
		 * Create a point which encodes with point compression.
		 *
		 * @param curve the curve to use
		 * @param x affine x co-ordinate
		 * @param y affine y co-ordinate
		 */
		public FpPoint(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y)
			: this(curve, x, y, false)
		{
		}

		/**
		 * Create a point that encodes with or without point compresion.
		 *
		 * @param curve the curve to use
		 * @param x affine x co-ordinate
		 * @param y affine y co-ordinate
		 * @param withCompression if true encode with point compression
		 */
		public FpPoint(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y,
			bool			withCompression)
			: base(curve, x, y, withCompression)
		{
			if ((x != null && y == null) || (x == null && y != null))
				throw new ArgumentException("Exactly one of the field elements is null");
		}

		protected internal override bool YTilde
		{
			get
			{
				return this.Y.ToBigInteger().TestBit(0);
			}
		}

		// B.3 pg 62
		public override ECPoint Add(
			ECPoint b)
		{
			if (this.IsInfinity)
				return b;

			if (b.IsInfinity)
				return this;

			// Check if b = this or b = -this
			if (this.x.Equals(b.x))
			{
				if (this.y.Equals(b.y))
				{
					// this = b, i.e. this must be doubled
					return this.Twice();
				}

				Debug.Assert(this.y.Equals(b.y.Negate()));

				// this = -b, i.e. the result is the point at infinity
				return this.curve.Infinity;
			}

			ECFieldElement gamma = b.y.Subtract(this.y).Divide(b.x.Subtract(this.x));

			ECFieldElement x3 = gamma.Square().Subtract(this.x).Subtract(b.x);
			ECFieldElement y3 = gamma.Multiply(this.x.Subtract(x3)).Subtract(this.y);

			return new FpPoint(curve, x3, y3);
		}

		// B.3 pg 62
		public override ECPoint Twice()
		{
			// Twice identity element (point at infinity) is identity
			if (this.IsInfinity)
				return this;

			// if y1 == 0, then (x1, y1) == (x1, -y1)
			// and hence this = -this and thus 2(x1, y1) == infinity
			if (this.y.ToBigInteger().SignValue == 0)
				return this.curve.Infinity;

			ECFieldElement TWO = this.curve.FromBigInteger(BigInteger.Two);
			ECFieldElement THREE = this.curve.FromBigInteger(BigInteger.Three);
			ECFieldElement gamma = this.x.Square().Multiply(THREE).Add(curve.a).Divide(y.Multiply(TWO));

			ECFieldElement x3 = gamma.Square().Subtract(this.x.Multiply(TWO));
			ECFieldElement y3 = gamma.Multiply(this.x.Subtract(x3)).Subtract(this.y);

			return new FpPoint(curve, x3, y3, this.withCompression);
		}

		// D.3.2 pg 102 (see Note:)
		public override ECPoint Subtract(
			ECPoint b)
		{
			if (b.IsInfinity)
				return this;

			// Add -b
			return Add(new FpPoint(this.curve, b.x, b.y.Negate(), this.withCompression));
		}
	}

	/**
	 * Elliptic curve points over F2m
	 */
	public class F2mPoint
		: ECPointBase
	{
		/**
		 * @param curve base curve
		 * @param x x point
		 * @param y y point
		 */
		public F2mPoint(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y)
			:  this(curve, x, y, false)
		{
		}

		/**
		 * @param curve base curve
		 * @param x x point
		 * @param y y point
		 * @param withCompression true if encode with point compression.
		 */
		public F2mPoint(
			ECCurve			curve,
			ECFieldElement	x,
			ECFieldElement	y,
			bool			withCompression)
			: base(curve, x, y, withCompression)
		{
			if ((x != null && y == null) || (x == null && y != null))
			{
				throw new ArgumentException("Exactly one of the field elements is null");
			}

			if (x != null)
			{
				// Check if x and y are elements of the same field
				F2mFieldElement.CheckFieldElements(this.x, this.y);

				// Check if x and a are elements of the same field
				F2mFieldElement.CheckFieldElements(this.x, this.curve.A);
			}
		}

		/**
		 * Constructor for point at infinity
		 */
		[Obsolete("Use ECCurve.Infinity property")]
		public F2mPoint(
			ECCurve curve)
			: this(curve, null, null)
		{
		}

		protected internal override bool YTilde
		{
			get
			{
				// X9.62 4.2.2 and 4.3.6:
				// if x = 0 then ypTilde := 0, else ypTilde is the rightmost
				// bit of y * x^(-1)
				return this.X.ToBigInteger().SignValue != 0
					&& this.Y.Multiply(this.X.Invert()).ToBigInteger().TestBit(0);
			}
		}

		/* (non-Javadoc)
		 * @see Org.BouncyCastle.Math.EC.ECPoint#add(Org.BouncyCastle.Math.EC.ECPoint)
		 */
		public override ECPoint Add(
			ECPoint b)
		{
			// Check, if points are on the same curve
			if (!curve.Equals(b.Curve))
				throw new ArgumentException("Only points on the same curve can be added");

			if (this.IsInfinity)
				return b;

			if (b.IsInfinity)
				return this;

			F2mFieldElement.CheckFieldElements(this.x, b.X);
			F2mFieldElement x2 = (F2mFieldElement) b.X;
			F2mFieldElement y2 = (F2mFieldElement) b.Y;

			// Check if b = this or b = -this
			if (this.x.Equals(x2))
			{
				// this = b, i.e. this must be doubled
				if (this.y.Equals(y2))
					return this.Twice();

				// this = -b, i.e. the result is the point at infinity
				return this.curve.Infinity;
			}

			ECFieldElement xSum = this.x.Add(x2);

			F2mFieldElement lambda
				= (F2mFieldElement)(this.y.Add(y2)).Divide(xSum);

			F2mFieldElement x3
				= (F2mFieldElement)lambda.Square().Add(lambda).Add(xSum).Add(this.curve.A);

			F2mFieldElement y3
				= (F2mFieldElement)lambda.Multiply(this.x.Add(x3)).Add(x3).Add(this.y);

			return new F2mPoint(curve, x3, y3, withCompression);
		}

		/* (non-Javadoc)
		 * @see Org.BouncyCastle.Math.EC.ECPoint#subtract(Org.BouncyCastle.Math.EC.ECPoint)
		 */
		public override ECPoint Subtract(
			ECPoint b)
		{
			if (b.IsInfinity)
				return this;

			// Add -b
			F2mPoint minusB = new F2mPoint(this.curve, b.x, b.x.Add(b.y), this.withCompression);

			return Add(minusB);
		}

		/* (non-Javadoc)
		 * @see Org.BouncyCastle.Math.EC.ECPoint#twice()
		 */
		public override ECPoint Twice()
		{
			// Twice identity element (point at infinity) is identity
			if (this.IsInfinity)
				return this;

			// if x1 == 0, then (x1, y1) == (x1, x1 + y1)
			// and hence this = -this and thus 2(x1, y1) == infinity
			if (this.x.ToBigInteger().SignValue == 0)
				return this.curve.Infinity;

			F2mFieldElement lambda = (F2mFieldElement) this.x.Add(this.y.Divide(this.x));
			F2mFieldElement x3 = (F2mFieldElement)lambda.Square().Add(lambda).Add(this.curve.A);
			ECFieldElement ONE = this.curve.FromBigInteger(BigInteger.One);
			F2mFieldElement y3 = (F2mFieldElement)this.x.Square().Add(
				x3.Multiply(lambda.Add(ONE)));

			return new F2mPoint(this.curve, x3, y3, withCompression);
		}
	}
}
