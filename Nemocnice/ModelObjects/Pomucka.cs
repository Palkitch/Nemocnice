using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Pomucka
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public int Pocet { get; set; }
        public Pomucka(int id, string nazev, int pocet)
        {
            Id = id;
            Nazev = nazev;
            Pocet = pocet;
        }
    }
}
