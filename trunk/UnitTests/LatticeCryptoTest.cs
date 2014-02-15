using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LatticeCrypto.Models;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class LatticeCryptoTest
    {
        [TestMethod]
        public void GGHMethod()
        {
            //Private key, public key and error vector taken from the book:
            //Hoffstein, J.; Pipher, J. & Silverman, J. H. 
            //Axler, S. & Ribet, K. A. (Eds.) 
            //'An Introduction to Mathematical Cryptography'
            //Springer, 2008
            //Chapter 'The GGH public key cryptosystem'
            //Pages 384ff

            VectorND[] privateVectors =
            {
                new VectorND(new BigInteger[] {-97, 19, 19}),
                new VectorND(new BigInteger[] {-36, 30, 86}),
                new VectorND(new BigInteger[] {-184, -64, 78})
            };
            LatticeND privateKey = new LatticeND(privateVectors, false);
            
            VectorND[] publicVectors =
            {
                new VectorND(new BigInteger[] { -4179163, -1882253, 583183 }), 
                new VectorND(new BigInteger[] { -3184353, -1434201, 444361 }), 
                new VectorND(new BigInteger[] { -5277320, -2376852, 736426 })
            };
            LatticeND publicKey = new LatticeND(publicVectors, false);
            
            VectorND errorVector = new VectorND(new BigInteger[] {-4, -3, - 2});

            GGHModel ggh = new GGHModel(3, privateKey.ToMatrixND(), publicKey.ToMatrixND(), errorVector);

            for (int i = 0; i < 1000; i++)
            {
                string message = GenerateMessage(100);
                VectorND cipher = ggh.Encrypt(message);
                string decryptedMessage = ggh.Decrypt(cipher);
                Assert.AreEqual(decryptedMessage, message);
            }
        }

        public string GenerateMessage(int maxLength)
        {
            Random r = new Random();
            int length = r.Next(maxLength);
            StringBuilder message = new StringBuilder(length);

            for (int i = 0; i < maxLength; i++)
                message.Append((char)r.Next(94, 127));

            return message.ToString();
        }
    }
}
