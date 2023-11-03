using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }

        public int Salary { get; set; }
        public int HospitalWardId { get; set; }
        public int? SuperiorId { get; set; }
        public int AddressId { get; set; }
        public char TypeOfEmployment { get; set; }
        public Employee(int id, string name, string surName, int salary, int hospitalWardId, int? superiorId, int addressId, char typeOfEmployment)
        {
            Id = id;
            Name = name;
            SurName = surName;
            Salary = salary;
            HospitalWardId = hospitalWardId;
            SuperiorId = superiorId;
            AddressId = addressId;
            TypeOfEmployment = typeOfEmployment;
        }
    }
}
