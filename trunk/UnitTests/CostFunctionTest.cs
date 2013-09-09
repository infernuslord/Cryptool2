using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
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
            double epsilon = 0.000001;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding(); // String to Byte Conversion
            CostFunction cf = new CostFunction();
            
            //Index of Conincidence
            double target = 0.0738051470588235;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            testContextInstance.WriteLine(enc.GetString(cf.InputText));
            cf.changeFunctionType(0);
            cf.PreExecution(); // important, wont work without this
            cf.Execute();

            Assert.AreEqual(target, cf.Value, epsilon); // This _is_ close enough. => Floating point arithmetic!

            //Entropy
            target = 4.25374598691653;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(1);
            cf.PreExecution(); 
            cf.Execute();

            Assert.AreEqual(target, cf.Value, epsilon); 
            
            //Bigrams: log 2
            target = 265.318365029242;
            string path = Path.Combine(Environment.CurrentDirectory, "CrypPlugins\\Data\\StatisticsCorpusDE"); // TODO: not platform specific (x64/x86)            
            this.testContextInstance.WriteLine(path);
            cf.setDataPath(path);
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(2);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //Bigrams: Sinkov
            target = -548.360297827531;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(3);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //Bigrams: Percentaged
            target = 0.20132645541636;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(4);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //RegEx - Match
            target = 1.0;
            cf.Initialize();
            cf.InputText = enc.GetBytes("In der Kryptographie 1234567890");
            cf.changeFunctionType(5);
            cf.setRegEx("[a-zA-Z0-9 ]*");
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //RegEx - Not a Match
            target = -256.0;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(5);
            cf.setRegEx("[0-9]"); // String = Number?
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon);

            //Weighted Bigrams/Trigrams
            target = -777.230685764826;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(6);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 
        }

       
    }
}
