using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.Windows.Shapes;

namespace Nemocnice.ModelObjects
{
    public class Budova
    {
        public int Id { get; set; }
        public string Nazev { get; set; }
        public int? PocetPater { get; set; }
        public int IdAdresa { get; set; }

        public Budova(int id, string nazev, int? pocetPater, int idAdresa)
        {
            Id = id;
            Nazev = nazev;
            PocetPater = pocetPater;
            IdAdresa = idAdresa;
        }

        public override string? ToString()
        {
            return $"{Nazev}";
        }
    }
}
