using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre.Indices;

namespace MemreDb.Memre
{
    internal class WhereClause
    {
        private SubClause _subClause;
        private Query _parentQuery;
        internal WhereClause(Query parentQuery)
        {
            this._parentQuery = parentQuery;
            this._subClause = new SubClause(_parentQuery);
        }

        internal SubClause GetTopLevelClause()
        {
            return this._subClause;
        }

        internal ITreeIndexInternals Execute()
        {
            return this._subClause.Execute();
        }
    }
}
