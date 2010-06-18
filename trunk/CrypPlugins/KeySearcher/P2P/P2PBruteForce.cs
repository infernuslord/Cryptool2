using System;
using System.Numerics;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Nodes;

namespace KeySearcher.P2P
{
    class P2PBruteForce
    {
        private readonly KeySearcher _keySearcher;
        private readonly KeySearcherSettings _settings;
        private readonly KeyPatternPool _patternPool;
        private readonly KeyPoolTree _keyPoolTree;

        public P2PBruteForce(KeySearcher keySearcher, KeyPattern keyPattern, KeySearcherSettings settings, KeyQualityHelper keyQualityHelper)
        {
            _keySearcher = keySearcher;
            _settings = settings;

            // TODO when setting is still default (21), it is only displayed as 21 - but the settings-instance contains 0 for that key!
            if (settings.ChunkSize == 0)
            {
                settings.ChunkSize = 21;
            }

            _patternPool = new KeyPatternPool(keyPattern, new BigInteger(Math.Pow(2, settings.ChunkSize)));
            _keyPoolTree = new KeyPoolTree(_patternPool, _settings, _keySearcher, keyQualityHelper);

            _keySearcher.GuiLogMessage(
                "Total amount of patterns: " + _patternPool.Length + ", each containing " + _patternPool.PartSize +
                " keys.", NotificationLevel.Info);

            Leaf currentLeaf;
            while (!_keySearcher.stop)
            {
                try
                {
                    currentLeaf = _keyPoolTree.FindNextLeaf();
                    if (currentLeaf == null)
                    {
                        break;
                    }
                } catch(AlreadyCalculatedException)
                {
                    _keySearcher.GuiLogMessage("Node was already calculated.", NotificationLevel.Warning);
                    _keyPoolTree.Reset();
                    continue;
                }

                if (!currentLeaf.ReserveLeaf())
                {
                    _keySearcher.GuiLogMessage(
                        "Pattern #" + currentLeaf.PatternId() +
                        " was reserved before it could be reserved for this CrypTool instance.",
                        NotificationLevel.Warning);
                    _keyPoolTree.Reset();
                    continue;
                }

                _keySearcher.GuiLogMessage(
                    "Running pattern #" + (currentLeaf.PatternId() + 1) + " of " + _patternPool.Length,
                    NotificationLevel.Info);

                try
                {
                    var result = _keySearcher.BruteForceWithLocalSystem(_patternPool[currentLeaf.PatternId()]);

                    if (!_keySearcher.stop)
                    {
                        _keyPoolTree.ProcessCurrentPatternCalculationResult(currentLeaf, result);
                    }
                    else
                    {
                        _keySearcher.GuiLogMessage("Brute force was stopped, not saving results...", NotificationLevel.Info);
                    }

                    _keySearcher.GuiLogMessage(
                        string.Format("Best match: {0} with {1}", result.First.Value.key, result.First.Value.value),
                        NotificationLevel.Info);                
                }
                catch(ReservationRemovedException)
                {
                    _keySearcher.GuiLogMessage("Reservation removed by another node (while calculating). " +
                                               "To avoid a state in limbo, proceeding to first available leaf...",
                                               NotificationLevel.Warning);
                    _keyPoolTree.Reset();
                    continue;
                }
                catch (UpdateFailedException e)
                {
                    _keySearcher.GuiLogMessage("Could not store results: " + e.Message, NotificationLevel.Warning);
                    _keyPoolTree.Reset();
                    continue;
                }
            }

            // Set progress to 100%
            if (!_keySearcher.stop && _keyPoolTree.IsCalculationFinished())
            {
                _keySearcher.showProgress(_keySearcher.costList, 1, 1, 1);
                _keySearcher.GuiLogMessage("Calculation complete.", NotificationLevel.Info);
            }
        }
    }
}
