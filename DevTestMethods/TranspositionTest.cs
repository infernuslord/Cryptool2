
using System;
using System.Text;
using System.Security.Cryptography;
using Transposition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Testclass for Tiger hash
    /// </summary>
    [TestClass]
    public class TranspositionTest
    {

        public TranspositionTest()
        {
            // nothing to do
        }

        private TestContext testContextInstance;
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

        [TestMethod]
        public void TranspositionTestMethod()
        {
            String[] source = 
            {
                    "Wir wurden entdeckt, flieh wenn du kannst","A"
            };

            String[] source2 = { "ZEBRAS","HUHN"};

            String[,] result1 = 
            {
                    {"w cle tred  dnidt,h n nefwusuekinkWrntena",
                    "enn du deckt, den entflieh wkannstWir wur",
                    "wri uW ednercdteknl ,fite hwne d ukntnnsa",
                    "eddfkWneelaincninr k en dtehswu,n tu  twr"},{"A","A","A","A"}
                     };

            

           System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            


            Transposition.Transposition trans = new Transposition.Transposition();
            for (int i = 0; i < 2; i++)
            {
                trans.Initialize();
                trans.Input = enc.GetBytes(source[i]);
                trans.Keyword = source2[i];

                trans.changeSettings("ReadIn", 0);
                trans.changeSettings("Permute", 1);
                trans.changeSettings("ReadOut", 1);
                trans.Execute();

                String b = enc.GetString(trans.Output);
                Assert.AreEqual(result1[i, 0], b);

                trans.changeSettings("ReadIn", 1);
                trans.changeSettings("Permute", 1);
                trans.changeSettings("ReadOut", 1);
                trans.Execute();

                b = enc.GetString(trans.Output);
                Assert.AreEqual(result1[i, 1], b);

                trans.changeSettings("ReadIn", 1);
                trans.changeSettings("Permute", 0);
                trans.changeSettings("ReadOut", 1);
                trans.Execute();

                b = enc.GetString(trans.Output);
                Assert.AreEqual(result1[i, 2], b);

                trans.changeSettings("ReadIn", 0);
                trans.changeSettings("Permute", 0);
                trans.changeSettings("ReadOut", 1);
                trans.Execute();

                b = enc.GetString(trans.Output);
                Assert.AreEqual(result1[i, 3], b);
            }
            
        }

    }
}
