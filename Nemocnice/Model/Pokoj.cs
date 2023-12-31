﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.Windows.Shapes;

namespace Nemocnice.ModelObjects
{
    public class Pokoj
    {
        public int Id { get; set; }
        public int CisloPokoje { get; set; }
        public int IdOddeleni { get; set; }

        public Pokoj(int id, int cisloPokoje, int idOddeleni)
        {
            Id = id;
            CisloPokoje = cisloPokoje;
            IdOddeleni = idOddeleni;
        }


        public override string? ToString()
        {
            return $"{CisloPokoje}";
        }
    }
}

