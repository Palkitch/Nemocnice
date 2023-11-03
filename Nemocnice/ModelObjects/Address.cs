using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    internal class Address
    {
        public int Id { get; set; }
        public int PostNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int PostCode { get; set; }
        public Address(int id, int postNumber, string street, string city, string country, int postCode)
        {
            Id = id;
            PostNumber = postNumber;
            Street = street;
            City = city;
            Country = country;
            PostCode = postCode;
        }
    }
}
