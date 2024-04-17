using MemreDb.Memre.Indices.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices.IndexOperations
{
    static internal class RangeOperations
    {
        internal static void GetValuesNotEqual(IComparable compareAgainst, ITreeNodeIterator iterator, TreeIndex values)
        {
            while (iterator.Node != null)
            {
                int comparison = iterator.Node.IndexValue.CompareTo(compareAgainst);
                if (comparison != 0)
                {
                    values.InsertFromTreeNode(iterator.Node);
                }
                else
                {
                    return;
                }
                iterator.MoveNext();
            }
        }

        internal static void GetValuesLessThan(IComparable compareAgainst, ITreeNodeIterator iterator, TreeIndex values, int comparisonPoint)
        {
            while (iterator.Node != null)
            {
                int comparison = iterator.Node.IndexValue.CompareTo(compareAgainst);
                if (comparison < comparisonPoint)
                {
                    // Inserts either single or multiple values
                    values.InsertFromTreeNode(iterator.Node);
                }
                else
                {
                    return;
                }
                iterator.MoveNext();
            }
        }

        internal static void GetValuesGreaterThan(IComparable compareAgainst, ITreeNodeIterator iterator, TreeIndex values, int comparisonPoint)
        {
            while (iterator.Node != null)
            {
                int comparison = iterator.Node.IndexValue.CompareTo(compareAgainst);
                if (comparison > comparisonPoint)
                {
                    // Inserts either single or multiple values
                    values.InsertFromTreeNode(iterator.Node);
                }
                else
                {
                    return;
                }
                iterator.MoveNext();
            }
        }
    }
}
