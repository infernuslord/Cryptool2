using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace Cryptool.MD5.Test
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
        public void CompareResultsToBuiltinMd5()
        {
            const int TEST_RUNS = 20;

            for (int i = 0; i < TEST_RUNS; i++)
            {
                byte[] testData = GenerateTestData();

                System.Security.Cryptography.MD5 builtinMD5 = System.Security.Cryptography.MD5.Create();
                byte[] builtinResult = builtinMD5.ComputeHash(testData);

                PresentableMd5 presentableMd5 = new PresentableMd5(new MemoryStream(testData));
                presentableMd5.NextStepUntilFinished();
                byte[] presentableMd5Result = presentableMd5.HashValueBytes;

                Assert.AreEqual(builtinResult, presentableMd5Result);
            }
        }

        private byte[] GenerateTestData()
        {
            const int MIN_TEST_DATA_LENGTH = 1;
            const int MAX_TEST_DATA_LENGTH = 1000;

            int resultLength = MIN_TEST_DATA_LENGTH + RNG.Next(MAX_TEST_DATA_LENGTH - MIN_TEST_DATA_LENGTH) + 1;
            byte[] result = new byte[resultLength];

            RNG.NextBytes(result);

            return result;
        }
    }
}

