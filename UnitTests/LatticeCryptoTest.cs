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
        public void GGHTest()
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
                string message = GenerateMessage(1, 100);
                VectorND cipher = ggh.Encrypt(message);
                string decryptedMessage = ggh.Decrypt(cipher);
                Assert.AreEqual(message, decryptedMessage);
            }
        }

        [TestMethod]
        public void MerkleHellmanTest()
        {
            //Example taken from:
            //http://en.wikipedia.org/wiki/Merkle%E2%80%93Hellman_knapsack_cryptosystem

            VectorND privateKey = new VectorND(new BigInteger[] { 2, 7, 11, 21, 42, 89, 180, 354 });
            MerkleHellmanModel merkleHellman = new MerkleHellmanModel(privateKey, 588, 881);

            //Cryptosystem correct?
            VectorND publicKey = new VectorND(new BigInteger[] { 295, 592, 301, 14, 28, 353, 120, 236 });
            Assert.AreEqual(publicKey.ToString(), merkleHellman.publicKey.ToString());
            BigInteger rI = 442;
            Assert.AreEqual(rI, merkleHellman.rI);
            
            //Encryption and decryption correct?
            string message = "a";
            VectorND cipher = merkleHellman.Encrypt(message);
            VectorND expectedCipher = new VectorND(new BigInteger[]{1129});
            Assert.AreEqual(cipher.ToString(), expectedCipher.ToString());
            string decryptedMessage = merkleHellman.Decrypt(cipher);
            Assert.AreEqual(message, decryptedMessage);

            //Cryptoanalysis correct?
            string cryptanalysedMessage = merkleHellman.Cryptanalysis(cipher, null);
            Assert.AreEqual(message, cryptanalysedMessage);

            //More Testing
            for (int i = 0; i < 1000; i++)
            {
                message = GenerateMessage(1, 100);
                cipher = merkleHellman.Encrypt(message);
                decryptedMessage = merkleHellman.Decrypt(cipher);
                Assert.AreEqual(message, decryptedMessage);
            }
        }

        [TestMethod]
        public void RSATest()
        {
            //There is no need to test the generation of a RSA cryptosystem, because BouncyCastle is used
            Random r = new Random();

            for (int i = 0; i < 10; i++)
            {
                int bitSize = r.Next(32, 1024);
                RSAModel rsa = new RSAModel(bitSize);

                //Encryption and decryption correct?
                string message = GenerateMessage(3, rsa.GetBlockSize());
                BigInteger cipher = rsa.Encrypt(message);
                string decryptedMessage = rsa.Decrypt(cipher);
                Assert.AreEqual(message, decryptedMessage);

                //Cryptoanalysis correct?
                int unknownStart = r.Next(message.Length);
                int unknownLength = r.Next(1, message.Length - unknownStart);
                string left = message.Substring(0, unknownStart);
                string right = message.Substring(unknownStart + unknownLength);
                string unknownMessageResult = rsa.StereotypedAttack(left, right, unknownLength, cipher, "4");
                Assert.AreEqual(message.Substring(unknownStart, unknownLength), unknownMessageResult);
            }
        }

        public string GenerateMessage(int min, int max)
        {
            Random r = new Random();
            int length = r.Next(min, max);
            StringBuilder message = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                message.Append((char)r.Next(33, 127));

            return message.ToString();
        }
    }
}
