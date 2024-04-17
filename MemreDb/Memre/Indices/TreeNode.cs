using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    class TreeNode
    {
        internal int Height { get; set; }
        internal IComparable IndexValue { get; set; }
        internal List<object> Values { get; set; }
        internal TreeNode LeftNode;
        internal TreeNode RightNode;

        internal TreeNode(IComparable indexValue, object value)
        {
            IndexValue = indexValue;
            Values = new List<Object> { value };
            LeftNode = null;
            RightNode = null;
            Height = 1;
        }

        internal TreeNode(IComparable indexValue, List<object> values)
        {
            IndexValue = indexValue;

            Values = new List<object>();
            Values.AddRange(values);
            LeftNode = null;
            RightNode = null;
            Height = 1;
        }

        internal void CopyData(TreeNode rhs)
        {
            IndexValue = rhs.IndexValue;
            Values = rhs.Values;
        }

        internal void AddValue(object newValue)
        {
            Values.Add(newValue);
        }

        internal void AddValues(List<object> newValues)
        {
            Values.AddRange(newValues);
        }
    }
}
