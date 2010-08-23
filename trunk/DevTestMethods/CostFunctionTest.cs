using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cryptool.Plugins.CostFunction;
namespace Tests
{
    [TestClass]
    public class CostFunctionTest
    {
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
        public void CostFunctionTests()
        {
            string input = "In der Kryptographie ist die Transposition ein Verschluesselungsverfahren, bei dem die Zeichen einer Botschaft (des Klartextes) umsortiert werden. Jedes Zeichen bleibt unveraendert erhalten, jedoch wird die Stelle, an der es steht, geaendert. Dies steht im Gegensatz zu den Verfahren der (monoalphabetischen oder polyalphabetischen) Substitution, bei denen jedes Zeichen des Klartextes seinen Platz behaelt, jedoch durch ein anderes Zeichen ersetzt (substituiert) wird.";
            double output = 7.38051470588235;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding(); // String to Byte Conversion
            CostFunction cf = new CostFunction();

            cf.Initialize();

            cf.InputText = enc.GetBytes(input);
            
            testContextInstance.WriteLine(enc.GetString(cf.InputText));
            cf.changeFunctionType(0);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(output, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo


            output = 4.25374598691653;
            

            cf.Initialize();

            cf.InputText = enc.GetBytes(input);

            testContextInstance.WriteLine(enc.GetString(cf.InputText));
            cf.changeFunctionType(1);
            cf.PreExecution(); // MUST BE CALLED!
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(output, cf.Value, 0.00000000000001); // Uhm. Close enough? Oo

        }

       
    }
}
