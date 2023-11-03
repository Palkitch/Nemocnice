using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Medicament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
        public Medicament(int id, string name, string category, int price)
        {
            Id = id;
            Name = name;
            Category = category;
            Price = price;
        }
    }
}