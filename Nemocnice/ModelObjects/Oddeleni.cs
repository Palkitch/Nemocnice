using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Oddeleni
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int BuildingId { get; set; }

        public Oddeleni(int id, string name, int buildingId) 
        {
            ID = id;
            Name = name;
            BuildingId = buildingId;
        }
    }
}
