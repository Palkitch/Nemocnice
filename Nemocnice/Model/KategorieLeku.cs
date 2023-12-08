using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.Model
{

    public class KategorieLeku
    {
        public int Id { get; set; }
        public string Nazev { get; set; }

        public KategorieLeku(int id, string nazev) 
        {
            Id = id;
            Nazev = nazev;
        }

        public override string? ToString()
        {
            return Nazev;
        }
    }
}
