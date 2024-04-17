using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MemreDb.Memre.Indices;
using MemreDb.Memre.Ancillary;
using MemreDb.Memre.Indices.IndexOperations;
using MemreDb.Memre.Indices.Iterators;

namespace MemreDb.Memre
{
    internal class TreeIndex : ITreeIndex, ITreeIndexInternals
    {
        internal PropertyInfo KeyMemberInfo { get; set; } = null;
        internal bool IsUnique
        {
            get
            {
                return this._unique;
            }
        }

        TreeNode _root;
        bool _unique;
        internal TreeIndex(bool unique)
        {
            this._root = null;
            this._unique = unique;
        }

        private TreeIndex(TreeIndex rhs)
        {
            this._root = null;
            this._unique = rhs._unique;
            KeyMemberInfo = rhs.KeyMemberInfo;
        }

        #region ITreeIndex implementation
        public bool Empty
        {
            get
            {
                return this._root == null;
            }
        }

        public Object GetAnyValue()
        {
            if (this._root.Values.Count==0)
            {
                return null;
            }
            return this._root.Values[0];
        }

        public void Update(IComparable indexValue, object value)
        {
            List<object> values = new List<Object> { value };
            this._root = NodeOperations.Insert(this._unique, this._root, indexValue, values, true, false);
        }

        public void Delete(IComparable indexValue)
        {
            this._root = NodeOperations.DeleteNode(this._root, indexValue);
        }

        public void Delete(IComparable indexValue, Object valueToDelete)
        {
            this._root = NodeOperations.DeleteNode(this._root, indexValue, valueToDelete);
        }

