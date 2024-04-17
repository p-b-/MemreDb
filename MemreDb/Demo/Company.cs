using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Company
    {
        internal string CompanyName { get; set; }
        internal uint CompanyId { get; set; }
        public Company()
        {
            CompanyName = String.Empty;
            CompanyId = 0;
        }
    }
}
