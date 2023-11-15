using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Lek
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public string Kategorie { get; set; }
        public int Cena { get; set; }
        public Lek(int id, string nazev, string kategorie, int cena)
        {
            Id = id;
            Nazev = nazev;
            Kategorie = kategorie;
            Cena = cena;
        }
    }
}