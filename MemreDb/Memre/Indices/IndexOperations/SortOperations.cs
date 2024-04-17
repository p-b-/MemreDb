using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre.Ancillary;

namespace MemreDb.Memre.Indices.IndexOperations
{
    static internal class SortOperations
    {
        static public void SortNodeValues(List<OrderBy> orderByCollection, ITreeNodeIterator iterator)
        {
            OrderBy primeOrder = orderByCollection.First();
            orderByCollection.RemoveAt(0);
            if (orderByCollection.Count == 0)
            {
                // No need to sort tree just by a single ordering - this will be taken care 
                //  of by the choice of iterator
                return;
            }
            TreeNode node = iterator.Node;
            while (node != null)
            {
                if (node.Values.Count > 1)
                {
                    // Sort values
                    List<Object> values = node.Values;
                    values = SortHelper.SortList(values, primeOrder, orderByCollection, 0);
                    node.Values = values;
                }
                node = iterator.MoveNext();
            }
        }
    }
}
