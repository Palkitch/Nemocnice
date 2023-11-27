using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class KrevniSkupina
    {
        public int Id { get; set; }
        public string Typ { get; set; }
        public KrevniSkupina(int id, string typ)
        {
            this.Id = id;
            this.Typ = typ;
        }

        public override string? ToString()
        {
            return Typ;
        }
    }
}
