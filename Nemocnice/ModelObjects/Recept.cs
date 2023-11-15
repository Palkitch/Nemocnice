using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Recept
    {
        public int Id { get; set; }
        public int IdDoktora { get; set; }
        public int IdPacienta { get; set; }
        public string Datum { get; set; }
        public Recept(int id, int idDoktora, int idPacienta, string datum)
        {
            Id = id;
            IdDoktora = idDoktora;
            IdPacienta = idPacienta;
            Datum = datum;
        }
    }
}