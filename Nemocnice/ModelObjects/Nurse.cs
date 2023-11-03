using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Nurse
    {
        public int EmployeeId { get; set; }
        public Nurse(int employeeId)
        {
            EmployeeId = employeeId;
        }
    }
}