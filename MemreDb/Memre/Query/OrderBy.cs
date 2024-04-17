using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre
{
    internal class OrderBy
    {
        public string MemberToOrder { get; set; }
        public OrderDirection Direction { get; set; }
        public PropertyInfo MemberAccessor { get; set; }

        internal OrderBy(string memberToOrder, OrderDirection direction)
        {
            MemberToOrder = memberToOrder;
            Direction = direction;
            MemberAccessor = null;
        }

        internal OrderBy()
        {
            MemberToOrder = String.Empty;
            Direction = OrderDirection.Ascending;
            MemberAccessor = null;
        }
    }
}
