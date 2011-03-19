using System;
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
        private static readonly string[] users = new[] {"Sven", "Nils", "Simone", "Dennis", "Arno", "Theo", "Christian", "Daniel", "Viktor", "Lorenz", "Christopher"};

        private Random random = new Random();
        private byte[] currentKeyArray;
        private List<KeyValuePair<string, double>> topList = new List<KeyValuePair<string, double>>();
        private KeyQualityHelper keyQualityHelper;

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

        private string RandomUser()
        {
            return users[random.Next(0, users.Length)];
        }

        private byte[] GetNextKeyArray()
        {
            for (int i = 0; i < currentKeyArray.Length; i++)
            {
                if (currentKeyArray[i] < 255)
                {
                    currentKeyArray[i]++;
                    break;
                }
                else
                {
                    currentKeyArray[i] = 0;
                }
            }
            return (byte[])currentKeyArray.Clone();
        }

        private void Init()
        {
            KSP2PManager.wrapper = new RandomP2PWrapper();

            var cf = new CostFunction();
            cf.Initialize();
            cf.changeFunctionType(1);
            var cfc = cf.ControlSlave;
            keyQualityHelper = new KeyQualityHelper(cfc);
            currentKeyArray = new byte[32];
        }

        /// <summary>
        /// This unit test method can be used to test the distributed KeySearcher tree (e.g. the statistics and results).
        /// This is not a deterministic test, because it uses a simulated error prone p2p connection.
        /// This test can take some minutes, depending on your CPU.
        /// </summary>
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

                    var result = GetResults(leaf.From);

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
                bool contains = topList.Any(tl => tl.Value == r1.value && tl.Key == r1.user);
                Assert.IsTrue(contains);
            }
        }

        private Dictionary<BigInteger, LinkedList<KeySearcher.ValueKey>> nodeResultCache = new Dictionary<BigInteger, LinkedList<KeySearcher.ValueKey>>();
        private LinkedList<KeySearcher.ValueKey> GetResults(BigInteger nodeid)
        {
            if (nodeResultCache.ContainsKey(nodeid))
                return nodeResultCache[nodeid];

            var list = new LinkedList<KeySearcher.ValueKey>();
            for (int c = 0; c < 10; c++)
            {
                var user = RandomUser();
                var val = random.NextDouble()*100;

                PushSortedInLinkedList(list, new KeySearcher.ValueKey()
                                                 {
                                                     decryption = new byte[128],
                                                     key = "This string is not important for this unit test",
                                                     keya = GetNextKeyArray(),
                                                     maschid = 2,
                                                     maschname = "This string is not important for this unit test",
                                                     time = DateTime.UtcNow,
                                                     user = user,
                                                     value = val
                                                 });
                PushToToplist(user, val);
            }
            nodeResultCache.Add(nodeid, list);
            return list;
        }

        private void PushSortedInLinkedList(LinkedList<KeySearcher.ValueKey> list, KeySearcher.ValueKey valueKey)
        {
            var val = valueKey.value;

            var node = list.First;
            while (node != null)
            {
                if (keyQualityHelper.IsBetter(valueKey.value, node.Value.value))
                {
                    list.AddBefore(node, valueKey);
                    return;
                }
                node = node.Next;
            }//end while
            list.AddLast(valueKey);
        }

        private void PushToToplist(string user, double val)
        {
            var entry = new KeyValuePair<string, double>(user, val);
            try
            {
                foreach (var keyValuePair in topList)
                {
                    if (keyQualityHelper.IsBetter(val, keyValuePair.Value))
                    {
                        topList.Insert(topList.IndexOf(keyValuePair), entry);
                        return;
                    }
                }

                topList.Add(entry);
            }
            finally
            {
                if (topList.Count > 10)
                    topList.RemoveRange(10, topList.Count - 10);
            }
        }
    }
}
