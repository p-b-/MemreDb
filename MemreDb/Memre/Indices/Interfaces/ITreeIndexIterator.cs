using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    internal interface ITreeIndexIterator
    {
        IComparable CurrentIndex { get; }
        Object CurrentValue { get; }
        Object MoveNext();
        bool Valid { get; }
    }
}
