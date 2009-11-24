﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Cryptool.MD5.Algorithm;

namespace Test.Cryptool.MD5.Algorithm
{
    [TestFixture]
    class PresentableMD5Test
    {
        Random RNG;

        [SetUp]
        public void SetUp()
        {
            RNG = new Random();
        }

        [Test]
        public void Construction()
        {
            new PresentableMD5();
        }

        [Test]
        public void UninitializedStateAfterConstruction()
        {
            PresentableMD5 md5 = new PresentableMD5();

            Assert.AreEqual(md5.CurrentState.State, MD5StateDescription.UNINITIALIZED);
        }

        [Test]
        public void InitializedStateAfterInitialization()
        {
            PresentableMD5 md5 = new PresentableMD5();
            Stream stream = new MemoryStream(new byte[0]);
            md5.Initialize(stream);

            Assert.AreEqual(md5.CurrentState.State, MD5StateDescription.INITIALIZED);
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
            VerifyResult(new byte[0]);
        }

        [Test]
        public void VerifyResultsForRandomDataWithInterestingLength()
        {
            VerifyResult(GenerateTestData(54));
            VerifyResult(GenerateTestData(55));
            VerifyResult(GenerateTestData(56));
        }

        private void VerifyResult(byte[] data)
        {
            System.Security.Cryptography.MD5 builtinMD5 = System.Security.Cryptography.MD5.Create();
            byte[] builtinResult = builtinMD5.ComputeHash(data);

            PresentableMD5 presentableMD5 = new PresentableMD5();
            presentableMD5.Initialize(new MemoryStream(data));
            presentableMD5.NextStepUntilFinished();
            byte[] presentableMD5Result = presentableMD5.HashValueBytes;

            Assert.AreEqual(builtinResult, presentableMD5Result);
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

