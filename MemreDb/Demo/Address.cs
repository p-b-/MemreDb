using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Address
    {
        internal uint AddressId { get; set; }
        internal string FirstLine { get; set; }
        internal string SecondLine { get; set; }
        internal string Town { get; set; }
        internal string County { get; set; }
        internal string PostCode { get; set; }
        internal uint EmployeeId { get; set; }

        public Address()
        {
            FirstLine = String.Empty;
            SecondLine = String.Empty;
            Town = String.Empty;
            County = String.Empty;
            PostCode = String.Empty;
            EmployeeId = 0;
        }
    }
}
