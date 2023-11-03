using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Diagnosis
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Diagnosis(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}