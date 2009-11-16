using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Cryptool.MD5;

namespace Test.Cryptool.MD5
{
    [TestFixture]
    class PresentableMd5Test
    {
        Random RNG;

        [SetUp]
        public void SetUp()
        {
            RNG = new Random();
        }

        [Test]
        public void VerifyResultsForRandomData()
        {
            const int TEST_RUNS = 20;
            const int MIN_TEST_DATA_LENGTH = 1;
            const int MAX_TEST_DATA_LENGTH = 1000;

            for (int i = 0; i < TEST_RUNS; i++)
            {
                byte[] testData = GenerateTestData(MIN_TEST_DATA_LENGTH, MAX_TEST_DATA_LENGTH);
                VerifyResult(testData);
            }
        }

        [Test]
        public void VerifyResultForEmptyData()
        {
            byte[] emptyByteArray = new byte[0];
            VerifyResult(emptyByteArray);
        }

        [Test]
        public void VerifyResultForRandomDataWithInterestingLength()
        {
            VerifyResult(GenerateTestData(54));
            VerifyResult(GenerateTestData(55));
            VerifyResult(GenerateTestData(56));
        }

        private void VerifyResult(byte[] data)
        {
            System.Security.Cryptography.MD5 builtinMD5 = System.Security.Cryptography.MD5.Create();
            byte[] builtinResult = builtinMD5.ComputeHash(data);

            PresentableMd5 presentableMd5 = new PresentableMd5(new MemoryStream(data));
            presentableMd5.NextStepUntilFinished();
            byte[] presentableMd5Result = presentableMd5.HashValueBytes;

            Assert.AreEqual(builtinResult, presentableMd5Result);
        }

        private byte[] GenerateTestData(int minLength, int maxLength)
        {
            int resultLength = minLength + RNG.Next(maxLength - minLength) + 1;
            return GenerateTestData(resultLength);
        }

        private byte[] GenerateTestData(int length)
        {
            byte[] result = new byte[length];

            RNG.NextBytes(result);

            return result;
        }
    }
}

