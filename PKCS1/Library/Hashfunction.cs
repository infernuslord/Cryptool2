using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace PKCS1.Library
{
    class Hashfunction
    {
        private static IDigest hashFunctionDigest = DigestUtilities.GetDigest(HashFunctionHandler.SHA1.diplayName); // default SHA1

        public static byte[] generateHashDigest(string input, HashFunctionIdent hashIdent)
        {
            return generateHashDigest(Encoding.ASCII.GetBytes(input), hashIdent);
        }

        public static byte[] generateHashDigest(byte[] input, HashFunctionIdent hashIdent)
        {   
            hashFunctionDigest = DigestUtilities.GetDigest(hashIdent.diplayName);
            byte[] hashDigest = new byte[hashFunctionDigest.GetDigestSize()];
            hashFunctionDigest.BlockUpdate(input, 0, input.Length);
            hashFunctionDigest.DoFinal(hashDigest, 0);            

            return hashDigest;
        }

        // gibt länge in bytes zurück!
        public static int getDigestSize()
        {
            return hashFunctionDigest.GetDigestSize();
        }

        public static string getAlgorithmName()
        {
            return hashFunctionDigest.AlgorithmName;
        }
    }
}