        public Object GetValueSingular(IComparable indexValue)
        {
            if (this._root == null)
            {
                return null;
            }
            IComparable indexBy = ComparableHelper.CastToCompatibleType(this._root.IndexValue, indexValue);
            TreeNode foundNode = SearchForExactNode(indexBy);
            if (foundNode != null)
            {
                if (foundNode.Values.Count > 0)
                {
                    return foundNode.Values[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<Object> GetValuesEqualsAsList(IComparable indexValue)
        {
            TreeNode foundNode = SearchForExactNode(indexValue);
            if (foundNode != null)
            {
                return foundNode.Values;
            }
            else
            {
                return null;
            }
        }

        public void WriteTreeToConsole()
        {
            TreeUtils.WriteTreeToConsole(this._root);
        }

        public void WriteTreeAsOrderedListToConsole()
        {
            TreeUtils.WriteTreeAsOrderedListToConsole(this._root);
        }

        public void WriteTreeAsOrderedListToConsoleUsingIterators(bool ascending)
        {
            ITreeNodeIterator iterator;
            if (ascending)
            {
                iterator = GetIterator();
            }
            else
            {
                iterator = GetReverseIterator();
            }
            TreeUtils.WriteTreeAsOrderedListToConsole(iterator);
        }
        #endregion

        #region ITreeIndexInternals implementation
        public ITreeNodeIterator GetIterator()
        {
            return new TreeNodeIterator(this._root);
        }

        public ITreeNodeIterator GetReverseIterator()
        {
            return new TreeNodeReverseIterator(this._root);
        }

        public ITreeIndexIterator GetTreeIndexIterator()
        {
            return new TreeIndexIterator(this._root);
        }

        public ITreeIndexIterator GetReverseTreeIndexIterator()
        {
            return new TreeIndexReverseIterator(this._root);
        }

        public ITreeIndexInternals SetOperation(ITreeIndexInternals otherTree, SetOperation operation)
        {
            switch (operation)
            {
                case Memre.SetOperation.Or:
                    return SetOperations.MergeTreeTakingAllValues(this, GetIterator(), otherTree.GetIterator());
                case Memre.SetOperation.And:
                    return SetOperations.MergeTreeTakingCommonValues(this, GetIterator(), otherTree.GetIterator());
            }
            return null;
        }

        ITreeIndexInternals ITreeIndexInternals.ConstructTreeIndexFromTreeIndex()
        {
            ITreeIndexInternals toReturn = new TreeIndex(this);

            return toReturn;
        }

        public void Insert(IComparable indexValue, object value)
        {
            List<object> values = new List<Object> { value };
            this._root = NodeOperations.Insert(this._unique, this._root, indexValue, values, false, true);
        }

        public void Insert(IComparable indexValue, List<object> values)
        {
            this._root = NodeOperations.Insert(this._unique, this._root, indexValue, values, false, true);
        }

        public void InsertFromTreeNode(TreeNode insertNode)
        {
            this._root = NodeOperations.Insert(this._unique, this._root, insertNode.IndexValue, insertNode.Values, false, true); ;
        }

        public void Sort(List<OrderBy> orderByCollection)
        {
            SortOperations.SortNodeValues(orderByCollection, GetIterator());
        }

        public void AddFromOtherTreeAndSort(TreeIndex addFromTree, List<OrderBy> orderByCollection)
        {
            OrderBy primeOrder = orderByCollection.First();
            KeyMemberInfo = primeOrder.MemberAccessor;

            // First copy data from other tree, to this, collating into nodes values with equal key
            NodeOperations.AddFromOtherTree(this, addFromTree, KeyMemberInfo);

            // Next sort by any other orderings
            if (orderByCollection.Count>1)
            {
                SortOperations.SortNodeValues(orderByCollection, GetIterator());
            }
        }

        public ITreeIndexInternals GetValuesEquals(IComparable indexValue)
        {
            IComparable indexBy = ComparableHelper.CastToCompatibleType(this._root.IndexValue, indexValue);

            TreeIndex toReturn = new TreeIndex(this);
            TreeNode foundNode = SearchForExactNode(indexBy);
            if (foundNode != null)
            {
                toReturn.InsertFromTreeNode(foundNode);
            }
            return toReturn;
        }

        public ITreeIndexInternals GetValuesLessThan(IComparable compareAgainst, IncludeEquals includeEquals)
        {
            int comparisonPoint = includeEquals == IncludeEquals.Include ? 1 : 0;
            TreeIndex toReturn = new TreeIndex(this);
            ITreeNodeIterator iterator = GetIterator();
            RangeOperations.GetValuesLessThan(compareAgainst, iterator, toReturn, comparisonPoint);

            return toReturn;
        }

        public ITreeIndexInternals GetValuesGreaterThan(IComparable compareAgainst, IncludeEquals includeEquals)
        {
            int comparisonPoint = includeEquals == IncludeEquals.Include ? -1 : 0;
            TreeIndex toReturn = new TreeIndex(this);
            ITreeNodeIterator iterator = GetReverseIterator();
            RangeOperations.GetValuesGreaterThan(compareAgainst, iterator, toReturn, comparisonPoint);

            return toReturn;
        }

        public ITreeIndexInternals GetValuesNotEquals(IComparable compareAgainst)
        {
            TreeIndex toReturn = new TreeIndex(this);
            ITreeNodeIterator iterator = GetIterator();
            RangeOperations.GetValuesNotEqual(compareAgainst, iterator, toReturn);

            return toReturn;
        }
        #endregion

        TreeNode SearchForExactNode(IComparable indexValue)
        {
            bool found = false;
            TreeNode parentNode = null;
            TreeNode searchFrom = this._root;
            while (!found)
            {
                if (searchFrom == null)
                {
                    return null;
                }
                int comparison = indexValue.CompareTo(searchFrom.IndexValue);
                if (comparison == 0)
                {
                    found = true;
                }
                else if (comparison < 0)
                {
                    parentNode = searchFrom;
                    searchFrom = searchFrom.LeftNode;
                }
                else
                {
                    parentNode = searchFrom;
                    searchFrom = searchFrom.RightNode;
                }
            }
            return searchFrom;
        }
    }
}
