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
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Birthdate { get; set; }
        public string PIN { get; set; } 
        public string StartDate { get; set; }
        public int DoctorId { get; set; }
        public int AddressId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public Pacient(int id, string name, string surname, string birthdate, string pin, string startDate, int doctorId, int addressId, int insuranceCompanyId)
        {
            Id = id;
            Name = name;
            Surname = surname;
            Birthdate = birthdate;
            PIN = pin;
            StartDate = startDate;
            DoctorId = doctorId;
            AddressId = addressId;
            InsuranceCompanyId = insuranceCompanyId;
        }
    }
}
