using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    internal class InsuranceCompany
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Code { get; set; }
        public InsuranceCompany(string name, int id, int code)
        {
            Name = name;
            Id = id;
            Code = code;
        }
    }
}
