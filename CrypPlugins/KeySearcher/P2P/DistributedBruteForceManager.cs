using System;
using System.Collections.Generic;
using System.Numerics;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;

namespace KeySearcher.P2P
{
    internal class DistributedBruteForceManager
    {
        private readonly StorageKeyGenerator keyGenerator;
        private readonly KeyPoolTree keyPoolTree;
        private readonly KeySearcher keySearcher;
        private readonly KeyPatternPool patternPool;

        public DistributedBruteForceManager(KeySearcher keySearcher, KeyPattern.KeyPattern keyPattern, KeySearcherSettings settings,
                                            KeyQualityHelper keyQualityHelper)
        {
            this.keySearcher = keySearcher;

            // TODO when setting is still default (21), it is only displayed as 21 - but the settings-instance contains 0 for that key!
            if (settings.ChunkSize == 0)
            {
                settings.ChunkSize = 21;
            }

            keyGenerator = new StorageKeyGenerator(keySearcher, settings);
            patternPool = new KeyPatternPool(keyPattern, new BigInteger(Math.Pow(2, settings.ChunkSize)));
            keyPoolTree = new KeyPoolTree(patternPool, this.keySearcher, keyQualityHelper, keyGenerator);

            keySearcher.GuiLogMessage(
                "Total amount of patterns: " + patternPool.Length + ", each containing " + patternPool.PartSize +
                " keys.", NotificationLevel.Info);
        }

        public void Execute()
        {
            Leaf currentLeaf;
            while (!keySearcher.stop)
            {
                try
                {
                    currentLeaf = keyPoolTree.FindNextLeaf();
                    if (currentLeaf == null)
                    {
                        break;
                    }
                }
                catch (AlreadyCalculatedException)
                {
                    keySearcher.GuiLogMessage("Node was already calculated.", NotificationLevel.Warning);
                    keyPoolTree.Reset();
                    continue;
                }

                if (!currentLeaf.ReserveLeaf())
                {
                    keySearcher.GuiLogMessage(
                        "Pattern #" + currentLeaf.PatternId() +
                        " was reserved before it could be reserved for this CrypTool instance.",
                        NotificationLevel.Warning);
                    keyPoolTree.Reset();
                    continue;
                }

                keySearcher.GuiLogMessage(
                    "Running pattern #" + (currentLeaf.PatternId() + 1) + " of " + patternPool.Length,
                    NotificationLevel.Info);

                try
                {
                    LinkedList<KeySearcher.ValueKey> result =
                        keySearcher.BruteForceWithLocalSystem(patternPool[currentLeaf.PatternId()]);

                    if (!keySearcher.stop)
                        KeyPoolTree.ProcessCurrentPatternCalculationResult(currentLeaf, result);
                    else
                        keySearcher.GuiLogMessage("Brute force was stopped, not saving results...",
                                                  NotificationLevel.Info);

                    keySearcher.GuiLogMessage(
                        string.Format("Best match: {0} with {1}", result.First.Value.key, result.First.Value.value),
                        NotificationLevel.Info);
                }
                catch (ReservationRemovedException)
                {
                    keySearcher.GuiLogMessage("Reservation removed by another node (while calculating). " +
                                              "To avoid a state in limbo, proceeding to first available leaf...",
                                              NotificationLevel.Warning);
                    keyPoolTree.Reset();
                    continue;
                }
                catch (UpdateFailedException e)
                {
                    keySearcher.GuiLogMessage("Could not store results: " + e.Message, NotificationLevel.Warning);
                    keyPoolTree.Reset();
                    continue;
                }
            }

            // Set progress to 100%
            if (!keySearcher.stop && keyPoolTree.IsCalculationFinished())
            {
                keySearcher.showProgress(keySearcher.costList, 1, 1, 1);
                keySearcher.GuiLogMessage("Calculation complete.", NotificationLevel.Info);
            }
        }
    }
}