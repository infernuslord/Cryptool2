using System.Collections.Generic;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    internal class KeyPoolTree
    {
        private readonly KeySearcher keySearcher;
        private readonly StatusContainer statusContainer;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly KeyPatternPool patternPool;
        private readonly NodeBase rootNode;
        private readonly StorageHelper storageHelper;

        private NodeBase currentNode;
        private bool skippedReservedNodes;

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper, StorageKeyGenerator identifierGenerator, StatusContainer statusContainer, StatisticsGenerator statisticsGenerator)
        {
            this.patternPool = patternPool;
            this.keySearcher = keySearcher;
            this.statusContainer = statusContainer;
            this.statisticsGenerator = statisticsGenerator;

            storageHelper = new StorageHelper(keySearcher, statisticsGenerator, statusContainer);
            skippedReservedNodes = false;

            rootNode = NodeFactory.CreateNode(storageHelper, keyQualityHelper, null, 0, this.patternPool.Length - 1,
                                              identifierGenerator.Generate());
            currentNode = rootNode;
        }

        public Leaf FindNextLeaf()
        {
            statusContainer.IsSearchingForReservedNodes = false;
            statisticsGenerator.MarkStartOfNodeSearch();

            var nodeBeforeStarting = currentNode;
            var foundNode = FindNextLeaf(false);

            if (foundNode == null && skippedReservedNodes)
            {
                keySearcher.GuiLogMessage("Searching again with reserved nodes enabled...", NotificationLevel.Info);

                currentNode = nodeBeforeStarting;
                statusContainer.IsSearchingForReservedNodes = true;
                foundNode = FindNextLeaf(true);
                currentNode = foundNode;

                statisticsGenerator.MarkEndOfNodeSearch();
                return foundNode;
            }

            currentNode = foundNode;

            statisticsGenerator.MarkEndOfNodeSearch();
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
            currentNode.UpdateCache();
            while (currentNode.IsCalculated() || (!useReservedLeafs && (isReserved = currentNode.IsReserverd())))
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
                currentNode.UpdateCache();
            }

            // currentNode is calculateable => find leaf
            currentNode.UpdateCache();
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