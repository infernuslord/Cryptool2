using System;
using System.Collections;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class PbeParameter
		: Asn1Encodable
    {
        private readonly Asn1OctetString octStr;
        private readonly DerInteger iterationCount;

		public static PbeParameter GetInstance(
            object o)
        {
            if (o is PbeParameter || o == null)
            {
                return (PbeParameter) o;
            }

			if (o is Asn1Sequence)
            {
                return new PbeParameter((Asn1Sequence) o);
            }

			throw new ArgumentException("unknown object in factory: " + o);
        }

		private PbeParameter(
			Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			octStr = Asn1OctetString.GetInstance(seq[0]);
            iterationCount = DerInteger.GetInstance(seq[1]);
        }

		public PbeParameter(
            byte[]	salt,
            int		iterationCount)
        {
            this.octStr = new DerOctetString(salt);
            this.iterationCount = new DerInteger(iterationCount);
        }

		public byte[] GetSalt()
        {
            return octStr.GetOctets();
        }

		public BigInteger IterationCount
		{
			get { return iterationCount.Value; }
		}

		public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(octStr, iterationCount);
        }
    }
}
