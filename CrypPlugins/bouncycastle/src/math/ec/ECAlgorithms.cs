using System;

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Math.EC
{
	public class ECAlgorithms
	{
		/*
		* "Shamir's Trick", originally due to E. G. Straus
		* (Addition chains of vectors. American Mathematical Monthly,
		* 71(7):806–808, Aug./Sept. 1964)
		*  
		* Input: The points P, Q, scalar k = (km?, ... , k1, k0)
		* and scalar l = (lm?, ... , l1, l0).
		* Output: R = k * P + l * Q.
		* 1: Z <- P + Q
		* 2: R <- O
		* 3: for i from m-1 down to 0 do
		* 4:        R <- R + R        {point doubling}
		* 5:        if (ki = 1) and (li = 0) then R <- R + P end if
		* 6:        if (ki = 0) and (li = 1) then R <- R + Q end if
		* 7:        if (ki = 1) and (li = 1) then R <- R + Z end if
		* 8: end for
		* 9: return R
		*/
		public static ECPoint ShamirsTrick(
			ECPoint		P,
			BigInteger	k,
			ECPoint		Q,
			BigInteger	l)
		{
			if (!P.Curve.Equals(Q.Curve))
				throw new ArgumentException("P and Q must be on same curve");

			int m = System.Math.Max(k.BitLength, l.BitLength);
			ECPoint Z = P.Add(Q);
			ECPoint R = P.Curve.Infinity;

			for (int i = m - 1; i >= 0; --i)
			{
				R = R.Twice();

				if (k.TestBit(i))
				{
					if (l.TestBit(i))
					{
						R = R.Add(Z);
					}
					else
					{
						R = R.Add(P);
					}
				}
				else
				{
					if (l.TestBit(i))
					{
						R = R.Add(Q);
					}
				}
			}

			return R;
		}
	}
}
