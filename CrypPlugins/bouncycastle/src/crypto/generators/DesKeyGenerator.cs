using System;

using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Generators
{
    public class DesKeyGenerator
		: CipherKeyGenerator
    {
		public DesKeyGenerator()
		{
		}

		internal DesKeyGenerator(
			int defaultStrength)
			: base(defaultStrength)
		{
		}

		protected override byte[] engineGenerateKey()
        {
            byte[] newKey = new byte[DesParameters.DesKeyLength];

			do
            {
                random.NextBytes(newKey);
                DesParameters.SetOddParity(newKey);
            }
            while (DesParameters.IsWeakKey(newKey, 0));

			return newKey;
        }
    }
}
