using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Luzko
    {
        public int Id { get; set; }
        public int Cislo { get; set; }
        public int? IdSestra { get; set; }
        public int IdPokoje { get; set; }

        public Luzko(int id, int cislo, int? idSestra, int idPokoje)
        {
            Id = id;
            Cislo = cislo;
            IdSestra = idSestra;
            IdPokoje = idPokoje;
        }
    }
}
