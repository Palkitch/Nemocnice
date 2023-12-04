using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.Model
{
    public class Zadost
    {
        public int Id { get; set; }
        public string NazevTabulky { get; set; }
        public string DataTabulky { get; set; }
        public string Typ {  get; set; }

        public Zadost(int id, string nazev, string data, string typ) 
        {
            Id = id;
            NazevTabulky = nazev;
            DataTabulky = data;
            Typ = typ;
        }
    }
}
