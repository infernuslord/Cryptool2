using System;
using System.Collections.Generic;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    internal class KeyPoolTree
    {
        public readonly string Identifier;
        
        private readonly KeySearcher keySearcher;
        private readonly StatusContainer statusContainer;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly KeyPatternPool patternPool;
        private readonly NodeBase rootNode;
        private readonly StorageHelper storageHelper;
        private readonly StatusUpdater statusUpdater;
        private readonly int updateIntervalMod;

        private NodeBase currentNode;
        private bool skippedReservedNodes;

        private enum SearchOption { UseReservedLeafs, SkipReservedLeafs }

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper, StorageKeyGenerator identifierGenerator, StatusContainer statusContainer, StatisticsGenerator statisticsGenerator)
        {
            this.patternPool = patternPool;
            this.keySearcher = keySearcher;
            this.statusContainer = statusContainer;
            this.statisticsGenerator = statisticsGenerator;
            Identifier = identifierGenerator.Generate();

            storageHelper = new StorageHelper(keySearcher, statisticsGenerator, statusContainer);
            statusUpdater = new StatusUpdater(statusContainer, identifierGenerator.GenerateStatusKey());
            skippedReservedNodes = false;
            updateIntervalMod = 5;

            statisticsGenerator.MarkStartOfNodeSearch();
            rootNode = NodeFactory.CreateNode(storageHelper, keyQualityHelper, null, 0, this.patternPool.Length - 1,
                                              Identifier);
            statisticsGenerator.MarkEndOfNodeSearch();

            currentNode = rootNode;
        }

        public DateTime StartDate()
        {
            return storageHelper.StartDate(Identifier);
        }

        public Leaf FindNextLeaf()
        {
            // REMOVEME uncommenting the next line will cause a search for the next free pattern starting from the root node - for every leaf!
            //Reset();

            statusContainer.IsSearchingForReservedNodes = false;
            statisticsGenerator.MarkStartOfNodeSearch();

            var nodeBeforeStarting = currentNode;
            var foundNode = FindNextLeaf(SearchOption.SkipReservedLeafs);

            if (foundNode == null && skippedReservedNodes)
            {
                keySearcher.GuiLogMessage("Searching again with reserved nodes enabled...", NotificationLevel.Info);

                currentNode = nodeBeforeStarting;
                statusContainer.IsSearchingForReservedNodes = true;
                foundNode = FindNextLeaf(SearchOption.UseReservedLeafs);
                currentNode = foundNode;

                statisticsGenerator.MarkEndOfNodeSearch();
                return foundNode;
            }

            currentNode = foundNode;

            statisticsGenerator.MarkEndOfNodeSearch();
            return foundNode;
        }

        private Leaf FindNextLeaf(SearchOption useReservedLeafsOption)
        {
            if (currentNode == null)
                return null;

            var isReserved = false;
            var useReservedLeafs = useReservedLeafsOption == SearchOption.UseReservedLeafs;

            storageHelper.UpdateFromDht(currentNode, true);
            currentNode.UpdateCache();
            while (currentNode.IsCalculated() || (!useReservedLeafs && (isReserved = currentNode.IsReserved())))
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

        public void UpdateStatusForNewCalculation()
        {
            statusUpdater.SendUpdate(DistributedJobStatus.Status.New);
        }

        public void UpdateStatusForFinishedCalculation()
        {
            statusUpdater.SendUpdate(DistributedJobStatus.Status.Finished);
        }

        public void UpdateStatus(Leaf currentLeaf)
        {
            var isHigherPatternThanBefore = (currentLeaf.PatternId() + 1) >= statisticsGenerator.HighestChunkCalculated;
            var isLastPattern = currentLeaf.PatternId() == statisticsGenerator.TotalAmountOfChunks - 1;
            var patternIdQualifiesForUpdate = currentLeaf.PatternId() % updateIntervalMod == 0;

            if ((!isHigherPatternThanBefore || !patternIdQualifiesForUpdate) && !isLastPattern) return;
            statusUpdater.SendUpdate();
            keySearcher.GuiLogMessage("Updating status in DHT", NotificationLevel.Info);
        }
    }
}