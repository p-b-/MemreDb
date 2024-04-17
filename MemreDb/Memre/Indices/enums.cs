using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal enum IndexType
    {
        undefined,
        unsignedIntegerIndex,
        integerIndex,
        guidIndex,
        stringIndex
    }

    internal enum SortOrder
    {
        Ascending,
        Descending,
    }

    internal enum IncludeEquals
    {
        Include,
        DoNotInclude
    }

    internal enum SetOperation
    {
        Or,
        And
    }
}
