using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P;
using Cryptool.Plugins.CostFunction;
using KeySearcher.Helper;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeySearcher.P2P.UnitTests
{
    [TestClass]
    public class TreeTest
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

        private void Init()
        {
            KSP2PManager.wrapper = new RandomP2PWrapper();
            P2PSettings.Default.PeerName = "test";
        }

        [TestMethod]
        public void TestTreeRandomly()
        {
            Init();
            var cf = new CostFunction();
            var cfc = new CostFunctionControl(cf);
            
            var keyPoolTree = new KeyPoolTree(256, null, new KeyQualityHelper(cfc), new StorageKeyGeneratorDummy(null, null),
                                    new StatusContainer(null), null);
        }
    }
}
