using System;
using System.Collections.Generic;
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
            statisticsGenerator.MarkStartOfNodeSearch();

            var nodeBeforeStarting = currentNode;
            keySearcher.GuiLogMessage("Calling FindNextLeaf(SearchOption.SkipReservedLeafs) now!", NotificationLevel.Debug);
            var foundNode = FindNextLeaf(SearchOption.SkipReservedLeafs);
            keySearcher.GuiLogMessage("Returned from FindNextLeaf(SearchOption.SkipReservedLeafs)...", NotificationLevel.Debug);

            if (foundNode == null)
                keySearcher.GuiLogMessage("FindNextLeaf(SearchOption.SkipReservedLeafs) returned null!", NotificationLevel.Debug);

            if (skippedReservedNodes)
                keySearcher.GuiLogMessage("FindNextLeaf(SearchOption.SkipReservedLeafs) skipped reserved nodes!", NotificationLevel.Debug);

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
            try
            {
                if (currentNode == null)
                {
                    keySearcher.GuiLogMessage("Inside FindNextLeaf: currentNode is null!", NotificationLevel.Debug);
                    return null;
                }

                var isReserved = false;
                var useReservedLeafs = useReservedLeafsOption == SearchOption.UseReservedLeafs;

                keySearcher.GuiLogMessage("Inside FindNextLeaf: updating currentNode now!", NotificationLevel.Debug);
                storageHelper.UpdateFromDht(currentNode, true);
                currentNode.UpdateCache();
                keySearcher.GuiLogMessage("Inside FindNextLeaf: Now entering while loop!", NotificationLevel.Debug);
                while (currentNode.IsCalculated() || (!useReservedLeafs && (isReserved = currentNode.IsReserved())))
                {
                    if (isReserved)
                    {
                        keySearcher.GuiLogMessage("Inside FindNextLeaf: currentNode was reserved!", NotificationLevel.Debug);
                        skippedReservedNodes = true;
                    }
                    if (currentNode.IsCalculated())
                        keySearcher.GuiLogMessage("Inside FindNextLeaf: currentNode is already calculated!", NotificationLevel.Debug);

                    // Current node is calculated or reserved, 
                    // move one node up and update it
                    keySearcher.GuiLogMessage("Inside FindNextLeaf: set currentNode to its own parent!", NotificationLevel.Debug);
                    currentNode = currentNode.ParentNode;

                    // Root node calculated => everything finished
                    if (currentNode == null)
                    {
                        keySearcher.GuiLogMessage("Inside FindNextLeaf: parent was null, so set currentNode to rootNode!", NotificationLevel.Debug);
                        currentNode = rootNode;
                        return null;
                    }

                    keySearcher.GuiLogMessage("Inside FindNextLeaf: updating currentNode now!", NotificationLevel.Debug);
                    // Update the new _currentNode
                    storageHelper.UpdateFromDht(currentNode, true);
                    currentNode.UpdateCache();
                }

                keySearcher.GuiLogMessage("Inside FindNextLeaf: Exiting loop! Updating currentNode!", NotificationLevel.Debug);
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