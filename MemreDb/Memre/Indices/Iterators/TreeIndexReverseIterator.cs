using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices.Iterators
{
    internal class TreeIndexReverseIterator : ITreeIndexIterator
    {
        ITreeNodeIterator _iterator;
        int _nodeLeafIndex;

        private bool _valid;
        public bool Valid
        {
            get
            {
                return this._valid;
            }
        }

        Object _currentValue;
        public Object CurrentValue
        {
            get
            {
                return this._currentValue;
            }
        }

        IComparable _currentIndex;
        public IComparable CurrentIndex
        {
            get
            {
                return this._currentIndex;
            }
        }

        public TreeIndexReverseIterator(TreeNode node)
        {
            this._iterator = new TreeNodeReverseIterator(node);
            this._nodeLeafIndex = GetMaxLeafIndexForNode(this._iterator.Node);
            this._valid = this._iterator.Node != null;
            SetValue();
        }

        public Object MoveNext()
        {
            if (this._valid)
            {
                bool moveToNextNode = true;
                moveToNextNode = !MoveToNextNodeLeaf();

                if (moveToNextNode)
                {
                    TreeNode node = this._iterator.MoveNext();
                    if (node==null)
                    {
                        this._valid = false;
                    }
                    else
                    {
                        this._nodeLeafIndex = GetMaxLeafIndexForNode(node);
                    }
                }
                if (this._valid)
                {
                    SetValue();
                }
            }

            return CurrentValue;
        }

        bool MoveToNextNodeLeaf()
        {
            if (this._nodeLeafIndex == 0)
            {
                return false;
            }
            else
            {
                --this._nodeLeafIndex;
                SetValue();
                return true;
            }
        }

        void SetValue()
        {
            TreeNode n = this._iterator.Node;
            this._currentIndex = n.IndexValue;
            this._currentValue = n.Values[this._nodeLeafIndex];
        }

        int GetMaxLeafIndexForNode(TreeNode node)
        {
            return node.Values.Count - 1;
        }
    }
}
