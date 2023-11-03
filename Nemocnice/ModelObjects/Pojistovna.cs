using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Pojistovna
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Code { get; set; }
        public Pojistovna(int id, string name, int code)
        {
            Id = id;
            Name = name;
            Code = code;
        }
    }
}
