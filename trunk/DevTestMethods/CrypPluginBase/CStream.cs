using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Cryptool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests.CrypPluginBase
{
    /// <summary>
    /// Summary description for CStream
    /// </summary>
    [TestClass]
    public class CStream
    {
        Random rng = new Random();
        private CStreamWriter writer;

        private byte[] ShortData
        {
            get;
            set;
        }

        private byte[] LongData
        {
            get;
            set;
        }

        public CStream()
        {
            ShortData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            LongData = new byte[65535];
            rng.NextBytes(LongData);

            writer = new CStreamWriter();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SelfTest()
        {
            // assert 0 < ShortData < LongData
            Assert.IsTrue(0 < ShortData.Length);
            Assert.IsTrue(ShortData.Length < LongData.Length);
        }

        [TestMethod]
        public void TestWriter()
        {
            // put 6 bytes
            writer.Write(ShortData);

            // length == position == 6
            Assert.AreEqual(ShortData.Length, writer.Length);
            Assert.AreEqual(ShortData.Length, writer.Position);

            // not swapped
            Assert.IsFalse(writer.IsSwapped);
        }

        [TestMethod]
        public void TestReader()
        {
            // put 6 bytes
            writer.Write(ShortData);
            CStreamReader reader = writer.CreateReader();

            // length==6, position==0
            Assert.AreEqual(0, reader.Position);
            Assert.AreEqual(ShortData.Length, reader.Length);

            // try to read more bytes than available
            byte[] buf = new byte[ShortData.Length*1000];
            Assert.AreNotEqual(buf.Length, ShortData.Length);
            Assert.IsFalse(buf.SequenceEqual(ShortData));
            int read = reader.Read(buf);
            Assert.AreEqual(ShortData.Length, read);

            // assert the first few bytes are still correct
            byte[] buf2 = new byte[ShortData.Length];
            Array.Copy(buf, buf2, buf2.Length);
            Assert.IsTrue(buf2.SequenceEqual(ShortData));

            // not swapped
            Assert.IsFalse(reader.IsSwapped);
        }

        [TestMethod]
        public void TestSwap()
        {
            // fill buffer with Length-1 bytes
            writer.Write(LongData);
            Assert.AreEqual(LongData.Length, writer.Position);
            Assert.IsFalse(writer.IsSwapped);

            // fill last byte
            writer.WriteByte(5);
            Assert.AreEqual(LongData.Length + 1, writer.Position);
            Assert.IsFalse(writer.IsSwapped);

            // write one byte more, assert swap
            writer.WriteByte(10);
            Assert.AreEqual(LongData.Length + 2, writer.Position);
            Assert.IsTrue(writer.IsSwapped);
        }

        [TestMethod]
        public void TestSwapWithReader()
        {
            CStreamReader reader = writer.CreateReader();

            // write, not swapped
            writer.Write(LongData);
            Assert.IsFalse(writer.IsSwapped);

            // read a few bytes, but there are still a few bytes left
            byte[] buf = new byte[ShortData.Length];
            reader.Read(buf, 0, buf.Length);
            Assert.IsTrue(reader.Position > 0);
            Assert.IsTrue(reader.Length > reader.Position);

            // fill buffer, assert swap
            writer.Write(LongData);
            writer.Write(LongData);
            Assert.IsTrue(writer.IsSwapped);
            Assert.IsTrue(reader.IsSwapped);

            // try to read more than available, receive less
            buf = new byte[writer.Length*2];
            int read = reader.Read(buf);
            Assert.IsTrue(read < buf.Length);

            // close writer, asser eof
            writer.Close();
            int result = reader.ReadByte();
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestDestructor()
        {
            CStreamWriter_Accessor prvWriter = new CStreamWriter_Accessor();
            
            Assert.IsFalse(prvWriter.IsSwapped);
            prvWriter.Write(LongData);
            Assert.IsFalse(prvWriter.IsSwapped);
            prvWriter.Write(LongData);

            Assert.IsTrue(prvWriter.IsSwapped);

            // swapfile exists
            string filePath = prvWriter.FilePath;
            Assert.IsTrue(File.Exists(filePath));
            
            // force GC
            prvWriter = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // swapfile removed
            Assert.IsFalse(File.Exists(filePath));
        }

        [TestMethod]
        public void TestDestructorWithReader()
        {
            CStreamWriter_Accessor prvWriter = new CStreamWriter_Accessor();

            CStreamReader reader = prvWriter.CreateReader();
            byte[] buf = new byte[ShortData.Length];
            reader.Read(buf, 0, buf.Length);

            // have read something and there more left
            Assert.IsTrue(reader.Position > 0);
            Assert.IsTrue(reader.Length > reader.Position);

            // not swapped 

            prvWriter.Write(LongData);
            prvWriter.Write(LongData);

            // swapfile exists
            string filePath = prvWriter.FilePath;
            Assert.IsTrue(File.Exists(filePath));




        }
    }
}
