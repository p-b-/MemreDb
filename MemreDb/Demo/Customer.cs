using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Customer
    {
        internal uint CompanyId { get; set; }
        internal uint CustomerId { get; set; }

        internal string CustomerName { get; set; }

        internal string CustomerEmail { get; set; }
//        internal int EmployeeCount { get; set; }

        public Customer()
        {
            CompanyId = 0;
            CustomerId = 0;
            CustomerEmail = string.Empty;
            CustomerName = string.Empty;
        }
    }
}
