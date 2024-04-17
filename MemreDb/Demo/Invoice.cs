using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Invoice
    {
        internal uint InvoiceId { get; set; }
        internal string InvoiceTitle { get; set; }
        internal uint CompanyId { get; set; }
        internal uint CustomerId { get; set; }

        public Invoice()
        {
            InvoiceTitle = String.Empty;
            InvoiceId = 0;
        }
    }
}
