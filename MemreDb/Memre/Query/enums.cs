using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal enum QueryType
    {
        Select,
        Update,
        Delete
    }

    internal enum SubClauseType
    {
        EndNode,
        And,
        Or
    }

    internal enum SubClauseComparitor
    {
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual
    }

    internal enum OrderDirection
    {
        Ascending,
        Descending
    }
}
