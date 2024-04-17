using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    internal interface ITreeNodeIterator
    {
        TreeNode Node { get; }
        TreeNode MoveNext();
        bool Valid { get; }
    }
}
