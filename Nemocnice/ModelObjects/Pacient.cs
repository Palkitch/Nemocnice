using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Pacient
    {
        public int Id { get; set; }
        public string Jmeno { get; set; }
        public string Prijmeni { get; set; }
        public string DatumNarozeni { get; set; }
        public string RodneCislo { get; set; } 
        public string DatumNastupu { get; set; }
        public int IdDoktora { get; set; }
        public int IdAdresy { get; set; }
        public int IdPojistovny { get; set; }
        public Pacient(int id, string jmeno, string prijmeni, string datumNarozeni, string rodneCislo, string datumNastupu, int idDoktora, int idAdresy, int idPojistovny)
        {
            Id = id;
            Jmeno = jmeno;
            Prijmeni = prijmeni;
            DatumNarozeni = datumNarozeni;
            RodneCislo = rodneCislo;
            DatumNastupu = datumNastupu;
            IdDoktora = idDoktora;
            IdAdresy = idAdresy;
            IdPojistovny = idPojistovny;
        }
    }
}
