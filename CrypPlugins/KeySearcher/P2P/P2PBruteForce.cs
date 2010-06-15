using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;

namespace KeySearcher.P2P
{
    class P2PBruteForce
    {
        private readonly KeySearcher _keySearcher;
        private readonly KeyPattern _keyPattern;
        private readonly KeySearcherSettings _settings;
        private readonly KeyPatternPool _patternPool;
        private readonly KeyPoolTree _keyPoolTree;

        public P2PBruteForce(KeySearcher keySearcher, KeyPattern keyPattern, KeySearcherSettings settings, KeyQualityHelper keyQualityHelper)
        {
            _keySearcher = keySearcher;
            _keyPattern = keyPattern;
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
                " keys.", NotificationLevel.Debug);

            while (!_keySearcher.stop && _keyPoolTree.LocateNextPattern())
            {
                _keySearcher.GuiLogMessage(
                    "Running pattern #" + (_keyPoolTree.CurrentPatternId() + 1) + " of " + _patternPool.Length,
                    NotificationLevel.Info);

                try
                {
                    var result = _keySearcher.BruteForceWithLocalSystem(_keyPoolTree.CurrentPattern());

                    if (!_keySearcher.stop)
                    {
                        _keyPoolTree.ProcessCurrentPatternCalculationResult(result);
                    }
                    else
                    {
                        _keySearcher.GuiLogMessage("Brute force was stopped, not saving results...", NotificationLevel.Info);
                    }

                    _keySearcher.GuiLogMessage(
                        string.Format("Best match: {0} with {1}", result.First.Value.key, result.First.Value.value),
                        NotificationLevel.Info);                
                } catch (LeafReservedException e)
                {
                    _keySearcher.GuiLogMessage("Pattern was reserved before it could be reserved for this CrypTool instance.", NotificationLevel.Warning);
                    continue;
                }
            }

            // Set progress to 100%
            if (!_keySearcher.stop && !_keyPoolTree.LocateNextPattern())
            {
                _keySearcher.showProgress(_keySearcher.costList, 1, 1, 1);
            }

            if (!_keySearcher.stop)
                _keySearcher.GuiLogMessage("Calculation complete or no more free nodes found.", NotificationLevel.Info);
        }
    }
}
