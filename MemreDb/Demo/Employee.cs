using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Demo
{
    internal class Employee
    {
        public uint EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public uint Age { get; set; }
        public uint CompanyId { get; set; }
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(FirstName))
                {
                    if (!String.IsNullOrEmpty(Surname))
                    {
                        return FirstName + " " + Surname;
                    }
                    return FirstName;
                }
                else if (!String.IsNullOrEmpty(Surname))
                {
                    return Surname;
                }
                return String.Empty;
            }
        }

        public Employee()
        {
            FirstName = String.Empty;
            Surname = String.Empty;
        }
    }
}
