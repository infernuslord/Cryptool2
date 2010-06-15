using System.Collections.Generic;
using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Nodes;

namespace KeySearcher.P2P
{
    class KeyPoolTree
    {
        private readonly KeyPatternPool _patternPool;
        private readonly KeySearcherSettings _settings;
        private readonly P2PHelper _p2PHelper;
        private readonly NodeBase _rootNode;
        private readonly bool _calculationFinishedOnStart;
        private NodeBase _currentNode;
        private Leaf _currentLeaf;
        private bool _skippedReservedNodes;
        private bool _useReservedNodes;
        private BigInteger _lastPatternId;

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcherSettings settings, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper)
        {
            _patternPool = patternPool;
            _settings = settings;

            _p2PHelper = new P2PHelper(keySearcher);
            _skippedReservedNodes = false;
            _lastPatternId = -1;

            _rootNode = NodeFactory.CreateNode(_p2PHelper, keyQualityHelper, null, 0, _patternPool.Length - 1, _settings.DistributedJobIdentifier);

            AdvanceToFirstLeaf();

            _calculationFinishedOnStart = _rootNode.IsCalculated();
        }

        private void AdvanceToFirstLeaf()
        {
            _currentNode = _rootNode.CalculatableNode(false);
        }

        public bool LocateNextPattern()
        {
            _useReservedNodes = false;
            _skippedReservedNodes = false;
            var patternLocated = LocateNextPattern(false);

            if (!patternLocated && _skippedReservedNodes)
            {
                patternLocated = LocateNextPattern(true);
                _useReservedNodes = patternLocated;
            }

            return patternLocated;
        }

        private bool LocateNextPattern(bool includeReserved)
        {
            if (_calculationFinishedOnStart)
                return false;

            if (_rootNode is Leaf)
            {
                _currentLeaf = (Leaf) _rootNode;
                return _rootNode.IsCalculated();
            }

            var nextNode = _currentNode.ParentNode;
            while (nextNode != null)
            {
                _p2PHelper.UpdateFromDht(nextNode);
                if (nextNode.IsCalculated())
                {
                    nextNode = nextNode.ParentNode;
                    continue;
                }

                // Use next (independant of its reserved state), or use when not reserved
                if (includeReserved || !nextNode.IsReserverd())
                {
                    break;
                }
                
                _skippedReservedNodes = true;
                nextNode = nextNode.ParentNode;
            }

            if (nextNode == null)
            {
                return false;
            }

            _currentNode = nextNode.CalculatableNode(_useReservedNodes);

            if (((Leaf)_currentNode).PatternId() == _lastPatternId)
            {
                AdvanceToFirstLeaf();
            }

            _currentLeaf = (Leaf)_currentNode;
            _lastPatternId = CurrentPatternId();
            return true;
        }

        public KeyPattern CurrentPattern()
        {
            if (!_currentLeaf.ReserveLeaf())
            {
                throw new LeafReservedException();
            }

            return _patternPool[CurrentPatternId()];
        }

        public void ProcessCurrentPatternCalculationResult(LinkedList<KeySearcher.ValueKey> result)
        {
            _currentLeaf.HandleResults(result);
        }

        public BigInteger CurrentPatternId()
        {
            return _currentLeaf.PatternId();
        }
    }
}
