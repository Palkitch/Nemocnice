using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Diagnoza
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public Diagnoza(int id, string nazev)
        {
            Id = id;
            Nazev = nazev;
        }
    }
}