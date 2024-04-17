using MemreDb.Memre.Indices.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices.IndexOperations
{
    internal class SetOperations
    {
        static internal ITreeIndexInternals MergeTreeTakingAllValues(ITreeIndexInternals originatingIndex, ITreeNodeIterator lhsIterator, ITreeNodeIterator rhsIterator)
        {
            ITreeIndexInternals toReturn = originatingIndex.ConstructTreeIndexFromTreeIndex();
            while (lhsIterator.Node != null || rhsIterator.Node != null)
            {
                // If there are only left or right nodes to merge, just copy them
                if (rhsIterator.Node == null)
                {
                    CopyAllValuesToTree(toReturn, ref lhsIterator);
                }
                else if (lhsIterator.Node == null)
                {
                    CopyAllValuesToTree(toReturn, ref rhsIterator);
                }
                else
                {
                    // Whichever iterator has a lower value, copy from that up to the current value of the other iterator
                    int comparison = lhsIterator.Node.IndexValue.CompareTo(rhsIterator.Node.IndexValue);
                    if (comparison <= 0)
                    {
                        CopyRangeOfValuesToTree(toReturn, ref lhsIterator, ref rhsIterator);
                    }
                    else
                    {
                        CopyRangeOfValuesToTree(toReturn, ref rhsIterator, ref lhsIterator);
                    }
                }
            }
            return toReturn;
        }

        static internal ITreeIndexInternals MergeTreeTakingCommonValues(ITreeIndexInternals originatingIndex, ITreeNodeIterator lhsIterator, ITreeNodeIterator rhsIterator)
        {
            ITreeIndexInternals toReturn = originatingIndex.ConstructTreeIndexFromTreeIndex();
            while (lhsIterator.Node != null && rhsIterator.Node != null)
            {
                int comparison = lhsIterator.Node.IndexValue.CompareTo(rhsIterator.Node.IndexValue);
                if (comparison < 0)
                {
                    if (IterateToIteratorExactly(ref lhsIterator, ref rhsIterator))
                    {
                        comparison = 0;
                    }
                }
                else if (comparison > 0)
                {
                    if (IterateToIteratorExactly(ref rhsIterator, ref lhsIterator))
                    {
                        comparison = 0;
                    }
                }
                while (comparison == 0)
                {
                    toReturn.InsertFromTreeNode(lhsIterator.Node);
                    TreeNode lhsNode = lhsIterator.MoveNext();
                    TreeNode rhsNode = rhsIterator.MoveNext();
                    if (lhsNode != null && rhsNode != null)
                    {
                        comparison = lhsIterator.Node.IndexValue.CompareTo(rhsIterator.Node.IndexValue);
                    }
                    else
                    {
                        // If reached the end of one iterator, there can be no more index values in common
                        break;
                    }
                }
            }
            return toReturn;
        }

        static private bool IterateToIteratorExactly(ref ITreeNodeIterator iterateIterator, ref ITreeNodeIterator iterateTo)
        {
            int comparison = iterateIterator.Node.IndexValue.CompareTo(iterateTo.Node.IndexValue);
            TreeNode node = iterateIterator.Node;

            while (comparison < 0)
            {
                node = iterateIterator.MoveNext();
                if (node == null)
                {
                    return false;
                }
                comparison = iterateIterator.Node.IndexValue.CompareTo(iterateTo.Node.IndexValue);
            }
            if (comparison == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static private void CopyAllValuesToTree(ITreeIndexInternals tree, ref ITreeNodeIterator iterator)
        {
            TreeNode node = iterator.Node;
            while (node != null)
            {
                tree.InsertFromTreeNode(node);
                node = iterator.MoveNext()!;
            }
        }

        static private void CopyRangeOfValuesToTree(ITreeIndexInternals tree, ref ITreeNodeIterator lhsIterator, ref ITreeNodeIterator rhsIterator)
        {
            TreeNode lhsNode = lhsIterator.Node;
            TreeNode rhsNode = rhsIterator.Node;
            IComparable rhsNodeIndexValue = rhsNode.IndexValue;
            int comparison = lhsIterator.Node.IndexValue.CompareTo(rhsNodeIndexValue);

            while (comparison < 0)
            {
                //tree.Insert(lhsNode!.IndexValue, lhsNode.Value!);
                tree.Insert(lhsNode.IndexValue, lhsNode.Values);
                lhsNode = lhsIterator!.MoveNext();
                if (lhsNode != null)
                {
                    comparison = lhsIterator.Node.IndexValue.CompareTo(rhsNodeIndexValue);
                }
                else
                {
                    comparison = 1;
                }
            }
            if (comparison == 0)
            {
                tree.Insert(lhsNode.IndexValue, lhsNode.Values);
                lhsNode = lhsIterator.MoveNext();
                rhsNode = rhsIterator.MoveNext();
            }
            else
            {
                tree.Insert(rhsNodeIndexValue, rhsNode.Values);
                rhsIterator!.MoveNext();
            }
        }
    }
}
