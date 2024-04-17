using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Location
    {
        internal string LocationId { get; set; }
        internal string LocationName { get; set; }

        public Location()
        {
            LocationId = string.Empty;
            LocationName = string.Empty;
        }
    }
}
