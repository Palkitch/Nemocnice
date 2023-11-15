using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Adresa
    {
        public int Id { get; set; }
        public int CisloPopisne { get; set; }
        public string Ulice { get; set; }
        public string Mesto { get; set; }
        public string Stat { get; set; }
        public int PSC { get; set; }

        public Adresa(int id, int postNumber, string street, string city, string country, int postCode)
        {
            Id = id;
            CisloPopisne = postNumber;
            Ulice = street;
            Mesto = city;
            Stat = country;
            PSC = postCode;
        }
    }
}
