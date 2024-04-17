using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    internal interface ITreeIndexInternals
    {
        ITreeNodeIterator GetIterator();
        ITreeNodeIterator GetReverseIterator();
        ITreeIndexIterator GetTreeIndexIterator();
        ITreeIndexIterator GetReverseTreeIndexIterator();
        ITreeIndexInternals ConstructTreeIndexFromTreeIndex();
        void Insert(IComparable indexValue, object value);
        void Insert(IComparable indexValue, List<object> values);
        void InsertFromTreeNode(TreeNode insertNode);
        void Sort(List<OrderBy> orderByCollection);
        void AddFromOtherTreeAndSort(TreeIndex addFromTree, List<OrderBy> orderByCollection);
        ITreeIndexInternals SetOperation(ITreeIndexInternals otherTree, SetOperation operation);
        ITreeIndexInternals GetValuesEquals(IComparable indexValue);
        ITreeIndexInternals GetValuesNotEquals(IComparable compareAgainst);
        ITreeIndexInternals GetValuesLessThan(IComparable compareAgainst, IncludeEquals includeEquals);
        ITreeIndexInternals GetValuesGreaterThan(IComparable compareAgainst, IncludeEquals includeEquals);
    }
}
