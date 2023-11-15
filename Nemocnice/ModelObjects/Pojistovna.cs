using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Pojistovna
    {
        public string Nazev { get; set; }
        public int Id { get; set; }
        public int Cislo { get; set; }
        public Pojistovna(int id, string nazev, int cislo)
        {
            Id = id;
            Nazev = nazev;
            Cislo = cislo;
        }
    }
}
