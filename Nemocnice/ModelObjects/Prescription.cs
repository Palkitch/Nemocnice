using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    internal class Prescription
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime date { get; set; }
        public Prescription(int id, int doctorId, int patientId, DateTime date)
        {
            Id = id;
            DoctorId = doctorId;
            PatientId = patientId;
            this.date = date;
        }
    }
}