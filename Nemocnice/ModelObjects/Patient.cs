using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Birthdate { get; set; }
        public string PIN { get; set; } // Personal identification number
        public DateTime StartDate { get; set; }
        public int DoctorId { get; set; }
        public int AddressId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public Patient(int id, string name, string surname, DateTime birthdate, string pin, DateTime startDate, int doctorId, int addressId, int insuranceCompanyId)
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
