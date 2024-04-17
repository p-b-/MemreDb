using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices.Iterators
{
    class TreeNodeIterator : ITreeNodeIterator
    {
        public TreeNode Node
        {
            get
            {
                return _currentNode;
            }
        }

        bool _valid;
        public bool Valid
        {
            get
            {
                return this._valid;
            }
        }

        TreeNode _currentNode;
        byte _currentNodeState;
        List<TreeNode> _nodePath;
        List<byte> _nodePathStates;

        internal TreeNodeIterator(TreeNode node)
        {
            _nodePath = new List<TreeNode>();
            _nodePathStates = new List<byte>();
            _currentNode = node;
            this._valid = this._currentNode != null;
            if (_currentNode != null)
            {
                MoveLeft();
            }
        }

        public TreeNode MoveNext()
        {
            if (!this._valid)
            {
                throw new Exception("Attempt to iterate on an invalid index interator");
            }
            // 0 means go right if possible, 1 means go up
            if (_currentNodeState == 0)
            {
                // Set state of current node to be 'go up'
                _currentNodeState = 1;
                if (Node.RightNode != null)
                {
                    _nodePathStates.Add(_currentNodeState);
                    _nodePath.Add(Node);
                    _currentNode = Node.RightNode;
                    // This sets _currentNodeState = 0
                    MoveLeft();
                }
                else
                {
                    // Search for a state 0 to go right from
                    if (!MoveUp())
                    {
                        this._valid = false;
                        _currentNode = null;
                        return null;
                    }
                }
            }

            return _currentNode;
        }

        internal TreeNode AdvanceToGreaterOrEqualValue(IComparable advanceToValue)
        {
            int comparison = Node.IndexValue.CompareTo(advanceToValue);
            while (comparison < 0)
            {
                MoveNext();
                comparison = Node.IndexValue.CompareTo(advanceToValue);
            }
            return Node;
        }

        bool MoveUp()
        {
            while (_nodePathStates.Count > 0)
            {
                _currentNodeState = _nodePathStates.Last();
                _nodePathStates.RemoveAt(_nodePathStates.Count - 1);
                _currentNode = _nodePath.Last();
                _nodePath.RemoveAt(_nodePath.Count - 1);

                if (_currentNodeState == 0)
                {
                    return true;
                }
            }
            return false;
        }

        void MoveLeft()
        {
            while (_currentNode.LeftNode != null)
            {
                _nodePath.Add(_currentNode);
                _nodePathStates.Add(0);

                _currentNode = _currentNode.LeftNode;
            }
            _currentNodeState = 0;
        }
    }
}
