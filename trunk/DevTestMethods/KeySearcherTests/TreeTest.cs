﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

        private Random random = new Random();

        private void Init()
        {
            KSP2PManager.wrapper = new RandomP2PWrapper();
            P2PSettings.Default.PeerName = "test";

            var cf = new CostFunction();
            cf.Initialize();
            cf.changeFunctionType(1);
            var cfc = cf.ControlSlave;
            keyQualityHelper = new KeyQualityHelper(cfc);
        }

        private List<KeyValuePair<string, double>> TopList = new List<KeyValuePair<string, double>>();
        private KeyQualityHelper keyQualityHelper;

        [TestMethod]
        public void TestTreeRandomly()
        {
            BigInteger testingLength = 256 * 64;
            Init();

            KeyPoolTree keyPoolTree = null;
            bool treeInitialized = false;
            do
            {
                try
                {
                    keyPoolTree = new KeyPoolTree(testingLength, null, keyQualityHelper, new StorageKeyGeneratorDummy(null, null),
                                                new StatusContainer(null), null);
                    treeInitialized = true;
                }
                catch (Exception)
                {
                }
            } while (!treeInitialized);

            String hostname = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetHostName();
            Int64 hostid = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();

            while (true)
            {
                try
                {
                    var leaf = keyPoolTree.FindNextLeaf();
                    if (leaf == null)
                        break;

                    if (!leaf.ReserveLeaf())
                    {
                        keyPoolTree.Reset();
                        continue;
                    }

                    var result = GetResults();

                    hostid = random.Next(0, 15555);
                    KeyPoolTree.ProcessCurrentPatternCalculationResult(leaf, result, hostid, hostname);
                    leaf.GiveLeafFree();
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine("Exception in loop: {0}", ex.Message);
                    keyPoolTree.Reset();
                }
            }

            bool isCalc = false;
            do
            {
                try
                {
                    var isFinished = keyPoolTree.IsCalculationFinished();
                    Assert.IsTrue(isFinished);
                    isCalc = true;
                }
                catch (Exception)
                {
                }
            } while (!isCalc);

            TestContext.WriteLine("Finished Calculating.. Validating!");

            //Finished, validate results:
            var rootNode = (Node)keyPoolTree.RootNode;
            
            //count entries in statistics:
            BigInteger count = rootNode.Activity.SelectMany(a => a.Value).Aggregate<KeyValuePair<long, Information>, BigInteger>(0, (current, b) => current + b.Value.Count);
            Assert.IsTrue(testingLength == count);

            //compare toplist:
            foreach (var r in rootNode.Result)
            {
                KeySearcher.ValueKey r1 = r;
                bool contains = TopList.Any(tl => tl.Value == r1.value && tl.Key == r1.user);
                Assert.IsTrue(contains);
            }

            TestContext.WriteLine("Everything fine :)");
        }

        private LinkedList<KeySearcher.ValueKey> GetResults()
        {
            var list = new LinkedList<KeySearcher.ValueKey>();
            for (int c = 0; c < 10; c++)
            {
                var val = random.NextDouble()*100;
                list.AddFirst(new KeySearcher.ValueKey()
                {
                    decryption = new byte[128],
                    key = "ABC",
                    keya = new byte[16],
                    maschid = 2,
                    maschname = "Bla", 
                    time = DateTime.UtcNow, user = "Sven", 
                    value = val});
                PushToToplist("Sven", val);
            }
            return list;
        }
        
        private void PushToToplist(string user, double val)
        {
            var entry = new KeyValuePair<string, double>(user, val);
            try
            {
                foreach (var keyValuePair in TopList)
                {
                    if (keyQualityHelper.IsBetter(val, keyValuePair.Value))
                    {
                        TopList.Insert(TopList.IndexOf(keyValuePair), entry);
                        return;
                    }
                }

                TopList.Add(entry);
            }
            finally
            {
                if (TopList.Count > 10)
                    TopList.RemoveRange(10, TopList.Count - 10);
            }
        }
    }
}
