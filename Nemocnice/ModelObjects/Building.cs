using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfFloors { get; set; }
        public int AddressId { get; set; }

        public Building(int id, string name, int numberOfFloors, int addressId)
        {
            Id = id;
            Name = name;
            NumberOfFloors = numberOfFloors;
            AddressId = addressId;
        }


    }
}
