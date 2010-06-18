using System.Collections.Generic;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    internal class KeyPoolTree
    {
        private readonly KeySearcher keySearcher;
        private readonly KeyPatternPool patternPool;
        private readonly NodeBase rootNode;
        private readonly StorageHelper storageHelper;
        private NodeBase currentNode;
        private bool skippedReservedNodes;

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper,
                           StorageKeyGenerator identifierGenerator)
        {
            this.patternPool = patternPool;
            this.keySearcher = keySearcher;

            storageHelper = new StorageHelper(keySearcher);
            skippedReservedNodes = false;

            rootNode = NodeFactory.CreateNode(storageHelper, keyQualityHelper, null, 0, this.patternPool.Length - 1,
                                              identifierGenerator.Generate());
            currentNode = rootNode;
        }

        public Leaf FindNextLeaf()
        {
            var nodeBeforeStarting = currentNode;
            var foundNode = FindNextLeaf(false);

            if (foundNode == null && skippedReservedNodes)
            {
                keySearcher.GuiLogMessage("Searching again with reserved nodes enabled...", NotificationLevel.Warning);

                currentNode = nodeBeforeStarting;
                foundNode = FindNextLeaf(true);
                currentNode = foundNode;
                return foundNode;
            }

            currentNode = foundNode;
            return foundNode;
        }

        private Leaf FindNextLeaf(bool useReservedLeafs)
        {
            if (currentNode == null)
            {
                return null;
            }

            bool isReserved = false;
            storageHelper.UpdateFromDht(currentNode, true);
            while (currentNode.IsCalculated() || ((isReserved = currentNode.IsReserverd()) && !useReservedLeafs))
            {
                if (isReserved)
                    skippedReservedNodes = true;

                // Current node is calculated or reserved, 
                // move one node up and update it
                currentNode = currentNode.ParentNode;

                // Root node calculated => everything finished
                if (currentNode == null)
                {
                    currentNode = rootNode;
                    return null;
                }

                // Update the new _currentNode
                storageHelper.UpdateFromDht(currentNode, true);
            }

            // currentNode is calculateable => find leaf
            return currentNode.CalculatableLeaf(useReservedLeafs);
        }


        internal bool IsCalculationFinished()
        {
            storageHelper.UpdateFromDht(rootNode, true);
            return rootNode.IsCalculated();
        }

        internal void Reset()
        {
            rootNode.Reset();
            currentNode = rootNode;
            skippedReservedNodes = false;
        }

        public static void ProcessCurrentPatternCalculationResult(Leaf currentLeaf,
                                                                  LinkedList<KeySearcher.ValueKey> result)
        {
            currentLeaf.HandleResults(result);
        }
    }
}