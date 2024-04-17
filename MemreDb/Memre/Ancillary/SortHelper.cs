using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Ancillary
{
    static internal class SortHelper
    {
        static internal List<object> SortList(List<object> values, OrderBy primeOrder, List<OrderBy> orderByCollection, int orderByIndex)
        {
            List<object> sortedList = new List<object>();

            bool sortByAscending = orderByCollection[orderByIndex].Direction == OrderDirection.Ascending;
            if (primeOrder.Direction == OrderDirection.Descending)
            {
                sortByAscending = !sortByAscending;
            }

            while (values.Count > 0)
            {
                List<object> extremeValues = RemoveExtremeValuesFromListAndReturn(values, orderByCollection[orderByIndex].MemberAccessor, sortByAscending);
                if (extremeValues != null && extremeValues.Count == 1)
                {
                    sortedList.Add(extremeValues[0]);
                }
                else
                {
                    if (orderByIndex < orderByCollection.Count - 1)
                    {
                        extremeValues = SortList(extremeValues, primeOrder, orderByCollection, orderByIndex + 1);
                    }
                    sortedList.AddRange(extremeValues);
                }
            }
            return sortedList;
        }

        static internal List<Object> RemoveExtremeValuesFromListAndReturn(List<object> values, PropertyInfo keyToSortOn, bool removeSmallestNotLargest)
        {
            // This would be 'lowestValue' or 'largestValue' if there were two methods
            //  instead of just one
            IComparable extremistValue = keyToSortOn.GetValue(values[0]) as IComparable;

            List<Object> toReturn = new List<Object>();
            List<int> indicesInValueList = new List<int>();
            toReturn.Add(values[0]);
            indicesInValueList.Add(0);

            for (int i = 1; i < values.Count; ++i)
            {
                IComparable compareTo = keyToSortOn.GetValue(values[i]) as IComparable;
                int comparison = compareTo.CompareTo(extremistValue);
                if (comparison == 0)
                {
                    toReturn.Add(values[i]);
                }
                else if ((comparison < 0 && removeSmallestNotLargest) ||
                        (comparison > 0 && !removeSmallestNotLargest))
                {
                    toReturn.Clear();
                    extremistValue = compareTo!;
                    toReturn.Add(values[i]);
                }
            }
            // TODO Determine if this should be optimised
            foreach (Object deleteFromValuesList in toReturn)
            {
                values.Remove(deleteFromValuesList);
            }
            return toReturn;
        }
    }
}
