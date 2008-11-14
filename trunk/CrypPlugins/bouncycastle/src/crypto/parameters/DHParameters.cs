using System;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHParameters
		: ICipherParameters
    {
		private readonly BigInteger p, g, q;
		private readonly int j;
		private readonly DHValidationParameters validation;

		public DHParameters(
            BigInteger	p,
            BigInteger	g)
			: this(p, g, null, 0, null)
        {
        }

		public DHParameters(
			BigInteger	p,
			BigInteger	g,
			BigInteger	q)
			: this(p, g, q, 0, null)
		{
		}

		public DHParameters(
			BigInteger	p,
			BigInteger	g,
			int			j)
			: this(p, g, null, j, null)
		{
		}

		public DHParameters(
            BigInteger	p,
            BigInteger	g,
            BigInteger	q,
            int			j)
			: this(p, g, q, j, null)
        {
        }

		public DHParameters(
            BigInteger				p,
            BigInteger				g,
            BigInteger				q,
            int						j,
            DHValidationParameters	validation)
        {
			if (p == null)
				throw new ArgumentNullException("p");
			if (g == null)
				throw new ArgumentNullException("g");
			if (!p.TestBit(0))
				throw new ArgumentException("field must be an odd prime", "p");
			if (g.CompareTo(BigInteger.ValueOf(2)) < 0
				|| g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
				throw new ArgumentException("generator must in the range [2, p -2]", "g");

			this.p = p;
			this.g = g;
			this.q = q;
            this.j = j;
            this.validation = validation;
        }

		public BigInteger P
        {
            get { return p; }
        }

		public BigInteger G
        {
            get { return g; }
        }

		public BigInteger Q
        {
            get { return q; }
        }

		public int J
        {
            get { return j; }
        }

		public DHValidationParameters ValidationParameters
        {
			get { return validation; }
        }

		public override bool Equals(
			object obj)
        {
			if (obj == this)
				return true;

			DHParameters other = obj as DHParameters;

			if (other == null)
				return false;

			return Equals(other);
		}

		protected bool Equals(
			DHParameters other)
		{
			return j == other.J
				&& p.Equals(other.p)
				&& g.Equals(other.g)
				&& Platform.Equals(q, other.q);
		}

		public override int GetHashCode()
        {
			int hc = j.GetHashCode() ^ p.GetHashCode() ^ g.GetHashCode();

			if (q != null)
			{
				hc ^= q.GetHashCode();
			}

			return hc;
        }
    }
}
