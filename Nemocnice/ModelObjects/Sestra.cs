using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Sestra
    {
        public int IdZamestnance { get; set; }
        public Sestra(int idZamestnance)
        {
            IdZamestnance = idZamestnance;
        }
    }
}