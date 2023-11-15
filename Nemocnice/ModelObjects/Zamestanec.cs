using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Zamestanec
    {
        public int Id { get; set; }
        public string Jmeno { get; set; }
        public string Prijmeni { get; set; }
        public int Plat { get; set; }
        public int IdOddeleni { get; set; }
        public int? IdNadrizeneho { get; set; }
        public int IdAdresy { get; set; }
        public char Druh { get; set; }
        public Zamestanec(int id, string jmeno, string prijmeni, int plat, int idOddeleni, int? idNadrizeneho, int idAdresy, char druh)
        {
            Id = id;
            Jmeno = jmeno;
            Prijmeni = prijmeni;
            Plat = plat;
            IdOddeleni = idOddeleni;
            IdNadrizeneho = idNadrizeneho;
            IdAdresy = idAdresy;
            Druh = druh;
        }
    }
}
