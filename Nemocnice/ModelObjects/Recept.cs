using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Recept
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string Date { get; set; }
        public Recept(int id, int doctorId, int patientId, string date)
        {
            Id = id;
            DoctorId = doctorId;
            PatientId = patientId;
            Date = date;
        }
    }
}