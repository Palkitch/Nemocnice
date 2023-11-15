using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Oddeleni
    {
        public int ID { get; set; }
        public string Nazev { get; set; }
        public int IdBudovy { get; set; }

        public Oddeleni(int id, string nazev, int idBudovy) 
        {
            ID = id;
            Nazev = nazev;
            IdBudovy = idBudovy;
        }
    }
}
