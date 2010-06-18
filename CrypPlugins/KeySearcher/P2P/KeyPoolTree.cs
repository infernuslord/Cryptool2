using System.Collections.Generic;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.P2P.Nodes;

namespace KeySearcher.P2P
{
    class KeyPoolTree
    {
        private readonly KeyPatternPool _patternPool;
        private readonly KeySearcherSettings _settings;
        private readonly KeySearcher _keySearcher;
        private readonly P2PHelper _p2PHelper;
        private readonly NodeBase _rootNode;
        private NodeBase _currentNode;
        private bool _skippedReservedNodes;

        public KeyPoolTree(KeyPatternPool patternPool, KeySearcherSettings settings, KeySearcher keySearcher, KeyQualityHelper keyQualityHelper)
        {
            _patternPool = patternPool;
            _settings = settings;
            _keySearcher = keySearcher;

            _p2PHelper = new P2PHelper(keySearcher);
            _skippedReservedNodes = false;

            _rootNode = NodeFactory.CreateNode(_p2PHelper, keyQualityHelper, null, 0, _patternPool.Length - 1, _settings.DistributedJobIdentifier);
            _currentNode = _rootNode;
        }

        public Leaf FindNextLeaf()
        {
            var nodeBeforeStarting = _currentNode;
            var foundNode = FindNextLeaf(false);

            if (foundNode == null && _skippedReservedNodes)
            {
                _keySearcher.GuiLogMessage("Searching again with reserved nodes enabled...", NotificationLevel.Warning);

                _currentNode = nodeBeforeStarting;
                foundNode = FindNextLeaf(true);
                _currentNode = foundNode;
                return foundNode;
            }

            _currentNode = foundNode;
            return foundNode;
        }

        private Leaf FindNextLeaf(bool useReservedLeafs)
        {
            if (_currentNode == null)
            {
                return null;
            }

            var isReserved = false;
            _p2PHelper.UpdateFromDht(_currentNode, true);
            while (_currentNode.IsCalculated() || ((isReserved = _currentNode.IsReserverd()) && !useReservedLeafs))
            {
                if (isReserved)
                    _skippedReservedNodes = true;

                // Current node is calculated or reserved, 
                // move one node up and update it
                _currentNode = _currentNode.ParentNode;

                // Root node calculated => everything finished
                if (_currentNode == null)
                {
                    _currentNode = _rootNode;
                    return null;
                }

                // Update the new _currentNode
                _p2PHelper.UpdateFromDht(_currentNode, true);
            }

            // currentNode is calculateable => find leaf
            return _currentNode.CalculatableLeaf(useReservedLeafs);
        }


        internal bool IsCalculationFinished()
        {
            _p2PHelper.UpdateFromDht(_rootNode, true);
            return _rootNode.IsCalculated();
        }

        internal void Reset()
        {
            _rootNode.Reset();
            _currentNode = _rootNode;
            _skippedReservedNodes = false;
        }

        public void ProcessCurrentPatternCalculationResult(Leaf currentLeaf, LinkedList<KeySearcher.ValueKey> result)
        {
            currentLeaf.HandleResults(result);
        }
    }
}
