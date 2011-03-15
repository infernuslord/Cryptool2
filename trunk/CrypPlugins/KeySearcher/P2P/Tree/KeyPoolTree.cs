using System;
using System.Collections.Generic;
using System.Numerics;
using Cryptool.P2PEditor.Distributed;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.Properties;

namespace KeySearcher.P2P.Tree
{
    internal class KeyPoolTree
    {
        public readonly string Identifier;
        
        private readonly KeySearcher keySearcher;
        private readonly StatusContainer statusContainer;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly NodeBase rootNode;
        private readonly StorageHelper storageHelper;
        private readonly StatusUpdater statusUpdater;
        private readonly int updateIntervalMod;

        private NodeBase currentNode;
        private bool skippedReservedNodes;

        private enum SearchOption { UseReservedLeafs, SkipReservedLeafs }

        public NodeBase RootNode
        {
            get { return rootNode; }
        }

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper, StorageKeyGenerator identifierGenerator, StatusContainer statusContainer, StatisticsGenerator statisticsGenerator)
            : this(patternPool.Length, keySearcher, keyQualityHelper, identifierGenerator, statusContainer, statisticsGenerator)
        {
        }

        public KeyPoolTree(BigInteger length, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper, StorageKeyGenerator identifierGenerator, StatusContainer statusContainer, StatisticsGenerator statisticsGenerator)
        {
            this.keySearcher = keySearcher;
            this.statusContainer = statusContainer;
            this.statisticsGenerator = statisticsGenerator;
            Identifier = identifierGenerator.Generate();

            storageHelper = new StorageHelper(keySearcher, statisticsGenerator, statusContainer);
            statusUpdater = new StatusUpdater(statusContainer, identifierGenerator.GenerateStatusKey());
            skippedReservedNodes = false;
            updateIntervalMod = 5;

            if (statisticsGenerator != null)
                statisticsGenerator.MarkStartOfNodeSearch();
            rootNode = NodeFactory.CreateNode(storageHelper, keyQualityHelper, null, 0, length-1,
                                              Identifier);
            if (statisticsGenerator != null)
                statisticsGenerator.MarkEndOfNodeSearch();

            currentNode = rootNode;
        }

        public DateTime StartDate()
        {
            return storageHelper.StartDate(Identifier);
        }

        //---------------------------------------------------
        public long SubmitterID()
        {
            return storageHelper.SubmitterID(Identifier);
        }
        //----------------------------------------------------

        public Leaf FindNextLeaf()
        {
            // REMOVEME uncommenting the next line will cause a search for the next free pattern starting from the root node - for every leaf!
            //Reset();

            statusContainer.IsSearchingForReservedNodes = false;
            if (statisticsGenerator != null)
                statisticsGenerator.MarkStartOfNodeSearch();

            var nodeBeforeStarting = currentNode;
            var foundNode = FindNextLeaf(SearchOption.SkipReservedLeafs);
            
            if (foundNode == null && skippedReservedNodes)
            {
                if (keySearcher != null)
                    keySearcher.GuiLogMessage("Searching again with reserved nodes enabled...", NotificationLevel.Info);

                currentNode = nodeBeforeStarting;
                statusContainer.IsSearchingForReservedNodes = true;
                foundNode = FindNextLeaf(SearchOption.UseReservedLeafs);
                currentNode = foundNode;

                if (statisticsGenerator != null)
                    statisticsGenerator.MarkEndOfNodeSearch();
                return foundNode;
            }

            currentNode = foundNode;

            if (statisticsGenerator != null)
                statisticsGenerator.MarkEndOfNodeSearch();
            return foundNode;
        }

        private Leaf FindNextLeaf(SearchOption useReservedLeafsOption)
        {
            try
            {
                if (currentNode == null)
                {
                    return null;
                }

                var isReserved = false;
                var useReservedLeafs = useReservedLeafsOption == SearchOption.UseReservedLeafs;

                storageHelper.UpdateFromDht(currentNode, true);
                currentNode.UpdateCache();
                while (currentNode.IsCalculated() || (!useReservedLeafs && (isReserved = currentNode.IsReserved())))
                {
                    if (isReserved)
                    {
                        skippedReservedNodes = true;
                    }

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
            catch (KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }
        }


        internal bool IsCalculationFinished()
        {
            try
            {
                storageHelper.UpdateFromDht(rootNode, true);
                return rootNode.IsCalculated();
            }
            catch (KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }
        }

        internal void Reset()
        {
            ((Node)rootNode).ClearChildsLocal();
            currentNode = rootNode;
            skippedReservedNodes = false;
        }

        public static void ProcessCurrentPatternCalculationResult(Leaf currentLeaf,
                                                                  LinkedList<KeySearcher.ValueKey> result, Int64 id, String hostname)
        {
            currentLeaf.HandleResults(result, id, hostname);
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
            keySearcher.GuiLogMessage(Resources.Updating_status_in_DHT, NotificationLevel.Info);
        }
    }
}