using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal class Constraint
    {
        internal string ParentTableName { get; set; } = String.Empty;
        internal string ChildTableName { get; set; } = String.Empty;
        internal string ChildForeignKeyMember { get; set; } = string.Empty;
        internal bool OneToOneNotMany { get; set; } = false;
        internal bool CascadeDelete { get; set; } = false;
        internal PropertyInfo ChildForeignKeyPropertyInfo { get; set; } = null;
    }

    internal class GenericConstraint<ParentType, ChildType> : Constraint
        where ParentType : class, new()
        where ChildType : class, new()
    {
        internal Table<ParentType> ParentTable { get; set; }
        internal Table<ChildType> ChildTable { get; set; }
    }
}
