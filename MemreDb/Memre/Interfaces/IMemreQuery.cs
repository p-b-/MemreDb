using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal interface IMemreQuery
    {
        WhereClause AddWhere();
        void OrderBy(List<OrderBy> orderByCollection);
        List<ContainedType> Select<ContainedType>(string tableName)
            where ContainedType : new();
        int? Execute();
    }
}
